using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // Code-built uGUI pieces for the Versions tab and the ending: colors and fonts of the mockup in one place.
    public static class UiKit
    {
        public static TMP_FontAsset Font;
        public static readonly Color Bg = new Color32(0x16, 0x18, 0x1F, 255);
        public static readonly Color Panel = new Color32(0x1D, 0x20, 0x28, 255);
        public static readonly Color Panel2 = new Color32(0x25, 0x28, 0x33, 255);
        public static readonly Color Line = new Color32(0x2C, 0x30, 0x3B, 255);
        public static readonly Color Accent = new Color32(0xFF, 0x8A, 0x4C, 255);
        public static readonly Color Text = new Color32(0xEC, 0xED, 0xEF, 255);
        public static readonly Color Dim = new Color32(0xA9, 0xAD, 0xB8, 255);
        public static readonly Color Green = new Color32(0x3D, 0xBE, 0x86, 255);
        public static readonly Color Red = new Color32(0xFF, 0x5A, 0x5F, 255);
        public static readonly Color Yellow = new Color32(0xF2, 0xC9, 0x4C, 255);

        public static RectTransform Node(Transform parent, string name)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static Image Box(Transform parent, string name, Color color)
        {
            var img = Node(parent, name).gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        public static TMP_Text Label(Transform parent, string text, float size, Color color,
            FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
        {
            var t = Node(parent, "Text").gameObject.AddComponent<TextMeshProUGUI>();
            if (Font != null) t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }

        public static VerticalLayoutGroup V(RectTransform rt, float spacing, int pad = 0)
        {
            var g = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(pad, pad, pad, pad);
            g.childControlWidth = g.childControlHeight = true;
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = false;
            return g;
        }

        public static HorizontalLayoutGroup H(RectTransform rt, float spacing, int pad = 0)
        {
            var g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing;
            g.padding = new RectOffset(pad, pad, pad, pad);
            g.childControlWidth = g.childControlHeight = true;
            g.childForceExpandWidth = false;
            g.childForceExpandHeight = false;
            return g;
        }

        public static LayoutElement Size(Component c, float w = -1, float h = -1, float flexW = -1, float flexH = -1)
        {
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;
            le.flexibleWidth = flexW;
            le.flexibleHeight = flexH;
            return le;
        }

        public static Outline Border(Image img, Color c)
        {
            var o = img.gameObject.AddComponent<Outline>();
            o.effectColor = c;
            o.effectDistance = new Vector2(2, -2);
            return o;
        }

        // A clickable box; children (labels) are added by the caller. Returns the background image.
        public static Image ButtonBox(Transform parent, string name, Color bg, Action click, bool interactable = true)
        {
            var img = Box(parent, name, bg);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.interactable = interactable;
            b.transition = Selectable.Transition.None;
            b.onClick.AddListener(() => click());
            return img;
        }

        public static Image TextButton(Transform parent, string text, Color bg, Color fg, Action click, float height = 52, bool interactable = true)
        {
            var img = ButtonBox(parent, "Button", bg, click, interactable);
            var v = V(img.rectTransform, 0, 12);
            v.childAlignment = TextAnchor.MiddleCenter;
            Label(img.transform, text, 18, interactable ? fg : Dim, FontStyles.Bold, TextAlignmentOptions.Center);
            Size(img, h: height);
            return img;
        }

        // Vertical scroll area; returns its content rect (a vertical layout that grows with children).
        public static RectTransform Scroll(Transform parent, float spacing)
        {
            var root = Node(parent, "Scroll");
            var sr = root.gameObject.AddComponent<ScrollRect>();
            root.gameObject.AddComponent<RectMask2D>();
            var content = Node(root, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            V(content, spacing);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = content;
            sr.horizontal = false;
            sr.scrollSensitivity = 30;
            sr.movementType = ScrollRect.MovementType.Clamped;
            return content;
        }
    }
}
