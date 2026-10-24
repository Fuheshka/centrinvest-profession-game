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
            Assert.IsTrue(def.anyone || def.d == Discipline.Mechanics);
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
            Assert.AreEqual(1, S.pendingEvents.Count);
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
    }
}
