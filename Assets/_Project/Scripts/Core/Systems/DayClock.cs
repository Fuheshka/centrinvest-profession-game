using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    public static class DayClock
    {
        // Ends the day: moves the clock, switches the stage if its day came, returns the morning's new messages.
        public static List<MessageSO> EndDay(GameState state, ContentDB db)
        {
            var fresh = new List<MessageSO>();
            if (state.stage == Stage.Done) return fresh;

            state.day += 1;
            AdvanceStage(state, db);
            if (state.stage == Stage.Done) return fresh;

            var followUps = FollowUpIds(db);
            foreach (var m in AllMessages(db))
                if (m.stage == state.stage && m.day <= state.day && !followUps.Contains(m.id))
                    Deliver(state, m, fresh);

            for (int i = state.pending.Count - 1; i >= 0; i--)
            {
                var p = state.pending[i];
                if (p.day > state.day) continue;
                state.pending.RemoveAt(i);
                var m = Find(db, p.messageId);
                if (m == null) Debug.LogWarning("DayClock: follow-up " + p.messageId + " is not in ContentDB, dropped");
                else Deliver(state, m, fresh);
            }
            return fresh;
        }

        public static bool IsOverDeadline(GameState state) => state.day > state.deadline;

        // Idea -> Production happens only in BoardRules.Commit.
        static void AdvanceStage(GameState state, ContentDB db)
        {
            if (state.stage == Stage.Production)
            {
                int start = ReleaseStartDay(db);
                if (start > 0 && state.day >= start) state.stage = Stage.Release;
            }
            if (state.stage == Stage.Release && state.versions.Exists(v => v.approved))
                state.stage = Stage.Done;
            if (IsOverDeadline(state)) state.stage = Stage.Done;   // shipped as is
        }

        static int ReleaseStartDay(ContentDB db)
        {
            if (db.releaseStartDay > 0) return db.releaseStartDay;
            int first = 0;
            foreach (var m in db.messages)
                if (m != null && m.stage == Stage.Release && (first == 0 || m.day < first)) first = m.day;
            return first;
        }

        static void Deliver(GameState state, MessageSO m, List<MessageSO> fresh)
        {
            if (state.inbox.Contains(m.id) || state.answered.Contains(m.id)) return;
            state.inbox.Add(m.id);
            fresh.Add(m);
        }

        static IEnumerable<MessageSO> AllMessages(ContentDB db)
        {
            foreach (var m in db.messages) if (m != null) yield return m;
        }

        static MessageSO Find(ContentDB db, string id)
        {
            if (db.brief != null && db.brief.id == id) return db.brief;
            foreach (var m in AllMessages(db)) if (m.id == id) return m;
            return null;
        }

        // Follow-ups live in db.messages for lookup but arrive only through `pending`.
        static HashSet<string> FollowUpIds(ContentDB db)
        {
            var ids = new HashSet<string>();
            foreach (var m in AllMessages(db))
                foreach (var o in m.options)
                    if (o != null && o.followUp != null) ids.Add(o.followUp.id);
            if (db.brief != null)
                foreach (var o in db.brief.options)
                    if (o != null && o.followUp != null) ids.Add(o.followUp.id);
            return ids;
        }
    }
}
