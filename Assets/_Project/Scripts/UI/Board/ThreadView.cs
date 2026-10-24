using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // A thread is a stretched and rotated Image set between two pins (no LineRenderer, see ROADMAP "Риски").
    // Colour is always backed by shape: Match solid, Conflict double, Unknown dashed, Neutral thin.
    public class ThreadView : MonoBehaviour
    {
        public RectTransform line;          // rotated child, carries the click target
        public RectTransform plate;         // upright label above the line
        public TMP_Text plateText;
        public Image plateBackground;
        public Button hit;

        public static readonly Color Green = new Color(0.18f, 0.62f, 0.416f);
        public static readonly Color Red = new Color(0.788f, 0.227f, 0.247f);
        public static readonly Color Yellow = new Color(0.949f, 0.788f, 0.298f);
        public static readonly Color Grey = new Color(0.36f, 0.38f, 0.44f);

        const float PinGap = 12f;           // thread stops at the pin edge, not at its centre
        const float HitHeight = 26f;

        public void Setup(Vector2 from, Vector2 to, ThreadKind kind, string label, Action onClick)
        {
            var delta = to - from;
            var len = Mathf.Max(0f, delta.magnitude - 2f * PinGap);
            ((RectTransform)transform).localPosition = (from + to) / 2f;

            line.sizeDelta = new Vector2(len, HitHeight);
            line.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            foreach (Transform c in line) Destroy(c.gameObject);
            DrawLine(line, len, kind);

            hit.onClick.RemoveAllListeners();
            hit.onClick.AddListener(() => onClick());

            plate.gameObject.SetActive(label != null);
            if (label == null) return;
            plateText.text = label;
            plateBackground.color = kind == ThreadKind.Conflict ? new Color(0.29f, 0.12f, 0.14f) : new Color(0.12f, 0.24f, 0.2f);
            plateText.color = kind == ThreadKind.Conflict ? new Color(1f, 0.85f, 0.86f) : new Color(0.85f, 0.96f, 0.9f);
            plate.sizeDelta = new Vector2(plateText.GetPreferredValues(label).x + 28f, 34f);
            plate.localPosition = new Vector2(0, 30f);
        }

        // Draws the shape of a kind into `parent` (centred, along local x). Also used by the legend.
        public static void DrawLine(RectTransform parent, float len, ThreadKind kind)
        {
            switch (kind)
            {
                case ThreadKind.Match:
                    Bar(parent, 0, 0, len, 6, Green);
                    break;
                case ThreadKind.Conflict:
                    Bar(parent, 0, 4, len, 3, Red);
                    Bar(parent, 0, -4, len, 3, Red);
                    break;
                case ThreadKind.Unknown:
                    Dashes(parent, -len / 2f, len / 2f, 0, 5, 14, 9, Yellow);
                    break;
                default:
                    Bar(parent, 0, 0, len, 2, Grey);
                    break;
            }
        }

        public static Image Bar(RectTransform parent, float x, float y, float w, float h, Color color)
        {
            var go = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static void Dashes(RectTransform parent, float x0, float x1, float y, float h, float dash, float gap, Color color)
        {
            for (var x = x0; x < x1; x += dash + gap)
            {
                var w = Mathf.Min(dash, x1 - x);
                Bar(parent, x + w / 2f, y, w, h, color);
            }
        }
    }
}
