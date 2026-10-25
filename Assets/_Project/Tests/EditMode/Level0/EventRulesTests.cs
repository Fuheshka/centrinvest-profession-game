using NUnit.Framework;
using Studio.Core;
using UnityEditor;

namespace Studio.Tests
{
    public class EventRulesTests
    {
        LevelSO L;
        LevelState S;

        [SetUp]
        public void SetUp()
        {
            L = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
            S = LevelState.New(L, "x");
            S.stage = LevelStage.Production; S.concept.approved = true;
            S.tasks.Add(new TaskState { id = "t", d = Discipline.Mechanics, title = "T", tier = 3, effort = 450 });
            Staffing.Hire(S, L, "dima");
            WorkSim.Assign(S, "t", "dima");
            S.Employee("dima").currentTaskId = "t";   // working now
        }

        static int Zero(int n) => 0;

        [Test]
        public void Tick_SpawnsAfterFirstDelay_OnlyForFittingEmployee()
        {
            EventRules.Tick(S, L, 10, Zero);
            Assert.AreEqual(0, S.pendingEvents.Count);
            EventRules.Tick(S, L, L.tuning.eventFirst, Zero);
            Assert.AreEqual(1, S.pendingEvents.Count);
            var def = L.events.Find(e => e.id == S.pendingEvents[0].eventId);
            Assert.IsTrue(def.boss || def.anyone || def.d == Discipline.Mechanics);
            Assert.AreEqual("event", S.chat[S.chat.Count - 1].kind);
        }

        [Test]
        public void Tick_NoEventWhileNobodyWorks()
        {
            S.Employee("dima").currentTaskId = null;
            EventRules.Tick(S, L, 1000, Zero);
            Assert.AreEqual(0, S.pendingEvents.Count);
        }

        [Test]
        public void Tick_OnePendingPerEmployee()
        {
            EventRules.Tick(S, L, 1000, Zero);
            EventRules.Tick(S, L, 1000, Zero);
            Assert.AreEqual(1, S.pendingEvents.FindAll(p => p.employeeId == "dima").Count);   // the boss may have one more
        }

        [Test]
        public void Resolve_AppliesEffects_NeverCompletesTask_Once()
        {
            S.pendingEvents.Add(new PendingEvent { eventId = "physics", employeeId = "dima" });
            S.Employee("dima").energy = 50;
            Assert.IsTrue(EventRules.Resolve(S, L, "physics", "dima", 0));
            Assert.AreEqual(55f, S.Employee("dima").energy, 0.01f);
            Assert.AreEqual(90f, S.Task("t").progress, 0.01f);
            Assert.AreEqual(0, S.pendingEvents.Count);
            Assert.AreEqual("me", S.chat[S.chat.Count - 2].kind);
            StringAssert.Contains("Упростить", S.chat[S.chat.Count - 2].text);
            Assert.IsFalse(EventRules.Resolve(S, L, "physics", "dima", 0), "already answered");
            S.pendingEvents.Add(new PendingEvent { eventId = "bug", employeeId = "dima" });
            S.Task("t").progress = 440;
            EventRules.Resolve(S, L, "bug", "dima", 0);
            Assert.Less(S.Task("t").progress, S.Task("t").effort);
        }

        [Test]
        public void Resolve_RefusesWhenBudgetShort()
        {
            S.pendingEvents.Add(new PendingEvent { eventId = "pack", employeeId = "lera" });
            Staffing.Hire(S, L, "lera");
            S.budget = 1000;
            Assert.IsFalse(EventRules.Resolve(S, L, "pack", "lera", 0));
            Assert.AreEqual(1, S.pendingEvents.Count);
            Assert.AreEqual(1000, S.budget);
        }

        void Pending(string id, string who) => S.pendingEvents.Add(new PendingEvent { eventId = id, employeeId = who });

        [Test]
        public void Resolve_TeamEnergyAndDisciplineShares()
        {
            Staffing.Hire(S, L, "lera");
            S.tasks.Add(new TaskState { id = "a", d = Discipline.Design, title = "A", tier = 1, effort = 100, progress = 50 });
            Pending("slop", "dima");
            Assert.IsTrue(EventRules.Resolve(S, L, "slop", "dima", 0));
            Assert.AreEqual(40f, S.Employee("lera").energy, 0.01f);
            Assert.AreEqual(30f, S.Task("a").progress, 0.01f, "design -20 %");
            Assert.AreEqual(45f, S.Task("t").progress, 0.01f, "others +10 %");
            Assert.IsTrue(S.eventsSeen.Contains("slop"));
        }

        [Test]
        public void Resolve_Fire_RemovesThatDiscipline()
        {
            Staffing.Hire(S, L, "lera");
            Pending("writer2", "dima");
            Assert.IsTrue(EventRules.Resolve(S, L, "writer2", "dima", 2));   // "Ладно": the writer leaves
            Assert.IsNull(S.Employee("dima"));
            Assert.IsNotNull(S.Employee("lera"));
        }

        [Test]
        public void Resolve_SpeedAndRate()
        {
            Pending("internet", LevelState.Boss);
            EventRules.Resolve(S, L, "internet", LevelState.Boss, 1);
            Assert.AreEqual(0.25f, S.Mul(Discipline.Sound), 0.001f);
            Pending("writer2", "dima");
            int rate = Staffing.Rate(S, L, "dima");
            EventRules.Resolve(S, L, "writer2", "dima", 0);
            Assert.AreEqual(rate + 100, Staffing.Rate(S, L, "dima"));
        }

        [Test]
        public void Resolve_PowerOutage_FailRollAndEndDay()
        {
            S.Task("t").progress = 100;
            Pending("power", "dima");
            EventRules.Resolve(S, L, "power", "dima", 2, n => 0);   // roll 0 < 50: hits
            Assert.AreEqual(100 - 22.5f, S.Task("t").progress, 0.01f);
            S.eventsSeen.Clear(); S.Task("t").progress = 100;
            Pending("power", "dima");
            EventRules.Resolve(S, L, "power", "dima", 2, n => 99);  // misses
            Assert.AreEqual(100f, S.Task("t").progress, 0.01f);
            S.eventsSeen.Clear();
            Pending("power", "dima");
            EventRules.Resolve(S, L, "power", "dima", 0);
            int day = S.day;
            WorkSim.Tick(S, L, 0.1f);
            Assert.AreEqual(day + 1, S.day, "weekend closes the day");
        }

        [Test]
        public void Spawn_ChainedEventWaitsForItsPredecessor()
        {
            foreach (var e in L.events) if (e.id != "writer1") S.eventsSeen.Add(e.id);
            S.eventsSeen.Remove("slop");
            Pending("slop", "dima");   // asked, not answered yet
            EventRules.Tick(S, L, 1000, Zero);
            Assert.AreEqual(1, S.pendingEvents.Count, "writer1 needs slop answered first");
            S.pendingEvents.Clear(); S.eventsSeen.Add("slop");
            EventRules.Tick(S, L, 1000, Zero);
            Assert.AreEqual("writer1", S.pendingEvents[0].eventId);
        }

        [Test]
        public void Tick_CroakFiresEveryTwelveSeconds()
        {
            int n = 0;
            System.Action h = () => n++;
            EventRules.Croaked += h;
            S.croak = true; S.croakTimer = 12;
            EventRules.Tick(S, L, 11, Zero); Assert.AreEqual(0, n);
            EventRules.Tick(S, L, 2, Zero); Assert.AreEqual(1, n);
            EventRules.Croaked -= h;
        }
    }
}
