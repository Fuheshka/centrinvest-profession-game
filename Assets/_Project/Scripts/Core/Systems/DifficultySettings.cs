using System;

namespace Studio.Core
{
    [Serializable]
    public struct DifficultySettings
    {
        public float budgetMul;
        public int daysBonus;
        public float aiErrorChance;
        public bool showThreads;
        public int reviewTolerance;
    }
}
