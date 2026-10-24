using Studio.Core;
using UnityEngine;

namespace Studio.UI.Mvp
{
    // The four cards of the right column (mockup 1): the hired team with energy and current work.
    public static class TeamCards
    {
        public static void Fill(GameLoop game, RowView[] cards)
        {
            var s = game.S; var l = game.L;
            for (int i = 0; i < cards.Length; i++)
            {
                if (i >= s.staff.Count) { cards[i].Set("Свободное место", "найми в хедхантере"); continue; }
                var e = s.staff[i];
                var c = l.Candidate(e.id);
                var t = e.currentTaskId == null ? null : s.Task(e.currentTaskId);
                cards[i].Set(c.name + " · " + Names.Of(c.d),
                    "Энергия " + Mathf.RoundToInt(e.energy) + " · " + (t != null ? "работает" : "свободен(на)"));
            }
        }
    }
}
