using System;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    // Random work events: during the workday an employee writes in the chat and offers two choices.
    // Kept apart from WorkSim so the deterministic route tests do not see random events; the game loop calls Tick.
    public static class EventRules
    {
        const int MaxPending = 2;
        const float CroakEvery = 12;

        // The programmer's virus: the UI plays a sound on every croak.
        public static event Action Croaked;

        // next(max) returns an int in [0, max); injected so tests are deterministic.
        public static void Tick(LevelState s, LevelSO level, float dt, Func<int, int> next)
        {
            if (s.stage != LevelStage.Production) return;
            if (s.croak && (s.croakTimer -= dt) <= 0) { s.croakTimer = CroakEvery; Croaked?.Invoke(); }
            if (level.events.Count == 0) return;
            var tu = level.tuning;
            if (!s.staff.Exists(e => e.currentTaskId != null)) return;   // nobody is working: nothing to write about
            if (!s.eventsStarted) { s.eventsStarted = true; s.eventTimer = tu.eventFirst; }
            s.eventTimer -= dt;
            if (s.eventTimer > 0 || s.pendingEvents.Count >= MaxPending) return;
            if (Spawn(s, level, next)) s.eventTimer = tu.eventEvery + next((int)tu.eventJitter + 1);
            else s.eventTimer = 10;   // nothing fits right now, look again soon
        }

        static bool Spawn(LevelState s, LevelSO level, Func<int, int> next)
        {
            var pairs = new List<(string who, EventDef ev)>();
            var forced = new List<(string who, EventDef ev)>();
            foreach (var ev in level.events)
            {
                if (s.eventsSeen.Contains(ev.id) || s.pendingEvents.Exists(p => p.eventId == ev.id)) continue;
                if (!string.IsNullOrEmpty(ev.after) &&!s.eventsSeen.Contains(ev.after)) continue;
                if (!ev.needs.TrueForAll(d => s.staff.Exists(x => x.d == d))) continue;
                var mine = ev.must ? forced : pairs;
                bool bossFree = !s.pendingEvents.Exists(p => p.employeeId == LevelState.Boss);
                if (ev.boss)
                {
                    if (bossFree) mine.Add((LevelState.Boss, ev));
                    continue;
                }
                int before = mine.Count;
                foreach (var e in s.staff)
                    if (e.currentTaskId != null && (ev.anyone || ev.d == e.d) && !s.pendingEvents.Exists(p => p.employeeId == e.id)) mine.Add((e.id, ev));
                if (ev.must && mine.Count == before && bossFree) mine.Add((LevelState.Boss, ev));   // nobody fits: the boss passes it on
            }
            if (forced.Count > 0) pairs = forced;
            if (pairs.Count == 0) return false;
            var (who, def) = pairs[next(pairs.Count)];
            s.pendingEvents.Add(new PendingEvent { eventId = def.id, employeeId = who });
            s.Say(who, "event", def.text, def.id);
            return true;
        }

        public static PendingEvent Find(LevelState s, string eventId, string employeeId) =>
            s.pendingEvents.Find(p => p.eventId == eventId && p.employeeId == employeeId);

        // The task the effect lands on: what the employee is doing now, else their first open task.
        static TaskState Target(LevelState s, EmployeeState e)
        {
            var cur = e.currentTaskId == null ? null : s.Task(e.currentTaskId);
            if (cur != null && !cur.done) return cur;
            return s.tasks.Find(t => !t.done && t.assigneeId == e.id);
        }

        static void Bump(TaskState t, float share) => t.progress = Mathf.Clamp(t.progress + share * t.effort, 0, t.effort * 0.99f);   // never completes a task by itself

        public static bool Resolve(LevelState s, LevelSO level, string eventId, string employeeId, int option, Func<int, int> next = null)
        {
            var p = Find(s, eventId, employeeId);
            var def = level.events.Find(x => x.id == eventId);
            var e = s.Employee(employeeId);   // null for the boss
            if (p == null || def == null || (e == null && employeeId != LevelState.Boss) || option < 0 || option >= def.options.Count) return false;
            var o = def.options[option];
            if (o.budget < 0 && s.budget < -o.budget) return false;   // can't pay for it
            next = next ?? (n => UnityEngine.Random.Range(0, n));
            s.budget = Mathf.Max(0, s.budget + o.budget);
            if (e != null)
            {
                e.energy = Mathf.Clamp(e.energy + o.energy, 0, 100);
                e.rateBonus += o.rate;
                var t = Target(s, e);
                if (t != null) Bump(t, o.progress);
            }
            foreach (var m in s.staff) m.energy = Mathf.Clamp(m.energy + o.teamEnergy, 0, 100);
            foreach (var t in s.tasks)
            {
                if (t.done) continue;
                var ds = o.disc.Find(x => x.d == t.d);
                Bump(t, ds != null ? ds.share : o.progressAll);
            }
            if (o.failChance > 0 && next(100) < o.failChance * 100) foreach (var t in s.tasks) if (!t.done) Bump(t, o.failAll);
            if (o.speed > 0)
            {
                if (o.speedAll || e == null) { s.mulDesign *= o.speed; s.mulMech *= o.speed; s.mulSound *= o.speed; }
                else s.SetMul(e.d, s.Mul(e.d) * o.speed);
            }
            if (o.croak) s.croak = true;
            if (o.endDay) s.dayElapsed = level.tuning.daySeconds;   // the next tick closes the day and the UI shows the night report
            s.pendingEvents.Remove(p);
            s.eventsSeen.Add(eventId);
            string fx = Names.Effects(o);
            s.Say(employeeId, "me", o.label + (fx.Length > 0 ? " (" + fx + ")" : ""));   // the player's side of the dialogue
            s.Say(employeeId, "text", o.reply);
            if (o.fire >= 0) { var leaver = s.staff.Find(x => x.d == (Discipline)o.fire); if (leaver != null) Staffing.Fire(s, leaver.id); }
            return true;
        }
    }
}
