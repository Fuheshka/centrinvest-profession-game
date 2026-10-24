using System;
using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class SmokeTests
    {
        [Test]
        public void Axis_HasFourValues()
        {
            Assert.AreEqual(4, Enum.GetValues(typeof(Axis)).Length);
        }

        [Test]
        public void GameState_SurvivesJsonRoundTrip()
        {
            var back = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(new GameState()));
            Assert.AreEqual(4, back.axes.Length);
            Assert.AreEqual(4, back.goals.Length);
            Assert.IsNotNull(back.board);
            Assert.IsNotNull(back.log);
            Assert.IsNotNull(back.versions);
        }

        [Test]
        public void VersionRecord_SurvivesJsonRoundTrip()
        {
            var back = JsonUtility.FromJson<VersionRecord>(JsonUtility.ToJson(new VersionRecord()));
            Assert.AreEqual(4, back.axes.Length);
            Assert.IsNotNull(back.quoteIds);
            Assert.IsNotNull(back.blamed);
        }

        [Test]
        public void Systems_ThrowNotImplementedWithCardNumber()
        {
            Expect("P-3", () => BoardRules.Thread(null, null, null, null));
            Expect("P-3", () => BoardRules.Goals(null, null));
            Expect("P-6", () => DayClock.EndDay(null, null));
            Expect("P-7", () => ReviewSystem.Review(null, null));
            Expect("P-7", () => ReviewSystem.Blame(null, null, null));
            Expect("P-7", () => ReviewSystem.ReopenCost(null));
            Expect("P-7", () => ReviewSystem.Reopen(null, null, null, null));
            Expect("P-9", () => Difficulty.For(DifficultyLevel.Intern));
            Expect("P-9", () => SaveSystem.Save(null));
            Expect("P-9", () => SaveSystem.Load());
            Expect("P-9", () => SaveSystem.HasSave());
        }

        static void Expect(string card, TestDelegate call)
        {
            var ex = Assert.Throws<NotImplementedException>(call);
            Assert.AreEqual(card, ex.Message);
        }
    }
}
