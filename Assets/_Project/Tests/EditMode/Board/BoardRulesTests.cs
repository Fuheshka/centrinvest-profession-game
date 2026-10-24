using System;
using System.Collections.Generic;
using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class BoardRulesTests
    {
        ContentDB db;
        GameState state;
        CardSO horror, kids, roguelike, loop;

        [SetUp]
        public void SetUp()
        {
            horror = Card("horror", Slot.Genre);
            roguelike = Card("roguelike", Slot.Genre, goals: new[] { 5, 0, 0, 0 });
            kids = Card("kids", Slot.Audience, goals: new[] { 0, 0, 10, 0 });
            loop = Card("loop", Slot.Hook, goals: new[] { 0, 0, 0, -5 });

            db = ScriptableObject.CreateInstance<ContentDB>();
            db.startBudget = 100;
            db.deadline = 10;
            db.cards = new[] { horror, roguelike, kids, loop };
            db.combos = new[]
            {
                Combo(roguelike, loop, ThreadKind.Match, new[] { 10, 5, 0, 0 }),
                Combo(horror, kids, ThreadKind.Conflict, new[] { 0, 0, 0, 0 }),
            };
            state = GameState.New(db, "tester", DifficultyLevel.Designer);
        }

        static CardSO Card(string id, Slot slot, int[] goals = null)
        {
            var c = ScriptableObject.CreateInstance<CardSO>();
            c.id = id;
            c.slot = slot;
            c.goalDeltas = goals ?? new int[4];
            return c;
        }

        static ComboSO Combo(CardSO a, CardSO b, ThreadKind kind, int[] deltas)
        {
            var c = ScriptableObject.CreateInstance<ComboSO>();
            c.a = a;
            c.b = b;
            c.kind = kind;
            c.deltas = deltas;
            return c;
        }

        // ---- Place / Remove ----

        [Test]
        public void Place_PutsCardInItsSlot()
        {
            BoardRules.Place(state, kids);
            Assert.AreEqual(1, state.board.placed.Count);
            Assert.AreEqual(Slot.Audience, state.board.placed[0].slot);
            Assert.AreEqual("kids", state.board.placed[0].cardId);
        }

        [Test]
        public void Place_ReplacesCardInSlotAndDropsItsLinks()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Place(state, loop);
            BoardRules.Connect(state, "horror", "kids");
            BoardRules.Connect(state, "kids", "loop");

            BoardRules.Place(state, roguelike); // same slot as horror

            Assert.AreEqual(3, state.board.placed.Count);
            Assert.IsFalse(state.board.placed.Exists(e => e.cardId == "horror"));
            Assert.IsTrue(state.board.placed.Exists(e => e.cardId == "roguelike"));
            Assert.AreEqual(1, state.board.links.Count);
            Assert.AreEqual("kids", state.board.links[0].a);
            Assert.AreEqual("loop", state.board.links[0].b);
        }

        [Test]
        public void Place_SameCardAgainKeepsLinks()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            BoardRules.Place(state, horror);
            Assert.AreEqual(2, state.board.placed.Count);
            Assert.AreEqual(1, state.board.links.Count);
        }

        [Test]
        public void Remove_ClearsSlotAndLinks()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");

            BoardRules.Remove(state, Slot.Genre);

            Assert.AreEqual(1, state.board.placed.Count);
            Assert.AreEqual(0, state.board.links.Count);
        }

        [Test]
        public void Remove_EmptySlotIsNoOp()
        {
            BoardRules.Remove(state, Slot.Genre);
            Assert.AreEqual(0, state.board.placed.Count);
        }

        // ---- Connect / Disconnect ----

        [Test]
        public void Connect_AddsLinkBetweenTwoPlacedCards()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            Assert.IsTrue(BoardRules.Connect(state, "horror", "kids"));
            Assert.AreEqual(1, state.board.links.Count);
        }

        [Test]
        public void Connect_RejectsDuplicateInEitherOrder()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            Assert.IsTrue(BoardRules.Connect(state, "horror", "kids"));
            Assert.IsFalse(BoardRules.Connect(state, "horror", "kids"));
            Assert.IsFalse(BoardRules.Connect(state, "kids", "horror"));
            Assert.AreEqual(1, state.board.links.Count);
        }

        [Test]
        public void Connect_RejectsSelfLink()
        {
            BoardRules.Place(state, horror);
            Assert.IsFalse(BoardRules.Connect(state, "horror", "horror"));
            Assert.AreEqual(0, state.board.links.Count);
        }

        [Test]
        public void Connect_RejectsCardNotOnBoard()
        {
            BoardRules.Place(state, horror);
            Assert.IsFalse(BoardRules.Connect(state, "horror", "kids"));
            Assert.IsFalse(BoardRules.Connect(state, "nope", "horror"));
            Assert.AreEqual(0, state.board.links.Count);
        }

        [Test]
        public void Disconnect_RemovesLinkInEitherOrder()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            Assert.IsTrue(BoardRules.Disconnect(state, "kids", "horror"));
            Assert.AreEqual(0, state.board.links.Count);
            Assert.IsFalse(BoardRules.Disconnect(state, "kids", "horror"));
        }

        // ---- Thread ----

        [Test]
        public void Thread_MatchWhenBothRevealed_BothOrders()
        {
            state.board.revealed.AddRange(new[] { "roguelike", "loop" });
            Assert.AreEqual(ThreadKind.Match, BoardRules.Thread(roguelike, loop, db, state.board, false));
            Assert.AreEqual(ThreadKind.Match, BoardRules.Thread(loop, roguelike, db, state.board, false));
        }

        [Test]
        public void Thread_ConflictWhenBothRevealed_BothOrders()
        {
            state.board.revealed.AddRange(new[] { "horror", "kids" });
            Assert.AreEqual(ThreadKind.Conflict, BoardRules.Thread(horror, kids, db, state.board, false));
            Assert.AreEqual(ThreadKind.Conflict, BoardRules.Thread(kids, horror, db, state.board, false));
        }

        [Test]
        public void Thread_UnknownWhenComboExistsButNotRevealed()
        {
            Assert.AreEqual(ThreadKind.Unknown, BoardRules.Thread(horror, kids, db, state.board, false));
            state.board.revealed.Add("horror"); // one of two is not enough
            Assert.AreEqual(ThreadKind.Unknown, BoardRules.Thread(kids, horror, db, state.board, false));
        }

        [Test]
        public void Thread_ShowThreadsRevealsComboWithoutResearch()
        {
            Assert.AreEqual(ThreadKind.Match, BoardRules.Thread(loop, roguelike, db, state.board, true));
            Assert.AreEqual(ThreadKind.Conflict, BoardRules.Thread(horror, kids, db, state.board, true));
        }

        [Test]
        public void Thread_NeutralWhenNoCombo_EvenIfNotRevealed()
        {
            Assert.AreEqual(ThreadKind.Neutral, BoardRules.Thread(horror, loop, db, state.board, false));
            Assert.AreEqual(ThreadKind.Neutral, BoardRules.Thread(horror, loop, db, state.board, true));
        }

        // ---- Reveal / Research ----

        [Test]
        public void Reveal_AddsOnceToRevealed()
        {
            BoardRules.Reveal(state, "horror");
            BoardRules.Reveal(state, "horror");
            CollectionAssert.AreEqual(new[] { "horror" }, state.board.revealed);
        }

        [Test]
        public void Research_ChargesBudgetAndReveals()
        {
            BoardRules.Research(state, "horror", new Cost { budget = 30 });
            Assert.AreEqual(70, state.budget);
            CollectionAssert.Contains(state.board.revealed, "horror");
            Assert.AreEqual(1, state.day, "research costs budget only");
        }

        [Test]
        public void Research_ThrowsWithoutBudget()
        {
            Assert.Throws<InvalidOperationException>(
                () => BoardRules.Research(state, "horror", new Cost { budget = 101 }));
            Assert.AreEqual(100, state.budget);
            CollectionAssert.DoesNotContain(state.board.revealed, "horror");
        }

        [Test]
        public void Research_AlreadyRevealedIsFree()
        {
            BoardRules.Reveal(state, "horror");
            BoardRules.Research(state, "horror", new Cost { budget = 30 });
            Assert.AreEqual(100, state.budget);
        }

        // ---- Goals ----

        [Test]
        public void Goals_EmptyBoardIsFifty()
        {
            CollectionAssert.AreEqual(new[] { 50, 50, 50, 50 }, BoardRules.Goals(state.board, db));
        }

        [Test]
        public void Goals_AddsCardDeltas()
        {
            BoardRules.Place(state, roguelike);
            BoardRules.Place(state, kids);
            CollectionAssert.AreEqual(new[] { 55, 50, 60, 50 }, BoardRules.Goals(state.board, db));
        }

        [Test]
        public void Goals_AddsLinkedMatchDeltasOnly()
        {
            BoardRules.Place(state, roguelike);
            BoardRules.Place(state, loop);
            // cards only: 50+5, 50, 50, 50-5
            CollectionAssert.AreEqual(new[] { 55, 50, 50, 45 }, BoardRules.Goals(state.board, db));

            BoardRules.Connect(state, "loop", "roguelike");
            CollectionAssert.AreEqual(new[] { 65, 55, 50, 45 }, BoardRules.Goals(state.board, db));
        }

        [Test]
        public void Goals_ConflictLinkDoesNotChangeGoals()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            CollectionAssert.AreEqual(new[] { 50, 50, 60, 50 }, BoardRules.Goals(state.board, db));
        }

        [Test]
        public void Goals_ClampedTo0And100()
        {
            var big = Card("big", Slot.Mechanic, new[] { 80, -80, 0, 0 });
            db.cards = new List<CardSO>(db.cards) { big }.ToArray();
            BoardRules.Place(state, big);
            CollectionAssert.AreEqual(new[] { 100, 0, 50, 50 }, BoardRules.Goals(state.board, db));
        }

        // ---- Commit ----

        [Test]
        public void Commit_WritesGoalsAndMovesToProduction()
        {
            BoardRules.Place(state, roguelike);
            BoardRules.Place(state, kids);
            BoardRules.Commit(state, db);
            Assert.AreEqual(Stage.Production, state.stage);
            CollectionAssert.AreEqual(new[] { 55, 50, 60, 50 }, state.goals);
        }

        [Test]
        public void Commit_MatchAddsDeltasToAxes()
        {
            BoardRules.Place(state, roguelike);
            BoardRules.Place(state, loop);
            BoardRules.Connect(state, "roguelike", "loop");
            BoardRules.Commit(state, db);
            CollectionAssert.AreEqual(new[] { 40, 35, 30, 30 }, state.axes);
        }

        [Test]
        public void Commit_ConflictLowersCoherenceBy10()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            BoardRules.Commit(state, db);
            CollectionAssert.AreEqual(new[] { 30, 20, 30, 30 }, state.axes);
        }

        [Test]
        public void Commit_UnlinkedComboHasNoEffect()
        {
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Commit(state, db);
            CollectionAssert.AreEqual(new[] { 30, 30, 30, 30 }, state.axes);
        }

        [Test]
        public void Commit_ClampsAxes()
        {
            state.axes[(int)Axis.Coherence] = 5;
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            BoardRules.Commit(state, db);
            Assert.AreEqual(0, state.axes[(int)Axis.Coherence]);
        }

        [Test]
        public void Commit_ThrowsOutsideIdeaStage()
        {
            BoardRules.Commit(state, db);
            Assert.Throws<InvalidOperationException>(() => BoardRules.Commit(state, db));
        }
    }
}
