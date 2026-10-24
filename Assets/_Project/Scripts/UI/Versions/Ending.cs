using System.Collections.Generic;
using System.Linq;
using Studio.Core;

namespace Studio.UI
{
    public enum Verdict { Flop, Average, Hit, GameOfYear }

    public class Highlight
    {
        public string title;
        public string note;
        public bool good;
    }

    // Pure ending maths: score, verdict, game title, reviews and the decision breakdown.
    public static class Ending
    {
        public static readonly string[] AxisNames = { "Интерес", "Цельность", "Понятность", "Техничка" };

        public static int Score(int[] axes) => (int)System.Math.Round(axes.Average());

        public static Verdict VerdictFor(int score)
            => score < 40 ? Verdict.Flop : score < 60 ? Verdict.Average : score < 80 ? Verdict.Hit : Verdict.GameOfYear;

        public static string VerdictName(Verdict v)
        {
            switch (v)
            {
                case Verdict.Flop: return "Провал";
                case Verdict.Average: return "Середнячок";
                case Verdict.Hit: return "Хит";
                default: return "Игра года";
            }
        }

        public static string Headline(Verdict v, string title)
        {
            switch (v)
            {
                case Verdict.Flop: return $"«{title}» вышла и осталась незамеченной";
                case Verdict.Average: return $"«{title}» вышла";
                case Verdict.Hit: return $"«{title}» вышла – и её хвалят!";
                default: return $"«{title}» – игра года!";
            }
        }

        // Genre + hook cards of the committed board.
        public static string GameTitle(GameState s, ContentDB db)
        {
            string Pick(Slot slot)
            {
                var e = s.board.placed.Find(p => p.slot == slot);
                var card = e == null ? null : db.cards.FirstOrDefault(c => c != null && c.id == e.cardId);
                return card?.title;
            }
            var parts = new[] { Pick(Slot.Genre), Pick(Slot.Hook) }.Where(p => !string.IsNullOrEmpty(p)).ToArray();
            return parts.Length == 0 ? "Без названия" : string.Join(": ", parts);
        }

        // One review per axis: praise if the axis reached its goal, a complaint otherwise.
        public static List<QuoteSO> Reviews(GameState s, ContentDB db)
        {
            var list = new List<QuoteSO>();
            for (int i = 0; i < 4; i++)
            {
                bool praise = s.axes[i] >= s.goals[i];
                var q = db.quotes.FirstOrDefault(x => x != null && (int)x.axis == i && x.praise == praise);
                if (q != null) list.Add(q);
            }
            return list;
        }

        // Two best and two worst live decisions by total axis change, the board included.
        public static List<Highlight> Breakdown(GameState s, ContentDB db)
        {
            var all = s.log.Where(r => !r.replaced).ToList();
            all.Add(ReviewSystem.BoardRecord(s, db));
            var sorted = all.OrderByDescending(r => r.deltas.Sum()).ToList();
            var picked = new List<DecisionRecord>();
            picked.AddRange(sorted.Take(2));
            picked.AddRange(sorted.Skip(2).Reverse().Take(2));   // worst first, no overlap with the best
            return picked.Select(r => Describe(s, db, r)).ToList();
        }

        static Highlight Describe(GameState s, ContentDB db, DecisionRecord r)
        {
            bool good = r.deltas.Sum() >= 0;
            int axis = 0;
            for (int i = 1; i < 4; i++)
                if (good ? r.deltas[i] > r.deltas[axis] : r.deltas[i] < r.deltas[axis]) axis = i;
            int d = r.deltas[axis];
            return new Highlight
            {
                title = DecisionText(s, db, r),
                note = (d >= 0 ? "Дало +" : "Просело: ") + d + " " + AxisNames[axis] + ".",
                good = good,
            };
        }

        public static string DecisionText(GameState s, ContentDB db, DecisionRecord r)
        {
            if (r.messageId == ReviewSystem.BoardId) return "Доска идеи: " + GameTitle(s, db);
            var m = MessengerController.Find(db, r.messageId);
            var o = m == null ? null : m.options.FirstOrDefault(x => x != null && x.id == r.optionId);
            return o != null ? o.text : r.messageId;
        }

        public const string ProfessionText =
            "Геймдизайнер – это не «придумать идею». Это сотни решений, которые сочетаются между собой, " +
            "укладываются в сроки и нравятся людям. Ты только что сделал это за {0} дн.";
    }
}
