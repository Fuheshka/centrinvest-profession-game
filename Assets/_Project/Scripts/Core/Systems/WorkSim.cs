using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    // Workday: the only clock of the MVP. Time moves only through Tick; a day ends only through EndDay
    // (docs/MVP_RULES.md 3.1-3.7).
    public static class WorkSim
    {
        const float Step = 0.05f;   // inner step, so energy thresholds are hit within 0.02 energy

        public static bool Assign(LevelState s, string taskId, string employeeId)
        {
            var t = s.Task(taskId);
            var e = s.Employee(employeeId);
            if (s.stage != LevelStage.Production || t == null || t.done || e == null || e.d != t.d) return false;
            if (t.assigneeId != null && t.assigneeId != employeeId)
            {
                var old = s.Employee(t.assigneeId);
                if (old != null && old.currentTaskId == t.id) old.currentTaskId = null;
            }
            t.assigneeId = employeeId;
            return true;
        }

        public static bool SetPriority(LevelState s, string taskId, int index)
        {
            var t = s.Task(taskId);
            if (t == null || t.done) return false;
            s.tasks.Remove(t);
            s.tasks.Insert(Mathf.Clamp(index, 0, s.tasks.Count), t);
            return true;
        }

        // Nearest earlier open task of the same assignee: priority only matters inside one person's queue, so "up" skips others.
        public static int HigherSlot(LevelState s, string taskId)
        {
            var t = s.Task(taskId);
            if (t == null || t.done || t.assigneeId == null) return -1;
            for (int i = s.tasks.IndexOf(t) - 1; i >= 0; i--)
                if (!s.tasks[i].done && s.tasks[i].assigneeId == t.assigneeId) return i;
            return -1;
        }

        static bool CanPay(LevelState s, LevelSO level, EmployeeState e) =>
            e.paidDay == s.day || s.budget >= Staffing.Rate(s, level, e.id);

        static void Pay(LevelState s, LevelSO level, EmployeeState e)
        {
            if (e.paidDay == s.day) return;
            s.budget -= Staffing.Rate(s, level, e.id);
            e.paidDay = s.day;
        }

        // An employee with an unanswered question from the chat stops until the player answers.
        static bool Asking(LevelState s, EmployeeState e) => s.pendingEvents.Exists(p => p.employeeId == e.id);

        static bool Ready(LevelSO level, EmployeeState e, TaskState t) =>
            e.energy >= level.tuning.TierEnergy(t.tier) || (e.currentTaskId == t.id && e.energy > 0);

        // First task in priority order that the employee is fit for; null when none.
        static TaskState Pick(LevelState s, LevelSO level, EmployeeState e)
        {
            foreach (var t in s.tasks)
                if (!t.done && t.assigneeId == e.id && Ready(level, e, t)) return t;
            return null;
        }

        public static WorkStatus Status(LevelState s, LevelSO level, string taskId, out WaitReason why)
        {
            why = WaitReason.None;
            var t = s.Task(taskId);
            if (t == null) return WorkStatus.Open;
            if (t.done) return WorkStatus.Done;
            var e = t.assigneeId == null ? null : s.Employee(t.assigneeId);
            if (e == null) { why = WaitReason.NoAssignee; return WorkStatus.Open; }
            if (Asking(s, e)) { why = WaitReason.Question; return WorkStatus.Waiting; }
            if (!Ready(level, e, t)) { why = WaitReason.NoEnergy; return WorkStatus.Waiting; }
            if (!CanPay(s, level, e)) { why = WaitReason.NoBudget; return WorkStatus.Waiting; }
            if (Pick(s, level, e) != t) { why = WaitReason.NotFirst; return WorkStatus.Queued; }
            return s.stage == LevelStage.Production && s.concept.approved ? WorkStatus.Working : WorkStatus.Queued;
        }

        // Advances the day by dt (clamped). Returns the night report when the day ended in this call.
        public static NightReport Tick(LevelState s, LevelSO level, float dt)
        {
            if (s.stage != LevelStage.Production) return null;
            dt = Mathf.Min(dt, level.tuning.maxTickSeconds);
            while (dt > 1e-6f)
            {
                float h = Mathf.Min(Step, dt, level.tuning.daySeconds - s.dayElapsed);
                Advance(s, level, h);
                s.dayElapsed += h;
                dt -= h;
                if (s.dayElapsed >= level.tuning.daySeconds - 1e-4f) return EndDay(s, level, s.day);
            }
            return null;
        }

        static void Advance(LevelState s, LevelSO level, float h)
        {
            var tu = level.tuning;
            foreach (var e in s.staff)
            {
                var t = Pick(s, level, e);
                if (t != null && (!CanPay(s, level, e) || Asking(s, e))) t = null;
                if (t == null)
                {
                    e.currentTaskId = null;
                    e.energy = Mathf.Min(100, e.energy + tu.idleRegenPerSecond * h);
                    continue;
                }
                Pay(s, level, e);
                e.currentTaskId = t.id;
                var c = level.Candidate(e.id);
                float q = tu.qualityBase + tu.qualityPerPoint * c.quality - (e.energy < tu.fatigueBelow ? tu.fatiguePenalty : 0);
                float pts = Mathf.Min(c.speed * h * s.Mul(e.d), t.effort - t.progress);
                t.progress += pts;
                t.qualitySum += q * pts;
                e.energy = Mathf.Max(0, e.energy - tu.drainPerSecond * h);
                if (e.energy <= 0) e.currentTaskId = null;
                if (t.progress >= t.effort - 1e-4f) Complete(s, level, e, t);
            }
        }

        static void Complete(LevelState s, LevelSO level, EmployeeState e, TaskState t)
        {
            t.done = true;
            t.progress = t.effort;
            e.currentTaskId = null;
            float q = t.qualitySum / t.effort;
            FileState f;
            if (t.isFix && (f = s.File(t.fileId)) != null)
            {
                f.quality = (1 - level.tuning.fixEffortShare) * f.quality + level.tuning.fixEffortShare * q;
                f.revision++;
                f.authorId = e.id; f.day = s.day;
            }
            else
            {
                f = new FileState { id = t.id, taskId = t.id, d = t.d, title = t.title, revision = 1, quality = q, authorId = e.id, day = s.day };
                s.files.Add(f);
            }
            string text = f.title + " · v" + f.revision + " · качество " + Mathf.RoundToInt(f.quality);
            s.Say(e.id, "text", t.isFix ? "Готово, поправил(а): " + f.title : "Готово: " + f.title);
            s.Say(e.id, "file", text, f.id);
            if (s.tasks.TrueForAll(x => x.done)) s.Say(LevelState.Boss, "text", level.Text("all_done", "Все файлы готовы. Заходи во вкладку «Задачи» и собирай игру."));
        }

        // One transition per day: the timer and the button both pass the day they saw, the second call is a no-op.
        public static NightReport EndDay(LevelState s, LevelSO level, int expectedDay)
        {
            if (s.stage != LevelStage.Production || s.day != expectedDay) return null;
            var report = new NightReport { day = s.day };
            foreach (var e in s.staff)
            {
                e.energy = Mathf.Min(100, e.energy + level.tuning.nightRest);
                e.currentTaskId = null;
                report.lines.Add(level.Candidate(e.id).name + " отдыхал(а), энергия " + Mathf.RoundToInt(e.energy));
            }

            if (s.day >= s.deadline)
            {
                s.stage = LevelStage.Failed;
                s.failReason = FailReason.Deadline;
                s.score = s.builds.Count > 0 ? s.builds[s.builds.Count - 1].overall : 0;
                s.Say(LevelState.Boss, "text", level.Text("deadline_fail", "Срок вышел, а игры, которая набрала проходной балл, нет. Разберём, что пошло не так."));
                return report;
            }
            s.day++;
            s.dayElapsed = 0;
            s.mulDesign = s.mulMech = s.mulSound = 1;
            bool last = s.day >= s.deadline;
            s.Say(LevelState.Boss, "text", last
                ? level.Text("last_day", "Последний день. Собери игру до конца дня.")
                : level.Text("morning", "Утро, день {day}. До дедлайна дней: {left}.").Replace("{day}", s.day.ToString()).Replace("{left}", (s.deadline - s.day + 1).ToString()));
            return report;
        }
    }
}
