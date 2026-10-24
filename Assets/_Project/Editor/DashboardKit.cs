using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-4 (board): restyles DashboardWindow after docs/mockup/generated-2026-10-24/02-concept-board.png: dotted ivory board,
// olive "Блоки" column with icons, kit stickers as blocks, hint and approve button along the bottom, compact totals chip.
// Keeps every DashboardWindow reference. Safe to rerun; the prefab is edited by hand afterwards.
public static class DashboardKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string BlockPath = "Assets/_Project/Prefabs/UI/Mvp/Block.prefab";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    static readonly Color Cream = Palette.Hex("EFF2DF"), Light = Palette.Hex("D8E2A6");

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static Image Img(RectTransform r, Sprite s, Color c, float mult = 1.5f, bool raycast = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (s != null && s.border != Vector4.zero) { i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = mult; }
        return i;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.Left)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
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

    // anchored to a corner: x, y are offsets from that corner (positive inwards)
    static RectTransform At(string name, Transform parent, Vector2 anchor, float x, float y, float w, float h)
    {
        var r = Node(name, parent);
        r.anchorMin = r.anchorMax = anchor; r.pivot = anchor;
        r.anchoredPosition = new Vector2(anchor.x == 0 ? x : -x, anchor.y == 0 ? y : -y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static void Stretch(RectTransform r, float l = 0, float t = 0, float rr = 0, float b = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rr, -t);
    }

    [MenuItem("Tools/UI/Apply kit to board")]
    public static void Apply()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        Sticker();

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var dash = (RectTransform)root.transform.Find("Body/DashboardWindow");
            var dw = dash.GetComponent<DashboardWindow>();
            foreach (var n in new[] { "Palette", "Side", "DotGrid", "Bottom", "TotalsChip" })
            {
                var old = dash.Find(n);
                if (old) Object.DestroyImmediate(old.gameObject);
            }

            // dotted board under everything
            var dots = Node("DotGrid", dash); Stretch(dots, 6, 6, 6, 6);
            var di = Img(dots, Spr("Backgrounds/backgrounds/ivory-dot-grid"), Color.white);
            dots.SetAsFirstSibling();

            // olive column with the six block types
            var pal = At("Palette", dash, new Vector2(0, 1), 18, 18, 276, 650);
            Img(pal, Spr("Panels/panels/sidebar-olive"), Color.white);
            var v = pal.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(18, 18, 22, 22); v.spacing = 10;
            v.childControlWidth = v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            var head = Txt(pal, "Блоки", 38, Cream, heading);
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 56;
            string[] icons = { "genre", "audience", "setting", "style", "mechanics", "phone" };
            string[] names = { "Жанр", "Аудитория", "Сеттинг", "Стиль", "Механики", "Платформа" };
            for (int i = 0; i < 6; i++)
            {
                var tile = Node("Type" + i, pal);
                dw.palette[i] = Flat(tile, Palette.Hex("55612C"), Palette.Hex("3E4820"));
                var h = tile.gameObject.AddComponent<HorizontalLayoutGroup>();
                h.padding = new RectOffset(12, 8, 6, 6); h.spacing = 10; h.childAlignment = TextAnchor.MiddleLeft;
                h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = false; h.childForceExpandHeight = false;
                var ic = Node("Icon", tile);
                var ii = ic.gameObject.AddComponent<Image>(); ii.sprite = Spr("Icons/icons-concept/" + icons[i]); ii.preserveAspect = true; ii.raycastTarget = false;
                var le = ic.gameObject.AddComponent<LayoutElement>(); le.preferredWidth = le.preferredHeight = 40;
                var lab = Txt(tile, names[i], 21, Cream, heading);
                lab.name = "Label"; lab.textWrappingMode = TextWrappingModes.NoWrap;
                tile.gameObject.AddComponent<LayoutElement>().preferredHeight = 76;
            }

            // board area: from the column to the right edge
            var cv = (RectTransform)dash.Find("Canvas");
            cv.offsetMin = new Vector2(312, 20); cv.offsetMax = new Vector2(-20, -20);

            // bottom strip over the board: link mode + hint on the left, approve + note on the right
            var bottom = Node("Bottom", dash); bottom.anchorMin = new Vector2(0, 0); bottom.anchorMax = new Vector2(1, 0);
            bottom.pivot = new Vector2(0.5f, 0); bottom.offsetMin = new Vector2(312, 24); bottom.offsetMax = new Vector2(-30, 124);
            var mode = At("LinkMode", bottom, new Vector2(0, 0), 0, 18, 280, 64);
            dw.linkMode = Flat(mode, Palette.Hex("EFF2DF"), Palette.Hex("B9C08B"));
            dw.mode = Txt(mode, "Связать блоки", 22, Palette.Ink, heading, TextAlignmentOptions.Center);
            Stretch(dw.mode.rectTransform, 8, 4, 8, 4);
            var bulb = At("Bulb", bottom, new Vector2(0, 0), 300, 24, 52, 52);
            var bi = bulb.gameObject.AddComponent<Image>(); bi.sprite = Spr("Decor/decor/lightbulb"); bi.preserveAspect = true; bi.raycastTarget = false;
            dw.reason = Txt(bottom, "", 22, Palette.Hex("5B4A3A"), body, TextAlignmentOptions.MidlineLeft);
            var rr = dw.reason.rectTransform; rr.anchorMin = new Vector2(0, 0); rr.anchorMax = new Vector2(1, 0); rr.pivot = new Vector2(0, 0);
            rr.offsetMin = new Vector2(362, 18); rr.offsetMax = new Vector2(-440, 82);
            var ap = At("Approve", bottom, new Vector2(1, 0), 0, 30, 400, 76);
            dw.approve = Flat(ap, Palette.Primary, Palette.Hex("B89A1E"));
            var al = Txt(ap, "Утвердить концепт", 30, Palette.Ink, heading, TextAlignmentOptions.Center); Stretch(al.rectTransform, 10, 4, 10, 4);
            var note = Txt(bottom, "Создаст задачи для команды", 20, Palette.Hex("5B4A3A"), body, TextAlignmentOptions.MidlineRight);
            var nr = note.rectTransform; nr.anchorMin = nr.anchorMax = new Vector2(1, 0); nr.pivot = new Vector2(1, 0); nr.anchoredPosition = new Vector2(0, 0); nr.sizeDelta = new Vector2(400, 28);

            // totals: one compact chip in the top right corner
            var chip = At("TotalsChip", dash, new Vector2(1, 1), 30, 22, 640, 56);
            Img(chip, Spr("Assets/_Project/Art/UI/rounded.png"), Palette.Hex("55612C"), 1f);
            dw.totals = Txt(chip, "Итог концепта", 22, Cream, heading, TextAlignmentOptions.Center);
            Stretch(dw.totals.rectTransform, 12, 2, 12, 2);

            // popup, tag layer and tags stay on top
            ((RectTransform)dash.Find("Popup")).SetAsLastSibling();
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Block();
        Debug.Log("Board restyled from the kit.");
    }

    // The stickers are not part of the sheet import rules: make them plain sprites.
    static void Sticker()
    {
        foreach (var n in new[] { "sticker-two-side-ports", "sticker-four-edge-ports", "sticker-double-side-ports" })
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(Kit + "Stickers/" + n + ".png");
            if (ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single) continue;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.filterMode = FilterMode.Point; ti.mipmapEnabled = false; ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 512;
            ti.SaveAndReimport();
        }
    }

    static void Block()
    {
        var root = PrefabUtility.LoadPrefabContents(BlockPath);
        try
        {
            var bv = root.GetComponent<BlockView>();
            bv.background.sprite = Spr("Stickers/sticker-two-side-ports");
            bv.background.type = Image.Type.Simple; bv.background.preserveAspect = true;
            ((RectTransform)root.transform).sizeDelta = new Vector2(270, 270);
            var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
            bv.type.font = heading; bv.type.fontSize = 22; bv.type.color = Palette.Hex("7A5A1E"); bv.type.alignment = TextAlignmentOptions.Center;
            bv.value.font = heading; bv.value.color = Palette.Ink;
            var tr = bv.type.rectTransform; tr.offsetMin = new Vector2(46, 150); tr.offsetMax = new Vector2(-46, -70);
            var vr = bv.value.rectTransform; vr.offsetMin = new Vector2(44, 56); vr.offsetMax = new Vector2(-44, -104);
            PrefabUtility.SaveAsPrefabAsset(root, BlockPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
