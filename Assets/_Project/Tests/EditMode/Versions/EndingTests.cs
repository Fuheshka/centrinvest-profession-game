using NUnit.Framework;
using Studio.Core;
using Studio.UI;
using UnityEngine;

namespace Studio.Tests.Versions
{
    public class EndingTests
    {
        [TestCase(0, Verdict.Flop)]
        [TestCase(39, Verdict.Flop)]
        [TestCase(40, Verdict.Average)]
        [TestCase(59, Verdict.Average)]
        [TestCase(60, Verdict.Hit)]
        [TestCase(79, Verdict.Hit)]
        [TestCase(80, Verdict.GameOfYear)]
        [TestCase(100, Verdict.GameOfYear)]
        public void VerdictThresholds(int score, Verdict expected) => Assert.AreEqual(expected, Ending.VerdictFor(score));

        [Test]
        public void ScoreIsAxisAverage() => Assert.AreEqual(70, Ending.Score(new[] { 85, 55, 70, 70 }));

        static ContentDB Db()
        {
            var db = ScriptableObject.CreateInstance<ContentDB>();
            db.cards = new[] { Card("g", Slot.Genre, "Рогалик"), Card("h", Slot.Hook, "Котики") };
            return db;
        }

        static CardSO Card(string id, Slot slot, string title)
        {
            var c = ScriptableObject.CreateInstance<CardSO>();
            c.id = id; c.slot = slot; c.title = title;
            return c;
        }

        [Test]
        public void TitleIsGenrePlusHook()
        {
            var s = new GameState();
            s.board.placed.Add(new SlotEntry { slot = Slot.Genre, cardId = "g" });
            s.board.placed.Add(new SlotEntry { slot = Slot.Hook, cardId = "h" });
            Assert.AreEqual("Рогалик: Котики", Ending.GameTitle(s, Db()));
        }

        [Test]
        public void EmptyBoardHasFallbackTitle() => Assert.AreEqual("Без названия", Ending.GameTitle(new GameState(), Db()));

        [Test]
        public void BreakdownTakesTwoBestAndTwoWorst()
        {
            var s = new GameState();
            foreach (var d in new[] { 25, 10, 0, -5, -10 })
                s.log.Add(new DecisionRecord { messageId = "m" + d, optionId = "o", deltas = new[] { d, 0, 0, 0 } });
            s.log.Add(new DecisionRecord { messageId = "gone", deltas = new[] { 99, 0, 0, 0 }, replaced = true });
            var list = Ending.Breakdown(s, Db());
            Assert.AreEqual(4, list.Count);
            Assert.IsTrue(list[0].good && list[1].good);
            Assert.IsFalse(list[2].good && list[3].good);
        }
    }
}
