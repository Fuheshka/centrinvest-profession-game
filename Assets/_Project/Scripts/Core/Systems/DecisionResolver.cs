using System;
using System.Collections.Generic;

namespace Studio.Core
{
    public static class DecisionResolver
    {
        const int MatchBonus = 5;
        const int ConflictPenalty = 8;

        public static bool CanAfford(GameState state, OptionSO option)
            => state.budget >= option.cost.budget && state.day + option.cost.days <= state.deadline;

        public static DecisionRecord Apply(GameState state, MessageSO message, OptionSO option, ContentDB db)
        {
            if (!CanAfford(state, option))
                throw new InvalidOperationException("Cannot afford option " + option.id);

            var deltas = new int[4];
            for (int i = 0; i < 4; i++) deltas[i] = option.deltas[i];
            deltas[(int)Axis.Coherence] += TagCoherence(state, option, db);

            var record = new DecisionRecord
            {
                messageId = message.id,
                optionId = option.id,
                stage = state.stage,
                day = state.day,
                deltas = deltas,
            };

            state.budget -= option.cost.budget;
            state.day += option.cost.days;
            for (int i = 0; i < 4; i++)
                state.axes[i] = Math.Max(0, Math.Min(100, state.axes[i] + deltas[i]));

            state.inbox.Remove(message.id);
            if (!state.answered.Contains(message.id)) state.answered.Add(message.id);
            state.log.Add(record);
            if (option.followUp != null)
                state.pending.Add(new Pending { messageId = option.followUp.id, day = state.day + option.followUpDelayDays });
            return record;
        }

        static int TagCoherence(GameState state, OptionSO option, ContentDB db)
        {
            var boardTags = new HashSet<string>();
            foreach (var entry in state.board.placed)
                foreach (var card in db.cards)
                    if (card != null && card.id == entry.cardId)
                        foreach (var t in card.tags) boardTags.Add(t);

            int sum = 0;
            foreach (var tag in option.tags)
            {
                if (boardTags.Contains(tag)) sum += MatchBonus;
                foreach (var pair in db.tagConflicts)
                    if ((pair.a == tag && boardTags.Contains(pair.b)) || (pair.b == tag && boardTags.Contains(pair.a)))
                        sum -= ConflictPenalty;
            }
            return sum;
        }
    }
}
