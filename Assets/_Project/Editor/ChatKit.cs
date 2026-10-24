using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-5 (chat): message lines with an avatar beside each bubble; incoming left, the player's right (mockup 04).
// Builds the ChatLine and ChatAvatar prefabs and wires ChatWindow. Safe to rerun.
public static class ChatKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string Dir = "Assets/_Project/Prefabs/UI/Mvp/";

    [MenuItem("Tools/UI/Apply kit to chat")]
    public static void Apply()
    {
        var line = new GameObject("ChatLine", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var h = line.GetComponent<HorizontalLayoutGroup>();
        h.spacing = 12; h.childAlignment = TextAnchor.UpperLeft;
        h.childControlWidth = h.childControlHeight = true; h.childForceExpandWidth = h.childForceExpandHeight = false;
        var linePrefab = PrefabUtility.SaveAsPrefabAsset(line, Dir + "ChatLine.prefab"); Object.DestroyImmediate(line);

        var av = new GameObject("ChatAvatar", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(LayoutElement));
        var ai = av.GetComponent<Image>();
        ai.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/avatar-circle.png"); ai.color = Palette.Hex("2A1810"); ai.raycastTarget = false;
        av.GetComponent<Mask>().showMaskGraphic = true;
        var le = av.GetComponent<LayoutElement>(); le.preferredWidth = le.preferredHeight = le.minWidth = le.minHeight = 60; le.flexibleWidth = 0;
        var face = new GameObject("Face", typeof(RectTransform), typeof(Image));
        face.transform.SetParent(av.transform, false);
        var fr = (RectTransform)face.transform; fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = fr.offsetMax = Vector2.zero;
        face.GetComponent<Image>().raycastTarget = false;
        var avPrefab = PrefabUtility.SaveAsPrefabAsset(av, Dir + "ChatAvatar.prefab"); Object.DestroyImmediate(av);

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var cw = root.GetComponentInChildren<ChatWindow>(true);
            cw.linePrefab = linePrefab.transform; cw.avatarPrefab = avPrefab.transform;
            cw.playerAvatar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/player-neutral.png");
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Chat lines applied.");
    }
}
