using System;
using System.Collections.Generic;
using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class ReviewSystemTests
    {
        ContentDB db;
        GameState state;

        [SetUp]
        public void SetUp()
        {
            db = ScriptableObject.CreateInstance<ContentDB>();
            db.startBudget = 100;
            db.deadline = 20;
            db.quotes = new[]
            {
                Quote("int1", Axis.Interest),
                Quote("int2", Axis.Interest),
                Quote("coh1", Axis.Coherence),
                Quote("cla1", Axis.Clarity),
                Quote("tech1", Axis.Tech),
                Quote("praiseInt", Axis.Interest, praise: true),
                Quote("praiseCoh", Axis.Coherence, praise: true),
            };
            state = GameState.New(db, "tester", DifficultyLevel.Designer);
            state.stage = Stage.Release;
            state.day = 13;
            state.goals = new[] { 50, 50, 50, 50 };
        }

        static QuoteSO Quote(string id, Axis axis, bool praise = false)
        {
            var q = ScriptableObject.CreateInstance<QuoteSO>();
            q.id = id;
            q.axis = axis;
            q.praise = praise;
            return q;
        }

        static OptionSO Option(string id, Cost cost, int[] deltas)
        {
            var o = ScriptableObject.CreateInstance<OptionSO>();
            o.id = id;
            o.cost = cost;
            o.deltas = deltas;
            return o;
        }

        static CardSO Card(string id, Slot slot)
        {
            var c = ScriptableObject.CreateInstance<CardSO>();
            c.id = id;
            c.slot = slot;
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

        DecisionRecord Log(string messageId, Stage stage, int[] deltas)
        {
            var r = new DecisionRecord { messageId = messageId, stage = stage, day = state.log.Count + 4, deltas = deltas };
            state.log.Add(r);
            return r;
        }

        static VersionRecord Version(params string[] quoteIds)
            => new VersionRecord { number = 1, quoteIds = new List<string>(quoteIds) };

        // Roguelike + time loop linked by a thread: Interest +10, Coherence +5 on Commit.
        void CommitMatchedBoard()
        {
            var rogue = Card("roguelike", Slot.Genre);
            var loop = Card("loop", Slot.Hook);
            db.cards = new[] { rogue, loop };
            db.combos = new[] { Combo(rogue, loop, ThreadKind.Match, new[] { 10, 5, 0, 0 }) };
            state.stage = Stage.Idea;
            BoardRules.Place(state, rogue);
            BoardRules.Place(state, loop);
            BoardRules.Connect(state, "roguelike", "loop");
            BoardRules.Commit(state, db);
            state.stage = Stage.Release;
        }

        // ---- Review: approval ----

        [Test]
        public void Review_ApprovesWhenEveryAxisIsWithinTolerance()
        {
            state.difficulty = DifficultyLevel.Intern;
            state.axes = new[] { 45, 50, 70, 46 };
            Assert.IsTrue(ReviewSystem.Review(state, db, tolerance: 5).approved);
        }

        [Test]
        public void Review_RejectsAxisBelowTolerance()
        {
            state.difficulty = DifficultyLevel.Intern;
            state.axes = new[] { 44, 50, 70, 50 };
            Assert.IsFalse(ReviewSystem.Review(state, db, tolerance: 5).approved);
        }

        [TestCase(DifficultyLevel.Designer)]
        [TestCase(DifficultyLevel.Lead)]
        public void Review_FirstVersionIgnoresToleranceAboveIntern(DifficultyLevel level)
        {
            state.difficulty = level;
            state.axes = new[] { 48, 50, 50, 50 };
            Assert.IsFalse(ReviewSystem.Review(state, db, tolerance: 5).approved, "v0.1");
            Assert.IsTrue(ReviewSystem.Review(state, db, tolerance: 5).approved, "v0.2");
        }

        [Test]
        public void Review_FirstVersionOnDesignerPassesWhenAllGoalsMet()
        {
            state.axes = new[] { 50, 60, 50, 55 };
            Assert.IsTrue(ReviewSystem.Review(state, db, tolerance: 5).approved);
        }

        [Test]
        public void Review_NumbersVersionsSnapshotsAxesAndCostsADay()
        {
            state.axes = new[] { 40, 41, 42, 43 };
            var v1 = ReviewSystem.Review(state, db);
            state.axes[0] = 99;
            var v2 = ReviewSystem.Review(state, db);

            Assert.AreEqual(1, v1.number);
            Assert.AreEqual(2, v2.number);
            Assert.AreEqual(13, v1.day);
            Assert.AreEqual(14, v2.day);
            Assert.AreEqual(15, state.day);
            CollectionAssert.AreEqual(new[] { 40, 41, 42, 43 }, v1.axes);
            CollectionAssert.AreEqual(new[] { v1, v2 }, state.versions);
        }

        [Test]
        public void Review_OutsideReleaseThrows()
        {
            state.stage = Stage.Production;
            Assert.Throws<InvalidOperationException>(() => ReviewSystem.Review(state, db));
        }

        // ---- Review: quotes ----

        [Test]
        public void Review_QuotesOnlyWeakAxesPlusOnePraise()
        {
            state.axes = new[] { 40, 70, 49, 55 };   // Interest and Clarity below, Coherence best above
            var v = ReviewSystem.Review(state, db);
            CollectionAssert.AreEqual(new[] { "int1", "cla1", "praiseCoh" }, v.quoteIds);
        }

        [Test]
        public void Review_BigGapGetsTwoQuotes()
        {
            state.axes = new[] { 20, 50, 50, 50 };
            CollectionAssert.AreEqual(new[] { "int1", "int2", "praiseCoh" }, ReviewSystem.Review(state, db).quoteIds);
        }

        [Test]
        public void Review_PraiseFallsBackToNextStrongAxisWithAQuote()
        {
            state.axes = new[] { 55, 50, 50, 90 };   // Tech is best but has no praise quote
            CollectionAssert.AreEqual(new[] { "praiseInt" }, ReviewSystem.Review(state, db).quoteIds);
        }

        [Test]
        public void Review_DoesNotRepeatQuotesWhileNewOnesAreLeft()
        {
            state.axes = new[] { 40, 10, 10, 10 };
            state.goals = new[] { 50, 0, 0, 0 };
            Assert.AreEqual("int1", ReviewSystem.Review(state, db).quoteIds[0]);
            Assert.AreEqual("int2", ReviewSystem.Review(state, db).quoteIds[0]);
            Assert.AreEqual("int1", ReviewSystem.Review(state, db).quoteIds[0], "repeats once new ones ran out");
        }

        // ---- Blame ----

        [Test]
        public void Blame_PicksDecisionWithWorstDeltaOnQuoteAxis()
        {
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            Log("b", Stage.Production, new[] { -10, 0, 0, 0 });
            Log("c", Stage.Production, new[] { 0, -20, 0, 0 });
            Assert.AreEqual("b", ReviewSystem.Blame(state, Version("int1"), "int1", db));
        }

        [Test]
        public void Blame_PrefersLaterDecisionOnTie()
        {
            Log("a", Stage.Production, new[] { -10, 0, 0, 0 });
            Log("b", Stage.Production, new[] { -10, 0, 0, 0 });
            Assert.AreEqual("b", ReviewSystem.Blame(state, Version("int1"), "int1", db));
        }

        [Test]
        public void Blame_SkipsReplacedDecisions()
        {
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            Log("b", Stage.Production, new[] { -10, 0, 0, 0 }).replaced = true;
            Assert.AreEqual("a", ReviewSystem.Blame(state, Version("int1"), "int1", db));
        }

        [Test]
        public void Blame_IdeaStageDecisionIsTheBoard()
        {
            Log("brief", Stage.Idea, new[] { -10, 0, 0, 0 });
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            Assert.AreEqual(ReviewSystem.BoardId, ReviewSystem.Blame(state, Version("int1"), "int1", db));
        }

        [Test]
        public void Blame_ConflictThreadOnTheBoardIsTheBoard()
        {
            var horror = Card("horror", Slot.Genre);
            var kids = Card("kids", Slot.Audience);
            db.cards = new[] { horror, kids };
            db.combos = new[] { Combo(horror, kids, ThreadKind.Conflict, new int[4]) };
            state.stage = Stage.Idea;
            BoardRules.Place(state, horror);
            BoardRules.Place(state, kids);
            BoardRules.Connect(state, "horror", "kids");
            BoardRules.Commit(state, db);
            state.stage = Stage.Release;
            Log("a", Stage.Production, new[] { 0, -5, 0, 0 });

            Assert.AreEqual(ReviewSystem.BoardId, ReviewSystem.Blame(state, Version("coh1"), "coh1", db));
        }

        [Test]
        public void Blame_PraiseOrForeignQuoteHasNoCulprit()
        {
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            Assert.IsNull(ReviewSystem.Blame(state, Version("praiseInt"), "praiseInt", db));
            Assert.IsNull(ReviewSystem.Blame(state, Version("int1"), "int2", db));
        }

        // ---- CheckBlame ----

        [Test]
        public void CheckBlame_RightGuessIsRecordedForFree()
        {
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            var v = Version("int1");
            Assert.IsTrue(ReviewSystem.CheckBlame(state, v, "int1", "a", db));
            Assert.AreEqual(13, state.day);
            Assert.AreEqual(1, v.blamed.Count);
            Assert.AreEqual("int1", v.blamed[0].quoteId);
            Assert.AreEqual("a", v.blamed[0].messageId);
        }

        [Test]
        public void CheckBlame_WrongGuessCostsADay()
        {
            Log("a", Stage.Production, new[] { -5, 0, 0, 0 });
            Log("b", Stage.Production, new[] { 5, 0, 0, 0 });
            var v = Version("int1");
            Assert.IsFalse(ReviewSystem.CheckBlame(state, v, "int1", "b", db));
            Assert.AreEqual(14, state.day);
            Assert.IsEmpty(v.blamed);
        }

        // ---- ReopenCost ----

        [Test]
        public void ReopenCost_ProductionIsOriginalPrice()
        {
            var r = new DecisionRecord { stage = Stage.Production, cost = new Cost { days = 2, budget = 10 } };
            var c = ReviewSystem.ReopenCost(r);
            Assert.AreEqual(2, c.days);
            Assert.AreEqual(10, c.budget);
        }

        [Test]
        public void ReopenCost_IdeaIsDoublePrice()
        {
            var r = new DecisionRecord { stage = Stage.Idea, cost = new Cost { days = 1, budget = 10 } };
            var c = ReviewSystem.ReopenCost(r);
            Assert.AreEqual(2, c.days);
            Assert.AreEqual(20, c.budget);
        }

        [Test]
        public void ReopenCost_IsAtLeastOneDay()
        {
            Assert.AreEqual(1, ReviewSystem.ReopenCost(new DecisionRecord { stage = Stage.Production }).days);
        }

        [Test]
        public void ReopenCost_BoardIsTwoDays()
        {
            CommitMatchedBoard();
            var c = ReviewSystem.ReopenCost(ReviewSystem.BoardRecord(state, db));
            Assert.AreEqual(2, c.days);
            Assert.AreEqual(0, c.budget);
        }

        // ---- Reopen ----

        MessageSO ArtMessage(out OptionSO bad, out OptionSO good)
        {
            bad = Option("bad", new Cost { days = 2, budget = 10 }, new[] { 0, -10, 0, 0 });
            good = Option("good", new Cost { days = 1, budget = 0 }, new[] { 0, 5, 0, 0 });
            var m = ScriptableObject.CreateInstance<MessageSO>();
            m.id = "art";
            m.stage = Stage.Production;
            m.options = new[] { bad, good };
            db.messages = new[] { m };
            return m;
        }

        [Test]
        public void Reopen_RevertsOldDecisionAndAppliesNewOption()
        {
            var m = ArtMessage(out var bad, out var good);
            state.stage = Stage.Production;
            state.day = 5;
            var old = DecisionResolver.Apply(state, m, bad, db);   // Coherence 30 -> 20, day 7, budget 90
            state.stage = Stage.Release;
            state.day = 13;

            ReviewSystem.Reopen(state, old, good, db);

            Assert.IsTrue(old.replaced);
            Assert.AreEqual(35, state.axes[(int)Axis.Coherence]);   // back to 30, then +5
            Assert.AreEqual(13 + 2 + 1, state.day);                  // reopen price 2 days, new option 1 day
            Assert.AreEqual(80, state.budget);                       // reopen price 10
            var fresh = state.log[state.log.Count - 1];
            Assert.AreEqual("good", fresh.optionId);
            Assert.AreEqual(Stage.Production, fresh.stage);
            Assert.IsFalse(fresh.replaced);
            Assert.AreEqual(Stage.Release, state.stage);
        }

        [Test]
        public void Reopen_UnaffordableThrowsAndChangesNothing()
        {
            var m = ArtMessage(out var bad, out var good);
            state.stage = Stage.Production;
            var old = DecisionResolver.Apply(state, m, bad, db);
            state.stage = Stage.Release;
            state.budget = 5;
            var axes = (int[])state.axes.Clone();
            int day = state.day;

            Assert.Throws<InvalidOperationException>(() => ReviewSystem.Reopen(state, old, good, db));
            Assert.IsFalse(old.replaced);
            CollectionAssert.AreEqual(axes, state.axes);
            Assert.AreEqual(day, state.day);
            Assert.AreEqual(5, state.budget);
        }

        [Test]
        public void Reopen_BoardGoesBackToIdeaAndRecommitReturnsToRelease()
        {
            CommitMatchedBoard();
            CollectionAssert.AreEqual(new[] { 40, 35, 30, 30 }, state.axes);

            ReviewSystem.Reopen(state, ReviewSystem.BoardRecord(state, db), null, db);
            Assert.AreEqual(Stage.Idea, state.stage);
            CollectionAssert.AreEqual(new[] { 30, 30, 30, 30 }, state.axes);
            Assert.AreEqual(15, state.day);

            ReviewSystem.Recommit(state, db);
            Assert.AreEqual(Stage.Release, state.stage);
            CollectionAssert.AreEqual(new[] { 40, 35, 30, 30 }, state.axes, "combo applied once, not twice");
        }
    }
}
