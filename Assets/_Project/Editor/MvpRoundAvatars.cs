using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Makes the messenger and headhunter portraits round: the old image becomes a circle with a Mask, the portrait is its child "Face".
// Run after "Apply messenger avatars" and "Apply kit to headhunter". Safe to rerun.
public static class MvpRoundAvatars
{
    static Image Round(Image holder)
    {
        var circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/avatar-circle.png");
        var old = holder.transform.Find("Face");
        Image face;
        if (old != null) face = old.GetComponent<Image>();
        else
        {
            var go = new GameObject("Face", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(holder.transform, false);
            var r = (RectTransform)go.transform;
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
            face = go.GetComponent<Image>();
            face.sprite = holder.sprite; face.preserveAspect = false; face.raycastTarget = false;
        }
        holder.sprite = circle; holder.color = Palette.Hex("2A1810"); holder.type = Image.Type.Simple; holder.preserveAspect = false;
        var m = holder.GetComponent<Mask>() ?? holder.gameObject.AddComponent<Mask>();
        m.showMaskGraphic = true;
        return face;
    }

    [MenuItem("Tools/UI/Round avatars")]
    public static void Apply()
    {
        const string cp = "Assets/_Project/Art/UI/avatar-circle.png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(cp);
        if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; ti.mipmapEnabled = false; ti.alphaIsTransparency = true; ti.SaveAndReimport(); }
        // drawn people for the headhunter cards
        var level = AssetDatabase.LoadAssetAtPath<Studio.Core.LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
        foreach (var cd in level.candidates)
            cd.character = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/" + cd.id + "-neutral.png");
        EditorUtility.SetDirty(level); AssetDatabase.SaveAssets();

        const string screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
        var root = PrefabUtility.LoadPrefabContents(screen);
        try
        {
            var cw = root.GetComponentInChildren<ChatWindow>(true);
            cw.bossAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/boris-neutral.png");   // one drawn person per chat, no repeats
            cw.headerAvatar = Round(cw.headerAvatar.transform.parent.GetComponent<Image>() != null && cw.headerAvatar.name == "Face" ? cw.headerAvatar.transform.parent.GetComponent<Image>() : cw.headerAvatar);
            PrefabUtility.SaveAsPrefabAsset(root, screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        const string row = "Assets/_Project/Prefabs/UI/Mvp/RowAvatar.prefab";
        var r = PrefabUtility.LoadPrefabContents(row);
        try
        {
            var rv = r.GetComponent<RowView>();
            rv.avatar = Round(rv.avatar.name == "Face" ? rv.avatar.transform.parent.GetComponent<Image>() : rv.avatar);
            PrefabUtility.SaveAsPrefabAsset(r, row);
        }
        finally { PrefabUtility.UnloadPrefabContents(r); }

        const string card = "Assets/_Project/Prefabs/UI/Mvp/CandidateCard.prefab";
        var c = PrefabUtility.LoadPrefabContents(card);
        try
        {
            var cv = c.GetComponent<CandidateCardView>();
            cv.portrait = Round(cv.portrait.name == "Face" ? cv.portrait.transform.parent.GetComponent<Image>() : cv.portrait);
            PrefabUtility.SaveAsPrefabAsset(c, card);
        }
        finally { PrefabUtility.UnloadPrefabContents(c); }
        Debug.Log("Avatars are round.");
    }
}
