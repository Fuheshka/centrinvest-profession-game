using System;
using System.Collections.Generic;

namespace Studio.Core
{
    public static class BoardRules
    {
        const int ConflictCoherencePenalty = 10;

        public static void Place(GameState state, CardSO card)
        {
            var placed = state.board.placed;
            var i = placed.FindIndex(e => e.slot == card.slot);
            if (i >= 0)
            {
                if (placed[i].cardId == card.id) return;
                DropLinks(state.board, placed[i].cardId);
                placed[i].cardId = card.id;
            }
            else placed.Add(new SlotEntry { slot = card.slot, cardId = card.id });
        }

        public static void Remove(GameState state, Slot slot)
        {
            var placed = state.board.placed;
            var i = placed.FindIndex(e => e.slot == slot);
            if (i < 0) return;
            DropLinks(state.board, placed[i].cardId);
            placed.RemoveAt(i);
        }

        // Cards are addressed by id. Returns false when nothing was added.
        public static bool Connect(GameState state, string a, string b)
        {
            if (a == b || !OnBoard(state.board, a) || !OnBoard(state.board, b)) return false;
            if (state.board.links.Exists(l => SamePair(l.a, l.b, a, b))) return false;
            state.board.links.Add(new Link { a = a, b = b });
            return true;
        }

        public static bool Disconnect(GameState state, string a, string b)
            => state.board.links.RemoveAll(l => SamePair(l.a, l.b, a, b)) > 0;

        // showThreads comes from DifficultySettings; the caller passes it so Core rules stay independent of Difficulty.
        public static ThreadKind Thread(CardSO a, CardSO b, ContentDB db, BoardState board, bool showThreads)
        {
            var combo = FindCombo(db, a.id, b.id);
            if (combo == null) return ThreadKind.Neutral;
            var known = showThreads || (board.revealed.Contains(a.id) && board.revealed.Contains(b.id));
            return known ? combo.kind : ThreadKind.Unknown;
        }

        public static void Reveal(GameState state, string cardId)
        {
            if (!state.board.revealed.Contains(cardId)) state.board.revealed.Add(cardId);
        }

        // "Research market": costs budget only (cost.days is ignored). Already revealed card is free.
        public static void Research(GameState state, string cardId, Cost cost)
        {
            if (state.board.revealed.Contains(cardId)) return;
            if (state.budget < cost.budget)
                throw new InvalidOperationException("Not enough budget to research " + cardId);
            state.budget -= cost.budget;
            Reveal(state, cardId);
        }

        public static int[] Goals(BoardState board, ContentDB db)
        {
            var goals = new[] { 50, 50, 50, 50 };
            foreach (var entry in board.placed)
            {
                var card = FindCard(db, entry.cardId);
                if (card != null) Add(goals, card.goalDeltas);
            }
            foreach (var combo in LinkedCombos(board, db))
                if (combo.kind == ThreadKind.Match) Add(goals, combo.deltas);
            Clamp(goals);
            return goals;
        }

        // Fixes the board at the end of stage 1: writes goals, applies linked combo effects to axes.
        public static void Commit(GameState state, ContentDB db)
        {
            if (state.stage != Stage.Idea)
                throw new InvalidOperationException("Board is already committed (stage " + state.stage + ")");

            state.goals = Goals(state.board, db);
            foreach (var combo in LinkedCombos(state.board, db))
            {
                if (combo.kind == ThreadKind.Match) Add(state.axes, combo.deltas);
                else if (combo.kind == ThreadKind.Conflict) state.axes[(int)Axis.Coherence] -= ConflictCoherencePenalty;
            }
            Clamp(state.axes);
            state.stage = Stage.Production;
        }

        static IEnumerable<ComboSO> LinkedCombos(BoardState board, ContentDB db)
        {
            foreach (var link in board.links)
            {
                if (!OnBoard(board, link.a) || !OnBoard(board, link.b)) continue;
                var combo = FindCombo(db, link.a, link.b);
                if (combo != null) yield return combo;
            }
        }

        static ComboSO FindCombo(ContentDB db, string a, string b)
        {
            foreach (var c in db.combos)
                if (c != null && c.a != null && c.b != null && SamePair(c.a.id, c.b.id, a, b)) return c;
            return null;
        }

        static CardSO FindCard(ContentDB db, string id)
        {
            foreach (var c in db.cards)
                if (c != null && c.id == id) return c;
            return null;
        }

        static bool OnBoard(BoardState board, string cardId) => board.placed.Exists(e => e.cardId == cardId);

        static bool SamePair(string a1, string b1, string a2, string b2)
            => (a1 == a2 && b1 == b2) || (a1 == b2 && b1 == a2);

        static void DropLinks(BoardState board, string cardId)
            => board.links.RemoveAll(l => l.a == cardId || l.b == cardId);

        static void Add(int[] target, int[] deltas)
        {
            for (int i = 0; i < 4; i++) target[i] += deltas[i];
        }

        static void Clamp(int[] values)
        {
            for (int i = 0; i < 4; i++) values[i] = Math.Max(0, Math.Min(100, values[i]));
        }
    }
}
