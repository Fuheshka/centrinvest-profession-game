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
    }
}
