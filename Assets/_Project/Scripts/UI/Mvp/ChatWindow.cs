using System.Collections.Generic;
using Studio.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Chats are built from LevelState.chat: the boss from the start, an employee after hiring.
    public class ChatWindow : MonoBehaviour
    {
        public GameLoop game;
        public Transform chatList, feed;
        public RowView chatRowPrefab, messagePrefab;
        public ScrollRect feedScroll;
        public TMPro.TMP_Text header;
        public Sprite bossAvatar;
        public Transform linePrefab;   // message row wrapper used to align incoming and outgoing bubbles

        readonly Dictionary<string, int> read = new Dictionary<string, int>();   // messages of each chat the player has seen
        string open = LevelState.Boss;
        int drawnCount = -1;
        string drawnChat;

        void Awake() => game.Changed += Redraw;
        void OnEnable() { drawnCount = -1; Redraw(); }

        void Redraw()
        {
            if (game.S == null || !isActiveAndEnabled) return;
            var s = game.S;
            if (s.chat.Count == drawnCount && open == drawnChat && !NeedsButtons()) return;
            drawnCount = s.chat.Count; drawnChat = open;

            foreach (Transform c in chatList) Destroy(c.gameObject);
            var ids = new List<string> { LevelState.Boss };
            foreach (var e in s.staff) ids.Add(e.id);
            foreach (var id in ids)
            {
                var row = Instantiate(chatRowPrefab, chatList);
                var c = game.L.Candidate(id);
                int unread = id == open ? 0 : Count(s, id) - (read.TryGetValue(id, out var r) ? r : 0);
                row.Set(id == LevelState.Boss ? "Борис Петрович" : c.name + " · " + Names.Of(c.d),
                        Last(s, id), unread > 0 ? "<color=#C93A3F><b>● " + unread + "</b></color>" : null);
                row.background.color = id == open ? Palette.Yellow : Palette.Cream;
                SetAvatar(row.avatar, id);
                string k = id;
                row.main.onClick.AddListener(() => { open = k; Redraw(); });
            }

            var oc = game.L.Candidate(open);
            header.text = open == LevelState.Boss ? "Борис Петрович · руководитель" : oc.name + " · " + Names.Of(oc.d);
            read[open] = Count(s, open);
            foreach (Transform c in feed) Destroy(c.gameObject);
            foreach (var m in s.chat)
            {
                if (m.chatId != open) continue;
                var line = Instantiate(linePrefab, feed);
                bool mine = m.kind == "me";
                line.GetComponent<HorizontalLayoutGroup>().childAlignment = mine ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                var row = Instantiate(messagePrefab, line);
                var le = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = Mathf.Clamp(row.title.GetPreferredValues(m.text, 820, 0).x + 64, 260, 880);   // a bubble hugs its text
                row.Set(m.text, "день " + m.day);
                row.title.font = row.subtitle.font;   // message text is body text, not a heading
                row.background.color = m.kind == "file" ? Palette.Cream : m.kind == "review" ? Palette.Yellow : Palette.Hex("FBF8EA");
                if (m.kind == "file") FileAttachment(row, m);
                else if (m.kind == "event") EventButtons(row, m);
                else if (m.kind == "me") { row.background.color = Palette.Hex("CFE8B8"); row.title.alignment = row.subtitle.alignment = TMPro.TextAlignmentOptions.TopRight; row.Button1(null, false, null); }
                else row.Button1(null, false, null);
            }
            Canvas.ForceUpdateCanvases();
            feedScroll.verticalNormalizedPosition = 0;
        }

        // Mascot portrait of a chat: the boss from this window, an employee from his CandidateDef.
        void SetAvatar(Image img, string id)
        {
            var sp = id == LevelState.Boss ? bossAvatar : (game.L.Candidate(id)?.character ?? game.L.Candidate(id)?.portrait);
            if (img == null || sp == null) return;
            var face = img.transform.Find("Face");
            if (face != null && face.TryGetComponent(out Image faceImage)) img = faceImage;
            img.sprite = sp; img.color = Color.white;
        }

        bool NeedsButtons() => false;

        static int Count(LevelState s, string chatId) => s.chat.FindAll(m => m.chatId == chatId).Count;

        static string Last(LevelState s, string id)
        {
            for (int i = s.chat.Count - 1; i >= 0; i--)
                if (s.chat[i].chatId == id) { var t = s.chat[i].text; return t.Length > 34 ? t.Substring(0, 34) + "…" : t; }
            return "";
        }

        void EventButtons(RowView row, ChatEntry m)
        {
            var def = game.L.events.Find(x => x.id == m.refId);
            if (def == null || EventRules.Find(game.S, m.refId, m.chatId) == null) return;   // already answered
            row.background.color = Palette.Hex("FBF8EA");
            row.Set(m.text);
            row.Button1(Choice(def.options[0].label, Names.Effects(def.options[0])), true, () => Pick(m, 0));
            row.Button2(Choice(def.options[1].label, Names.Effects(def.options[1])), true, () => Pick(m, 1));
            StyleChoice(row.button1, row.button1Label, Palette.Olive);
            StyleChoice(row.button2, row.button2Label, Palette.Hex("657936"));
        }

        static string Choice(string label, string effect) => label + "\n<size=18>" + effect + "</size>";

        static void StyleChoice(Button button, TMPro.TMP_Text label, Color color)
        {
            if (button == null || label == null) return;
            button.GetComponent<Image>().color = color;
            label.color = Palette.Cream;
            label.fontSize = 20;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var layout = button.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.minWidth = 250; layout.preferredWidth = 320;
                layout.minHeight = 72; layout.preferredHeight = 78;
            }
        }

        void Pick(ChatEntry m, int option)
        {
            if (!EventRules.Resolve(game.S, game.L, m.refId, m.chatId, option))
                game.modal.Show("Не хватает денег", "На такой вариант в бюджете пока нет средств. Выбери другой.", ("Понятно", null));
            game.Commit();
            // Resolving an event may not add a chat entry. Force the open conversation to redraw
            // so its two answer cards disappear as soon as the player has chosen.
            drawnCount = -1;
            Redraw();
        }

        void FixButton(RowView row, string fileId)
        {
            var s = game.S;
            bool rework = s.builds.Count > 0 && !s.builds[s.builds.Count - 1].passed && s.stage == LevelStage.Production;
            row.Button1(rework ? "На исправление" : null, true, () => FixPicker.Open(game, fileId));
        }

        void FileAttachment(RowView row, ChatEntry message)
        {
            var file = game.S.File(message.refId);
            if (file == null)
            {
                row.Set(message.text, "Вложение · день " + message.day);
                FixButton(row, message.refId);
                return;
            }

            row.Set(message.text, file.title + " · " + Names.Of(file.d));
            row.background.color = Palette.Cream;
            row.title.fontStyle = TMPro.FontStyles.Bold;
            row.subtitle.color = Palette.Hex("5B4A3A");
            if (row.right != null)
            {
                row.right.text = Mathf.RoundToInt(file.quality) + "/100";
                row.right.color = file.quality >= 70 ? Palette.Green : Palette.Primary;
                row.right.gameObject.SetActive(true);
            }
            FixButton(row, file.id);
        }
    }

    // Choose who fixes the file: any hired employee of the file's discipline.
    public static class FixPicker
    {
        public static void Open(GameLoop game, string fileId)
        {
            var s = game.S; var l = game.L;
            var f = s.File(fileId);
            var opts = new List<(string, System.Action)>();
            string reasons = "";
            foreach (var e in s.staff)
            {
                if (e.d != f.d) continue;
                var c = l.Candidate(e.id);
                if (Review.CanFix(s, l, fileId, e.id, out var why))
                    opts.Add((c.name + " (качество " + c.quality + ")", () => { Review.SendToFix(s, l, fileId, e.id); game.Commit(); }));
                else reasons += "\n" + c.name + ": " + why;
            }
            opts.Add(("Отмена", null));
            game.modal.Show("Исправить: " + f.title,
                "Сейчас качество " + Mathf.RoundToInt(f.quality) + ". Новое качество = половина старого + половина работы исправления. Нужен сотрудник: " + Names.Of(f.d) + "." + reasons
                + (opts.Count == 1 ? "\nНайми подходящего в хедхантере." : ""), opts.ToArray());
        }
    }
}
