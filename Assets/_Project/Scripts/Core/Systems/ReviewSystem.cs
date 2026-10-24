using System;
using System.Collections.Generic;

namespace Studio.Core
{
    public static class ReviewSystem
    {
        public const string BoardId = "board";   // the board fixed at the end of stage 1 as one decision
        const int BuildDays = 1;
        const int BigGap = 15;                   // an axis this far below its goal gets two quotes
        const int PitchRedoDays = 1;             // base price of the board, doubled as a stage 1 decision
        const int FirstVersionCritique = 2;      // v0.1 always gets at least this many critical quotes

        // Builds the next version: snapshot, verdict, feedback quotes. Costs a day.
        // Story beat (B-020): v0.1 always comes back for rework, so every game meets the feedback loop;
        // a later version is approved only once every critical quote of the previous one has its cause found.
        public static VersionRecord Review(GameState state, ContentDB db, int tolerance = 0)
        {
            if (state.stage != Stage.Release)
                throw new InvalidOperationException("Versions are built in Release, not " + state.stage);
            bool first = state.versions.Count == 0;

            var used = new HashSet<string>();
            foreach (var v in state.versions) used.UnionWith(v.quoteIds);

            var version = new VersionRecord
            {
                number = state.versions.Count + 1,
                day = state.day,
                axes = (int[])state.axes.Clone(),
                approved = !first && AllSorted(state.versions[state.versions.Count - 1], db),
            };
            int critique = 0;
            for (int i = 0; i < 4; i++)
            {
                int gap = state.goals[i] - state.axes[i];
                if (gap > tolerance) version.approved = false;
                if (gap > 0) critique += Pick(db, (Axis)i, false, gap >= BigGap ? 2 : 1, used, version.quoteIds);
            }
            var critiqued = new List<int>();
            if (first)
                foreach (int i in WeakestMetAxes(state))
                {
                    if (critique >= FirstVersionCritique) break;
                    if (Pick(db, (Axis)i, false, 1, used, version.quoteIds) > 0) { critique++; critiqued.Add(i); }
                }
            foreach (int i in StrongAxes(state))
                if (!critiqued.Contains(i) && Pick(db, (Axis)i, true, 1, used, version.quoteIds) > 0) break;

            state.day += BuildDays;
            state.versions.Add(version);
            return version;
        }

        // Every critical (non-praise) quote of the version has its cause found.
        public static bool AllSorted(VersionRecord version, ContentDB db)
        {
            foreach (var id in version.quoteIds)
            {
                var q = FindQuote(db, id);
                if (q != null && !q.praise && !version.blamed.Exists(b => b.quoteId == id)) return false;
            }
            return true;
        }

        // Decisions the journal highlights for a quote: live ones that lowered its axis, plus the worst ones
        // (so a quote on an axis nobody lowered still has a suspect). Idea-stage decisions answer as BoardId.
        public static List<string> Suspects(GameState state, string quoteId, ContentDB db)
        {
            var list = new List<string>();
            var quote = FindQuote(db, quoteId);
            if (quote == null || quote.praise) return list;
            int axis = (int)quote.axis;
            foreach (var r in Live(state, db))
                if (r.deltas[axis] < 0 && !list.Contains(Id(r))) list.Add(Id(r));
            foreach (var id in Culprits(state, axis, db))
                if (!list.Contains(id)) list.Add(id);
            return list;
        }

        // All live decisions tied for the worst delta on the axis.
        static List<string> Culprits(GameState state, int axis, ContentDB db)
        {
            int worst = int.MaxValue;
            foreach (var r in Live(state, db)) worst = Math.Min(worst, r.deltas[axis]);
            var ids = new List<string>();
            foreach (var r in Live(state, db))
                if (r.deltas[axis] == worst && !ids.Contains(Id(r))) ids.Add(Id(r));
            return ids;
        }

        // The board record first, then decisions not replaced by a reopen.
        static IEnumerable<DecisionRecord> Live(GameState state, ContentDB db)
        {
            yield return BoardRecord(state, db);
            foreach (var r in state.log)
                if (!r.replaced) yield return r;
        }

        static string Id(DecisionRecord r) => r.stage == Stage.Idea ? BoardId : r.messageId;

        // Axes at or above their goal, smallest surplus first, axis order on a tie.
        static List<int> WeakestMetAxes(GameState s)
        {
            var list = StrongAxes(s);
            list.Sort((a, b) =>
            {
                int c = (s.axes[a] - s.goals[a]).CompareTo(s.axes[b] - s.goals[b]);
                return c != 0 ? c : a.CompareTo(b);
            });
            return list;
        }

        // The decision behind a quote: the worst delta on its axis among live decisions, later one on a tie.
        // Stage 1 decisions and the board itself answer as BoardId. Null for praise or a quote not in the version.
        public static string Blame(GameState state, VersionRecord version, string quoteId, ContentDB db)
        {
            var quote = FindQuote(db, quoteId);
            if (quote == null || quote.praise || !version.quoteIds.Contains(quoteId)) return null;

            int axis = (int)quote.axis;
            var worst = BoardRecord(state, db);
            foreach (var r in state.log)
                if (!r.replaced && r.deltas[axis] <= worst.deltas[axis]) worst = r;
            return worst.stage == Stage.Idea ? BoardId : worst.messageId;
        }

