using Studio.UI;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-3: rebuilds Prefabs/UI/Menu/MenuScreen from the kit after docs/mockup/generated-2026-10-24/01-menu.png.
// Replaces everything inside the prefab and rewires MenuController. After the first run the prefab is edited by hand.
public static class MenuKit
{
    const string Path = "Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    static readonly Color Cream = Palette.Hex("EFF2DF"), Olive = Palette.Hex("718733"), Dark = Palette.Hex("210D08"), Ink = Palette.Ink;

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // top-left anchored box: x, y measured from the top-left corner of the 1920x1080 screen
    static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = Node(name, parent);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

    static Image Img(RectTransform r, Sprite s, Color c, float mult = 1.5f, bool raycast = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (s != null && s.border != Vector4.zero) { i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = mult; }
        return i;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.TopLeft, float spacing = 0)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false; t.characterSpacing = spacing;
        return t;
    }

    static Button Flat(RectTransform r, Color color, Color? outline = null)
    {
        var img = Img(r, Spr("Assets/_Project/Art/UI/rounded.png"), color, 1f, true);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors; cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1); cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1); cb.disabledColor = new Color(1, 1, 1, 0.45f); b.colors = cb;
        if (outline.HasValue) { var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = outline.Value; ol.effectDistance = new Vector2(3, -3); }
        return b;
    }

    static Image Icon(Transform parent, string sprite, float x, float y, float size)
    {
        var r = Box("Icon", parent, x, y, size, size);
        var i = Img(r, Spr(sprite), Color.white);
        i.preserveAspect = true;
        return i;
    }

    [MenuItem("Tools/UI/Build menu from kit")]
    public static void Build()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        const string heroPath = "Assets/_Project/Art/UI/menu-hero.png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(heroPath);
        if (ti.textureType != TextureImporterType.Sprite || ti.spriteImportMode != SpriteImportMode.Single)
        {
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true; ti.mipmapEnabled = false; ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }
        var root = PrefabUtility.LoadPrefabContents(Path);
        try
        {
            foreach (var c in root.GetComponents<Component>())
                if (c is MenuController || c is Image) Object.DestroyImmediate(c);   // rebuilt below; Transform and Canvas parts stay
            for (int i = root.transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var mc = root.AddComponent<MenuController>();

            // background: burgundy pixels, studio picture on the right, fading into the background on its left edge
            Img(Node("Bg", root.transform), Spr("Backgrounds/backgrounds/burgundy-pixels"), Color.white).rectTransform.Fill();
            var hero = Node("Hero", root.transform);
            hero.anchorMin = new Vector2(1, 0); hero.anchorMax = new Vector2(1, 1); hero.pivot = new Vector2(1, 0.5f);
            hero.sizeDelta = new Vector2(1035, 0); hero.anchoredPosition = Vector2.zero;
            Img(hero, Spr("Assets/_Project/Art/UI/menu-hero.png"), Color.white);

            // title plate
            var plate = Box("Title", root.transform, 70, 50, 880, 250);
            Img(plate, Spr("Panels/panels/title-olive"), Color.white, 1.2f);
            var tt = Txt(plate, "РЕЛИЗ!", 80, Cream, heading, TextAlignmentOptions.MidlineLeft);
            tt.textWrappingMode = TextWrappingModes.NoWrap; tt.enableAutoSizing = true; tt.fontSizeMin = 40; tt.fontSizeMax = 80;
            var tr = tt.rectTransform; tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = new Vector2(56, 70); tr.offsetMax = new Vector2(-180, -34);
            var st = Txt(plate, "СИМУЛЯТОР ГЕЙМДИЗАЙНЕРА", 26, new Color32(0xD8, 0xE2, 0xA6, 255), body, TextAlignmentOptions.MidlineLeft, 8);
            var sr = st.rectTransform; sr.anchorMin = Vector2.zero; sr.anchorMax = new Vector2(1, 0); sr.pivot = new Vector2(0, 0); sr.offsetMin = new Vector2(64, 26); sr.offsetMax = new Vector2(-100, 70);
            var lp = Box("LogoPlate", plate, 690, 25, 160, 160);
            var logo = Icon(lp, "Assets/_Project/Art/UI/Mvp/logo.png", 5, 5, 150);
            logo.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/UI/LogoIvory.mat");

            // pitch
            Txt(Box("Pitch", root.transform, 80, 330, 900, 150).transform, "Первая работа.\nПервый релиз.", 66, Cream, heading).rectTransform.Fill();
            var sub = Txt(Box("Subline", root.transform, 80, 500, 900, 70).transform, "Собери концепт, найми команду,\nвыпусти игру.", 28, new Color32(0xD8, 0xE2, 0xA6, 255), body);
            sub.rectTransform.Fill();

            // level card
            var card = Box("LevelCard", root.transform, 70, 590, 880, 190);
            Img(card, Spr("Panels/panels/workspace-ivory"), Color.white, 1.2f);
            Icon(card, Kit + "Decor/decor/planet.png", 36, 34, 124);
            Txt(Box("L0", card, 190, 26, 400, 56), "Уровень 0", 46, Ink, heading).rectTransform.Fill();
            Txt(Box("Name", card, 190, 80, 400, 44), "Обучение", 32, Olive, heading).rectTransform.Fill();
            Txt(Box("Desc", card, 190, 124, 640, 40), "Твой первый проект в студии.", 26, Palette.Hex("5B4A3A"), body).rectTransform.Fill();
            

            // nickname
            Txt(Box("NameLabel", root.transform, 80, 800, 500, 34), "Как тебя зовут?", 26, new Color32(0xD8, 0xE2, 0xA6, 255), body).rectTransform.Fill();
            var field = Box("NameField", root.transform, 70, 840, 880, 72);
            Img(field, Spr("Assets/_Project/Art/UI/rounded.png"), Cream, 1f, true);
            var area = Node("TextArea", field); area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one; area.offsetMin = new Vector2(26, 6); area.offsetMax = new Vector2(-90, -6);
            area.gameObject.AddComponent<RectMask2D>();
            var ph = Txt(area, "Саша", 30, new Color(0.2f, 0.1f, 0.05f, 0.4f), body, TextAlignmentOptions.MidlineLeft); ph.rectTransform.Fill(); ph.fontStyle = FontStyles.Italic; ph.name = "Placeholder";
            var tx = Txt(area, "", 30, Ink, body, TextAlignmentOptions.MidlineLeft); tx.rectTransform.Fill(); tx.name = "Text";
            var input = field.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = area; input.textComponent = tx; input.placeholder = ph; input.targetGraphic = field.GetComponent<Image>();
            mc.nameInput = input;

            // buttons
            var start = Box("StartButton", root.transform, 70, 934, 440, 84);
            mc.startButton = Flat(start, Palette.Primary, Palette.Hex("B89A1E"));
            Icon(start, Kit + "Icons/icons-navigation/play.png", 22, 26, 32).raycastTarget = false;
            var sl = Txt(start, "Начать смену", 36, Ink, heading, TextAlignmentOptions.Center); sl.rectTransform.Fill();
            var cont = Box("ContinueButton", root.transform, 540, 934, 410, 84);
            mc.continueButton = Flat(cont, Palette.Hex("2A1810"), Olive);
            mc.continueLabel = Txt(cont, "Продолжить", 30, new Color32(0xD8, 0xE2, 0xA6, 255), body, TextAlignmentOptions.Center);
            mc.continueLabel.rectTransform.Fill();

            // footnote
            Icon(root.transform, Kit + "Icons/icons-navigation/save.png", 76, 1030, 34);
            Txt(Box("Footnote", root.transform, 124, 1032, 700, 30), "Прогресс сохраняется автоматически", 22, new Color32(0xA9, 0xA0, 0x90, 255), body).rectTransform.Fill();

            // "start over" confirmation, shown only when a save exists
            var conf = Node("Confirm", root.transform); conf.gameObject.SetActive(false); Stretch(conf);
            Img(conf, null, new Color(0.08f, 0.04f, 0, 0.75f), 1f, true);
            var panel = Node("Panel", conf); panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(760, 380);
            Img(panel, Spr("Panels/panels/modal-ivory"), Color.white, 1.5f, true);
            var q = Txt(Box("Q", panel, 60, 60, 640, 60), "Начать заново?", 44, Ink, heading, TextAlignmentOptions.Center); q.rectTransform.Fill();
            var qb = Txt(Box("Body", panel, 60, 130, 640, 90), "Сохранённая игра будет удалена.", 26, Ink, body, TextAlignmentOptions.Center); qb.rectTransform.Fill();
            var yes = Box("Yes", panel, 60, 250, 310, 76);
            mc.confirmYes = Flat(yes, Palette.Primary);
            var yl = Txt(yes, "Начать заново", 28, Ink, heading, TextAlignmentOptions.Center); yl.rectTransform.Fill();
            var no = Box("No", panel, 390, 250, 310, 76);
            mc.confirmNo = Flat(no, Palette.Secondary);
            var nl = Txt(no, "Отмена", 28, Ink, heading, TextAlignmentOptions.Center); nl.rectTransform.Fill();
            mc.confirm = conf.gameObject;

            PrefabUtility.SaveAsPrefabAsset(root, Path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("MenuScreen rebuilt from the kit.");
    }

    [MenuItem("Tools/UI/Center start button content")]
    public static void CenterStartButtonContent()
    {
        var root = PrefabUtility.LoadPrefabContents(Path);
        try
        {
            var start = root.transform.Find("StartButton");
            var label = start != null ? start.Find("Text")?.GetComponent<TMP_Text>() : null;
            if (label == null) throw new System.InvalidOperationException("Start button label was not found.");

            label.rectTransform.Fill();
            var icon = start.Find("Icon") as RectTransform;
            if (icon != null)
            {
                icon.anchorMin = icon.anchorMax = new Vector2(0, 1);
                icon.pivot = new Vector2(0, 1);
                icon.anchoredPosition = new Vector2(22, -26);
                icon.sizeDelta = new Vector2(32, 32);
            }

            PrefabUtility.SaveAsPrefabAsset(root, Path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Start button label centered.");
    }

    static void Fill(this RectTransform r) => Stretch(r);
}
