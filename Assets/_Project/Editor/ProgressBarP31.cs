using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot (P-31): thin progress bar under the subtitle of RowOlive (task rows). Safe to rerun.
public static class ProgressBarP31
{
    const string Path = "Assets/_Project/Prefabs/UI/Mvp/RowOlive.prefab";

    [MenuItem("Tools/Mvp/Add task progress bar P-31")]
    public static void Run()
    {
        var root = PrefabUtility.LoadPrefabContents(Path);
        var row = root.GetComponent<RowView>();
        if (row.progressBar == null)
        {
            var rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png");
            var texts = row.subtitle.transform.parent;
            var bar = new GameObject("Bar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            bar.transform.SetParent(texts, false);
            var le = bar.GetComponent<LayoutElement>(); le.preferredHeight = 12; le.minHeight = 12;
            var bg = bar.GetComponent<Image>(); bg.sprite = rounded; bg.type = Image.Type.Sliced; bg.color = new Color(0.12f, 0.1f, 0.02f, 0.55f); bg.raycastTarget = false;
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bar.transform, false);
            var fr = (RectTransform)fill.transform;
            fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0, 1); fr.offsetMin = fr.offsetMax = Vector2.zero;
            var fi = fill.GetComponent<Image>(); fi.sprite = rounded; fi.type = Image.Type.Sliced; fi.color = new Color(0.84f, 0.75f, 0.31f); fi.raycastTarget = false;
            row.progressBar = bar; row.progressFill = fr;
        }
        PrefabUtility.SaveAsPrefabAsset(root, Path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("Progress bar added (P-31)");
    }
}
