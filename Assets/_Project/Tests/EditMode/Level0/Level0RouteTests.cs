using NUnit.Framework;
using Studio.Core;
using UnityEditor;
using UnityEngine;

namespace Studio.Tests
{
    // The three paper routes of docs/MVP_RULES.md section 5, through the real systems and Level0 data.
    public class Level0RouteTests
    {
        LevelSO L;

        [SetUp]
        public void SetUp() => L = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");

        LevelState Concept(params (BlockType t, string p)[] blocks)
        {
            var s = LevelState.New(L, "Тест");
            string prev = null;
            foreach (var b in blocks)
            {
                var id = ConceptRules.Place(s, L, b.t, 0, 0);
                Assert.IsTrue(ConceptRules.SetParam(s, L, id, b.p));
                if (prev != null) Assert.IsTrue(ConceptRules.Connect(s, prev, id));
                prev = id;
            }
            Assert.IsTrue(ConceptRules.Approve(s, L));
            return s;
        }

        static void Run(LevelState s, LevelSO l, float seconds)
        {
            for (float t = 0; t < seconds - 1e-4f && s.stage == LevelStage.Production; t += 0.25f)
                if (WorkSim.Tick(s, l, 0.25f) != null) return;
        }

        void Team(LevelState s, string design, string mech, string sound, params string[] mechOrder)
        {
            Run(s, L, 20);   // the player hires and assigns during the first 20 seconds
            Assert.IsTrue(Staffing.Hire(s, L, design));
            Assert.IsTrue(Staffing.Hire(s, L, mech));
            Assert.IsTrue(Staffing.Hire(s, L, sound));
            Assert.IsTrue(WorkSim.Assign(s, "art", design));
            Assert.IsTrue(WorkSim.Assign(s, "sound", sound));
            for (int i = 0; i < mechOrder.Length; i++)
            {
                Assert.IsTrue(WorkSim.Assign(s, mechOrder[i], mech));
                WorkSim.SetPriority(s, mechOrder[i], i);
            }
        }

        [Test]
        public void RouteA_Success88OnDay2()
        {
            var s = Concept((BlockType.Genre, "puzzle"), (BlockType.Audience, "teen"), (BlockType.Setting, "space"),
                            (BlockType.Style, "pixel"), (BlockType.Mechanics, "gravity"), (BlockType.Platform, "phone"));
            CollectionAssert.AreEqual(new[] { 3, 3, 1 }, ConceptRules.Totals(s, L));
            Team(s, "lera", "kostya", "anya", "mech", "levels");
            Run(s, L, 280);
            Assert.AreEqual(2, s.day);
            Run(s, L, 60);
            Assert.IsTrue(Review.CanBuild(s, L, out var why), why);
            var b = Review.Build(s, L);
            CollectionAssert.AreEqual(new[] { 97, 79, 89 }, b.scores);
            Assert.AreEqual(88, b.overall);
            Assert.AreEqual(LevelStage.Passed, s.stage);
            Assert.AreEqual(82000, s.budget);
        }

        [Test]
        public void RouteB_69ThenFixByLeraTo74()
        {
            var s = Concept((BlockType.Genre, "platformer"), (BlockType.Audience, "teen"), (BlockType.Setting, "underwater"),
                            (BlockType.Style, "realism"), (BlockType.Mechanics, "jumps"), (BlockType.Platform, "phone"));
            CollectionAssert.AreEqual(new[] { -1, 1, 0 }, ConceptRules.Totals(s, L));
            Team(s, "vika", "dima", "gosha", "mech", "levels");
            Run(s, L, 280);
            Run(s, L, 140);
            Assert.IsNotNull(WorkSim.EndDay(s, L, 2));   // the player ends day 2 when Dima is out of energy
            Assert.IsNull(WorkSim.EndDay(s, L, 2), "second end of the same day is a no-op");
            Run(s, L, 90);
            var b1 = Review.Build(s, L);
            Assert.AreEqual(69, b1.overall);
            Assert.AreEqual(0, Review.Weakest(b1));
            Assert.IsFalse(Review.CanBuild(s, L, out _), "nothing changed, no rebuild");
            Assert.IsTrue(Staffing.Hire(s, L, "lera"));
            Assert.IsTrue(Review.SendToFix(s, L, "art", "lera"));
            Assert.IsFalse(Review.SendToFix(s, L, "art", "lera"), "one fix per file");
            Run(s, L, 120);
            var b2 = Review.Build(s, L);
            Assert.AreEqual(74, b2.overall);
            Assert.AreEqual(LevelStage.Passed, s.stage);
            Assert.AreEqual(3, s.day);
            Assert.AreEqual(76000, s.budget);
        }

        [Test]
        public void RouteC_FailsAtDeadlineWithoutBuild()
        {
            var s = Concept((BlockType.Genre, "horror"), (BlockType.Audience, "kids"), (BlockType.Setting, "hospital"),
                            (BlockType.Style, "realism"), (BlockType.Mechanics, "craft"), (BlockType.Platform, "pc"));
            CollectionAssert.AreEqual(new[] { 0, -1, 1 }, ConceptRules.Totals(s, L));
            Team(s, "vika", "dima", "gosha", "mech", "levels");
            for (int i = 0; i < 4 && s.stage == LevelStage.Production; i++) Run(s, L, 300);
            Assert.AreEqual(LevelStage.Failed, s.stage);
            Assert.AreEqual(FailReason.Deadline, s.failReason);
            Assert.AreEqual(0, s.builds.Count);
            Assert.AreEqual(450, Mathf.RoundToInt(s.Task("mech").progress), 1);   // full overnight rest: Dima finishes the craft task, the levels task still misses the deadline
            Assert.AreEqual(81000, s.budget);
        }
    }
}
