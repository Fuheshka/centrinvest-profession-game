using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// One-shot (P-31): TMP font assets from the author's fonts and their assignment in the MVP and menu prefabs.
// FindSans Pro Bold for headings by role (see IsHeading), FindSans Pro Regular for the rest. Safe to rerun.
public static class FontsP31
{
    const string Dir = "Assets/_Project/Art/Fonts/";

    static TMP_FontAsset Make(string ttf, string name)
    {
        var path = Dir + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing) return existing;
        var font = AssetDatabase.LoadAssetAtPath<Font>(Dir + ttf);
        var fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        fa.name = name;
        AssetDatabase.CreateAsset(fa, path);
        fa.material.name = name + " Material"; AssetDatabase.AddObjectToAsset(fa.material, fa);
        fa.atlasTexture.name = name + " Atlas"; AssetDatabase.AddObjectToAsset(fa.atlasTexture, fa);
        AssetDatabase.SaveAssets();
        return fa;
    }

    // Role, not size: names of people and screens, the day and budget plate, parameter names on notes, modal titles.
    static bool IsHeading(TMPro.TMP_Text t, string prefab)
    {
        if (prefab.Contains("/Menu/")) return t.fontSize >= 36;
        return t.name == "Title" || t.name == "Name" || t.name == "Day" || t.name == "Budget" || t.name == "Value" || t.name == "Title Score";
    }

    [MenuItem("Tools/Mvp/Apply fonts P-31")]
    public static void Run()
    {
        AssetDatabase.Refresh();
        var head = Make("FindSansPro-Bold.ttf", "Heading SDF");
        var body = Make("FindSansPro-Regular.ttf", "Body SDF");
        head.TryAddCharacters("АБВЯабвяЁё", out string mh); body.TryAddCharacters("АБВЯабвяЁё", out string mb);
        EditorUtility.SetDirty(head); EditorUtility.SetDirty(body);
        Debug.Log("Cyrillic missing: heading '" + mh + "', body '" + mb + "'");
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs/UI/Mvp", "Assets/_Project/Prefabs/UI/Menu" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                bool heading = IsHeading(t, path);
                t.font = heading ? head : body;
                if (heading) t.fontStyle &= ~FontStyles.Bold;
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        Debug.Log("Fonts applied (P-31)");
    }
}
