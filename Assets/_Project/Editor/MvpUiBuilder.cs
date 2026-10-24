using Studio.Core;
using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-shot generator of the MVP screen prefabs (Prefabs/UI/Mvp) and their placement in the Game scene.
// After the first run the prefabs are the source of truth and are edited in Unity by hand.
public static class MvpUiBuilder
{
    const string Dir = "Assets/_Project/Prefabs/UI/Mvp";
    static TMP_FontAsset font;
    static Sprite rounded;

    [MenuItem("Tools/Mvp/Build UI prefabs and place in Game")]
    public static void Build()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/UIFont SDF.asset");
        rounded = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/rounded.png");
        if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/_Project/Prefabs/UI", "Mvp");

        ImportSprites();
        var rowLight = Save(RowPrefab("RowLight", Palette.Cream, Palette.Ink, Palette.Dim, false), "RowLight").GetComponent<RowView>();
        var rowOlive = Save(RowPrefab("RowOlive", Palette.Olive, Palette.Cream, Palette.Light, false), "RowOlive").GetComponent<RowView>();
        var rowAvatar = Save(RowPrefab("RowAvatar", Palette.Cream, Palette.Ink, Palette.Dim, true), "RowAvatar").GetComponent<RowView>();
        var option = Save(OptionPrefab(), "Option");
        var block = Save(BlockPrefab(), "Block");
        var link = Save(LinkPrefab(), "Link");
        var screen = Save(ScreenPrefab(rowLight, rowOlive, rowAvatar, option.GetComponent<Button>(), block.GetComponent<BlockView>(), link.GetComponent<LinkView>()), "MvpScreen");
        AssetDatabase.DeleteAsset(Dir + "/Row.prefab");

        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Game.unity");
        var canvas = Object.FindObjectOfType<Canvas>();
        foreach (Transform c in canvas.transform) if (c.name != "MvpScreen") c.gameObject.SetActive(false);
        var old = canvas.transform.Find("MvpScreen");
        if (old) Object.DestroyImmediate(old.gameObject);
        var inst = (GameObject)PrefabUtility.InstantiatePrefab(screen, canvas.transform);
        Stretch((RectTransform)inst.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.OpenScene("Assets/_Project/Scenes/Menu.unity");
        Debug.Log("MVP UI built");
    }

