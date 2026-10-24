using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot (P-32): link tags in a layer above blocks, day clock bar, smaller modal, help text. Safe to rerun.
public static class MvpPatch2
{
    const string Dir = "Assets/_Project/Prefabs/UI/Mvp/";

    static Sprite Rounded => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png");

    [MenuItem("Tools/Mvp/Patch P-32")]
    public static void Run()
    {
        PatchLink();
        PatchScreen();
        Debug.Log("MVP patch P-32 done");
    }

    static void PatchLink()
    {
        var root = PrefabUtility.LoadPrefabContents(Dir + "Link.prefab");
        var lv = root.GetComponent<LinkView>();
        if (lv.tag == null)
        {
            foreach (var n in new[] { "Plate", "Label" }) { var c = root.transform.Find(n); if (c) Object.DestroyImmediate(c.gameObject); }
            var tag = new GameObject("Tag", typeof(RectTransform), typeof(Image), typeof(Button));
            tag.transform.SetParent(root.transform, false);
            var tr = (RectTransform)tag.transform;
            tr.anchorMin = tr.anchorMax = new Vector2(0, 1); tr.pivot = new Vector2(0.5f, 0.5f); tr.sizeDelta = new Vector2(120, 38);
            var img = tag.GetComponent<Image>(); img.sprite = Rounded; img.type = Image.Type.Sliced; img.color = new Color(0.16f, 0.06f, 0.01f);
            tag.GetComponent<Button>().targetGraphic = img;
            var lgo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lgo.transform.SetParent(tag.transform, false);
            var lr = (RectTransform)lgo.transform; lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = lr.offsetMax = Vector2.zero;
            var t = lgo.GetComponent<TextMeshProUGUI>();
            t.fontSize = 22; t.color = new Color(0.93f, 0.95f, 0.89f); t.alignment = TextAlignmentOptions.Center; t.textWrappingMode = TextWrappingModes.NoWrap; t.raycastTarget = false;
            lv.tag = tr; lv.tagButton = tag.GetComponent<Button>(); lv.label = t;
        }
        PrefabUtility.SaveAsPrefabAsset(root, Dir + "Link.prefab");
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void PatchScreen()
    {
        var root = PrefabUtility.LoadPrefabContents(Dir + "MvpScreen.prefab");
        var dw = root.GetComponentInChildren<DashboardWindow>(true);
        if (dw.tagLayer == null)
        {
            var go = new GameObject("Tags", typeof(RectTransform));
            go.transform.SetParent(dw.canvas, false);
            var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; r.pivot = new Vector2(0, 1);
            dw.tagLayer = r;
        }
        dw.tagLayer.SetAsLastSibling();
        foreach (var t in dw.GetComponentsInChildren<TMP_Text>(true))
            if (t.name == "Help")
                t.text = "Тип слева – заметка на доске. Нажми заметку – выбери параметр, тяни – двигай. «Связать блоки», затем две заметки – линия, не больше шести. Метка на линии: Д – дизайн, М – механики, З – звук, зелёный плюс, красный минус. Итог считается только по линиям.";

        var top = root.GetComponent<TopBarMvp>();
        if (top.timeFill == null)
        {
            var info = top.dayText.transform.parent;
            var day = (RectTransform)top.dayText.transform;
            day.offsetMin = new Vector2(day.offsetMin.x, 34);
            var bar = new GameObject("TimeBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(info, false);
            var br = (RectTransform)bar.transform;
            br.anchorMin = new Vector2(0, 0); br.anchorMax = new Vector2(0.45f, 0); br.offsetMin = new Vector2(30, 14); br.offsetMax = new Vector2(-18, 26);
            var bi = bar.GetComponent<Image>(); bi.sprite = Rounded; bi.type = Image.Type.Sliced; bi.color = new Color(0.12f, 0.1f, 0.02f, 0.6f); bi.raycastTarget = false;
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bar.transform, false);
            var fr = (RectTransform)fill.transform; fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0, 1); fr.offsetMin = fr.offsetMax = Vector2.zero;
            var fi = fill.GetComponent<Image>(); fi.sprite = Rounded; fi.type = Image.Type.Sliced; fi.color = new Color(0.84f, 0.75f, 0.31f); fi.raycastTarget = false;
            top.timeFill = fr;
        }

        var panel = root.transform.Find("Modal/Panel") as RectTransform;
        if (panel)
        {
            panel.sizeDelta = new Vector2(860, panel.sizeDelta.y);
            if (!panel.GetComponent<ContentSizeFitter>()) panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var body = panel.Find("Body");
            if (body) { var le = body.GetComponent<LayoutElement>(); if (le) { le.flexibleHeight = 0; le.minHeight = 0; } }
        }
        PrefabUtility.SaveAsPrefabAsset(root, Dir + "MvpScreen.prefab");
        PrefabUtility.UnloadPrefabContents(root);
    }
}
