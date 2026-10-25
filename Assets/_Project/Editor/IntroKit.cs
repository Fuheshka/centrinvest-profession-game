using Studio.UI;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Builds the first-visit dashboard tutorial under Body/DashboardWindow/Intro and links it to DashboardWindow.intro.
// Left: title, text, button. Right: ivory board with two stickers, a link button, a line and a cursor (animated by DashboardIntro).
// Safe to rerun; the objects are edited by hand afterwards.
public static class IntroKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    const string Rounded = "Assets/_Project/Art/UI/rounded.png";
    static readonly Color Cream = Palette.Hex("EFF2DF");

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");

    static RectTransform Node(string name, Transform parent, float x = 0, float y = 0, float w = 0, float h = 0)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = (RectTransform)go.transform;
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static Image Img(RectTransform r, Sprite s, Color c, bool sliced = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = false; i.preserveAspect = !sliced && s != null;
        if (sliced && s != null && s.border != Vector4.zero) i.type = Image.Type.Sliced;
        return i;
    }

    static TMP_Text Txt(RectTransform r, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al)
    {
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform r, float l = 0, float t = 0, float rr = 0, float b = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rr, -t);
    }

    static RectTransform Label(Transform parent, string text, float size, Color c, TMP_FontAsset font)
    {
        var r = Node("Label", parent); Stretch(r);
        Txt(r, text, size, c, font, TextAlignmentOptions.Center);
        return r;
    }

    static RectTransform Block(string name, Transform parent, float x, float y, string type, string value, TMP_FontAsset heading, out Image bg)
    {
        var r = Node(name, parent, x, y, 200, 200);
        bg = Img(r, Spr("Stickers/sticker-two-side-ports"), Color.white);
        Txt(Node("Type", r, 0, 34, 150, 40), type, 16, Palette.Hex("7A5A1E"), heading, TextAlignmentOptions.Center);
        Txt(Node("Value", r, 0, -10, 150, 60), value, 18, Palette.Ink, heading, TextAlignmentOptions.Center);
        return r;
    }

    [MenuItem("Tools/UI/Build dashboard intro")]
    public static void Build()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        foreach (var path in new[] { "Assets/_Project/Art/UI/cursor.png", Kit + "Stickers/sticker-two-side-ports.png" })
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti == null || ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single) continue;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; ti.filterMode = FilterMode.Point;
            ti.mipmapEnabled = false; ti.alphaIsTransparency = true; ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var dashT = root.transform.Find("Body/DashboardWindow");
            var dw = dashT.GetComponent<DashboardWindow>();
            var old = dashT.Find("Intro");
            if (old) Object.DestroyImmediate(old.gameObject);

            // child of the dashboard: the dim covers only it, the top bar stays visible
            var rt = Node("Intro", dashT); Stretch(rt);
            var dim = Img(rt, null, new Color(0.08f, 0.04f, 0, 0.7f)); dim.raycastTarget = true;
            var panel = Node("Panel", rt, 0, 0, 1500, 660);
            Img(panel, Spr("Assets/_Project/Art/UI/menu-title-plate.png"), Color.white, true);

            var title = Txt(Node("Title", panel, -380, 235, 560, 60), "Как собрать концепт", 36, Cream, heading, TextAlignmentOptions.TopLeft);
            title.fontStyle = FontStyles.Bold; title.textWrappingMode = TextWrappingModes.NoWrap;
            Txt(Node("Text", panel, -380, 20, 560, 380),
                "Блоки и связи между ними показывают, как части игры зависят друг от друга.\n\nСоединяй блоки и ищи сочетания, где одно естественно следует из другого: такая связь и есть удачная идея.",
                28, Cream, body, TextAlignmentOptions.TopLeft);
            var okR = Node("Ok", panel, -380, -250, 380, 72);
            var okImg = Img(okR, Spr(Rounded), Palette.Primary, true); okImg.raycastTarget = true;
            var ok = okR.gameObject.AddComponent<Button>(); ok.targetGraphic = okImg;
            okR.gameObject.AddComponent<SfxButton>().fx = Fx.Confirm;
            Label(okR, "Понятно", 30, Palette.Ink, heading);

            // demo board, clipped
            var demo = Node("Demo", panel, 300, 30, 700, 460);
            Img(demo, Spr("Backgrounds/backgrounds/ivory-dot-grid"), Color.white, true);
            demo.gameObject.AddComponent<RectMask2D>();
            var content = Node("Content", demo); Stretch(content);

            var line = Node("Line", content, 0, 40, 200, 14);
            Img(line, null, Palette.Green);
            var a = Block("BlockA", content, -200, 40, "Жанр", "Пазл", heading, out var aImg);
            var b = Block("BlockB", content, 200, 40, "Аудитория", "Подростки", heading, out _);
            var tag = Node("Tag", content, 0, 74, 130, 38);
            Img(tag, Spr(Rounded), Palette.Hex("2A1F14"), true);
            Label(tag, "<color=#7BE0A4>Д+1</color> <color=#FF8F8F>З–1</color>", 22, Cream, heading);

            var lb = Node("LinkButton", content, -170, -130, 280, 64);
            var lbImg = Img(lb, Spr(Rounded), Palette.Primary, true);
            Label(lb, "Связать блоки", 22, Palette.Ink, heading);

            var cursor = Node("Cursor", content, 0, -180, 48, 68);
            cursor.pivot = new Vector2(0.1f, 0.95f);   // the arrow tip is the hotspot
            Img(cursor, Spr("Assets/_Project/Art/UI/cursor.png"), Color.white);

            var cap = Txt(Node("Caption", panel, 300, -250, 700, 50), "", 28, Cream, heading, TextAlignmentOptions.Center);

            var intro = rt.gameObject.AddComponent<DashboardIntro>();
            intro.root = rt.gameObject; intro.ok = ok; intro.cursor = cursor; intro.blockA = a; intro.blockB = b;
            intro.linkButton = lb; intro.line = line; intro.tag = tag; intro.linkButtonImage = lbImg; intro.blockAImage = aImg; intro.caption = cap;
            dw.intro = intro;
            rt.SetAsLastSibling();
            rt.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Dashboard intro built.");
    }
}