        // Right guess (any decision tied for the worst, B-021) is recorded in version.blamed; a wrong one costs a day.
        public static bool CheckBlame(GameState state, VersionRecord version, string quoteId, string guessedMessageId, ContentDB db)
        {
            var quote = FindQuote(db, quoteId);
            if (Blame(state, version, quoteId, db) == null || !Culprits(state, (int)quote.axis, db).Contains(guessedMessageId))
            {
                state.day += 1;
                return false;
            }
            if (!version.blamed.Exists(b => b.quoteId == quoteId))
                version.blamed.Add(new QuoteBlame { quoteId = quoteId, messageId = guessedMessageId });
            return true;
        }

        // The committed board as a decision record, for Blame and for Reopen of BoardId.
        public static DecisionRecord BoardRecord(GameState state, ContentDB db)
        {
            // Commit on a scratch state measures what the board added to the axes.
            // shortcut: a single combo over +-50 on one axis gets clamped, fine for authored content.
            var scratch = new GameState { stage = Stage.Idea, axes = new[] { 50, 50, 50, 50 }, board = state.board };
            BoardRules.Commit(scratch, db);
            var deltas = new int[4];
            for (int i = 0; i < 4; i++) deltas[i] = scratch.axes[i] - 50;
            return new DecisionRecord
            {
                messageId = BoardId,
                stage = Stage.Idea,
                deltas = deltas,
                cost = new Cost { days = PitchRedoDays },
            };
        }

        // Original price x2 for stage 1, x1 for stage 2, at least a day.
        public static Cost ReopenCost(DecisionRecord record)
        {
            int mul = record.stage == Stage.Idea ? 2 : 1;
            return new Cost { days = Math.Max(1, record.cost.days * mul), budget = record.cost.budget * mul };
        }

        // Undoes the record and pays ReopenCost. A message decision then takes newOption at once;
        // the board goes back to stage Idea for rework (newOption is ignored), finish it with Recommit.
        public static void Reopen(GameState state, DecisionRecord record, OptionSO newOption, ContentDB db)
        {
            bool board = record.messageId == BoardId;
            var message = board ? null : FindMessage(db, record.messageId);
            if (!board && message == null)
                throw new ArgumentException("Message " + record.messageId + " is not in ContentDB");

            var cost = ReopenCost(record);
            int days = cost.days + (board ? 0 : newOption.cost.days);
            int budget = cost.budget + (board ? 0 : newOption.cost.budget);
            if (state.budget < budget || state.day + days > state.deadline)
                throw new InvalidOperationException("Cannot afford to reopen " + record.messageId);

            for (int i = 0; i < 4; i++)
                state.axes[i] = Math.Max(0, Math.Min(100, state.axes[i] - record.deltas[i]));
            state.day += cost.days;
            state.budget -= cost.budget;
            record.replaced = true;

            if (board)
            {
                state.stage = Stage.Idea;
                return;
            }
            DecisionResolver.Apply(state, message, newOption, db).stage = record.stage;
        }

        // Pitch of a reopened board: Commit, then straight back to Release.
        public static void Recommit(GameState state, ContentDB db)
        {
            BoardRules.Commit(state, db);
            state.stage = Stage.Release;
        }

        // Axes at or above their goal, biggest surplus first, axis order on a tie.
        static List<int> StrongAxes(GameState s)
        {
            var list = new List<int>();
            for (int i = 0; i < 4; i++)
                if (s.axes[i] >= s.goals[i]) list.Add(i);
            list.Sort((a, b) =>
            {
                int c = (s.axes[b] - s.goals[b]).CompareTo(s.axes[a] - s.goals[a]);
                return c != 0 ? c : a.CompareTo(b);
            });
            return list;
        }

        // Adds up to n quotes of the axis in db order, unseen in earlier versions first; returns how many.
        static int Pick(ContentDB db, Axis axis, bool praise, int n, HashSet<string> used, List<string> into)
        {
            int added = 0;
            for (int pass = 0; pass < 2 && added < n; pass++)   // pass 1 repeats old quotes once new ones ran out
                foreach (var q in db.quotes)
                {
                    if (added == n) break;
                    if (q == null || q.axis != axis || q.praise != praise || into.Contains(q.id)) continue;
                    if (pass == 0 && used.Contains(q.id)) continue;
                    into.Add(q.id);
                    added++;
                }
            return added;
        }

        static QuoteSO FindQuote(ContentDB db, string id)
        {
            foreach (var q in db.quotes)
                if (q != null && q.id == id) return q;
            return null;
        }

        static MessageSO FindMessage(ContentDB db, string id)
        {
            if (db.brief != null && db.brief.id == id) return db.brief;
            foreach (var m in db.messages)
                if (m != null && m.id == id) return m;
            return null;
        }
    }
}
