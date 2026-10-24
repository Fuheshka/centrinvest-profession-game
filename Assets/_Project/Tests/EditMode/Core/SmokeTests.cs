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
    }
}
