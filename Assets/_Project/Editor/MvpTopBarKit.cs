using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-2: rebuilds the top bar of MvpScreen from the kit (mockups 02-05): fox logo, four icon tabs, calendar/clock/coins
// capsule, pause and end-day buttons. Replaces Tabs, Info, EndDay, Pause, Hint; keeps the TopBarMvp references.
// Safe to rerun. After the first run the prefab is edited by hand.
public static class MvpTopBarKit
{
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    const float H = 64;   // one height for every plate of the bar
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";

    static Sprite Spr(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path.StartsWith("Assets") ? path : Kit + path + ".png");
    static GameObject Pre(string n) => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/Kit/" + n + ".prefab");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform r, float pad = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(pad, pad); r.offsetMax = new Vector2(-pad, -pad);
    }

    static LayoutElement Le(Component c, float w = -1, float h = -1, float flexW = -1)
    {
        var le = c.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = w;
        if (h >= 0) le.preferredHeight = h;
        if (flexW >= 0) le.flexibleWidth = flexW;
        return le;
    }

    static HorizontalLayoutGroup Row(RectTransform r, float spacing, int l = 0, int rt = 0, TextAnchor a = TextAnchor.MiddleLeft)
    {
        var h = r.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing; h.padding = new RectOffset(l, rt, 0, 0); h.childAlignment = a;
        h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = h.childForceExpandHeight = false;
        return h;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.Left)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }

    static Image Icon(Transform parent, string spritePath, float size)
    {
        var r = Node("Icon", parent);
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = Spr(spritePath); i.preserveAspect = true; i.raycastTarget = false;
        Le(i, size, size);
        return i;
    }

    [MenuItem("Tools/UI/Apply kit to top bar")]
    public static void Apply()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var top = root.GetComponent<TopBarMvp>();
            foreach (var n in new[] { "Tabs", "Info", "EndDay", "Pause", "Hint", "TopBar" })
            {
                var old = root.transform.Find(n);
                if (old) Object.DestroyImmediate(old.gameObject);
            }

            root.GetComponent<Image>().sprite = Spr("Backgrounds/backgrounds/burgundy-pixels");

            var bar = Node("TopBar", root.transform);
            bar.anchorMin = new Vector2(0, 1); bar.anchorMax = Vector2.one; bar.pivot = new Vector2(0.5f, 1);
            bar.offsetMin = new Vector2(16, -92); bar.offsetMax = new Vector2(-16, -8);
            Row(bar, 18);

            // logo and title
            var logo = Node("Logo", bar);
            Row(logo, 10);
            var li = Node("LogoImage", logo);
            var lim = li.gameObject.AddComponent<Image>(); lim.sprite = Spr("Assets/_Project/Art/UI/Mvp/logo.png"); lim.preserveAspect = true; lim.raycastTarget = false;
            lim.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/UI/LogoIvory.mat");
            Le(li, 80, 80);
            var title = Txt(logo, "РЕЛИЗ!", 24, new Color32(0xEF, 0xF2, 0xDF, 255), heading);
            Le(title, 110, 64);
            Le(logo, 205, H);

            // tabs: flat rounded plates (dark, yellow when selected), as in the mockups
            var tabs = Node("Tabs", bar);
            Row(tabs, 12);
            string[] names = { "Чат", "Доска", "Хедхантер", "Таск-лист" };
            string[] icons = { "chat", "dashboard", "hire", "tasks" };
            for (int i = 0; i < 4; i++)
            {
                var rt = Node("Tab" + i, tabs);
                top.tabs[i] = Flat(rt, Palette.TabOff);
                Row(rt, 6, 8, 8, TextAnchor.MiddleCenter);
                Icon(rt, "Icons/icons-navigation/" + icons[i], 26);
                var lab = Txt(rt, names[i], 22, new Color32(0xEF, 0xF2, 0xDF, 255), heading);
                lab.name = "Label";
                Le(rt, 196, H);
            }
            top.tabOnColor = Palette.Primary; top.tabOffColor = Palette.TabOff;
            var badge = (GameObject)PrefabUtility.InstantiatePrefab(Pre("Badge"), top.tabs[0].transform);
            badge.AddComponent<LayoutElement>().ignoreLayout = true;
            var br = (RectTransform)badge.transform;
            br.anchorMin = br.anchorMax = new Vector2(1, 1); br.pivot = new Vector2(0.5f, 0.5f); br.anchoredPosition = new Vector2(-14, -8);
            top.chatBadge = badge; top.chatBadgeText = badge.GetComponentInChildren<TMP_Text>();
            top.chatBadgeText.font = heading;   // the kit badge was built with the UI font, digits looked off

            var spacer = Node("Spacer", bar); Le(spacer, 0, 0, 1);

            // status capsule: day, part of the day, money
            var caps = Node("Status", bar);
            var ci = caps.gameObject.AddComponent<Image>();
            ci.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png"); ci.type = Image.Type.Sliced; ci.color = Palette.TabOff; ci.raycastTarget = false;
            var cout = caps.gameObject.AddComponent<Outline>(); cout.effectColor = new Color32(0x9A, 0x78, 0x55, 255); cout.effectDistance = new Vector2(2, -2);
            Row(caps, 10, 16, 16, TextAnchor.MiddleCenter);
            top.dayText = Chip(caps, "calendar", body, "День 1 / 3");
            top.phaseText = Chip(caps, "clock", body, "утро");
            top.phaseText.transform.parent.gameObject.SetActive(false);   // the strip under the capsule already shows the time of day
            top.budgetText = Chip(caps, "coins", body, "100 000");
            Le(caps, -1, H);
            var clock = Node("DayClock", caps);   // flat strip along the bottom of the capsule: share of the workday gone
            clock.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            clock.anchorMin = new Vector2(0, 0); clock.anchorMax = new Vector2(1, 0); clock.pivot = new Vector2(0.5f, 0);
            clock.offsetMin = new Vector2(24, 6); clock.offsetMax = new Vector2(-24, 10);
            var trackImg = clock.gameObject.AddComponent<Image>(); trackImg.color = new Color32(0x4A, 0x34, 0x25, 255); trackImg.raycastTarget = false;
            var fill = Node("Fill", clock); Stretch(fill);
            var fi = fill.gameObject.AddComponent<Image>(); fi.color = Palette.Primary; fi.raycastTarget = false;
            top.timeFill = fill;

            // pause (dark square like the tabs) and end day (ivory plate)
            var pause = Node("Pause", bar);
            top.pause = Flat(pause, Palette.TabOff);
            var pic = Icon(pause, "Icons/icons-navigation/pause", 30); pic.GetComponent<LayoutElement>().ignoreLayout = true;
            var pir = (RectTransform)pic.transform; pir.anchorMin = pir.anchorMax = new Vector2(0.5f, 0.5f); pir.sizeDelta = new Vector2(30, 30); pir.anchoredPosition = Vector2.zero;
            Le(pause, H, H);

            var end = Node("EndDay", bar);
            top.endDay = Flat(end, new Color32(0xEF, 0xF2, 0xDF, 255));
            Row(end, 8, 16, 16, TextAnchor.MiddleCenter);
            var el = Txt(end, "Закончить день", 21, Palette.Ink, heading); el.name = "Label";
            Le(end, 240, H);

            // hint line under the bar, right side
            var hint = Txt(root.transform, "", 22, new Color32(0xD8, 0xE2, 0xA6, 255), body, TextAlignmentOptions.Right);
            hint.name = "Hint";
            var hr = hint.rectTransform;
            hr.anchorMin = new Vector2(0, 1); hr.anchorMax = new Vector2(1, 1); hr.pivot = new Vector2(1, 1);
            hr.offsetMin = new Vector2(400, -124); hr.offsetMax = new Vector2(-30, -94);
            top.hint = hint;

            bar.SetSiblingIndex(0); hint.transform.SetSiblingIndex(1);   // behind the windows and overlays, which never overlap the bar
            var bodyNode = (RectTransform)root.transform.Find("Body");
            bodyNode.offsetMin = new Vector2(24, 24); bodyNode.offsetMax = new Vector2(-24, -132);
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("MvpScreen top bar rebuilt from the kit.");
    }

    // Flat rounded plate with a colour-tint button: the look of the mockup header (not the ticket-shaped kit tabs).
    static Button Flat(RectTransform r, Color color)
    {
        var img = r.gameObject.AddComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png"); img.type = Image.Type.Sliced; img.color = color;
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.ColorTint;
        var cb = b.colors; cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1); cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1); cb.disabledColor = new Color(0.66f, 0.66f, 0.66f, 1);   // dimmed, still opaque: translucent plates showed the bar through them
        b.colors = cb;
        var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = new Color32(0x9A, 0x78, 0x55, 255); ol.effectDistance = new Vector2(2, -2);   // visible edge on the dark bar
        return b;
    }

    // icon + text, text sized by the layout
    static TMP_Text Chip(RectTransform parent, string icon, TMP_FontAsset font, string text)
    {
        var r = Node("Chip_" + icon, parent);
        Row(r, 10, 0, 0, TextAnchor.MiddleCenter);
        Icon(r, "Icons/icons-navigation/" + icon, 28);
        var t = Txt(r, text, 22, new Color32(0xEF, 0xF2, 0xDF, 255), font);
        return t;
    }
}
