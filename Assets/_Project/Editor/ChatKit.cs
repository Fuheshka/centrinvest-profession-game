using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-5 (chat): avatars belong to the left chat list; message bubbles align by sender without repeating portraits.
// Builds the ChatLine prefab and wires ChatWindow. Safe to rerun.
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

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var cw = root.GetComponentInChildren<ChatWindow>(true);
            cw.linePrefab = linePrefab.transform;
            var headerAvatar = cw.header != null ? cw.header.transform.parent.Find("Avatar") : null;
            if (headerAvatar != null) Object.DestroyImmediate(headerAvatar.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Chat lines applied.");
    }
}
