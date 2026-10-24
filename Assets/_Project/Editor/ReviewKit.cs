using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-6 (review): restyles ReviewScreen after docs/mockup/generated-2026-10-24/07-review.png: ivory kit panel, dark text,
// light bar tracks, compact rows. Keeps every ReviewView reference. Safe to rerun.
public static class ReviewKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";

    [MenuItem("Tools/UI/Apply kit to review")]
    public static void Apply()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var rv = root.GetComponentInChildren<ReviewView>(true);
            var panel = (RectTransform)rv.transform.Find("Panel");
            var pi = panel.GetComponent<Image>();
            pi.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Panels/panels/modal-ivory.png");
            pi.type = Image.Type.Sliced; pi.pixelsPerUnitMultiplier = 1.5f; pi.color = Color.white;
            panel.sizeDelta = new Vector2(1000, 0);
            var vl = panel.GetComponent<VerticalLayoutGroup>(); vl.padding = new RectOffset(70, 70, 56, 56); vl.spacing = 12;
            rv.title.color = Palette.Ink; rv.title.font = heading; rv.title.fontSize = 44;
            rv.subtitle.color = Palette.Hex("5B4A3A"); rv.subtitle.font = body;
            rv.score.fontSize = 120;
            rv.need.color = Palette.Hex("5B4A3A"); rv.need.font = body;
            for (int i = 0; i < 3; i++)
            {
                rv.names[i].color = Palette.Ink; rv.names[i].font = heading; rv.names[i].fontSize = 26;
                rv.names[i].textWrappingMode = TextWrappingModes.NoWrap;
                rv.names[i].GetComponent<LayoutElement>().preferredWidth = 190;
                rv.values[i].color = Palette.Ink; rv.values[i].font = heading; rv.values[i].fontSize = 24;
                rv.values[i].textWrappingMode = TextWrappingModes.NoWrap;
                rv.values[i].GetComponent<LayoutElement>().preferredWidth = 270;
                var track = rv.fills[i].parent.GetComponent<Image>();
                track.color = Palette.Hex("E3E0CC");
            }
            rv.remarks.color = Palette.Ink; rv.remarks.font = body; rv.remarks.fontSize = 22;
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Review restyled.");
    }
}
