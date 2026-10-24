using System;

namespace Studio.Core
{
    public static class ReviewSystem
    {
        public static VersionRecord Review(GameState state, ContentDB db)
            => throw new NotImplementedException("P-7");

        public static string Blame(GameState state, VersionRecord version, string quoteId)
            => throw new NotImplementedException("P-7");

        public static Cost ReopenCost(DecisionRecord record)
            => throw new NotImplementedException("P-7");

        public static void Reopen(GameState state, DecisionRecord record, OptionSO newOption, ContentDB db)
            => throw new NotImplementedException("P-7");
    }
}
