using System.Collections.Generic;
using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class DayClockTests
    {
        ContentDB db;
        GameState state;

        [SetUp]
        public void SetUp()
        {
            db = ScriptableObject.CreateInstance<ContentDB>();
            db.startBudget = 100;
            db.deadline = 10;
            db.brief = Msg("brief", Stage.Idea, 1);
            state = GameState.New(db, "tester", DifficultyLevel.Designer);
        }

        MessageSO Add(string id, Stage stage, int day)
        {
            var m = Msg(id, stage, day);
            var list = new List<MessageSO>(db.messages) { m };
            db.messages = list.ToArray();
            return m;
        }

        static MessageSO Msg(string id, Stage stage, int day)
        {
            var m = ScriptableObject.CreateInstance<MessageSO>();
            m.id = id;
            m.stage = stage;
            m.day = day;
            return m;
        }

        static List<string> Ids(List<MessageSO> list) => list.ConvertAll(m => m.id);

        [Test]
        public void EndDay_AdvancesDayAndDeliversThatDaysMessages()
        {
            Add("d2a", Stage.Idea, 2);
            Add("d2b", Stage.Idea, 2);
            Add("d3", Stage.Idea, 3);

            var fresh = DayClock.EndDay(state, db);

            Assert.AreEqual(2, state.day);
            CollectionAssert.AreEquivalent(new[] { "d2a", "d2b" }, Ids(fresh));
            CollectionAssert.IsSubsetOf(new[] { "d2a", "d2b" }, state.inbox);
            CollectionAssert.DoesNotContain(state.inbox, "d3");
        }

        [Test]
        public void EndDay_IgnoresMessagesOfOtherStages()
        {
            Add("prod2", Stage.Production, 2);
            Assert.IsEmpty(DayClock.EndDay(state, db));
            CollectionAssert.DoesNotContain(state.inbox, "prod2");
        }

        [Test]
        public void EndDay_DoesNotRedeliverMessagesAlreadySeen()
        {
            Add("d2", Stage.Idea, 2);
            DayClock.EndDay(state, db);
            state.day = 1;   // clock rewound, e.g. a reload
            Assert.IsEmpty(DayClock.EndDay(state, db));
            Assert.AreEqual(1, state.inbox.FindAll(i => i == "d2").Count);
        }

        [Test]
        public void EndDay_DeliversDaysSkippedByMultiDayDecisions()
        {
            Add("d2", Stage.Idea, 2);
            Add("d3", Stage.Idea, 3);
            state.day = 3;   // a 2-day decision jumped over day 2 without a mail round
            CollectionAssert.AreEquivalent(new[] { "d2", "d3" }, Ids(DayClock.EndDay(state, db)));
        }

        [Test]
        public void EndDay_PendingArrivesOnItsDayNotBefore()
        {
            var followUp = Add("f1", Stage.Idea, 0);
            var opt = ScriptableObject.CreateInstance<OptionSO>();
            opt.id = "o";
            opt.followUp = followUp;
            opt.followUpDelayDays = 2;
            db.brief.options = new[] { opt };
            DecisionResolver.Apply(state, db.brief, opt, db);   // pending on day 3

            Assert.IsEmpty(DayClock.EndDay(state, db));    // day 2
            Assert.AreEqual(1, state.pending.Count);

            var fresh = DayClock.EndDay(state, db);        // day 3
            CollectionAssert.AreEqual(new[] { "f1" }, Ids(fresh));
            CollectionAssert.Contains(state.inbox, "f1");
            Assert.IsEmpty(state.pending);
        }

        [Test]
        public void EndDay_FollowUpNeverArrivesAsAScheduledMessage()
        {
            var followUp = Add("f1", Stage.Idea, 0);   // day 0 would otherwise match every day
            var opt = ScriptableObject.CreateInstance<OptionSO>();
            opt.followUp = followUp;
            var holder = Add("holder", Stage.Idea, 5);
            holder.options = new[] { opt };

            Assert.IsEmpty(DayClock.EndDay(state, db));
            CollectionAssert.DoesNotContain(state.inbox, "f1");
        }

        [Test]
        public void EndDay_UnansweredMessagesStayInInbox()
        {
            Add("d2", Stage.Idea, 2);
            DayClock.EndDay(state, db);
            DayClock.EndDay(state, db);
            CollectionAssert.Contains(state.inbox, "brief");
            CollectionAssert.Contains(state.inbox, "d2");
        }

        [Test]
        public void EndDay_ProductionToReleaseOnFirstReleaseMessageDay()
        {
            state.stage = Stage.Production;
            Add("rel4", Stage.Release, 4);
            state.day = 2;

            DayClock.EndDay(state, db);   // day 3
            Assert.AreEqual(Stage.Production, state.stage);

            var fresh = DayClock.EndDay(state, db);   // day 4
            Assert.AreEqual(Stage.Release, state.stage);
            CollectionAssert.AreEqual(new[] { "rel4" }, Ids(fresh));
        }

        [Test]
        public void EndDay_ReleaseStartDayOverridesDerivedDay()
        {
            state.stage = Stage.Production;
            db.releaseStartDay = 3;
            Add("rel6", Stage.Release, 6);
            state.day = 2;
            DayClock.EndDay(state, db);   // day 3
            Assert.AreEqual(Stage.Release, state.stage);
        }

        [Test]
        public void EndDay_ProductionStaysWithoutAnyReleaseDay()
        {
            state.stage = Stage.Production;
            state.day = 8;
            DayClock.EndDay(state, db);
            Assert.AreEqual(Stage.Production, state.stage);
        }

        [Test]
        public void EndDay_IdeaNeverLeavesWithoutCommit()
        {
            db.releaseStartDay = 2;
            Add("rel2", Stage.Release, 2);
            DayClock.EndDay(state, db);
            Assert.AreEqual(Stage.Idea, state.stage);
        }

        [Test]
        public void EndDay_ReleaseToDoneWhenVersionApproved()
        {
            state.stage = Stage.Release;
            DayClock.EndDay(state, db);
            Assert.AreEqual(Stage.Release, state.stage);

            state.versions.Add(new VersionRecord { number = 1, approved = true });
            DayClock.EndDay(state, db);
            Assert.AreEqual(Stage.Done, state.stage);
        }

        [Test]
        public void IsOverDeadline_OnlyAfterTheDeadlineDay()
        {
            state.day = 10;
            Assert.IsFalse(DayClock.IsOverDeadline(state));
            state.day = 11;
            Assert.IsTrue(DayClock.IsOverDeadline(state));
        }

        [Test]
        public void EndDay_DeadlineMovesToDone()
        {
            state.stage = Stage.Release;
            state.day = 10;
            DayClock.EndDay(state, db);   // day 11 > deadline 10
            Assert.AreEqual(Stage.Done, state.stage);
        }

        [Test]
        public void Pending_SurvivesJsonRoundTrip()
        {
            state.pending.Add(new Pending { messageId = "f1", day = 4 });
            var back = JsonUtility.FromJson<GameState>(JsonUtility.ToJson(state));
            Assert.AreEqual(1, back.pending.Count);
            Assert.AreEqual("f1", back.pending[0].messageId);
            Assert.AreEqual(4, back.pending[0].day);
        }

        [Test]
        public void EndDay_DoneStateIsFrozen()
        {
            state.stage = Stage.Done;
            state.day = 12;
            Assert.IsEmpty(DayClock.EndDay(state, db));
            Assert.AreEqual(12, state.day);
        }

        [Test]
        public void EndDay_ThreeDaysOnTestScenario_DeliversNextDaysAndFollowUp()
        {
            var followUp = Add("reply", Stage.Idea, 0);
            Add("d2", Stage.Idea, 2);
            Add("d3", Stage.Idea, 3);
            var opt = ScriptableObject.CreateInstance<OptionSO>();
            opt.followUp = followUp;
            opt.followUpDelayDays = 1;
            db.brief.options = new[] { opt };
            DecisionResolver.Apply(state, db.brief, opt, db);   // pending on day 2

            var all = new List<string>();
            for (int i = 0; i < 3; i++) all.AddRange(Ids(DayClock.EndDay(state, db)));

            CollectionAssert.AreEquivalent(new[] { "d2", "reply", "d3" }, all);
            Assert.AreEqual(4, state.day);
        }
    }
}
