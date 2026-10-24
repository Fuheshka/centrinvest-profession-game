using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot patch of Prefabs/UI/Mvp/MvpScreen.prefab (P-30): smooth clamped scroll, unread badge on the Chat tab,
// centred text in the framed cards. Safe to rerun.
public static class MvpPatch
{
    const string Path = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";

    [MenuItem("Tools/Mvp/Patch P-30")]
    public static void Run()
    {
        var root = PrefabUtility.LoadPrefabContents(Path);
        foreach (var sr in root.GetComponentsInChildren<ScrollRect>(true))
        {
            sr.movementType = ScrollRect.MovementType.Clamped;
            if (!sr.GetComponent<SmoothWheel>()) sr.gameObject.AddComponent<SmoothWheel>();
        }

        var top = root.GetComponent<TopBarMvp>();
        if (top.chatBadge == null)
        {
            var tab = top.tabs[0].transform;
            var go = new GameObject("Badge", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(tab, false);
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1, 1); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(40, 40); rt.anchoredPosition = new Vector2(-8, -4);
            var img = go.GetComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/circle.png");
            img.color = new Color(0.79f, 0.23f, 0.25f); img.raycastTarget = false;
            var tgo = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
            tgo.transform.SetParent(go.transform, false);
            var tr = (RectTransform)tgo.transform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
            var t = tgo.GetComponent<TextMeshProUGUI>();
            t.font = top.tabs[0].GetComponentInChildren<TMP_Text>().font;
            t.fontSize = 24; t.fontStyle = FontStyles.Bold; t.color = Color.white; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
            t.enableAutoSizing = true; t.fontSizeMin = 14; t.fontSizeMax = 24;
            top.chatBadge = go; top.chatBadgeText = t;
            go.SetActive(false);
        }

        // centred text in framed cards
        foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
        {
            string p = Chain(t.transform);
            if (p.Contains("/Left/Card/") || p.Contains("/TotalsCard/")) Center(t, TextAlignmentOptions.Top);
            else if (p.EndsWith("/Reason") || p.EndsWith("/Modal/Panel/Title") || p.EndsWith("/Modal/Panel/Body")) Center(t, TextAlignmentOptions.Top);
        }
        foreach (var w in new[] { root.GetComponentInChildren<HireWindow>(true).team, root.GetComponentInChildren<TaskWindow>(true).team })
            foreach (var card in w)
            {
                Center(card.title, TextAlignmentOptions.Center);
                Center(card.subtitle, TextAlignmentOptions.Center);
                var vl = card.title.transform.parent.GetComponent<VerticalLayoutGroup>();
                if (vl) vl.childAlignment = TextAnchor.MiddleCenter;
            }

        PrefabUtility.SaveAsPrefabAsset(root, Path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("MvpScreen patched (P-30)");
    }

    static string Chain(Transform t) { string s = ""; for (; t != null; t = t.parent) s = "/" + t.name + s; return s; }

    static void Center(TMP_Text t, TextAlignmentOptions a)
    {
        t.alignment = a;
        var vl = t.transform.parent.GetComponent<VerticalLayoutGroup>();
        if (vl && a == TextAlignmentOptions.Top) vl.childAlignment = TextAnchor.UpperCenter;
    }
}
