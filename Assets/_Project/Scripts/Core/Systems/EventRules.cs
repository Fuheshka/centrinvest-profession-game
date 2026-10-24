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

        // next(max) returns an int in [0, max); injected so tests are deterministic.
        public static void Tick(LevelState s, LevelSO level, float dt, Func<int, int> next)
        {
            if (s.stage != LevelStage.Production || level.events.Count == 0) return;
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
            var pairs = new List<(EmployeeState e, EventDef ev)>();
            foreach (var e in s.staff)
            {
                if (e.currentTaskId == null || s.pendingEvents.Exists(p => p.employeeId == e.id)) continue;
                foreach (var ev in level.events)
                    if ((ev.anyone || ev.d == e.d) && !s.eventsSeen.Contains(ev.id) && !s.pendingEvents.Exists(p => p.eventId == ev.id)) pairs.Add((e, ev));
            }
            if (pairs.Count == 0) return false;
            var (emp, def) = pairs[next(pairs.Count)];
            s.pendingEvents.Add(new PendingEvent { eventId = def.id, employeeId = emp.id });
            s.Say(emp.id, "event", def.text, def.id);
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

        public static bool Resolve(LevelState s, LevelSO level, string eventId, string employeeId, int option)
        {
            var p = Find(s, eventId, employeeId);
            var def = level.events.Find(x => x.id == eventId);
            var e = s.Employee(employeeId);
            if (p == null || def == null || e == null || option < 0 || option >= def.options.Count) return false;
            var o = def.options[option];
            if (o.budget < 0 && s.budget < -o.budget) return false;   // can't pay for it
            s.budget = Mathf.Max(0, s.budget + o.budget);
            e.energy = Mathf.Clamp(e.energy + o.energy, 0, 100);
            var t = Target(s, e);
            if (t != null) t.progress = Mathf.Clamp(t.progress + o.progress * t.effort, 0, t.effort * 0.99f);   // never completes a task by itself
            s.pendingEvents.Remove(p);
            s.eventsSeen.Add(eventId);
            string fx = Names.Effects(o);
            s.Say(employeeId, "me", o.label + (fx.Length > 0 ? " (" + fx + ")" : ""));   // the player's side of the dialogue
            s.Say(employeeId, "text", o.reply);
            return true;
        }
    }
}
