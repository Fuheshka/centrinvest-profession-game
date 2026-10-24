using Studio.Core;
using TMPro;
using UnityEngine;

namespace Studio.UI
{
    public class SlotView : MonoBehaviour
    {
        public Slot slot;
        public TMP_Text hint;
        public RectTransform frame;         // serialized rounded background, visible for an empty slot
        public RectTransform cardAnchor;    // card stretches over this

        public RectTransform Rect => (RectTransform)transform;

        public static string Name(Slot s)
        {
            switch (s)
            {
                case Slot.Genre: return "Жанр";
                case Slot.Setting: return "Сеттинг";
                case Slot.Audience: return "Аудитория";
                case Slot.Mechanic: return "Главная механика";
                case Slot.Hook: return "Фишка";
                default: return "Референс";
            }
        }

        public static Color PinColor(Slot s)
        {
            switch (s)
            {
                case Slot.Genre: return new Color(0.9f, 0.28f, 0.3f);
                case Slot.Setting: return new Color(0.96f, 0.62f, 0.18f);
                case Slot.Audience: return new Color(0.49f, 0.36f, 0.95f);
                case Slot.Mechanic: return new Color(0.18f, 0.5f, 0.93f);
                case Slot.Hook: return new Color(0.9f, 0.4f, 0.7f);
                default: return new Color(0.24f, 0.7f, 0.5f);
            }
        }

        public void SetEmpty(bool value)
        {
            frame.gameObject.SetActive(value);
            hint.gameObject.SetActive(value);
            if (value) hint.text = $"Слот «{Name(slot)}»\nперетащи карточку";
        }

    }
}
