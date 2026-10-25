using Studio.UI.Mvp;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// UI-5 (task list): rebuilds TaskWindow after docs/mockup/generated-2026-10-24/03-tasks.png: task rows with type chip, assignee,
// status and progress; the "Команда" column with energy bars; a dark tray with build files and "Собрать игру".
// Builds TaskRow, TeamMember and FileSlot prefabs. Safe to rerun; run "Round avatars" is not needed (portraits are rounded here).
public static class TaskKit
{
    const string Screen = "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab";
    const string Dir = "Assets/_Project/Prefabs/UI/Mvp/";
    const string Kit = "Assets/_Project/Art/UI/GeneratedKit20261024/";
    static readonly Color Cream = Palette.Hex("EFF2DF"), Olive = Palette.Hex("718733"), Brown = Palette.Hex("5B4A3A");
    static TMP_FontAsset heading, body;

    static Sprite Spr(string p) => AssetDatabase.LoadAssetAtPath<Sprite>(p.StartsWith("Assets") ? p : Kit + p + ".png");
    static Sprite Rounded => Spr("Assets/_Project/Art/UI/rounded.png");

    static RectTransform Node(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static Image Img(RectTransform r, Sprite s, Color c, float mult = 1.5f, bool raycast = false)
    {
        var i = r.gameObject.AddComponent<Image>();
        i.sprite = s; i.color = c; i.raycastTarget = raycast;
        if (s != null && s.border != Vector4.zero) { i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = mult; }
        return i;
    }

    static TMP_Text Txt(Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.MidlineLeft)
    {
        var r = Node("Text", parent);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = al; t.raycastTarget = false;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        return t;
    }

    static TMP_Text TxtN(string name, Transform parent, string text, float size, Color c, TMP_FontAsset font, TextAlignmentOptions al = TextAlignmentOptions.MidlineLeft)
    {
        var t = Txt(parent, text, size, c, font, al); t.name = name; return t;
    }

    static LayoutElement Le(Component c, float w = -1, float h = -1, float flexW = -1, float flexH = -1)
    {
        var le = c.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
        if (w >= 0) le.preferredWidth = w;
        if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
        if (flexW >= 0) le.flexibleWidth = flexW;
        if (flexH >= 0) le.flexibleHeight = flexH;
        return le;
    }

    static T Group<T>(RectTransform r, float spacing, int l = 0, int t = 0, int rt = 0, int b = 0, TextAnchor a = TextAnchor.MiddleLeft) where T : HorizontalOrVerticalLayoutGroup
    {
        var g = r.gameObject.AddComponent<T>();
        g.spacing = spacing; g.padding = new RectOffset(l, rt, t, b); g.childAlignment = a;
        g.childControlWidth = g.childControlHeight = true; g.childForceExpandWidth = g.childForceExpandHeight = false;
        return g;
    }

    static Button Flat(RectTransform r, Color color, Color? outline = null)
    {
        var img = Img(r, Rounded, color, 1f, true);
        var b = r.gameObject.AddComponent<Button>();
        b.targetGraphic = img; b.transition = Selectable.Transition.ColorTint;
        var cb = b.colors; cb.highlightedColor = new Color(0.92f, 0.92f, 0.92f, 1); cb.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1); cb.disabledColor = new Color(0.66f, 0.66f, 0.66f, 1); b.colors = cb;
        if (outline.HasValue) { var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = outline.Value; ol.effectDistance = new Vector2(2, -2); }
        return b;
    }

    // round portrait holder: dark circle with a mask, the picture is the child "Face"
    static Image Portrait(Transform parent, float size)
    {
        var holder = Node("Avatar", parent);
        Img(holder, Spr("Assets/_Project/Art/UI/avatar-circle.png"), Palette.Hex("2A1810"));
        holder.gameObject.AddComponent<Mask>().showMaskGraphic = true;
        Le(holder, size, size);
        var face = Node("Face", holder); face.anchorMin = Vector2.zero; face.anchorMax = Vector2.one; face.offsetMin = face.offsetMax = Vector2.zero;
        var fi = face.gameObject.AddComponent<Image>(); fi.raycastTarget = false;
        return fi;
    }

    static GameObject Save(RectTransform r, string name)
    {
        var p = PrefabUtility.SaveAsPrefabAsset(r.gameObject, Dir + name + ".prefab");
        Object.DestroyImmediate(r.gameObject);
        return p;
    }

