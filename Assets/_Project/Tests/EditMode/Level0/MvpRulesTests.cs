using NUnit.Framework;
using Studio.Core;
using UnityEditor;
using UnityEngine;

namespace Studio.Tests
{
    public class MvpRulesTests
    {
        LevelSO L;
        LevelState S;

        [SetUp]
        public void SetUp()
        {
            L = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
            S = LevelState.New(L, "");
        }

        [Test]
        public void New_StartsWithBriefOnlyAndDefaults()
        {
            Assert.AreEqual("Стажёр", S.nickname);
            Assert.AreEqual(LevelStage.Concept, S.stage);
            Assert.AreEqual(100000, S.budget);
            Assert.AreEqual(3, S.deadline);
            Assert.AreEqual(1, S.chat.Count);
            StringAssert.Contains("Стажёр", S.chat[0].text);
            Assert.AreEqual(16, LevelState.New(L, new string('я', 30)).nickname.Length);
        }

        [Test]
        public void Concept_OneBlockPerType_LinksNoDuplicates()
        {
            var a = ConceptRules.Place(S, L, BlockType.Genre, 0, 0);
            Assert.IsNull(ConceptRules.Place(S, L, BlockType.Genre, 1, 1));
            Assert.IsFalse(ConceptRules.SetParam(S, L, a, "teen"));
            var p = ConceptRules.Place(S, L, BlockType.Platform, 0, 0);
            Assert.IsFalse(ConceptRules.Connect(S, a, p), "Genre and Platform are not linkable");
            var b = ConceptRules.Place(S, L, BlockType.Audience, 0, 0);
            Assert.IsTrue(ConceptRules.Connect(S, a, b));
            Assert.IsFalse(ConceptRules.Connect(S, b, a));
            Assert.IsFalse(ConceptRules.Connect(S, a, a));
            Assert.IsTrue(ConceptRules.Remove(S, b));
            Assert.AreEqual(0, S.concept.links.Count);
        }

        [Test]
        public void LinkColor_GreenOnlyWhenBuffsWin_TieIsRed()
        {
            var st = ConceptRules.Place(S, L, BlockType.Style, 0, 0);
            var m = ConceptRules.Place(S, L, BlockType.Mechanics, 0, 0);
            ConceptRules.SetParam(S, L, st, "realism"); ConceptRules.SetParam(S, L, m, "gravity");
            Assert.AreEqual(LinkColor.Green, ConceptRules.LinkColor(S, L, st, m));
            ConceptRules.SetParam(S, L, m, "craft");
            Assert.AreEqual(LinkColor.Red, ConceptRules.LinkColor(S, L, st, m), "no rule = 0:0 = red");
        }

        [Test]
        public void Totals_CountOnlyLinkedPairs_LinksCapped()
        {
            var g = ConceptRules.Place(S, L, BlockType.Genre, 0, 0);
            var a = ConceptRules.Place(S, L, BlockType.Audience, 0, 0);
            ConceptRules.SetParam(S, L, g, "puzzle"); ConceptRules.SetParam(S, L, a, "teen");
            CollectionAssert.AreEqual(new[] { 0, 0, 0 }, ConceptRules.Totals(S, L), "no link, no effect");
            Assert.IsTrue(ConceptRules.Connect(S, g, a));
            CollectionAssert.AreEqual(new[] { 1, 0, 0 }, ConceptRules.Totals(S, L));
            for (int i = 0; i < ConceptRules.MaxLinks; i++) S.concept.links.Add(new ConceptLink { a = "x" + i, b = "y" + i });
            var p = ConceptRules.Place(S, L, BlockType.Platform, 0, 0);
            Assert.IsFalse(ConceptRules.Connect(S, a, p), "over the link cap");
        }

        [Test]
        public void Approve_NeedsAllParamsAndOneNetwork()
        {
            string prev = null;
            foreach (BlockType t in System.Enum.GetValues(typeof(BlockType)))
            {
                var id = ConceptRules.Place(S, L, t, 0, 0);
                ConceptRules.SetParam(S, L, id, L.parameters.Find(x => x.type == t).id);
                if (prev != null && t != BlockType.Platform) ConceptRules.Connect(S, prev, id);
                prev = id;
            }
            Assert.IsFalse(ConceptRules.CanApprove(S, L, out var why));
            StringAssert.Contains("Платформа", why);
            ConceptRules.Connect(S, S.concept.blocks[1].id, prev);   // Audience-Platform chord
            Assert.IsTrue(ConceptRules.Approve(S, L));
            Assert.AreEqual(4, S.tasks.Count);
            Assert.IsFalse(ConceptRules.Approve(S, L));
            Assert.IsNull(ConceptRules.Place(S, L, BlockType.Genre, 0, 0));
        }

