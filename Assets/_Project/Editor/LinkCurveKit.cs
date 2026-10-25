using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Adds the optional curved ribbon to the existing editable link prefab without rebuilding the board.
public static class LinkCurveKit
{
    const string LinkPath = "Assets/_Project/Prefabs/UI/Mvp/Link.prefab";

    [MenuItem("Tools/UI/Enable curved board links")]
    public static void Apply()
    {
        var root = PrefabUtility.LoadPrefabContents(LinkPath);
        try
        {
            var view = root.GetComponent<LinkView>();
            if (view == null || view.line == null) throw new System.InvalidOperationException("Link prefab is missing its line.");
            var oldLine = view.line;
            var curve = oldLine as CurvedImage;
            if (curve == null)
            {
                var color = oldLine.color;
                var sprite = oldLine.sprite;
                var material = oldLine.material;
                var raycastTarget = oldLine.raycastTarget;
                var maskable = oldLine.maskable;
                var type = oldLine.type;
                var preserveAspect = oldLine.preserveAspect;

                var lineObject = oldLine.gameObject;
                Object.DestroyImmediate(oldLine, true);
                curve = lineObject.AddComponent<CurvedImage>();
                curve.color = color;
                curve.sprite = sprite;
                curve.material = material;
                curve.raycastTarget = raycastTarget;
                curve.maskable = maskable;
                curve.type = type;
                curve.preserveAspect = preserveAspect;
            }

            curve.raycastTarget = false;
            view.curve = curve;
            view.line = curve;
            if (view.button != null) view.button.targetGraphic = curve;
            EditorUtility.SetDirty(view);
            PrefabUtility.SaveAsPrefabAsset(root, LinkPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Board links can now bend smoothly around other blocks.");
    }
}
