using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Round-1 author feedback on UI-2 (2026-10-25): modal (pause, confirmations) restyled with the kit panel and flat buttons,
// compact; chat list rows of equal size with round avatars. Safe to rerun; the prefabs are edited by hand afterwards.
public static class MvpFeedbackKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";

    [MenuItem("Tools/UI/Apply feedback round 1")]
    public static void Apply()
    {
        Modal();
        Option();
        ChatRow();
        Debug.Log("Feedback round 1 applied: modal, option button, chat rows.");
    }

    static void Modal()
    {
        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var panel = (RectTransform)root.transform.Find("Modal/Panel");
            foreach (var e in panel.GetComponents<Shadow>()) Object.DestroyImmediate(e);   // Outline derives from Shadow
            var img = panel.GetComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Panels/panels/modal-ivory.png");
            img.type = Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1.5f; img.color = Color.white;
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(860, 0); panel.anchoredPosition = Vector2.zero;
            var fit = panel.GetComponent<ContentSizeFitter>() ?? panel.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize; fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            var v = panel.GetComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(64, 64, 56, 56); v.spacing = 14;
            var modal = root.GetComponent<ModalView>();
            modal.title.color = Palette.Ink; modal.body.color = Palette.Ink;
            modal.title.alignment = TextAlignmentOptions.Center; modal.body.alignment = TextAlignmentOptions.Center;
            modal.title.fontSize = 38; modal.body.fontSize = 24;
            modal.body.GetComponent<LayoutElement>().flexibleHeight = 0;
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void Option()
    {
        const string path = "Assets/_Project/Prefabs/UI/Mvp/Option.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var e in root.GetComponents<Shadow>()) Object.DestroyImmediate(e);
            var b = root.GetComponent<Button>();
            b.transition = Selectable.Transition.ColorTint;
            var cb = b.colors; cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1); cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1); b.colors = cb;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void ChatRow()
    {
        const string path = "Assets/_Project/Prefabs/UI/Mvp/RowAvatar.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var le = root.GetComponent<LayoutElement>() ?? root.AddComponent<LayoutElement>();
            le.preferredHeight = le.minHeight = 118;
            var av = root.transform.Find("Avatar").GetComponent<LayoutElement>();
            av.minWidth = av.minHeight = av.preferredWidth = av.preferredHeight = 72; av.flexibleWidth = 0;
            var row = root.GetComponent<RowView>();
            row.title.textWrappingMode = TextWrappingModes.NoWrap; row.title.overflowMode = TextOverflowModes.Overflow;   // Ellipsis hid the whole line when the row was a pixel short
            var tle = row.title.GetComponent<LayoutElement>() ?? row.title.gameObject.AddComponent<LayoutElement>(); tle.minHeight = 34;
            row.subtitle.overflowMode = TextOverflowModes.Ellipsis; row.subtitle.maxVisibleLines = 2; row.subtitle.fontSize = 20;
            var sle = row.subtitle.GetComponent<LayoutElement>() ?? row.subtitle.gameObject.AddComponent<LayoutElement>(); sle.minHeight = 52;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
