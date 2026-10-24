using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot generator of the kit prefabs (Prefabs/UI/Kit). After the first run the prefabs are edited by hand in Unity.
// Corner size on screen = sprite border / pixelsPerUnitMultiplier, so big kit borders (about 130 px) use a multiplier of 3-4.
public static class UiKitPrefabs
{
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    const string Dir = "Assets/_Project/Prefabs/UI/Kit";

    static Sprite Spr(string group, string sheet, string name) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(Kit + group + "/" + sheet + "/" + name + ".png");

    [MenuItem("Tools/UI/Build kit prefabs")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "Kit");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/UIFont SDF.asset");

        Save(Button("ButtonPrimary", "primary", Palette.Ink, font, 360, 84), "ButtonPrimary");
        Save(Button("ButtonSecondary", "secondary", Palette.Ink, font, 360, 84), "ButtonSecondary");
        Save(Button("ButtonOlive", "olive", Palette.Cream, font, 360, 84), "ButtonOlive");
        Save(Tab(font), "Tab");
        Save(PanelOf("PanelIvory", "workspace-ivory", 1.5f, 600, 400), "PanelIvory");
        Save(PanelOf("PanelOlive", "sidebar-olive", 1.5f, 400, 600), "PanelOlive");
        Save(PanelOf("PanelDark", "header-brown", 1.5f, 1000, 120), "PanelDark");
        Save(PanelOf("PanelModal", "modal-ivory", 1.5f, 800, 600), "PanelModal");
        Save(PanelOf("PanelTooltip", "tooltip-ivory", 1.5f, 400, 160), "PanelTooltip");
        Save(Chip(font), "Chip");
        Save(Badge(font), "Badge");
        Save(ProgressBar(), "ProgressBar");
        Save(ToggleOf(), "Toggle");
        AssetDatabase.SaveAssets();
        Debug.Log("Kit prefabs built in " + Dir);
    }

    static GameObject Save(GameObject go, string name)
    {
        var p = PrefabUtility.SaveAsPrefabAsset(go, Dir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return p;
    }

    static RectTransform Node(string name, Transform parent, float w = 0, float h = 0)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        if (w > 0) r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static void Stretch(RectTransform r, float pad = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(pad, pad); r.offsetMax = new Vector2(-pad, -pad);
    }

    static Image Img(RectTransform r, Sprite s, float multiplier, bool raycast = true)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.raycastTarget = raycast;
        i.type = s != null && s.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
        i.pixelsPerUnitMultiplier = multiplier;
        return i;
    }

    static TMP_Text Label(Transform parent, string text, float size, Color c, TMP_FontAsset font)
    {
        var r = Node("Label", parent);
        Stretch(r, 12);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
        return t;
    }

    static GameObject Button(string name, string family, Color text, TMP_FontAsset font, float w, float h)
    {
        var r = Node(name, null, w, h);
        var img = Img(r, Spr("Buttons", "buttons", family + "-normal"), 1.5f);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState
        {
            highlightedSprite = Spr("Buttons", "buttons", family + "-hover"),
            pressedSprite = Spr("Buttons", "buttons", family + "-pressed"),
            selectedSprite = Spr("Buttons", "buttons", family + "-hover"),
            disabledSprite = Spr("Buttons", "buttons", family + "-disabled"),
        };
        Label(r, "Кнопка", 32, text, font);
        return r.gameObject;
    }

    static GameObject Tab(TMP_FontAsset font)
    {
        var r = Node("Tab", null, 260, 76);
        var img = Img(r, Spr("Buttons", "buttons", "tab-normal"), 1.5f);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        b.transition = Selectable.Transition.SpriteSwap;
        b.spriteState = new SpriteState
        {
            highlightedSprite = Spr("Buttons", "buttons", "tab-hover"),
            pressedSprite = Spr("Buttons", "buttons", "tab-selected"),
            selectedSprite = Spr("Buttons", "buttons", "tab-selected"),
            disabledSprite = Spr("Buttons", "buttons", "tab-disabled"),
        };
        Label(r, "Вкладка", 28, Palette.Cream, font);
        return r.gameObject;
    }

    static GameObject PanelOf(string name, string sprite, float mult, float w, float h)
    {
        var r = Node(name, null, w, h);
        Img(r, Spr("Panels", "panels", sprite), mult);
        return r.gameObject;
    }

    static GameObject Chip(TMP_FontAsset font)
    {
        var r = Node("Chip", null, 170, 48);
        Img(r, Spr("Panels", "panels", "tooltip-ivory"), 4f);
        Label(r, "Тип", 24, Palette.Ink, font);
        return r.gameObject;
    }

    static GameObject Badge(TMP_FontAsset font)
    {
        var r = Node("Badge", null, 44, 44);
        Img(r, Spr("Controls", "controls", "notification-badge"), 1f, false);
        var t = Label(r, "1", 24, Color.white, font);
        t.rectTransform.offsetMin = Vector2.zero; t.rectTransform.offsetMax = Vector2.zero;
        return r.gameObject;
    }

    static GameObject ProgressBar()
    {
        var r = Node("ProgressBar", null, 320, 32);
        Img(Node("Track", r), Spr("Controls", "controls", "progress-track"), 1f, false);
        Stretch((RectTransform)r.Find("Track"));
        var fill = Node("Fill", r);
        Stretch(fill);
        var fi = Img(fill, Spr("Controls", "controls", "progress-green"), 1f, false);
        fi.type = Image.Type.Filled; fi.fillMethod = Image.FillMethod.Horizontal; fi.fillOrigin = 0; fi.fillAmount = 0.6f;
        return r.gameObject;
    }

    static GameObject ToggleOf()
    {
        var r = Node("Toggle", null, 120, 64);
        var bg = Node("Background", r);
        Stretch(bg);
        var bgi = Img(bg, Spr("Controls", "controls", "toggle-off"), 1f);
        var on = Node("On", bg);
        Stretch(on);
        var oni = Img(on, Spr("Controls", "controls", "toggle-on"), 1f, false);
        var t = r.gameObject.AddComponent<Toggle>();
        t.targetGraphic = bgi; t.graphic = oni; t.isOn = false;
        return r.gameObject;
    }
}
