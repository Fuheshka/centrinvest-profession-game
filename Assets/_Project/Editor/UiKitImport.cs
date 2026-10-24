using System.IO;
using UnityEditor;
using UnityEngine;

// Import rules for the generated UI kit (Art/UI/GeneratedKit20261024): individual PNGs become Single sprites,
// 9-slice borders come from the trimmed size. Runs on every (re)import, so new kit PNGs need no manual setup.
public class UiKitImport : AssetPostprocessor
{
    const string Root = "Assets/_Project/Art/UI/GeneratedKit20261024/";

    // Individual PNGs are trimmed to their opaque bounds (docs/art-source/ui-kit-2026-10-24/trim_kit.py), so the
    // 9-slice border is derived from the trimmed size: about 38% of the short side, at most 56 px.
    static Vector4 Border(string category, string file, int w, int h)
    {
        int b = Mathf.Min(Mathf.RoundToInt(0.38f * Mathf.Min(w, h)), 56);
        if (category == "Buttons" || category == "Panels" || category == "Cards") return new Vector4(b, b, b, b);
        if (category == "Controls" && (file.StartsWith("progress-") || file.StartsWith("scrollbar-track")))
        { int x = Mathf.RoundToInt(0.45f * h); return new Vector4(x, 0, x, 0); }
        return Vector4.zero;
    }

    static void PngSize(string path, out int w, out int h)
    {
        var head = new byte[24];
        using (var f = File.OpenRead(path)) f.Read(head, 0, 24);
        w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
        h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
    }

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(Root) || assetPath.EndsWith("-sheet.png") || !assetPath.EndsWith(".png")) return;
        var parts = assetPath.Substring(Root.Length).Split('/');
        if (parts.Length != 3) return;   // Category/sheet/file.png
        var category = parts[0];

        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 100;
        ti.alphaIsTransparency = true;
        ti.mipmapEnabled = false;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.maxTextureSize = 2048;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        var pixelArt = category == "Icons" || category == "Avatars" || category == "Decor";
        ti.filterMode = pixelArt ? FilterMode.Point : FilterMode.Bilinear;
        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        ti.SetTextureSettings(settings);
        PngSize(assetPath, out var w, out var h);
        ti.spriteBorder = Border(category, parts[2], w, h);   // left, bottom, right, top
    }

    [MenuItem("Tools/UI/Reimport kit")]
    public static void ReimportAll()
    {
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { Root }))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (!p.EndsWith("-sheet.png")) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        }
    }
}
