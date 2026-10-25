using Studio.UI;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds Prefabs/UI/Common/SettingsPanel (two volume sliders), adds the gear button to MenuScreen and
// places the panel under the Canvas of Menu and Game. After the first run the prefab is edited by hand.
public static class SettingsKit
{
    const string PanelPath = "Assets/_Project/Prefabs/UI/Common/SettingsPanel.prefab";
    const string MenuPath = "Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    const string Rounded = "Assets/_Project/Art/UI/rounded.png";

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static RectTransform Box(string name, Transform parent, float x, float y, float w, float h)
    {
        var r = Node(name, parent);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
        return r;
    }

    static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

    static Image Img(RectTransform r, Sprite s, Color c, bool raycast = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (s != null && s.border != Vector4.zero) { i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = 1.5f; }
        return i;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
        Stretch(r);
        return t;
    }

    static Slider MakeSlider(Transform parent, float y)
    {
        var rt = Box("Slider", parent, 60, y, 640, 44);
        var bg = Node("Background", rt); bg.anchorMin = new Vector2(0, 0.5f); bg.anchorMax = new Vector2(1, 0.5f); bg.offsetMin = new Vector2(0, -12); bg.offsetMax = new Vector2(0, 12);
        Img(bg, Spr(Rounded), Palette.Hex("2A1810"));
        var area = Node("Fill Area", rt); area.anchorMin = new Vector2(0, 0.5f); area.anchorMax = new Vector2(1, 0.5f); area.offsetMin = new Vector2(0, -12); area.offsetMax = new Vector2(-6, 12);
        var fill = Node("Fill", area); fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0, 1); fill.offsetMin = fill.offsetMax = Vector2.zero;
        Img(fill, Spr(Rounded), Palette.Hex("D5C04F"));
        var ha = Node("Handle Slide Area", rt); Stretch(ha); ha.offsetMin = new Vector2(20, 0); ha.offsetMax = new Vector2(-20, 0);
        var handle = Node("Handle", ha); handle.sizeDelta = new Vector2(40, 40);
        var hi = Img(handle, Spr("Assets/_Project/Art/UI/circle.png"), Palette.Hex("EFF2DF"), true);
        var ol = handle.gameObject.AddComponent<Outline>(); ol.effectColor = Palette.Olive; ol.effectDistance = new Vector2(3, -3);
        var s = rt.gameObject.AddComponent<Slider>();
        s.fillRect = fill; s.handleRect = handle; s.targetGraphic = hi; s.minValue = 0; s.maxValue = 1; s.value = 1;
        return s;
    }

    [MenuItem("Tools/UI/Build settings panel")]
    public static void Build()
    {
        var heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");

        var go = new GameObject("SettingsPanel", typeof(RectTransform));
        var rt = (RectTransform)go.transform; Stretch(rt);
        var sp = go.AddComponent<SettingsPanel>();

        var root = Node("Root", rt); Stretch(root);
        Img(root, null, new Color(0.08f, 0.04f, 0, 0.75f), true);
        sp.root = root.gameObject;
        var panel = Node("Panel", root); panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f); panel.sizeDelta = new Vector2(760, 610);
        Img(panel, Spr("Panels/panels/modal-ivory"), Color.white, true);
        Txt(Box("Title", panel, 60, 50, 640, 60), "Настройки", 44, Palette.Ink, heading, TextAlignmentOptions.Center);
        Txt(Box("MasterLabel", panel, 60, 140, 640, 34), "Общая громкость", 26, Palette.Ink, body, TextAlignmentOptions.Left);
        sp.master = MakeSlider(panel, 180);
        Txt(Box("MusicLabel", panel, 60, 250, 640, 34), "Музыка", 26, Palette.Ink, body, TextAlignmentOptions.Left);
        sp.music = MakeSlider(panel, 290);
        Txt(Box("SfxLabel", panel, 60, 360, 640, 34), "Звуки", 26, Palette.Ink, body, TextAlignmentOptions.Left);
        sp.sfx = MakeSlider(panel, 400);
        var close = Box("Close", panel, 220, 490, 320, 76);
        var cimg = Img(close, Spr(Rounded), Palette.Primary, true);
        sp.close = close.gameObject.AddComponent<Button>(); sp.close.targetGraphic = cimg;
        Txt(close, "Готово", 30, Palette.Ink, heading, TextAlignmentOptions.Center);

        PrefabUtility.SaveAsPrefabAsset(go, PanelPath);
        Object.DestroyImmediate(go);

        AddGear();
        PlaceInScene("Assets/_Project/Scenes/Menu.unity");
        PlaceInScene("Assets/_Project/Scenes/Game.unity");
        Debug.Log("Settings panel built.");
    }

    static void AddGear()
    {
        var root = PrefabUtility.LoadPrefabContents(MenuPath);
        try
        {
            var old = root.transform.Find("SettingsButton");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var r = Box("SettingsButton", root.transform, 1790, 30, 84, 84);
            var img = Img(r, Spr(Rounded), Palette.Hex("2A1810"), true);
            var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            var ic = Box("Icon", r, 18, 18, 48, 48);
            var ii = Img(ic, Spr("Icons/icons-navigation/settings"), Palette.Hex("D8E2A6")); ii.preserveAspect = true;
            root.GetComponent<MenuController>().settingsButton = b;
            PrefabUtility.SaveAsPrefabAsset(root, MenuPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void PlaceInScene(string path)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        foreach (var s in Object.FindObjectsByType<SettingsPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None)) Object.DestroyImmediate(s.gameObject);
        var canvas = GameObject.Find("Canvas");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
        inst.transform.SetAsLastSibling();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
