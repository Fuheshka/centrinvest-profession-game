using Studio.Core;
using TMPro;
using UnityEngine;

namespace Studio.UI
{
    public class SlotView : MonoBehaviour
    {
        public Slot slot;
        public TMP_Text hint;
        public RectTransform frame;         // dashed border, drawn for an empty slot only
        public RectTransform cardAnchor;    // card stretches over this

        Vector2 frameSize;
        bool empty = true;

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
            empty = value;
            frame.gameObject.SetActive(value);
            hint.gameObject.SetActive(value);
            if (value) hint.text = $"Слот «{Name(slot)}»\nперетащи карточку";
        }

        // Dashes depend on the slot size, which comes from anchors: rebuild when it changes.
        void LateUpdate()
        {
            var size = Rect.rect.size;
            if (!empty || size == frameSize || size.x < 10) return;
            frameSize = size;
            foreach (Transform c in frame) Destroy(c.gameObject);
            var colour = new Color(0.36f, 0.25f, 0.14f);
            float hw = size.x / 2f - 2f, hh = size.y / 2f - 2f;
            ThreadView.Dashes(frame, -hw, hw, hh, 3, 12, 8, colour);
            ThreadView.Dashes(frame, -hw, hw, -hh, 3, 12, 8, colour);
            VDashes(hh, hw, colour);
            VDashes(hh, -hw, colour);
        }

        void VDashes(float hh, float x, Color colour)
        {
            for (var y = -hh; y < hh; y += 20f)
                ThreadView.Bar(frame, x, y + Mathf.Min(12f, hh - y) / 2f, 3, Mathf.Min(12f, hh - y), colour);
        }
    }
}
