using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot (P-33): flat progress bars, text that fits (tabs, day plate, notes), dropdown popup for block parameters,
// customer review screen. Safe to rerun; run Tools/Mvp/Apply fonts P-31 afterwards.
public static class MvpPatch3
{
    const string Dir = "Assets/_Project/Prefabs/UI/Mvp/";
    static Sprite Spr(string n) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/" + n + ".png");

    [MenuItem("Tools/Mvp/Patch P-33")]
    public static void Run()
    {
        FlatBar(Dir + "RowOlive.prefab");
        var popupItem = MakePopupItem();
        var root = PrefabUtility.LoadPrefabContents(Dir + "MvpScreen.prefab");
        var top = root.GetComponent<TopBarMvp>();
        var game = root.GetComponent<GameLoop>();

        // flat time bar
        foreach (var im in top.timeFill.parent.GetComponentsInChildren<Image>(true)) { im.sprite = null; im.type = Image.Type.Simple; }

        // text that fits
        foreach (var tab in top.tabs) Fit(tab.GetComponentInChildren<TMP_Text>(), 16, 26, true);
        Fit(top.dayText, 18, 28, false); Fit(top.budgetText, 18, 28, false);
        foreach (var b in root.GetComponentsInChildren<BlockView>(true)) { }

        var dw = root.GetComponentInChildren<DashboardWindow>(true);
        if (dw.popup == null) dw.popup = MakePopup(dw, popupItem);
        if (game.review == null) game.review = MakeReview(root.transform, game, FindOption(root));

        PrefabUtility.SaveAsPrefabAsset(root, Dir + "MvpScreen.prefab");
        PrefabUtility.UnloadPrefabContents(root);

        var bp = PrefabUtility.LoadPrefabContents(Dir + "Block.prefab");
        var bv = bp.GetComponent<BlockView>();
        Fit(bv.value, 12, 28, false);
        Fit(bv.type, 12, 22, true);
        PrefabUtility.SaveAsPrefabAsset(bp, Dir + "Block.prefab");
        PrefabUtility.UnloadPrefabContents(bp);
        Debug.Log("MVP patch P-33 done");
    }

    static void Fit(TMP_Text t, float min, float max, bool noWrap)
    {
        t.enableAutoSizing = true; t.fontSizeMin = min; t.fontSizeMax = max;
        if (noWrap) t.textWrappingMode = TextWrappingModes.NoWrap;
    }

    static void FlatBar(string path)
    {
        var r = PrefabUtility.LoadPrefabContents(path);
        foreach (var im in r.GetComponent<RowView>().progressBar.GetComponentsInChildren<Image>(true)) { im.sprite = null; im.type = Image.Type.Simple; }
        PrefabUtility.SaveAsPrefabAsset(r, path);
        PrefabUtility.UnloadPrefabContents(r);
    }

    static Button FindOption(GameObject root) => root.GetComponent<ModalView>().optionPrefab;

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

