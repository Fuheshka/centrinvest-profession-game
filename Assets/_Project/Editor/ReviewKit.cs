using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-6 (review): restyles ReviewScreen after docs/mockup/generated-2026-10-24/07-review.png: ivory kit panel, dark text,
// light bar tracks, compact rows. Keeps every ReviewView reference. Safe to rerun.
public static class ReviewKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";

    [MenuItem("Tools/UI/Apply kit to review")]
    public static void Apply()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var rv = root.GetComponentInChildren<ReviewView>(true);
            var panel = (RectTransform)rv.transform.Find("Panel");
            var pi = panel.GetComponent<Image>();
            pi.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Panels/panels/modal-ivory.png");
            pi.type = Image.Type.Sliced; pi.pixelsPerUnitMultiplier = 1.5f; pi.color = Color.white;
            panel.sizeDelta = new Vector2(1000, 0);
            var vl = panel.GetComponent<VerticalLayoutGroup>(); vl.padding = new RectOffset(48, 48, 30, 30); vl.spacing = 8;
            rv.title.color = Palette.Ink; rv.title.font = heading; rv.title.fontSize = 44;
            rv.subtitle.color = Palette.Hex("5B4A3A"); rv.subtitle.font = body;
            rv.score.fontSize = 108;
            rv.need.color = Palette.Hex("5B4A3A"); rv.need.font = body;
            for (int i = 0; i < 3; i++)
            {
                rv.names[i].color = Palette.Ink; rv.names[i].font = heading; rv.names[i].fontSize = 26;
                rv.names[i].textWrappingMode = TextWrappingModes.NoWrap;
                rv.names[i].GetComponent<LayoutElement>().preferredWidth = 190;
                rv.values[i].color = Palette.Ink; rv.values[i].font = heading; rv.values[i].fontSize = 24;
                rv.values[i].textWrappingMode = TextWrappingModes.NoWrap;
                rv.values[i].GetComponent<LayoutElement>().preferredWidth = 270;
                var track = rv.fills[i].parent.GetComponent<Image>();
                track.color = Palette.Hex("E3E0CC");
            }
            rv.remarks.color = Palette.Ink; rv.remarks.font = body; rv.remarks.fontSize = 20;

            rv.gameCovers = new[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/GameCovers/starhop-original.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/GameCovers/starhop-sequel.png")
            };
            rv.testerPortraits = new[]
            {
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/lera-neutral.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/kostya-neutral.png"),
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/anya-neutral.png")
            };
            rv.mentorPortrait = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Avatars/avatars/boris-neutral.png");

            var summary = EnsureRow(panel, "ScoreAndCover", 155, true);
            if (rv.score.transform.parent != summary)
            {
                summary.SetSiblingIndex(rv.score.transform.GetSiblingIndex());
                rv.score.transform.SetParent(summary, false);
            }
            rv.score.alignment = TextAlignmentOptions.Center;
            var scoreLayout = rv.score.GetComponent<LayoutElement>() ?? rv.score.gameObject.AddComponent<LayoutElement>();
            scoreLayout.preferredWidth = 420; scoreLayout.preferredHeight = 140; scoreLayout.flexibleWidth = 1;
            var cover = EnsureImage(summary, "GameCover", 280, 158);
            cover.preserveAspect = true; cover.raycastTarget = false;
            rv.coverImage = cover;

            var quoteRow = EnsureRow(panel, "TesterQuote", 82, true);
            if (quoteRow.GetSiblingIndex() != rv.remarks.transform.GetSiblingIndex() - 1)
                quoteRow.SetSiblingIndex(rv.remarks.transform.GetSiblingIndex());
            var avatar = EnsureImage(quoteRow, "TesterPortrait", 68, 68);
            avatar.preserveAspect = true; avatar.raycastTarget = false;
            rv.testerImage = avatar;
            rv.quote = EnsureQuote(quoteRow, body);
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Review restyled.");
    }

    static RectTransform EnsureRow(RectTransform parent, string name, float height, bool horizontal)
    {
        var row = parent.Find(name) as RectTransform;
        if (row == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
            row = go.GetComponent<RectTransform>(); row.SetParent(parent, false);
        }
        var image = row.GetComponent<Image>(); image.color = Palette.Hex("F5EBCB"); image.raycastTarget = false;
        var layout = row.GetComponent<LayoutElement>(); layout.preferredHeight = height; layout.flexibleHeight = 0;
        var group = row.GetComponent<HorizontalLayoutGroup>();
        group.padding = new RectOffset(12, 12, 6, 6); group.spacing = 12;
        group.childAlignment = TextAnchor.MiddleCenter; group.childControlWidth = true; group.childControlHeight = true;
        group.childForceExpandWidth = false; group.childForceExpandHeight = false;
        return row;
    }

    static Image EnsureImage(RectTransform parent, string name, float width, float height)
    {
        var image = parent.Find(name)?.GetComponent<Image>();
        if (image == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false); image = go.GetComponent<Image>();
        }
        var size = image.GetComponent<LayoutElement>(); size.minWidth = width; size.preferredWidth = width;
        size.minHeight = height; size.preferredHeight = height; size.flexibleWidth = 0;
        return image;
    }

    static TMP_Text EnsureQuote(RectTransform parent, TMP_FontAsset body)
    {
        var text = parent.Find("Quote")?.GetComponent<TMP_Text>();
        if (text == null)
        {
            var go = new GameObject("Quote", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false); text = go.GetComponent<TMP_Text>();
        }
        text.font = body; text.fontSize = 19; text.color = Palette.Ink;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        var layout = text.GetComponent<LayoutElement>(); layout.flexibleWidth = 1; layout.minHeight = 68; layout.preferredHeight = 68;
        return text;
    }
}
