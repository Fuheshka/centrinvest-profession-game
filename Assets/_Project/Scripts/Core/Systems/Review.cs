using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    // Build from real files, score by sound/design/mechanics, negative review -> fix (docs/MVP_RULES.md 3.7, 3.10).
    public static class Review
    {
        static int Round(float x) => Mathf.FloorToInt(x + 0.5f);
        static BuildRecord Last(LevelState s) => s.builds.Count > 0 ? s.builds[s.builds.Count - 1] : null;

        public static bool CanBuild(LevelState s, LevelSO level, out string reason)
        {
            reason = null;
            if (s.stage != LevelStage.Production) { reason = s.stage == LevelStage.Concept ? "Сначала утверди концепт" : "Уровень завершён"; return false; }
            foreach (var t in s.tasks)
                if (!t.done) { reason = t.isFix ? "Идёт исправление: " + t.title : "Не готово: " + t.title; return false; }
            var last = Last(s);
            if (last != null)
            {
                bool changed = false;
                foreach (var f in s.files)
                {
                    var b = last.files.Find(x => x.fileId == f.id);
                    if (b == null || b.revision != f.revision) changed = true;
                }
                if (!changed) { reason = "Файлы не менялись: пересборка ничего не даст"; return false; }
            }
            return true;
        }

        public static BuildRecord Score(LevelState s, LevelSO level)
        {
            var r = new BuildRecord { number = s.builds.Count + 1, day = s.day };
            var concept = ConceptRules.Totals(s, level);
            var sum = new float[3]; var weight = new float[3];
            foreach (var f in s.files)
            {
                var t = s.Task(f.taskId);
                float w = t != null ? t.effort : 1;
                sum[(int)f.d] += f.quality * w; weight[(int)f.d] += w;
                r.files.Add(new BuiltFile { fileId = f.id, revision = f.revision, quality = f.quality });
            }
            float total = 0;
            for (int i = 0; i < 3; i++)
            {
                r.concept[i] = concept[i] * level.tuning.conceptStep;
                float q = weight[i] > 0 ? sum[i] / weight[i] : 0;
                r.scores[i] = Mathf.Clamp(Round(q + r.concept[i]), 0, 100);
                total += r.scores[i];
            }
            foreach (var req in level.requirements)
            {
                var b = s.BlockOf(req.type);
                if (b == null || b.paramId != req.paramId)
                    r.unmetRequirements.Add(Names.Of(req.type) + ": нужно «" + level.Param(req.paramId)?.title + "»");
            }
            r.overall = Mathf.Clamp(Round(total / 3) - level.tuning.requirementPenalty * r.unmetRequirements.Count, 0, 100);
            r.passed = r.overall >= level.passScore;
            return r;
        }

        public static BuildRecord Build(LevelState s, LevelSO level)
        {
            if (!CanBuild(s, level, out _)) return null;
            var r = Score(s, level);
            s.builds.Add(r);
            s.Say(LevelState.Boss, "review", Summary(r, level), r.number.ToString());
            if (r.passed)
            {
                s.stage = LevelStage.Passed;
                s.score = r.overall;
            }
            return r;
        }

        public static string Summary(BuildRecord r, LevelSO level)
        {
            string text = "Сборка " + r.number + ": оценка " + r.overall + ". Звук " + r.scores[2] + ", дизайн " + r.scores[0] + ", механики " + r.scores[1] + ".";
            if (r.unmetRequirements.Count > 0) text += " Не выполнено по ТЗ: " + string.Join("; ", r.unmetRequirements) + ".";
            text += r.passed ? " " + level.Text("passed", "Заказчик доволен, игра принята!")
                             : " " + level.Text("rework", "Ниже порога " + level.passScore + ". Отдай слабый файл на исправление.");
            return text;
        }

        static string Band(LevelSO level, string key, int score, string hi, string mid, string lo) =>
            level.Text(key + (score >= 85 ? "_hi" : score >= 70 ? "_mid" : "_lo"), score >= 85 ? hi : score >= 70 ? mid : lo);

        // What the customer says, criterion by criterion, plus unmet requirements and the verdict.
        public static string Remarks(BuildRecord r, LevelSO level)
        {
            var lines = new List<string>
            {
                "Дизайн: " + Band(level, "remark_design", r.scores[0], "картинка заказчику нравится, подросткам зайдёт.", "выглядит нормально, но без изюминки.", "картинка слабая, подростки не оценят."),
                "Механики: " + Band(level, "remark_mech", r.scores[1], "играть приятно, управление отзывчивое.", "играется, но местами неловко.", "играть неудобно, много шероховатостей."),
                "Звук: " + Band(level, "remark_sound", r.scores[2], "музыка и эффекты на месте.", "звук есть, но не запоминается.", "звук слабый, местами раздражает."),
            };
            foreach (var u in r.unmetRequirements) lines.Add("Не по ТЗ – " + u + ".");
            lines.Add(r.passed ? level.Text("passed", "Заказчик доволен, игра принята!") : level.Text("rework", "Ниже порога " + level.passScore + ". Отдай слабый файл на исправление."));
            return string.Join("\n", lines);
        }

        public static int Weakest(BuildRecord r)
        {
            int w = 0;
            for (int i = 1; i < 3; i++) if (r.scores[i] < r.scores[w]) w = i;
            return w;
        }

        public static bool CanFix(LevelState s, LevelSO level, string fileId, string employeeId, out string reason)
        {
            reason = null;
            var last = Last(s);
            var f = s.File(fileId);
            var e = s.Employee(employeeId);
            if (s.stage != LevelStage.Production) reason = "Уровень завершён";
            else if (last == null || last.passed) reason = "Исправления – после негативного отзыва";
            else if (f == null) reason = "Нет такого файла";
            else if (e == null || e.d != f.d) reason = "Нужен сотрудник: " + Names.Of(f.d);
            else if (s.tasks.Exists(t => t.isFix && !t.done && t.fileId == fileId)) reason = "Файл уже на исправлении";
            else if (e.paidDay != s.day && s.budget < Staffing.Rate(s, level, e.id)) reason = "Не хватает денег на день работы";
            return reason == null;
        }

        public static bool SendToFix(LevelState s, LevelSO level, string fileId, string employeeId)
        {
            if (!CanFix(s, level, fileId, employeeId, out _)) return false;
            var f = s.File(fileId);
            var orig = s.Task(f.taskId);
            var t = new TaskState
            {
                id = "fix-" + f.id + "-" + f.revision, d = f.d, title = "Исправить: " + f.title, tier = orig.tier,
                effort = orig.effort * level.tuning.fixEffortShare, assigneeId = employeeId, isFix = true, fileId = f.id,
            };
            s.tasks.Insert(0, t);
            s.Say(employeeId, "text", "Взял(а) на исправление: " + f.title);
            return true;
        }

        // Morning check, called by the game loop right after EndDay: true -> level failed, no money for any useful work.
        public static bool CheckBudgetDeadlock(LevelState s, LevelSO level)
        {
            if (s.stage != LevelStage.Production) return false;
            var need = new HashSet<Discipline>();
            foreach (var t in s.tasks) if (!t.done) need.Add(t.d);
            if (need.Count == 0)
            {
                if (CanBuild(s, level, out _)) return false;
                need.Add(Discipline.Design); need.Add(Discipline.Mechanics); need.Add(Discipline.Sound);
            }
            foreach (var d in need)
            {
                foreach (var e in s.staff)
                    if (e.d == d && (e.paidDay == s.day || s.budget >= Staffing.Rate(s, level, e.id))) return false;
                if (s.staff.Count < level.tuning.teamLimit)
                    foreach (var c in level.candidates)
                        if (c.d == d && s.Employee(c.id) == null && s.budget >= Staffing.DailyRate(c, level.tuning)) return false;
            }
            s.stage = LevelStage.Failed;
            s.failReason = FailReason.NoBudget;
            var last = Last(s);
            s.score = last != null ? last.overall : 0;
            s.Say(LevelState.Boss, "text", level.Text("budget_fail", "Деньги кончились: платить команде нечем. Проект закрыт."));
            return true;
        }
    }
}
