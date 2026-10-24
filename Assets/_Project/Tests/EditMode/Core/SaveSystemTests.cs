using NUnit.Framework;
using Studio.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace Studio.Tests
{
    public class SaveSystemTests
    {
        string backup;

        [SetUp]
        public void SetUp()
        {
            backup = PlayerPrefs.HasKey(SaveSystem.Key) ? PlayerPrefs.GetString(SaveSystem.Key) : null;
            SaveSystem.Delete();
        }

        [TearDown]
        public void TearDown()
        {
            if (backup != null) PlayerPrefs.SetString(SaveSystem.Key, backup);
            else SaveSystem.Delete();
        }

        [Test]
        public void HasSave_FalseWhenEmpty() => Assert.IsFalse(SaveSystem.HasSave());

        [Test]
        public void Load_NullWhenEmpty() => Assert.IsNull(SaveSystem.Load());

        [Test]
        public void SaveLoad_RoundTrip()
        {
            var db = ScriptableObject.CreateInstance<ContentDB>();
            db.startBudget = 100000;
            db.deadline = 12;
            var s = GameState.New(db, "Лера", DifficultyLevel.Lead);
            s.day = 4;
            s.axes = new[] { 10, 20, 30, 40 };
            s.inbox.Add("m1");
            s.answered.Add("m0");
            s.log.Add(new DecisionRecord { messageId = "m0", optionId = "o1", day = 3, deltas = new[] { 1, 2, 3, 4 } });
            s.pending.Add(new Pending { messageId = "m5", day = 6 });
            s.board.placed.Add(new SlotEntry { slot = Slot.Hook, cardId = "c1" });

            SaveSystem.Save(s);
            Assert.IsTrue(SaveSystem.HasSave());
            var r = SaveSystem.Load();

            Assert.AreEqual("Лера", r.nickname);
            Assert.AreEqual(DifficultyLevel.Lead, r.difficulty);
            Assert.AreEqual(4, r.day);
            Assert.AreEqual(s.budget, r.budget);
            Assert.AreEqual(s.deadline, r.deadline);
            CollectionAssert.AreEqual(s.axes, r.axes);
            CollectionAssert.AreEqual(s.inbox, r.inbox);
            CollectionAssert.AreEqual(s.answered, r.answered);
            Assert.AreEqual("o1", r.log[0].optionId);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, r.log[0].deltas);
            Assert.AreEqual(6, r.pending[0].day);
            Assert.AreEqual("c1", r.board.placed[0].cardId);
            Assert.AreEqual(1, r.saveVersion);
        }

        [Test]
        public void Load_BrokenJson_ReturnsNullAndLogs()
        {
            PlayerPrefs.SetString(SaveSystem.Key, "{not json");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("save"));
            Assert.IsNull(SaveSystem.Load());
        }

        [Test]
        public void Delete_RemovesSave()
        {
            SaveSystem.Save(GameState.New(ScriptableObject.CreateInstance<ContentDB>(), "x", DifficultyLevel.Designer));
            SaveSystem.Delete();
            Assert.IsFalse(SaveSystem.HasSave());
        }
    }
}
