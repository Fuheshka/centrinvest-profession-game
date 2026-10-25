using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Builds Resources/UI/PixelTransition.prefab: a full-screen overlay of 16x9 patterned cells (dark, olive, a few yellow accents).
// Edit colours and the grid here and run again, or tweak the prefab by hand.
public static class TransitionKit
{
    const string Path = "Assets/_Project/Resources/UI/PixelTransition.prefab";

    [MenuItem("Tools/UI/Build pixel transition prefab")]
    public static void Build()
    {
        const int cols = 16, rows = 9;
        System.IO.Directory.CreateDirectory("Assets/_Project/Resources/UI");
        var root = new GameObject("PixelTransition", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PixelTransition));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 1000;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = 0;

        Color dark = Palette.Hex("1E1A04"), olive = Palette.Hex("6A7837"), yellow = Palette.Hex("D5C04F");
        var cells = new RectTransform[cols * rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                var go = new GameObject("Cell " + x + "," + y, typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(root.transform, false);
                // overlap by a hair so no seams show between scaled cells
                rt.anchorMin = new Vector2(x / (float)cols, 1 - (y + 1) / (float)rows);
                rt.anchorMax = new Vector2((x + 1) / (float)cols, 1 - y / (float)rows);
                rt.offsetMin = new Vector2(-1, -1); rt.offsetMax = new Vector2(1, 1);
                go.GetComponent<Image>().color = (x * 3 + y * 5) % 7 == 0 ? yellow : (x + y) % 2 == 0 ? dark : olive;
                cells[y * cols + x] = rt;
            }
        var t = root.GetComponent<PixelTransition>();
        t.cells = cells; t.cols = cols; t.rows = rows;
        PrefabUtility.SaveAsPrefabAsset(root, Path);
        Object.DestroyImmediate(root);
        Debug.Log("TransitionKit: built " + Path);
    }
}