    static GameObject Save(GameObject go, string name)
    {
        var p = PrefabUtility.SaveAsPrefabAsset(go, Dir + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return p;
    }

    // ---------- helpers
    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        if (parent) go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform r, float l = 0, float t = 0, float rr = 0, float b = 0)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rr, -t);
    }

    static void Place(RectTransform r, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        r.anchorMin = aMin; r.anchorMax = aMax; r.offsetMin = oMin; r.offsetMax = oMax;
    }

    static Image Img(RectTransform r, Color c, bool raycast = true)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = rounded; i.type = Image.Type.Sliced; i.color = c; i.raycastTarget = raycast;
        return i;
    }

    static TMP_Text Txt(Transform parent, string name, string text, float size, Color c, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var r = Node(name, parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = align; t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        return t;
    }

    static Button Btn(Transform parent, string name, string label, Color bg, out TMP_Text text, float size = 26)
    {
        var r = Node(name, parent);
        var img = Img(r, bg);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors; cb.disabledColor = new Color(1, 1, 1, 0.35f); b.colors = cb;
        text = Txt(r, "Label", label, size, Palette.Cream, TextAlignmentOptions.Center);
        Stretch((RectTransform)text.transform, 10, 4, 10, 4);
        return b;
    }

    static LayoutElement Size(Component c, float w = -1, float h = -1, float flexW = -1)
    {
        var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = w;
        if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
        if (flexW >= 0) le.flexibleWidth = flexW;
        return le;
    }

    static T Layout<T>(Component c, int spacing, int pad) where T : HorizontalOrVerticalLayoutGroup
    {
        var g = c.gameObject.AddComponent<T>();
        g.spacing = spacing; g.padding = new RectOffset(pad, pad, pad, pad);
        g.childControlWidth = g.childControlHeight = true;
        g.childForceExpandWidth = typeof(T) == typeof(VerticalLayoutGroup);
        g.childForceExpandHeight = false;
        return g;
    }

    // Scroll area with a vertical list as content; returns the content transform.
    static Transform Scroll(Transform parent, string name, out ScrollRect sr)
    {
        var r = Node(name, parent);
        Img(r, Palette.Light);
        sr = r.gameObject.AddComponent<ScrollRect>();
        sr.horizontal = false; sr.scrollSensitivity = 40;
        var vp = Node("Viewport", r);
        Stretch(vp, 8, 8, 8, 8);
        vp.gameObject.AddComponent<RectMask2D>();
        var vpImg = vp.gameObject.AddComponent<Image>(); vpImg.color = new Color(0, 0, 0, 0);
        var content = Node("Content", vp);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
        content.offsetMin = content.offsetMax = Vector2.zero;
        Layout<VerticalLayoutGroup>(content, 10, 6);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.viewport = vp; sr.content = content;
        return content;
    }

    // ---------- prefabs (look of the author's mockups: docs/progress/*-reference, P-14 sprites)
    static Sprite Spr(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/" + name + ".png");

    static void ImportSprites()
    {
        foreach (var n in new[] { "note", "icon-hire", "icon-tasks", "icon-chat", "icon-review", "icon-dashboard", "logo" })
        {
            var path = "Assets/_Project/Art/UI/Mvp/" + n + ".png";
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            if (ti == null || (ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single)) continue;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true; ti.mipmapEnabled = false;
            ti.SaveAndReimport();
        }
    }

    static Image Panel(RectTransform r, string sprite, Color c, bool raycast = true)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = Spr(sprite); i.type = Image.Type.Sliced; i.color = c; i.raycastTarget = raycast;
        return i;
    }

    static Image Icon(Transform parent, string name, float size)
    {
        var r = Node("Icon", parent);
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = Spr("Mvp/" + name); i.preserveAspect = true; i.raycastTarget = false;
        Size(i, size, size);
        return i;
    }

    // Pill button with dark text (mockup tabs and menu buttons).
    static Button Pill(Transform parent, string name, string label, Color bg, out TMP_Text text, float size = 26, string icon = null)
    {
        var r = Node(name, parent);
        var img = Panel(r, "rounded", bg);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img;
        var cb = b.colors; cb.disabledColor = new Color(1, 1, 1, 0.45f); cb.highlightedColor = new Color(1, 1, 1, 0.9f); b.colors = cb;
        var h = Layout<HorizontalLayoutGroup>(r, 10, 10);
        h.childAlignment = TextAnchor.MiddleCenter; h.childForceExpandWidth = false;
        if (icon != null) Icon(r, icon, 40).color = Palette.Ink;
        text = Txt(r, "Label", label, size, Palette.Ink, TextAlignmentOptions.Center);
        return b;
    }

    static GameObject RowPrefab(string name, Color bg, Color fg, Color sub, bool avatar)
    {
        var r = Node(name, null);
        r.sizeDelta = new Vector2(1000, 96);
        var v = r.gameObject.AddComponent<RowView>();
        v.background = Panel(r, "rounded", bg);
        v.main = r.gameObject.AddComponent<Button>();
        v.main.transition = Selectable.Transition.None;
        var h = Layout<HorizontalLayoutGroup>(r, 16, 16);
        h.childAlignment = TextAnchor.MiddleLeft;
        if (avatar)
        {
            var av = Node("Avatar", r);
            var ai = av.gameObject.AddComponent<Image>(); ai.sprite = Spr("circle"); ai.color = Palette.Ink; ai.raycastTarget = false;
            Size(ai, 64, 64);
        }
        var texts = Node("Texts", r);
        Layout<VerticalLayoutGroup>(texts, 4, 0);
        Size(texts, flexW: 1);
        v.title = Txt(texts, "Title", "Title", 26, fg);
        v.subtitle = Txt(texts, "Subtitle", "", 20, sub);
        v.right = Txt(r, "Right", "", 26, fg, TextAlignmentOptions.MidlineRight);
        Size(v.right, 170);
        v.button1 = Pill(r, "Button1", "Действие", Palette.Yellow, out v.button1Label, 22);
        Size(v.button1, 250, 58);
        v.button2 = Pill(r, "Button2", "Действие", Palette.Cream, out v.button2Label, 22);
        Size(v.button2, 230, 58);
        return r.gameObject;
    }

    static GameObject OptionPrefab()
    {
        var r = Node("Option", null);
        var img = Panel(r, "rounded", Palette.Cream);
        var outline = r.gameObject.AddComponent<Outline>(); outline.effectColor = Palette.Light; outline.effectDistance = new Vector2(4, -4);
        var b = r.gameObject.AddComponent<Button>(); b.targetGraphic = img;
        var t = Txt(r, "Label", "Вариант", 28, Palette.Ink, TextAlignmentOptions.Center);
        Stretch((RectTransform)t.transform, 16, 4, 16, 4);
        Size(b, -1, 72);
        return r.gameObject;
    }

    static GameObject BlockPrefab()
    {
        var r = Node("Block", null);
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.sizeDelta = new Vector2(230, 230);
        var v = r.gameObject.AddComponent<BlockView>();
        v.background = r.gameObject.AddComponent<Image>();
        v.background.sprite = Spr("Mvp/note");
        v.type = Txt(r, "Type", "Жанр", 22, Palette.Ink, TextAlignmentOptions.Top);
        Stretch((RectTransform)v.type.transform, 30, 62, 30, 120);
        v.value = Txt(r, "Value", "–", 28, Palette.Ink, TextAlignmentOptions.Center);
        Stretch((RectTransform)v.value.transform, 26, 96, 26, 26);
        v.value.fontStyle = FontStyles.Bold;
        v.value.enableAutoSizing = true; v.value.fontSizeMin = 16; v.value.fontSizeMax = 28;
        return r.gameObject;
    }

    static GameObject LinkPrefab()
    {
        var r = Node("Link", null);
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.sizeDelta = new Vector2(100, 14);
        var v = r.gameObject.AddComponent<LinkView>();
        v.line = r.gameObject.AddComponent<Image>();
        v.button = r.gameObject.AddComponent<Button>();
        v.button.targetGraphic = v.line;
        var bg = Node("Plate", r); bg.sizeDelta = new Vector2(96, 34); bg.anchoredPosition = new Vector2(0, 28); Panel(bg, "rounded", Palette.Ink, false);
        v.label = Txt(r, "Label", "+1 / –0", 22, Palette.Cream, TextAlignmentOptions.Center);
        var lr = (RectTransform)v.label.transform;
        lr.sizeDelta = new Vector2(140, 34); lr.anchoredPosition = new Vector2(0, 28);
        return r.gameObject;
    }

    static Transform List(Transform parent, string name, out ScrollRect sr)
    {
        var content = Scroll(parent, name, out sr);
        Object.DestroyImmediate(sr.GetComponent<Image>());
        return content;
    }

    static GameObject ScreenPrefab(RowView rowLight, RowView rowOlive, RowView rowAvatar, Button option, BlockView block, LinkView link)
    {
        var root = Node("MvpScreen", null);
        root.sizeDelta = new Vector2(1920, 1080);
        var bg = root.gameObject.AddComponent<Image>(); bg.sprite = Spr("studio-background"); bg.color = Color.white;
        var game = root.gameObject.AddComponent<GameLoop>();
        game.level = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");

        // ---- top bar: pills, day/budget plate, end-day button (mockups 1, 2, 4)
        var top = root.gameObject.AddComponent<TopBarMvp>();
        top.game = game;
        var tabs = Node("Tabs", root);
        Place(tabs, new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -265), new Vector2(920, -140));
        var tl = Layout<HorizontalLayoutGroup>(tabs, 22, 0); tl.childAlignment = TextAnchor.MiddleLeft; tl.childForceExpandWidth = false;
        string[] names = { "Чат", "Дэшборд", "Найм", "Задачи" };
        string[] icons = { "icon-chat", "icon-dashboard", "icon-hire", "icon-tasks" };
        for (int i = 0; i < 4; i++) { top.tabs[i] = Pill(tabs, "Tab" + i, names[i], Palette.Mustard, out _, 26, icons[i]); Size(top.tabs[i], 196, 76); }
        var info = Node("Info", root);
        Place(info, new Vector2(0, 1), new Vector2(0, 1), new Vector2(944, -265), new Vector2(1483, -147));
        Panel(info, "panel-diagonal", Palette.Olive, false);
        top.dayText = Txt(info, "Day", "День 1", 30, Palette.Cream, TextAlignmentOptions.Center);
        Place((RectTransform)top.dayText.transform, Vector2.zero, new Vector2(0.45f, 1), new Vector2(10, 6), new Vector2(-6, -6));
        var div = Node("Divider", info); Place(div, new Vector2(0.45f, 0), new Vector2(0.45f, 1), new Vector2(-2, 10), new Vector2(2, -8));
        div.gameObject.AddComponent<Image>().color = Palette.Light;
        top.budgetText = Txt(info, "Budget", "100 000", 30, Palette.Cream, TextAlignmentOptions.Center);
        Place((RectTransform)top.budgetText.transform, new Vector2(0.45f, 0), Vector2.one, new Vector2(6, 6), new Vector2(-10, -6));
        top.endDay = Pill(root, "EndDay", "Закончить день", Palette.Yellow, out _, 30);
        Object.DestroyImmediate(top.endDay.GetComponent<HorizontalLayoutGroup>());
        top.endDay.GetComponentInChildren<TMP_Text>().rectTransform.anchorMin = Vector2.zero;
        Stretch(top.endDay.GetComponentInChildren<TMP_Text>().rectTransform, 10, 4, 10, 4);
        Place((RectTransform)top.endDay.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(1503, -265), new Vector2(1847, -140));
        top.pause = Pill(root, "Pause", "Пауза", Palette.Mustard, out _, 22);
        Place((RectTransform)top.pause.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(1687, -110), new Vector2(1847, -50));
        top.hint = Txt(root, "Hint", "", 22, Palette.Light, TextAlignmentOptions.MidlineLeft);
        Place((RectTransform)top.hint.transform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(56, -120), new Vector2(900, -60));

        var body = Node("Body", root);
        Place(body, Vector2.zero, Vector2.one, new Vector2(52, 68), new Vector2(-63, -300));

        // ---- modal: green title plate from the menu, cream pill options (mockup 3)
        var modalRoot = Node("Modal", root);
        Stretch(modalRoot);
        var dim = modalRoot.gameObject.AddComponent<Image>(); dim.color = new Color(0.08f, 0.04f, 0, 0.7f);
        var panel = Node("Panel", modalRoot);
        Place(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-560, -400), new Vector2(560, 400));
        var pimg = panel.gameObject.AddComponent<Image>(); pimg.sprite = Spr("menu-title-plate"); pimg.type = Image.Type.Simple;
        var pv = Layout<VerticalLayoutGroup>(panel, 16, 56);
        var modal = root.gameObject.AddComponent<ModalView>();
        modal.root = modalRoot.gameObject;
        modal.title = Txt(panel, "Title", "Заголовок", 40, Palette.Cream);
        modal.title.fontStyle = FontStyles.Bold;
        modal.body = Txt(panel, "Body", "Текст", 26, Palette.Cream);
        Size(modal.body).flexibleHeight = 1;
        var opts = Node("Options", panel);
        Layout<VerticalLayoutGroup>(opts, 12, 0);
        modal.options = opts;
        modal.optionPrefab = option;
        game.modal = modal;

        // ---- chat (mockup 4): olive list with cream rows, olive feed with light header and bubbles
        var chat = Node("ChatWindow", body); Stretch(chat);
        var cw = chat.gameObject.AddComponent<ChatWindow>(); cw.game = game;
        var listArea = Node("ListArea", chat); Place(listArea, Vector2.zero, new Vector2(0, 1), new Vector2(9, 0), new Vector2(531, 0));
        Panel(listArea, "panel-round", Palette.Olive, false);
        cw.chatList = List(listArea, "Chats", out var srList); Stretch((RectTransform)srList.transform, 4, 90, 4, 20);
        cw.chatRowPrefab = rowAvatar;
        var feedArea = Node("FeedArea", chat); Stretch(feedArea, 562, 0, 0, 0);
        Panel(feedArea, "panel-right", Palette.Olive, false);
        var head = Node("Header", feedArea); Place(head, new Vector2(0, 1), Vector2.one, new Vector2(9, -126), new Vector2(-9, -8));
        Panel(head, "panel-right", Palette.Light, false);
        var hav = Node("Avatar", head); Place(hav, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, -44), new Vector2(108, 44));
        var havi = hav.gameObject.AddComponent<Image>(); havi.sprite = Spr("circle"); havi.color = Palette.Ink;
        cw.header = Txt(head, "Name", "Борис Петрович", 32, Palette.Ink, TextAlignmentOptions.MidlineLeft);
        Place(cw.header.rectTransform, Vector2.zero, Vector2.one, new Vector2(130, 0), new Vector2(-20, 0));
        cw.feed = List(feedArea, "Feed", out cw.feedScroll); Stretch((RectTransform)cw.feedScroll.transform, 9, 140, 9, 10);
        cw.messagePrefab = rowLight;

        // ---- dashboard (mockup 2): light board, cream palette column with square tiles, sticky notes
        var dash = Node("DashboardWindow", body); Stretch(dash);
        var dw = dash.gameObject.AddComponent<DashboardWindow>(); dw.game = game;
        Panel(dash, "panel-bottom", Palette.Light, false);
        var pal = Node("Palette", dash); Place(pal, Vector2.zero, new Vector2(0, 1), new Vector2(25, 28), new Vector2(165, -37));
        Panel(pal, "rounded", Palette.Cream, false);
        var pl = Layout<VerticalLayoutGroup>(pal, 8, 12); pl.childForceExpandHeight = true;
        for (int i = 0; i < 6; i++)
        {
            var tile = Node("Type" + i, pal);
            var timg = Panel(tile, "rounded", Color.white);
            var sh = tile.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0.78f, 0.82f, 0.72f); sh.effectDistance = new Vector2(3, -5);
            var tb = tile.gameObject.AddComponent<Button>(); tb.targetGraphic = timg;
            var cb = tb.colors; cb.disabledColor = new Color(1, 1, 1, 0.35f); tb.colors = cb;
            var tt = Txt(tile, "Label", Names.Of((BlockType)i), 20, Palette.Ink, TextAlignmentOptions.Center);
            tt.enableAutoSizing = true; tt.fontSizeMin = 12; tt.fontSizeMax = 20;
            Stretch(tt.rectTransform, 4, 4, 4, 4);
            dw.palette[i] = tb;
        }
        var cv = Node("Canvas", dash); Stretch(cv, 190, 20, 20, 20);
        cv.gameObject.AddComponent<RectMask2D>();
        var cvImg = cv.gameObject.AddComponent<Image>(); cvImg.color = new Color(0, 0, 0, 0); cvImg.raycastTarget = false;
        dw.canvas = cv;
        var ll = Node("Links", cv); Stretch(ll); ll.pivot = new Vector2(0, 1);
        dw.linkLayer = ll;
        dw.blockPrefab = block; dw.linkPrefab = link;
        var side = Node("Side", dash); Place(side, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-380, 28), new Vector2(-28, -37));
        var sl = Layout<VerticalLayoutGroup>(side, 14, 0); sl.childAlignment = TextAnchor.UpperRight;
        var totalsCard = Node("TotalsCard", side); Panel(totalsCard, "rounded", Palette.Olive, false);
        Layout<VerticalLayoutGroup>(totalsCard, 6, 18);
        dw.totals = Txt(totalsCard, "Totals", "Итог концепта", 26, Palette.Cream);
        dw.linkMode = Pill(side, "LinkMode", "Связать блоки", Palette.Cream, out dw.mode, 24); Size(dw.linkMode, -1, 70);
        dw.reason = Txt(side, "Reason", "", 22, Palette.Ink);
        var help = Txt(side, "Help", "Тип слева – заметка на доске. Нажми заметку – выбери параметр, тяни – двигай. «Связать блоки», затем две заметки – линия. Нажми линию – что даёт сочетание. Зелёная: плюсов больше, красная: иначе.", 20, Palette.Dim);
        Size(help).flexibleHeight = 1;
        dw.approve = Pill(side, "Approve", "Утвердить концепт", Palette.Yellow, out _, 28); Size(dw.approve, -1, 96);

        // ---- headhunter and tasks share mockup 1: olive left column, light centre with olive rows, team cards, yellow button
        var hire = Node("HireWindow", body); Stretch(hire);
        var hw = hire.gameObject.AddComponent<HireWindow>(); hw.game = game; hw.top = top;
        ThreeColumns(hire, "Кандидаты", "Найм", "Кого берём?\n\nСкорость – как быстро\nКачество – насколько хорошо\nНочь – может ли работать ночью\n\nПлатишь за каждый рабочий день.", rowLight, new RowView[4], out var hireButton, out _);
        hw.toTasks = hireButton; hireButton.GetComponentInChildren<TMP_Text>().text = "К задачам";

        var tasks = Node("TaskWindow", body); Stretch(tasks);
        var tw = tasks.gameObject.AddComponent<TaskWindow>(); tw.game = game;
        tw.list = ThreeColumns(tasks, "Задачи", "Задачи", "", rowLight, tw.team, out tw.build, out tw.summary);
        tw.build.GetComponentInChildren<TMP_Text>().text = "Собрать игру";
        tw.rowPrefab = rowOlive;
        tw.buildReason = Txt(tasks, "BuildReason", "", 22, Palette.Ink);
        Place(tw.buildReason.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(330, 10), new Vector2(1380, 60));

        top.windows = new[] { chat.gameObject, dash.gameObject, hire.gameObject, tasks.gameObject };
        modalRoot.SetAsLastSibling();
        return root.gameObject;
    }

    // Left olive column (title + note), light centre list, right olive column with 4 team cards and a yellow button below.
    static Transform ThreeColumns(RectTransform win, string centreTitle, string leftTitle, string leftNote, RowView card, RowView[] team, out Button button, out TMP_Text leftText)
    {
        var left = Node("Left", win); Place(left, Vector2.zero, new Vector2(0, 1), Vector2.zero, new Vector2(270, 0));
        Panel(left, "panel-left", Palette.Olive, false);
        var ll = Layout<VerticalLayoutGroup>(left, 14, 14); ll.padding.top = 80;
        var lt = Node("Card", left); Panel(lt, "rounded", Palette.Cream, false); Layout<VerticalLayoutGroup>(lt, 6, 16);
        Txt(lt, "Title", leftTitle, 28, Palette.Ink).fontStyle = FontStyles.Bold;
        leftText = Txt(lt, "Text", leftNote, 20, Palette.Ink);

        var centre = Node("Centre", win); Place(centre, Vector2.zero, Vector2.one, new Vector2(292, 0), new Vector2(-407, 0));
        Panel(centre, "panel-bottom", Palette.Light, false);
        var av = Node("Avatar", centre); Place(av, new Vector2(0, 1), new Vector2(0, 1), new Vector2(43, -114), new Vector2(125, -32));
        var avi = av.gameObject.AddComponent<Image>(); avi.sprite = Spr("circle"); avi.color = Palette.Ink;
        var ct = Txt(centre, "Title", centreTitle, 34, Palette.Ink, TextAlignmentOptions.MidlineLeft);
        Place(ct.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(150, -114), new Vector2(-40, -32));
        var content = List(centre, "List", out var sr); Stretch((RectTransform)sr.transform, 36, 130, 36, 70);

        var right = Node("Right", win); Place(right, new Vector2(1, 0), Vector2.one, new Vector2(-385, 132), Vector2.zero);
        Panel(right, "panel-right", Palette.Olive, false);
        var rl = Layout<VerticalLayoutGroup>(right, 20, 20); rl.padding.top = 52;
        for (int i = 0; i < team.Length; i++)
        {
            team[i] = ((GameObject)PrefabUtility.InstantiatePrefab(card.gameObject, right)).GetComponent<RowView>();
            Size(team[i], -1, 100);
        }
        button = Pill(win, "Action", "Действие", Palette.Yellow, out _, 32);
        Object.DestroyImmediate(button.GetComponent<HorizontalLayoutGroup>());
        Stretch(button.GetComponentInChildren<TMP_Text>().rectTransform, 10, 4, 10, 4);
        Place((RectTransform)button.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-385, 0), new Vector2(0, 112));
        return content;
    }

    [MenuItem("Tools/Mvp/Use Studio web template")]
    public static void WebTemplate() { PlayerSettings.WebGL.template = "PROJECT:Studio"; AssetDatabase.SaveAssets(); }

    [MenuItem("Tools/Mvp/Put logo into MenuScreen")]
    public static void MenuLogo()
    {
        ImportSprites();
        const string path = "Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        var old = root.transform.Find("Logo");
        if (old) Object.DestroyImmediate(old.gameObject);
        var r = Node("Logo", root.transform);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(40, -30); r.sizeDelta = new Vector2(230, 230);
        var img = r.gameObject.AddComponent<Image>(); img.sprite = Spr("Mvp/logo"); img.preserveAspect = true; img.raycastTarget = false;
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