    [MenuItem("Tools/UI/Apply kit to tasks")]
    public static void Apply()
    {
        heading = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Heading SDF.asset");
        body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/Body SDF.asset");
        var row = Save(Row(), "TaskRow").GetComponent<TaskRowView>();
        var member = Save(Member(), "TeamMember").GetComponent<TeamMemberView>();
        var slot = Save(Slot(), "FileSlot").GetComponent<Image>();

        var root = PrefabUtility.LoadPrefabContents(Screen);
        try
        {
            var win = (RectTransform)root.transform.Find("Body/TaskWindow");
            var tw = win.GetComponent<TaskWindow>();
            for (int i = win.childCount - 1; i >= 0; i--) Object.DestroyImmediate(win.GetChild(i).gameObject);

            // centre: ivory workspace with the task rows
            var main = Node("Main", win); main.anchorMin = new Vector2(0, 0); main.anchorMax = new Vector2(0, 1); main.pivot = new Vector2(0, 0.5f);
            main.offsetMin = new Vector2(0, 128); main.offsetMax = new Vector2(1296, 0);
            Img(main, Spr("Panels/panels/workspace-ivory"), Color.white);
            var title = TxtN("Title", main, "Делаем первую игру", 54, Palette.Ink, heading);
            Anchor(title.transform, 44, 26, 1100, 72);
            var hint = TxtN("Hint", main, "Задачи идут сверху вниз. «Выше» поднимает задачу в очереди.", 22, Brown, body);
            Anchor(hint.transform, 48, 98, 1100, 34);
            var list = Node("List", main); list.anchorMin = new Vector2(0, 0); list.anchorMax = new Vector2(1, 1);
            list.offsetMin = new Vector2(30, 20); list.offsetMax = new Vector2(-30, -150);
            var vl = Group<VerticalLayoutGroup>(list, 14, 0, 0, 0, 0, TextAnchor.UpperLeft);
            vl.childForceExpandWidth = true;

            // right: olive "Команда" column
            var team = Node("Team", win); team.anchorMin = new Vector2(1, 0); team.anchorMax = new Vector2(1, 1); team.pivot = new Vector2(1, 0.5f);
            team.offsetMin = new Vector2(-556, 128); team.offsetMax = new Vector2(0, 0);
            Img(team, Spr("Panels/panels/sidebar-olive"), Color.white);
            var tt = TxtN("Title", team, "Команда", 46, Cream, heading);
            Anchor(tt.transform, 36, 24, 400, 62);
            var grid = Node("Grid", team); grid.anchorMin = Vector2.zero; grid.anchorMax = Vector2.one;
            grid.offsetMin = new Vector2(24, 24); grid.offsetMax = new Vector2(-24, -100);
            var tg = Group<VerticalLayoutGroup>(grid, 14, 0, 0, 0, 0, TextAnchor.UpperLeft); tg.childForceExpandWidth = true;

            // bottom tray: build files and the build button
            var tray = Node("Tray", win); tray.anchorMin = new Vector2(0, 0); tray.anchorMax = new Vector2(1, 0); tray.pivot = new Vector2(0.5f, 0);
            tray.offsetMin = new Vector2(0, 0); tray.offsetMax = new Vector2(0, 112);
            Img(tray, Rounded, Palette.Hex("2A1810"), 1f);
            var tol = tray.gameObject.AddComponent<Outline>(); tol.effectColor = Palette.Hex("6B5240"); tol.effectDistance = new Vector2(2, -2);
            var folder = Node("Folder", tray); Anchor(folder, 24, 20, 72, 72);
            var fi = Img(folder, Spr("Decor/decor/folder"), Color.white); fi.preserveAspect = true;
            var lab = TxtN("FilesLabel", tray, "Файлы для сборки: 0 / 4", 30, Cream, heading);
            Anchor(lab.transform, 112, 12, 600, 40);
            var reason = TxtN("Reason", tray, "", 20, Palette.Hex("D8E2A6"), body, TextAlignmentOptions.TopLeft);
            Anchor(reason.transform, 112, 54, 620, 52);
            var slots = Node("Slots", tray); Anchor(slots, 760, 22, 520, 68);
            Group<HorizontalLayoutGroup>(slots, 12, 0, 0, 0, 0, TextAnchor.MiddleLeft);
            var bt = Node("Build", tray); bt.anchorMin = bt.anchorMax = new Vector2(1, 0.5f); bt.pivot = new Vector2(1, 0.5f); bt.sizeDelta = new Vector2(380, 76); bt.anchoredPosition = new Vector2(-24, 0);
            tw.build = Flat(bt, Palette.Primary, Palette.Hex("B89A1E"));
            var bl = Txt(bt, "Собрать игру", 30, Palette.Ink, heading, TextAlignmentOptions.Center);

            tw.list = list; tw.teamGrid = grid; tw.slots = slots;
            tw.rowPrefab = row; tw.memberPrefab = member; tw.slotPrefab = slot;
            tw.fileDesign = Spr("Icons/icons-concept/file-image"); tw.fileMechanics = Spr("Icons/icons-concept/file-code"); tw.fileSound = Spr("Icons/icons-concept/file-audio");
            tw.buildReason = reason; tw.filesLabel = lab;
            PrefabUtility.SaveAsPrefabAsset(root, Screen);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        Debug.Log("Task list rebuilt from the kit.");
    }

    // top-left anchored placement
    static void Anchor(Transform t, float x, float y, float w, float h)
    {
        var r = (RectTransform)t;
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(w, h);
    }

    static RectTransform Row()
    {
        var r = Node("TaskRow", null); r.sizeDelta = new Vector2(1200, 124);
        var v = r.gameObject.AddComponent<TaskRowView>();
        v.background = Img(r, Rounded, Palette.Hex("FBF8EA"), 1f);
        var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = Palette.Hex("CFCBA6"); ol.effectDistance = new Vector2(2, -2);
        Group<HorizontalLayoutGroup>(r, 18, 22, 10, 18, 10);
        Le(r, -1, 124);

        v.number = TxtN("Number", r, "01", 40, Palette.Hex("2A1810"), heading, TextAlignmentOptions.Center); v.number.textWrappingMode = TextWrappingModes.NoWrap; Le(v.number, 84);

        var tb = Node("TitleBlock", r); Group<VerticalLayoutGroup>(tb, 8, 0, 0, 0, 0, TextAnchor.MiddleLeft); Le(tb, 430, -1, 0);
        v.title = TxtN("Title", tb, "Задача", 27, Palette.Ink, heading, TextAlignmentOptions.MidlineLeft);
        v.title.textWrappingMode = TextWrappingModes.Normal; v.title.fontSize = 25; v.title.maxVisibleLines = 2; v.title.overflowMode = TextOverflowModes.Ellipsis; v.title.alignment = TextAlignmentOptions.BottomLeft; Le(v.title, -1, 66);
        var chipBox = Node("Chip", tb); v.chipBg = Img(chipBox, Rounded, Palette.Hex("F4B9B2"), 1f); Le(chipBox, 150, 32);
        v.chip = Txt(chipBox, "Дизайн", 19, Palette.Ink, heading, TextAlignmentOptions.Center);

        v.avatar = Portrait(r, 64);

        var wb = Node("Who", r); Group<VerticalLayoutGroup>(wb, 2, 0, 0, 0, 0, TextAnchor.MiddleLeft).childForceExpandWidth = true; Le(wb, 150);
        v.who = TxtN("Name", wb, "Лера", 26, Palette.Ink, heading); Le(v.who, -1, 32);
        v.role = TxtN("Role", wb, "Дизайн", 19, Brown, body); v.role.textWrappingMode = TextWrappingModes.NoWrap; Le(v.role, -1, 26);

        var sb = Node("StatusBlock", r); Group<VerticalLayoutGroup>(sb, 10, 0, 0, 0, 0, TextAnchor.MiddleLeft).childForceExpandWidth = true; Le(sb, 230, -1, 1);
        var stBox = Node("Status", sb); v.statusBg = Img(stBox, Rounded, Palette.Hex("D5E6F7"), 1f); Le(stBox, -1, 36);
        v.status = Txt(stBox, "в работе", 20, Palette.Ink, heading, TextAlignmentOptions.Center);
        var bar = Node("Bar", sb); Group<HorizontalLayoutGroup>(bar, 10, 0, 0, 0, 0, TextAnchor.MiddleLeft); Le(bar, -1, 26);
        var track = Node("Track", bar); Img(track, Rounded, Palette.Hex("E3E0CC"), 1f); Le(track, -1, 14, 1);
        var fill = Node("Fill", track); fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
        Img(fill, Rounded, Olive, 1f);
        v.fill = fill;
        v.percent = TxtN("Percent", bar, "0%", 20, Palette.Ink, heading, TextAlignmentOptions.MidlineRight); Le(v.percent, 64);

        var bb = Node("Buttons", r); Group<VerticalLayoutGroup>(bb, 8, 0, 0, 0, 0, TextAnchor.MiddleCenter).childForceExpandWidth = true; Le(bb, 150);
        var a = Node("Assign", bb); v.assign = Flat(a, Palette.Primary, Palette.Hex("B89A1E")); Le(a, -1, 46);
        v.assignLabel = Txt(a, "Назначить", 20, Palette.Ink, heading, TextAlignmentOptions.Center);
        var u = Node("Up", bb); v.up = Flat(u, Palette.Hex("E7E3CC"), Palette.Hex("CFCBA6")); Le(u, -1, 40);
        Txt(u, "Выше", 19, Palette.Ink, heading, TextAlignmentOptions.Center);
        return r;
    }

    static RectTransform Member()
    {
        var r = Node("TeamMember", null); r.sizeDelta = new Vector2(500, 134);
        var v = r.gameObject.AddComponent<TeamMemberView>();
        Img(r, Rounded, Palette.Hex("FBF8EA"), 1f);
        Le(r, -1, 134);
        var content = Node("Content", r); content.anchorMin = Vector2.zero; content.anchorMax = Vector2.one; content.offsetMin = content.offsetMax = Vector2.zero;
        Group<HorizontalLayoutGroup>(content, 16, 18, 12, 18, 12);
        v.content = content.gameObject;
        v.avatar = Portrait(content, 92);
        var col = Node("Col", content); Group<VerticalLayoutGroup>(col, 4, 0, 0, 0, 0, TextAnchor.MiddleLeft); Le(col, -1, -1, 1);
        var head = Node("Head", col); Group<HorizontalLayoutGroup>(head, 10, 0, 0, 0, 0, TextAnchor.LowerLeft); Le(head, -1, 36);
        v.title = TxtN("Name", head, "Лера", 30, Palette.Ink, heading); Le(v.title, -1, 36);
        v.role = TxtN("Role", head, "Дизайн", 20, Olive, heading); Le(v.role, -1, 30, 1);
        var er = Node("Energy", col); Group<HorizontalLayoutGroup>(er, 10, 0, 0, 0, 0, TextAnchor.MiddleLeft); Le(er, -1, 28);
        var ic = Node("Icon", er); var ii = Img(ic, Spr("Icons/icons-navigation/energy"), Color.white); ii.preserveAspect = true; Le(ic, 30, 24);
        var track = Node("Track", er); Img(track, null, Palette.Hex("E3E0CC"), 1f); Le(track, -1, 14, 1);
        var fill = Node("Fill", track); fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
        v.energyFill = Img(fill, null, Olive, 1f); v.fillRect = fill;
        v.energy = TxtN("Value", er, "100/100", 19, Palette.Ink, heading, TextAlignmentOptions.MidlineRight); v.energy.textWrappingMode = TextWrappingModes.NoWrap; Le(v.energy, 112);
        v.state = TxtN("State", col, "Свободен(на)", 20, Brown, body); Le(v.state, -1, 28);
        v.empty = TxtN("Empty", r, "Свободное место\nнайми в хедхантере", 22, Brown, heading, TextAlignmentOptions.Center);
        v.empty.gameObject.SetActive(false);
        return r;
    }

    static RectTransform Slot()
    {
        var r = Node("FileSlot", null); r.sizeDelta = new Vector2(68, 68);
        Img(r, Rounded, Palette.Hex("3A2A20"), 1f);
        var ol = r.gameObject.AddComponent<Outline>(); ol.effectColor = Palette.Hex("6B5240"); ol.effectDistance = new Vector2(2, -2);
        Le(r, 68, 68);
        var ic = Node("Icon", r); ic.anchorMin = new Vector2(0.5f, 0.5f); ic.anchorMax = new Vector2(0.5f, 0.5f); ic.sizeDelta = new Vector2(46, 46);
        var i = Img(ic, null, Color.white); i.preserveAspect = true;
        ic.gameObject.SetActive(false);
        return r;
    }
}
