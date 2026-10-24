using Studio.Core;
using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Mascot portraits for the messenger (Art/UI/GeneratedKit20261024/MessengerAvatars): imports them as
// pixel sprites, writes them into the Level 0 candidates and wires the chat list rows and header. Safe to rerun.
public static class MvpAvatarsKit
{
    const string Dir = "Assets/_Project/Art/UI/GeneratedKit20261024/MessengerAvatars/";

    static Sprite Mascot(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Dir + name + ".png");

    [MenuItem("Tools/UI/Apply messenger avatars")]
    public static void Apply()
    {
        foreach (var n in new[] { "wizard-frog", "peach-lovebird", "surprised-giraffe", "mint-alien", "goose-messenger" })
        {
            var p = Dir + n + ".png";
            var ti = (TextureImporter)AssetImporter.GetAtPath(p);
            if (ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single && ti.filterMode == FilterMode.Point && ti.maxTextureSize == 256) continue;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.filterMode = FilterMode.Point; ti.maxTextureSize = 256; ti.mipmapEnabled = false; ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        // five mascots for seven chats: the boss gets the frog in the hat, repeats are placed so that colleagues of one discipline differ
        var by = new System.Collections.Generic.Dictionary<string, string>
        {
            { "lera", "peach-lovebird" }, { "vika", "surprised-giraffe" },   // design
            { "kostya", "mint-alien" }, { "dima", "goose-messenger" },       // mechanics
            { "anya", "goose-messenger" }, { "gosha", "mint-alien" },        // sound
        };
        var level = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
        foreach (var c in level.candidates) if (by.TryGetValue(c.id, out var m)) c.portrait = Mascot(m);
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();

        const string screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
        var root = PrefabUtility.LoadPrefabContents(screen);
        try
        {
            var cw = root.GetComponentInChildren<ChatWindow>(true);
            cw.bossAvatar = Mascot("wizard-frog");
            var ha = root.transform.Find("Body/ChatWindow/FeedArea/Header/Avatar").GetComponent<Image>();
            ha.sprite = Mascot("wizard-frog"); ha.color = Color.white; ha.preserveAspect = true;
            cw.headerAvatar = ha;
            PrefabUtility.SaveAsPrefabAsset(root, screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        const string row = "Assets/_Project/Prefabs/UI/Mvp/RowAvatar.prefab";
        var r = PrefabUtility.LoadPrefabContents(row);
        try
        {
            var av = r.transform.Find("Avatar").GetComponent<Image>();
            av.preserveAspect = true;
            r.GetComponent<RowView>().avatar = av;
            PrefabUtility.SaveAsPrefabAsset(r, row);
        }
        finally { PrefabUtility.UnloadPrefabContents(r); }
        Debug.Log("Messenger avatars applied.");
    }
}
