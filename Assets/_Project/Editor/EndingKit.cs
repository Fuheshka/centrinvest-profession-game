using Studio.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Applies the shared ivory kit style to the existing editable ending prefab.
public static class EndingKit
{
    const string PrefabPath = "Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab";
    const string PanelSpritePath = "Assets/_Project/Art/UI/GeneratedKit20261024/Panels/panels/modal-ivory.png";

    [MenuItem("Tools/UI/Apply kit to ending")]
    public static void Apply()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PanelSpritePath);
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var view = root.GetComponent<EndingPanelView>();
            if (view == null) throw new System.InvalidOperationException("EndingPanelView is missing from the ending prefab.");

            var content = root.transform.Find("Content");
            var panel = content != null ? content.GetComponent<Image>() : null;
            if (panel != null && panelSprite != null)
            {
                panel.sprite = panelSprite;
                panel.type = Image.Type.Sliced;
                panel.pixelsPerUnitMultiplier = 1.5f;
                panel.color = Color.white;
            }

            Style(view.kicker, body, new Color32(105, 91, 44, 255), 20, FontStyles.Bold);
            Style(view.headline, heading, new Color32(54, 31, 19, 255), 48, FontStyles.Bold);
            Style(view.scoreText, heading, new Color32(54, 31, 19, 255), 112, FontStyles.Bold);
            Style(view.verdictText, heading, new Color32(79, 111, 36, 255), 30, FontStyles.Bold);
            Style(view.verdictSub, body, new Color32(91, 74, 58, 255), 22, FontStyles.Normal);
            Style(view.professionText, body, new Color32(54, 31, 19, 255), 22, FontStyles.Normal);

            if (view.ring != null) view.ring.color = new Color32(229, 195, 49, 255);
            StyleButton(view.newButton, new Color32(229, 195, 49, 255), new Color32(54, 31, 19, 255));
            StyleButton(view.moreButton, new Color32(239, 242, 220, 255), new Color32(54, 31, 19, 255));

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        StyleRows("Assets/_Project/Prefabs/UI/Versions/ReviewCard.prefab", heading, body);
        StyleRows("Assets/_Project/Prefabs/UI/Versions/HighlightRow.prefab", heading, body);
        Debug.Log("Ending screen restyled with the shared ivory kit.");
    }

    static void StyleRows(string path, TMP_FontAsset heading, TMP_FontAsset body)
    {
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                text.color = new Color32(54, 31, 19, 255);
                text.font = text.transform.name == "Title" ? heading : body;
                text.fontSize = text.transform.name == "Title" ? 24 : 20;
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void Style(TMP_Text text, TMP_FontAsset font, Color color, float size, FontStyles weight)
    {
        if (text == null) return;
        text.font = font;
        text.color = color;
        text.fontSize = size;
        text.fontStyle = weight;
    }

    static void StyleButton(Button button, Color background, Color foreground)
    {
        if (button == null) return;
        var image = button.GetComponent<Image>();
        if (image != null) image.color = background;
        var text = button.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.color = foreground;
            text.fontSize = 25;
            text.fontStyle = FontStyles.Bold;
        }
    }
}
