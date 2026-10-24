using System;
using System.Collections.Generic;
using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class DecisionResolverTests
    {
        ContentDB db;
        MessageSO message;
        GameState state;

        [SetUp]
        public void SetUp()
        {
            message = Msg("m1");
            db = ScriptableObject.CreateInstance<ContentDB>();
            db.brief = message;
            db.startBudget = 100;
            db.deadline = 10;
            state = GameState.New(db, "tester", DifficultyLevel.Designer);
        }

        static MessageSO Msg(string id)
        {
            var m = ScriptableObject.CreateInstance<MessageSO>();
            m.id = id;
            return m;
        }

        static OptionSO Opt(string id, int days = 0, int budget = 0, int[] deltas = null, params string[] tags)
        {
            var o = ScriptableObject.CreateInstance<OptionSO>();
            o.id = id;
            o.cost = new Cost { days = days, budget = budget };
            o.deltas = deltas ?? new int[4];
            o.tags = tags;
            return o;
        }

        void Place(Slot slot, string cardId, params string[] tags)
        {
            var c = ScriptableObject.CreateInstance<CardSO>();
            c.id = cardId;
            c.slot = slot;
            c.tags = tags;
            db.cards = new List<CardSO>(db.cards) { c }.ToArray();
            state.board.placed.Add(new SlotEntry { slot = slot, cardId = cardId });
        }

        [Test]
        public void New_SetsStartingState()
        {
            Assert.AreEqual("tester", state.nickname);
            Assert.AreEqual(DifficultyLevel.Designer, state.difficulty);
            Assert.AreEqual(1, state.day);
            Assert.AreEqual(10, state.deadline);
            Assert.AreEqual(100, state.budget);
            Assert.AreEqual(Stage.Idea, state.stage);
            CollectionAssert.AreEqual(new[] { 30, 30, 30, 30 }, state.axes);
            CollectionAssert.AreEqual(new[] { 50, 50, 50, 50 }, state.goals);
            CollectionAssert.AreEqual(new[] { "m1" }, state.inbox);
        }

        [Test]
        public void Apply_ChargesCostAndMovesDay()
        {
            DecisionResolver.Apply(state, message, Opt("o", days: 2, budget: 30), db);
            Assert.AreEqual(70, state.budget);
            Assert.AreEqual(3, state.day);
        }

        [Test]
        public void Apply_AddsDeltasToAxes()
        {
            DecisionResolver.Apply(state, message, Opt("o", deltas: new[] { 5, 0, -3, 10 }), db);
            CollectionAssert.AreEqual(new[] { 35, 30, 27, 40 }, state.axes);
        }

        [Test]
        public void CanAfford_FalseWithoutBudget()
        {
            Assert.IsFalse(DecisionResolver.CanAfford(state, Opt("o", budget: 101)));
            Assert.Throws<InvalidOperationException>(() => DecisionResolver.Apply(state, message, Opt("o", budget: 101), db));
            Assert.AreEqual(100, state.budget);
        }

        [Test]
        public void CanAfford_FalsePastDeadline_TrueOnDeadline()
        {
            Assert.IsTrue(DecisionResolver.CanAfford(state, Opt("o", days: 9)));   // 1 + 9 == 10
            Assert.IsFalse(DecisionResolver.CanAfford(state, Opt("o", days: 10)));
            Assert.Throws<InvalidOperationException>(() => DecisionResolver.Apply(state, message, Opt("o", days: 10), db));
        }

        [Test]
        public void Apply_MatchingTagAddsFiveCoherencePerOptionTag()
        {
            Place(Slot.Audience, "kids", "kids");
            Place(Slot.Genre, "plat", "platformer");
            var rec = DecisionResolver.Apply(state, message, Opt("o", tags: new[] { "kids", "platformer", "other" }), db);
            Assert.AreEqual(40, state.axes[(int)Axis.Coherence]);
            Assert.AreEqual(10, rec.deltas[(int)Axis.Coherence]);
        }

        [Test]
        public void Apply_ConflictSubtractsEightInBothPairOrders()
        {
            db.tagConflicts = new[] { new TagPair { a = "horror", b = "kids" } };
            Place(Slot.Audience, "kids", "kids");
            DecisionResolver.Apply(state, message, Opt("o1", tags: new[] { "horror" }), db);
            Assert.AreEqual(22, state.axes[(int)Axis.Coherence]);

            var state2 = GameState.New(db, "t", DifficultyLevel.Designer);
            Place2(state2, "horror", "horror");
            DecisionResolver.Apply(state2, message, Opt("o2", tags: new[] { "kids" }), db);
            Assert.AreEqual(22, state2.axes[(int)Axis.Coherence]);
        }

        void Place2(GameState s, string cardId, params string[] tags)
        {
            var c = ScriptableObject.CreateInstance<CardSO>();
            c.id = cardId;
            c.tags = tags;
            db.cards = new List<CardSO>(db.cards) { c }.ToArray();
            s.board.placed.Add(new SlotEntry { slot = Slot.Genre, cardId = cardId });
        }

        [Test]
        public void Apply_ClampsAxesTo0And100()
        {
            state.axes = new[] { 95, 3, 50, 50 };
            DecisionResolver.Apply(state, message, Opt("o", deltas: new[] { 20, -10, 0, 0 }), db);
            Assert.AreEqual(100, state.axes[0]);
            Assert.AreEqual(0, state.axes[1]);
        }

        [Test]
        public void Apply_WritesLogAndMarksAnswered()
        {
            var rec = DecisionResolver.Apply(state, message, Opt("o", days: 1, deltas: new[] { 1, 2, 3, 4 }), db);
            Assert.AreSame(rec, state.log[0]);
            Assert.AreEqual("m1", rec.messageId);
            Assert.AreEqual("o", rec.optionId);
            Assert.AreEqual(Stage.Idea, rec.stage);
            Assert.AreEqual(1, rec.day);   // day the decision was made
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, rec.deltas);
            CollectionAssert.Contains(state.answered, "m1");
            CollectionAssert.DoesNotContain(state.inbox, "m1");
        }

        [Test]
        public void GameState_SurvivesJsonRoundTripAfterDecision()
        {
            Place(Slot.Genre, "g", "x");
            DecisionResolver.Apply(state, message, Opt("o", days: 1, budget: 5, deltas: new[] { 1, 2, 3, 4 }, "x"), db);
            var back = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(state));
            Assert.AreEqual(state.day, back.day);
            Assert.AreEqual(state.budget, back.budget);
            CollectionAssert.AreEqual(state.axes, back.axes);
            Assert.AreEqual(1, back.log.Count);
            Assert.AreEqual("o", back.log[0].optionId);
            CollectionAssert.AreEqual(state.answered, back.answered);
            Assert.AreEqual("g", back.board.placed[0].cardId);
        }
    }
}
