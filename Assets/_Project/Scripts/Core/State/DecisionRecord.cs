using System;

namespace Studio.Core
{
    [Serializable]
    public class DecisionRecord
    {
        public string messageId;
        public string optionId;
        public Stage stage;
        public int day;
        public int[] deltas = new int[4];
        public Cost cost;                // price of the chosen option, base of ReviewSystem.ReopenCost
        public bool replaced;            // reopened in Release, a newer record took its place
    }
}
