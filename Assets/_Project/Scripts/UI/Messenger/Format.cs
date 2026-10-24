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
