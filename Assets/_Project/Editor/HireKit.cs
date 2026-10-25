using Studio.Core;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-4 (headhunter): rebuilds HireWindow after docs/mockup/generated-2026-10-24/05-hiring.png: title, a grid of candidate
// cards (CandidateCard prefab), footnote and "К задачам". Also writes the candidates' quirk lines into Level0.asset.
// Replaces the old three-column layout; the night line of the mockup is left out (night shift is cut from the MVP).
public static class HireKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string CardPath = "Assets/_Project/Prefabs/UI/Mvp/CandidateCard.prefab";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    static readonly Color Cream = Palette.Hex("EFF2DF"), Olive = Palette.Hex("718733"), Brown = Palette.Hex("5B4A3A");

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // top-left anchored box
    static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = Node(name, parent);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static Image Img(RectTransform r, Sprite s, Color c, float mult = 1.5f, bool raycast = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (s != null && s.border != Vector4.zero) { i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = mult; }
        return i;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.TopLeft)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;   // fills its box
        return t;
    }

    static Button Flat(RectTransform r, Color color, Color? outline = null)
    {
        var img = Img(r, Spr("Assets/_Project/Art/UI/rounded.png"), color, 1f, true);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.ColorTint;
        var cb = b.colors; cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1); cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1); cb.disabledColor = new Color(0.66f, 0.66f, 0.66f, 1); b.colors = cb;
        if (outline.HasValue) { var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = outline.Value; ol.effectDistance = new Vector2(3, -3); }
        return b;
    }

    static void Stretch(RectTransform r, float l = 0, float t = 0, float rr = 0, float b = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rr, -t);
    }

    [MenuItem("Tools/UI/Apply kit to headhunter")]
    public static void Apply()
    {
        Quirks();
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var card = BuildCard(heading, body);

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var win = (RectTransform)root.transform.Find("Body/HireWindow");
            var hw = win.GetComponent<HireWindow>();
            for (int i = win.childCount - 1; i >= 0; i--) Object.DestroyImmediate(win.GetChild(i).gameObject);
            foreach (var im in win.GetComponents<Image>()) Object.DestroyImmediate(im);

            Img(win, Spr("Panels/panels/workspace-ivory"), Color.white);
            Txt(Box("Title", win, 40, 26, 1200, 76), "Собери свою команду", 58, Palette.Ink, heading);
            Txt(Box("Subtitle", win, 44, 100, 1200, 36), "Скорость, качество и отдых влияют на результат.", 24, Brown, body);

            var grid = Box("Grid", win, 36, 150, 1812, 680);
            var g = grid.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = new Vector2(580, 320); g.spacing = new Vector2(26, 26); g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = 3;
            g.startCorner = GridLayoutGroup.Corner.UpperLeft; g.childAlignment = TextAnchor.UpperLeft;

            var foot = Node("Footer", win); foot.anchorMin = new Vector2(0, 0); foot.anchorMax = new Vector2(1, 0); foot.pivot = new Vector2(0.5f, 0);
            foot.offsetMin = new Vector2(40, 18); foot.offsetMax = new Vector2(-40, 90);
            var info = Node("Info", foot); info.anchorMin = new Vector2(0, 0.5f); info.anchorMax = new Vector2(0, 0.5f); info.pivot = new Vector2(0, 0.5f); info.sizeDelta = new Vector2(40, 40); info.anchoredPosition = Vector2.zero;
            var ii = info.gameObject.AddComponent<Image>(); ii.sprite = Spr("Icons/icons-actions/info"); ii.preserveAspect = true; ii.raycastTarget = false;
            var note = Txt(foot, "Оплата – за каждый рабочий день. Уволенному деньги за сегодня не возвращаются.", 22, Brown, body, TextAlignmentOptions.MidlineLeft);
            note.rectTransform.anchorMin = Vector2.zero; note.rectTransform.anchorMax = Vector2.one; note.rectTransform.offsetMin = new Vector2(56, 0); note.rectTransform.offsetMax = new Vector2(-340, 0);
            var go = Node("ToTasks", foot); go.anchorMin = go.anchorMax = new Vector2(1, 0.5f); go.pivot = new Vector2(1, 0.5f); go.sizeDelta = new Vector2(320, 68); go.anchoredPosition = Vector2.zero;
            hw.toTasks = Flat(go, Palette.Primary, Palette.Hex("B89A1E"));
            var gl = Txt(go, "К задачам", 28, Palette.Ink, heading, TextAlignmentOptions.Center); Stretch(gl.rectTransform, 8, 4, 8, 4);

            hw.grid = grid; hw.cardPrefab = card;
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Headhunter rebuilt from the kit.");
    }

    static void Quirks()
    {
        var level = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
        var q = new System.Collections.Generic.Dictionary<string, string>
        {
            { "lera", "Рисую миры, в которых хочется остаться." },
            { "vika", "Люблю яркие цвета и чёткий силуэт." },
            { "kostya", "Сложные задачи требуют больше сил." },
            { "dima", "Медленно, зато почти без багов." },
            { "anya", "Слушаю, как звучит идея." },
            { "gosha", "Звук недорого, без лишних изысков." },
        };
        foreach (var c in level.candidates) if (q.TryGetValue(c.id, out var t)) c.quirk = t;
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
    }

    static CandidateCardView BuildCard(TMP_FontAsset heading, TMP_FontAsset body)
    {
        var r = Node("CandidateCard", null); r.sizeDelta = new Vector2(580, 320);
        var v = r.gameObject.AddComponent<CandidateCardView>();
        Img(r, Spr("Assets/_Project/Art/UI/rounded.png"), Palette.Hex("FBF8EA"), 1f);
        var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = Palette.Hex("CFCBA6"); ol.effectDistance = new Vector2(2, -2);

        var face = Box("Portrait", r, 22, 22, 120, 120);
        v.portrait = Img(face, null, Palette.Hex("2A1810")); v.portrait.preserveAspect = true;
        v.title = Txt(Box("Name", r, 158, 22, 400, 46), "Имя", 38, Palette.Ink, heading);
        v.role = Txt(Box("Role", r, 158, 72, 400, 30), "Профессия", 23, Olive, heading);
        v.quirk = Txt(Box("Quirk", r, 158, 106, 404, 60), "", 20, Brown, body);

        v.speedDots = Dots(r, "Скорость", 180, body);
        v.qualityDots = Dots(r, "Качество", 214, body);
        var coin = Box("Coin", r, 330, 184, 40, 40);
        var ci = Img(coin, Spr("Icons/icons-navigation/coins"), Color.white); ci.preserveAspect = true;
        v.rate = Txt(Box("Rate", r, 376, 186, 196, 40), "5 000 ₽ / день", 20, Palette.Ink, heading, TextAlignmentOptions.MidlineLeft);
        v.rate.textWrappingMode = TextWrappingModes.NoWrap;
        v.rate.enableAutoSizing = true; v.rate.fontSizeMin = 14; v.rate.fontSizeMax = 20;

        var row = Box("Buttons", r, 22, 250, 536, 58);
        var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 12; h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = true;
        var hire = Node("Hire", row);
        v.hire = Flat(hire, Palette.Primary, Palette.Hex("B89A1E"));
        hire.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        v.hireLabel = Txt(hire, "Нанять", 28, Palette.Ink, heading, TextAlignmentOptions.Center); Stretch(v.hireLabel.rectTransform, 8, 2, 8, 2);
        var fire = Node("Fire", row);
        v.fire = Flat(fire, Palette.Hex("E7E3CC"), Palette.Hex("CFCBA6"));
        fire.gameObject.AddComponent<LayoutElement>().preferredWidth = 150;
        var fl = Txt(fire, "Уволить", 22, Palette.Hex("8A2F2F"), heading, TextAlignmentOptions.Center); Stretch(fl.rectTransform, 6, 2, 6, 2);
        fire.gameObject.SetActive(false);

        var prefab = PrefabUtility.SaveAsPrefabAsset(r.gameObject, CardPath);
        Object.DestroyImmediate(r.gameObject);
        return prefab.GetComponent<CandidateCardView>();
    }

    // label + three round dots
    static Image[] Dots(RectTransform card, string label, float y, TMP_FontAsset font)
    {
        Txt(Box(label, card, 22, y, 130, 30), label, 22, Palette.Ink, font, TextAlignmentOptions.MidlineLeft);
        var dots = new Image[3];
        var circle = Spr("Assets/_Project/Art/UI/circle.png");
        for (int i = 0; i < 3; i++)
            dots[i] = Img(Box("Dot" + i, card, 162 + i * 40, y + 1, 28, 28), circle, Palette.Hex("D9D6C3"));
        return dots;
    }
}
