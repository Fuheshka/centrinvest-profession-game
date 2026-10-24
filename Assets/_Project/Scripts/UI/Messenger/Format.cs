using Studio.Core;
using UnityEngine;

namespace Studio.UI
{
    public static class Format
    {
        // Destroy is deferred to end of frame; hiding first keeps layout groups from counting dead children.
        public static void Clear(Transform t)
        {
            foreach (Transform c in t) { c.gameObject.SetActive(false); Object.Destroy(c.gameObject); }
        }

        // {name} in message and option texts is the player's nickname.
        // {days} and {budget} follow the difficulty, so the brief never promises numbers the top bar does not show.
        public static string Name(string text, GameState s)
            => text?.Replace("{name}", s.nickname).Replace("{days}", Days(s.deadline)).Replace("{budget}", Money(s.startBudget));

        // 1 день, 3 дня, 18 дней, 21 день.
        public static string Days(int n)
        {
            int m10 = n % 10, m100 = n % 100;
            var word = m10 == 1 && m100 != 11 ? "день" : m10 >= 2 && m10 <= 4 && (m100 < 12 || m100 > 14) ? "дня" : "дней";
            return n + " " + word;
        }

        public static string Money(int n) => n.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ') + " руб.";

        public static string Cost(Cost c)
        {
            if (c.days == 0 && c.budget == 0) return "0 дн.";
            return c.budget == 0 ? c.days + " дн." : c.days + " дн. · " + Money(c.budget);
        }

        public static string SenderName(Sender s)
        {
            switch (s)
            {
                case Sender.Boss: return "Борис Петрович · начальник";
                case Sender.Artist: return "Лера · художница";
                case Sender.Programmer: return "Костя · программист";
                case Sender.Producer: return "Марина · продюсер";
                case Sender.Nephew: return "Гоша · племянник";
                case Sender.Ai: return "Нейрон · ИИ-помощник";
                default: return "Общий чат студии";
            }
        }
    }
}
