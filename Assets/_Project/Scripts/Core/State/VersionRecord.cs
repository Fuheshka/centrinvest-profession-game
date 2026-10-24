using System;
using System.Collections.Generic;

namespace Studio.Core
{
    [Serializable]
    public class QuoteBlame
    {
        public string quoteId;
        public string messageId;
    }

    [Serializable]
    public class VersionRecord
    {
        public int number;
        public int day;
        public int[] axes = new int[4];
        public bool approved;
        public List<string> quoteIds = new List<string>();
        public List<QuoteBlame> blamed = new List<QuoteBlame>();
    }
}