    static TMP_Text Txt(Transform parent, string name, string text, float size, Color c, TextAlignmentOptions a)
    {
        var r = Node(name, parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = c; t.alignment = a; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    static Button MakePopupItem()
    {
        const string path = Dir + "PopupItem.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing) return existing.GetComponent<Button>();
        var r = Node("PopupItem", null);
        var img = r.gameObject.AddComponent<Image>(); img.sprite = Spr("rounded"); img.type = Image.Type.Sliced; img.color = Palette.Cream;
        var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = img;
        var le = r.gameObject.AddComponent<LayoutElement>(); le.preferredHeight = 56; le.minHeight = 56; le.preferredWidth = 340;
        var t = Txt(r, "Label", "Вариант", 24, Palette.Ink, TextAlignmentOptions.Center);
        Stretch((RectTransform)t.transform);
        var p = PrefabUtility.SaveAsPrefabAsset(r.gameObject, path);
        Object.DestroyImmediate(r.gameObject);
        return p.GetComponent<Button>();
    }

    static PopupMenu MakePopup(DashboardWindow dw, Button item)
    {
        var dash = dw.transform;
        var root = Node("Popup", dash); Stretch(root);
        var pm = root.gameObject.AddComponent<PopupMenu>();
        pm.root = root.gameObject;
        var back = Node("Backdrop", root); Stretch(back);
        var bi = back.gameObject.AddComponent<Image>(); bi.color = new Color(0.1f, 0.05f, 0, 0.25f);
        pm.backdrop = back.gameObject.AddComponent<Button>(); pm.backdrop.targetGraphic = bi; pm.backdrop.transition = Selectable.Transition.None;
        var panel = Node("Panel", root);
        panel.anchorMin = panel.anchorMax = new Vector2(0, 1); panel.pivot = new Vector2(0, 1);
        var pi = panel.gameObject.AddComponent<Image>(); pi.sprite = Spr("rounded"); pi.type = Image.Type.Sliced; pi.color = Palette.Olive;
        var sh = panel.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.45f); sh.effectDistance = new Vector2(6, -8);
        var vl = panel.gameObject.AddComponent<VerticalLayoutGroup>(); vl.spacing = 8; vl.padding = new RectOffset(14, 14, 14, 14);
        vl.childControlWidth = vl.childControlHeight = true; vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
        var fit = panel.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        pm.heading = Txt(panel, "Title", "Блок", 26, Palette.Cream, TextAlignmentOptions.Center);
        var items = Node("Items", panel);
        var il = items.gameObject.AddComponent<VerticalLayoutGroup>(); il.spacing = 8; il.childControlWidth = il.childControlHeight = true; il.childForceExpandWidth = true; il.childForceExpandHeight = false;
        pm.panel = panel; pm.items = items; pm.itemPrefab = item; pm.bounds = (RectTransform)dash;
        return pm;
    }

    static ReviewView MakeReview(Transform screen, GameLoop game, Button option)
    {
        var root = Node("ReviewScreen", screen); Stretch(root);
        var dim = root.gameObject.AddComponent<Image>(); dim.color = new Color(0.08f, 0.04f, 0, 0.75f);
        var rv = root.gameObject.AddComponent<ReviewView>();
        rv.root = root.gameObject; rv.optionPrefab = option;
        var panel = Node("Panel", root);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(1060, 700);
        var pimg = panel.gameObject.AddComponent<Image>(); pimg.sprite = Spr("menu-title-plate");
        var vl = panel.gameObject.AddComponent<VerticalLayoutGroup>(); vl.spacing = 10; vl.padding = new RectOffset(60, 60, 56, 50);
        vl.childControlWidth = vl.childControlHeight = true; vl.childForceExpandWidth = true; vl.childForceExpandHeight = false;
        panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        rv.title = Txt(panel, "Title", "Отзыв заказчика", 44, Palette.Cream, TextAlignmentOptions.Center);
        rv.subtitle = Txt(panel, "Subtitle", "Сборка 1", 24, Palette.Light, TextAlignmentOptions.Center);
        rv.score = Txt(panel, "Score", "0", 110, Palette.Cream, TextAlignmentOptions.Center);
        rv.score.gameObject.AddComponent<LayoutElement>().preferredHeight = 130;
        rv.score.name = "Title Score";   // headline number: Bold
        rv.need = Txt(panel, "Need", "нужно 70", 24, Palette.Light, TextAlignmentOptions.Center);
        for (int i = 0; i < 3; i++)
        {
            var row = Node("Criterion" + i, panel);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>(); h.spacing = 16; h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = false;
            rv.names[i] = Txt(row, "Name", "Дизайн", 26, Palette.Cream, TextAlignmentOptions.MidlineLeft);
            rv.names[i].gameObject.AddComponent<LayoutElement>().preferredWidth = 170;
            var bar = Node("Bar", row);
            var bl = bar.gameObject.AddComponent<LayoutElement>(); bl.flexibleWidth = 1; bl.preferredHeight = 20;
            var bg = bar.gameObject.AddComponent<Image>(); bg.color = new Color(0.12f, 0.1f, 0.02f, 0.5f); bg.raycastTarget = false;
            var fill = Node("Fill", bar); fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1); fill.offsetMin = fill.offsetMax = Vector2.zero;
            fill.gameObject.AddComponent<Image>().raycastTarget = false;
            rv.fills[i] = fill;
            rv.values[i] = Txt(row, "Value", "0", 26, Palette.Cream, TextAlignmentOptions.MidlineRight);
            rv.values[i].gameObject.AddComponent<LayoutElement>().preferredWidth = 250;
        }
        rv.remarks = Txt(panel, "Remarks", "", 24, Palette.Cream, TextAlignmentOptions.TopLeft);
        rv.remarks.gameObject.AddComponent<LayoutElement>().minHeight = 150;
        var opts = Node("Options", panel);
        var ol = opts.gameObject.AddComponent<VerticalLayoutGroup>(); ol.spacing = 10; ol.childControlWidth = ol.childControlHeight = true; ol.childForceExpandWidth = true; ol.childForceExpandHeight = false;
        rv.options = opts;
        root.SetAsLastSibling();
        game.review = rv;
        return rv;
    }
}
