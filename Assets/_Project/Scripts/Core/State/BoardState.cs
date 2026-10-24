using System;
using System.Collections.Generic;

namespace Studio.Core
{
    [Serializable]
    public class SlotEntry
    {
        public Slot slot;
        public string cardId;
    }

    [Serializable]
    public class Link
    {
        public string a;
        public string b;
    }

    [Serializable]
    public class BoardState
    {
        public List<SlotEntry> placed = new List<SlotEntry>();
        public List<Link> links = new List<Link>();
        public List<string> revealed = new List<string>();    // card ids
    }
}