        LevelState Working()
        {
            var s = LevelState.New(L, "x");
            s.stage = LevelStage.Production; s.concept.approved = true;
            s.tasks.Add(new TaskState { id = "t", d = Discipline.Mechanics, title = "T", tier = 3, effort = 450 });
            Staffing.Hire(s, L, "dima");
            WorkSim.Assign(s, "t", "dima");
            return s;
        }

        [Test]
        public void Fire_FreesTasksKeepsProgress_RehirePaysAgain()
        {
            var s = Working();
            for (int i = 0; i < 40; i++) WorkSim.Tick(s, L, 0.25f);
            s.pendingEvents.Add(new PendingEvent { eventId = "bug", employeeId = "dima" });
            Assert.IsTrue(Staffing.Fire(s, "dima"));
            Assert.IsNull(s.Employee("dima"));
            Assert.IsNull(s.Task("t").assigneeId);
            Assert.AreEqual(10f, s.Task("t").progress, 0.05f);
            Assert.AreEqual(0, s.pendingEvents.Count);
            Assert.IsFalse(Staffing.Fire(s, "dima"));
            int before = s.budget;
            Staffing.Hire(s, L, "dima"); WorkSim.Assign(s, "t", "dima");
            WorkSim.Tick(s, L, 0.25f);
            Assert.AreEqual(before - 4000, s.budget, "paid again for the same day");
        }

        [Test]
        public void HigherSlot_SkipsTasksOfOtherPeople()
        {
            S.tasks.Add(new TaskState { id = "a", assigneeId = "lera" });
            S.tasks.Add(new TaskState { id = "b", assigneeId = "dima" });
            S.tasks.Add(new TaskState { id = "c", assigneeId = "dima" });
            Assert.AreEqual(-1, WorkSim.HigherSlot(S, "a"));
            Assert.AreEqual(-1, WorkSim.HigherSlot(S, "b"));
            Assert.AreEqual(1, WorkSim.HigherSlot(S, "c"));
            Assert.IsTrue(WorkSim.SetPriority(S, "c", WorkSim.HigherSlot(S, "c")));
            Assert.AreEqual("c", S.tasks[1].id);
        }

        [Test]
        public void Energy_DrainsWhileWorking_ThresholdBlocksStart()
        {
            var s = Working();
            for (int i = 0; i < 40; i++) WorkSim.Tick(s, L, 0.25f);   // 10 s
            Assert.AreEqual(96f, s.staff[0].energy, 0.05f);
            Assert.AreEqual(10f, s.Task("t").progress, 0.05f);
            Assert.AreEqual(96000, s.budget, "paid once for the day");
            s.staff[0].energy = 50; s.staff[0].currentTaskId = null;
            Assert.AreEqual(WorkStatus.Waiting, WorkSim.Status(s, L, "t", out var why));
            Assert.AreEqual(WaitReason.NoEnergy, why);
        }

        [Test]
        public void Tick_ClampsLongFrames_EndDayOncePerDay()
        {
            var s = Working();
            WorkSim.Tick(s, L, 5f);
            Assert.AreEqual(0.25f, s.dayElapsed, 1e-4f);
            Assert.IsNotNull(WorkSim.EndDay(s, L, 1));
            Assert.IsNull(WorkSim.EndDay(s, L, 1));
            Assert.AreEqual(2, s.day);
        }

        [Test]
        public void NoMoney_NoStart_BudgetNeverNegative()
        {
            var s = Working();
            s.budget = 1000;
            for (int i = 0; i < 40; i++) WorkSim.Tick(s, L, 0.25f);
            Assert.AreEqual(1000, s.budget);
            Assert.AreEqual(0f, s.Task("t").progress);
            Assert.IsTrue(Review.CheckBudgetDeadlock(s, L));
            Assert.AreEqual(FailReason.NoBudget, s.failReason);
        }

        [Test]
        public void SaveLevel_RoundTrip_SeparateFromV1()
        {
            string backup = PlayerPrefs.HasKey(SaveSystem.LevelKey) ? PlayerPrefs.GetString(SaveSystem.LevelKey) : null;
            try
            {
                var s = Working();
                s.dayElapsed = 123.5f;
                SaveSystem.SaveLevel(s);
                var r = SaveSystem.LoadLevel();
                Assert.AreEqual(123.5f, r.dayElapsed);
                Assert.AreEqual("dima", r.staff[0].id);
                Assert.AreEqual(JsonUtility.ToJson(s), JsonUtility.ToJson(r));
                PlayerPrefs.SetString(SaveSystem.LevelKey, "{broken");
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
                Assert.IsNull(SaveSystem.LoadLevel());
            }
            finally
            {
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = false;
                if (backup != null) PlayerPrefs.SetString(SaveSystem.LevelKey, backup); else SaveSystem.DeleteLevel();
            }
        }
    }
}
