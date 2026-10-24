using NUnit.Framework;
using Studio.Core;
using UnityEngine;

namespace Studio.Tests
{
    public class DifficultyTests
    {
        [TestCase(DifficultyLevel.Intern, 1.3f, 3, 0.05f, true, 10)]
        [TestCase(DifficultyLevel.Designer, 1.0f, 0, 0.2f, false, 5)]
        [TestCase(DifficultyLevel.Lead, 0.8f, -2, 0.4f, false, 0)]
        public void For_ReturnsTableValues(DifficultyLevel level, float mul, int days, float aiError, bool threads, int tolerance)
        {
            var s = Difficulty.For(level);
            Assert.AreEqual(mul, s.budgetMul, 1e-4f);
            Assert.AreEqual(days, s.daysBonus);
            Assert.AreEqual(aiError, s.aiErrorChance, 1e-4f);
            Assert.AreEqual(threads, s.showThreads);
            Assert.AreEqual(tolerance, s.reviewTolerance);
        }

        [TestCase(DifficultyLevel.Intern, 130000, 15)]
        [TestCase(DifficultyLevel.Designer, 100000, 12)]
        [TestCase(DifficultyLevel.Lead, 80000, 10)]
        public void New_AppliesBudgetMultiplierAndDaysBonus(DifficultyLevel level, int budget, int deadline)
        {
            var db = ScriptableObject.CreateInstance<ContentDB>();
            db.startBudget = 100000;
            db.deadline = 12;
            var state = GameState.New(db, "tester", level);
            Assert.AreEqual(budget, state.budget);
            Assert.AreEqual(deadline, state.deadline);
            Assert.AreEqual(level, state.difficulty);
        }

        [Test]
        public void New_SetsSaveVersion()
        {
            var db = ScriptableObject.CreateInstance<ContentDB>();
            Assert.AreEqual(1, GameState.New(db, "tester", DifficultyLevel.Designer).saveVersion);
        }
    }
}
