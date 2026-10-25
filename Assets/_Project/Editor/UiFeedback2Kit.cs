using Studio.UI;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// One-shot (2026-10-25 playtest notes): Zhabagram chat title and frog background, ShiSh.ru tab, link button on top of the board,
// smaller settings sliders, readable gear in the menu. Safe to rerun.
public static class UiFeedback2Kit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string Pattern = "Assets/_Project/Art/UI/frog-pattern.png";

    static RectTransform Rt(Component c) => (RectTransform)c.transform;
    static RectTransform FindDeep(Transform t, string name)
    {
        foreach (Transform c in t) { if (c.name == name) return (RectTransform)c; var r = FindDeep(c, name); if (r) return r; }
        return null;
    }

    static void Box(RectTransform r, float x, float y, float w, float h)
    {
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
    }

    [MenuItem("Tools/UI/Apply feedback 2026-10-25")]
    public static void Run()
    {
        var imp = (TextureImporter)AssetImporter.GetAtPath(Pattern);
        imp.textureType = TextureImporterType.Sprite; imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled = false; imp.wrapMode = TextureWrapMode.Repeat; imp.alphaIsTransparency = true;
        var ts = new TextureImporterSettings(); imp.ReadTextureSettings(ts); ts.spriteMeshType = SpriteMeshType.FullRect; ts.spriteGenerateFallbackPhysicsShape = false; imp.SetTextureSettings(ts);
        imp.SaveAndReimport();
        var pattern = AssetDatabase.LoadAssetAtPath<Sprite>(Pattern);

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var heading = root.GetComponentInChildren<ChatWindow>(true).header.font;
            // messenger title and frog backdrop
            var list = FindDeep(root.transform, "ListArea");
            if (FindDeep(list, "AppTitle") == null)
            {
                var go = new GameObject("AppTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
                go.transform.SetParent(list, false);
                var r = (RectTransform)go.transform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(0, 1);
                r.anchoredPosition = new Vector2(30, -22); r.sizeDelta = new Vector2(-60, 64);
                var t = go.GetComponent<TextMeshProUGUI>();
                t.font = heading; t.text = "Жабаграм"; t.fontSize = 40; t.color = Palette.Cream; t.alignment = TextAlignmentOptions.MidlineLeft; t.raycastTarget = false;
            }
            var feedArea = FindDeep(root.transform, "FeedArea");
            var old = FindDeep(feedArea, "FrogPattern");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            {
                var go = new GameObject("FrogPattern", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(feedArea, false); go.transform.SetAsFirstSibling();
                var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(24, 24); r.offsetMax = new Vector2(-24, -24);
                var im = go.GetComponent<Image>(); im.sprite = pattern; im.type = Image.Type.Tiled; im.raycastTarget = false; im.color = new Color(1, 1, 1, 0.22f); im.pixelsPerUnitMultiplier = 1.4f;
            }
            var header = FindDeep(feedArea, "Header");
            var oldSeven = FindDeep(header, "SixSeven");
            if (oldSeven != null) Object.DestroyImmediate(oldSeven.gameObject);
            var seven = new GameObject("SixSeven", typeof(RectTransform), typeof(Image));
            seven.transform.SetParent(header, false);
            var sr = (RectTransform)seven.transform; sr.anchorMin = sr.anchorMax = new Vector2(1, 0.5f); sr.pivot = new Vector2(0.5f, 0.5f);
            sr.anchoredPosition = new Vector2(-70, 0); sr.sizeDelta = new Vector2(84, 84); sr.localRotation = Quaternion.Euler(0, 0, -9);
            var si = (TextureImporter)AssetImporter.GetAtPath("Assets/_Project/Art/UI/six-seven.png");
            if (si.textureType != TextureImporterType.Sprite || si.spriteImportMode != SpriteImportMode.Single) { si.textureType = TextureImporterType.Sprite; si.spriteImportMode = SpriteImportMode.Single; si.spritesheet = new SpriteMetaData[0]; si.SaveAndReimport(); }
            var sim = seven.GetComponent<Image>(); sim.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/six-seven.png"); sim.preserveAspect = true; sim.raycastTarget = false;
            var feed = FindDeep(feedArea, "Feed");
            var content = FindDeep(feed, "Content").GetComponent<VerticalLayoutGroup>();
            content.spacing = 6;

            // headhunter site name
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
                if (t.text.Contains("Хедхантер") || t.text.Contains("хедхантер"))
                    t.text = t.text.Replace("Хедхантер", "ШиШ.ру").Replace("хедхантере", "ШиШ.ру").Replace("хедхантер", "ШиШ.ру");

            // the "link blocks" button: yellow, top of the board, between the palette and the totals
            var dw = root.GetComponentInChildren<DashboardWindow>(true);
            var lm = Rt(dw.linkMode);
            lm.SetParent(dw.transform, false);
            Box(lm, 320, 22, 380, 60);
            lm.GetComponent<Image>().color = Palette.Primary;
            dw.mode.color = Palette.Ink;
            var bottom = FindDeep(dw.transform, "Bottom");
            foreach (Transform c in bottom)
            {
                var cr = (RectTransform)c;
                if (c.name == "Bulb") cr.anchoredPosition = new Vector2(0, cr.anchoredPosition.y);
                else if (c.name == "Text" && Mathf.Approximately(cr.anchoredPosition.x, 362)) { cr.anchoredPosition = new Vector2(62, cr.anchoredPosition.y); cr.sizeDelta = new Vector2(-502, cr.sizeDelta.y); }
            }

            // hint line under the top bar is what the player should look at
            var top = root.GetComponent<TopBarMvp>();
            var review = root.GetComponentInChildren<ReviewView>(true);
            review.gameCovers = new Sprite[0];
            var cover = FindDeep(root.transform, "GameCover");
            if (cover != null) cover.gameObject.SetActive(false);   // the "Starhop" space covers are gone
            top.hint.color = Palette.Primary;
            dw.reason.color = Palette.Ink; dw.reason.fontStyle = FontStyles.Bold;
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }

        Settings();
        TaskBar();
        MenuGear();
        AssetDatabase.SaveAssets();
        Debug.Log("UI feedback 2026-10-25 applied.");
    }

    static void Settings()
    {
        const string path = "Assets/_Project/Prefabs/UI/Common/SettingsPanel.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var sp = root.GetComponent<SettingsPanel>();
            var panel = FindDeep(root.transform, "Panel");
            panel.sizeDelta = new Vector2(600, 470);
            Box(FindDeep(panel, "Title"), 50, 34, 500, 56);
            FindDeep(panel, "Title").GetComponentInChildren<TMP_Text>().fontSize = 38;
            string[] labels = { "MasterLabel", "MusicLabel", "SfxLabel" };
            var sliders = new[] { sp.master, sp.music, sp.sfx };
            for (int i = 0; i < 3; i++)
            {
                var l = FindDeep(panel, labels[i]); Box(l, 50, 112 + 80 * i, 500, 30); l.GetComponentInChildren<TMP_Text>().fontSize = 22;
                var s = Rt(sliders[i]); Box(s, 50, 146 + 80 * i, 500, 28);
                foreach (var n in new[] { "Background", "Fill Area" })
                {
                    var b = FindDeep(s, n); b.offsetMin = new Vector2(b.offsetMin.x, -6); b.offsetMax = new Vector2(b.offsetMax.x, 6);
                }
                var h = Rt(sliders[i].handleRect); h.sizeDelta = new Vector2(26, -2);   // Slider stretches the handle over the 28 px slide area vertically
                var ol = h.GetComponent<Outline>(); if (ol) ol.effectDistance = new Vector2(2, -2);
            }
            Box(Rt(sp.close), 170, 366, 260, 64);
            sp.close.GetComponentInChildren<TMP_Text>().fontSize = 26;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    // Task progress bar: same flat yellow bar as the day clock, no rounded blob at low values.
    static void TaskBar()
    {
        const string path = "Assets/_Project/Prefabs/UI/Mvp/TaskRow.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var track = FindDeep(root.transform, "Track");
            foreach (var im in track.GetComponentsInChildren<Image>(true)) { im.sprite = null; im.type = Image.Type.Simple; }
            track.GetComponent<Image>().color = Palette.Hex("E2DFD0");
            FindDeep(track, "Fill").GetComponent<Image>().color = Palette.Yellow;
            // "waits for your answer" button: red plate with the chat icon, opens the chat with that employee
            var oldWait = FindDeep(root.transform, "Wait");
            if (oldWait != null) Object.DestroyImmediate(oldWait.gameObject);
            var wait = new GameObject("Wait", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            wait.transform.SetParent(root.transform, false);
            wait.transform.SetSiblingIndex(FindDeep(root.transform, "Buttons").GetSiblingIndex());
            var wi = wait.GetComponent<Image>();
            wi.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png"); wi.type = Image.Type.Sliced; wi.color = Palette.Red;
            wait.GetComponent<Button>().targetGraphic = wi;
            var wl = wait.GetComponent<LayoutElement>(); wl.minWidth = wl.preferredWidth = 64; wl.minHeight = wl.preferredHeight = 64;
            var ic = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            ic.transform.SetParent(wait.transform, false);
            var ir = (RectTransform)ic.transform; ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one; ir.offsetMin = new Vector2(12, 12); ir.offsetMax = new Vector2(-12, -12);
            var ii = ic.GetComponent<Image>(); ii.preserveAspect = true; ii.raycastTarget = false; ii.color = Palette.Cream;
            ii.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/GeneratedKit20261024/Icons/icons-navigation/chat.png");
            root.GetComponent<TaskRowView>().wait = wait.GetComponent<Button>();
            wait.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void MenuGear()
    {
        const string path = "Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var b = FindDeep(root.transform, "SettingsButton");
            b.GetComponent<Image>().color = Palette.Primary;
            var ol = b.GetComponent<Outline>() ?? b.gameObject.AddComponent<Outline>();
            ol.effectColor = Palette.Ink; ol.effectDistance = new Vector2(3, -3);
            FindDeep(b, "Icon").GetComponent<Image>().color = Color.white;
            var old = FindDeep(root.transform, "SixSeven");   // lived on the menu for a minute
            if (old != null) Object.DestroyImmediate(old.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
