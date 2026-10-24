namespace Studio.Core
{
    public static class Difficulty
    {
        public static DifficultySettings For(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Intern:
                    return new DifficultySettings { budgetMul = 1.3f, daysBonus = 3, aiErrorChance = 0.05f, showThreads = true, reviewTolerance = 10 };
                case DifficultyLevel.Lead:
                    return new DifficultySettings { budgetMul = 0.8f, daysBonus = -2, aiErrorChance = 0.4f, showThreads = false, reviewTolerance = 0 };
                default:
                    return new DifficultySettings { budgetMul = 1.0f, daysBonus = 0, aiErrorChance = 0.2f, showThreads = false, reviewTolerance = 5 };
            }
        }
    }
}
