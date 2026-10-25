using System.Collections;
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
        readonly Dictionary<string, int> seenTotal = new Dictionary<string, int>();   // messages per chat at the last draw: only growth gets an accent
        bool chatInited;

        void Awake() => game.Changed += Redraw;
        void OnEnable() { drawnCount = -1; Redraw(); }

        // Jump to the chat with one employee (from the task list).
        public void Open(string id)
        {
            open = id;
            drawnCount = -1;
            FindAnyObjectByType<TopBarMvp>().Show(0);
        }

        void Redraw()
        {
            if (game.S == null || !isActiveAndEnabled) return;
            var s = game.S;
            if (s.chat.Count == drawnCount && open == drawnChat && !NeedsButtons()) return;
            bool sameChat = open == drawnChat;
            int openBefore = seenTotal.TryGetValue(open, out var ob) ? ob : 0;
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
                int total = Count(s, id);
                if (chatInited && total > (seenTotal.TryGetValue(id, out var before) ? before : 0)) Pulse.Pop(this, row.transform, 1.05f, 0.4f);   // portrait and row of the chat that got a message
                seenTotal[id] = total;
            }

            var oc = game.L.Candidate(open);
            header.text = open == LevelState.Boss ? "Борис Петрович · руководитель" : oc.name + " · " + Names.Of(oc.d);
            read[open] = Count(s, open);
            foreach (Transform c in feed) Destroy(c.gameObject);
            int idx = 0;
            foreach (var m in s.chat)
            {
                if (m.chatId != open) continue;
                bool fresh = chatInited && sameChat && idx++ >= openBefore;
                var line = Instantiate(linePrefab, feed);
                bool mine = m.kind == "me";
                line.GetComponent<HorizontalLayoutGroup>().childAlignment = mine ? TextAnchor.UpperRight : TextAnchor.UpperLeft;
                var row = Instantiate(messagePrefab, line);
                var le = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = Mathf.Clamp(row.title.GetPreferredValues(Emphasize(m.text), 820, 0).x + 64, 260, 880);   // a bubble hugs its text
                le.flexibleWidth = 0; le.minWidth = 0;
                row.Set(m.kind == "me" ? m.text : Emphasize(m.text), "день " + m.day);
                row.title.font = row.subtitle.font;   // message text is body text, not a heading
                // Telegram-like: text on the left, small time stamp at the bottom right of the bubble
                row.title.alignment = TMPro.TextAlignmentOptions.TopLeft;
                row.subtitle.alignment = TMPro.TextAlignmentOptions.BottomRight;
                row.subtitle.fontSize = 16;
                row.subtitle.color = m.kind == "me" ? Palette.Hex("5E8A4C") : Palette.Hex("8A8F72");
                row.background.color = m.kind == "file" ? Palette.Cream : m.kind == "review" ? Palette.Yellow : Palette.Hex("FBF8EA");
                if (m.kind == "file") FileAttachment(row, m);
                else if (m.kind == "event") EventButtons(row, m);
                else if (m.kind == "me") { row.background.color = Palette.Hex("CFE8B8"); row.Button1(null, false, null); }
                else row.Button1(null, false, null);
                if (fresh) Pulse.Pop(this, row.transform, 1.04f, 0.3f);
            }
            chatInited = true;
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
            row.Set(Emphasize(m.text));
            // The bubble keeps only the text; answers sit under it, two per row.
            var buttons = new List<Button>();
            RowView r = null;
            for (int j = 0; j < def.options.Count; j++)
            {
                int k = j;
                if (j % 2 == 0) r = ChoiceRow(row.GetComponent<LayoutElement>().preferredWidth);
                string label = Choice(def.options[j].label, Names.Effects(def.options[j]));
                UnityEngine.Events.UnityAction act = () => Pick(m, k, row, buttons);
                if (j % 2 == 0) { r.Button1(label, true, act); buttons.Add(r.button1); StyleChoice(r.button1, r.button1Label, Palette.Olive); }
                else { r.Button2(label, true, act); buttons.Add(r.button2); StyleChoice(r.button2, r.button2Label, Palette.Hex("657936")); }
            }
        }

        RowView ChoiceRow(float width)
        {
            var line = Instantiate(linePrefab, feed);
            line.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
            var r = Instantiate(messagePrefab, line);
            var le = r.GetComponent<LayoutElement>() ?? r.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = Mathf.Max(720, width); le.flexibleWidth = 0; le.minWidth = 0;
            r.background.color = Palette.Hex("FBF8EA");   // same cream as the question above
            r.Set("");
            r.title.transform.parent.gameObject.SetActive(false);   // no text column: the buttons start at the left edge
            r.Button1(null, false, null); r.Button2(null, false, null);
            return r;
        }

        // Key parts in bold so they do not drown in the text: quoted names, numbers with units, places of the game.
        static readonly System.Text.RegularExpressions.Regex Key = new System.Text.RegularExpressions.Regex(
            @"«[^»]+»|\d[\d  ]*(?:\s?(?:%|₽|дн\w*|дня|минут\w*|баллов|или выше))?\+?|[Пп]одростк\w*|[Дд]оск\w*|ШиШ\.ру|[Тт]аск-лист\w*|[Кк]онцепт\w*|[Зз]елёная линия|\*\*[^*]+\*\*");
        internal static string Emphasize(string text) =>
            Key.Replace(text ?? "", m => "<b><color=#A8431E>" + m.Value.Trim('*') + "</color></b>");

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
                float need = label.GetPreferredValues(label.text, layout.preferredWidth - 24, 0).y + 24;   // long effect lines wrap
                layout.minHeight = Mathf.Max(72, need); layout.preferredHeight = Mathf.Max(78, need);
            }
        }

        void Pick(ChatEntry m, int option, RowView row, List<Button> buttons)
        {
            bool ok = EventRules.Resolve(game.S, game.L, m.refId, m.chatId, option);
            AudioManager.Play(ok ? Fx.MsgOut : Fx.Error);
            if (!ok) game.modal.Show("Не хватает денег", "На такой вариант в бюджете пока нет средств. Выбери другой.", ("Понятно", null));
            game.Commit();
            if (ok)
            {
                // short confirmation: the chosen card pops and both lock, then the answers leave
                foreach (var b in buttons) b.interactable = false;
                Pulse.Pop(this, buttons[option].transform, 1.1f, 0.3f);
                StartCoroutine(RedrawAfter(0.35f));
                return;
            }
            // Resolving an event may not add a chat entry. Force the open conversation to redraw
            // so its two answer cards disappear as soon as the player has chosen.
            drawnCount = -1;
            Redraw();
        }

        IEnumerator RedrawAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
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
