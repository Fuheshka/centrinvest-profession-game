using System.Collections.Generic;
using System.Linq;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // Stage 3 tab: version history, boss feedback with quotes, decision journal, blame and reopen.
    // Root prefab in Game next to MessengerPanel; the UI is built in code and covers the messenger's "Версии" tab area.
    public class VersionsPanelView : MonoBehaviour
    {
        const float ToastSeconds = 2.5f;

        MessengerController controller;
        TopBarView topBar;
        GameObject content, modal;
        RectTransform left, center, right, modalList;
        TMP_Text toast, modalTitle;
        Button buildButton;
        TMP_Text buildLabel;

        int selVersion = -1;
        string selQuote;
        float toastUntil;
        bool shown, boardReopened, dbCloned, built;
        int lastVersions = -1;

        GameState S => controller.State;
        ContentDB Db => controller.db;

        void Start()
        {
            controller = FindAnyObjectByType<MessengerController>();
            topBar = FindAnyObjectByType<TopBarView>();
            if (controller == null || topBar == null) { enabled = false; return; }
            UiKit.Font = topBar.budgetText.font;
            BuildSkeleton();
            DockBuildButton();
        }

        void BuildSkeleton()
        {
            var bg = UiKit.Box(transform, "Content", UiKit.Bg);
            content = bg.gameObject;
            UiKit.Stretch(bg.rectTransform);
            UiKit.H(bg.rectTransform, 0).childForceExpandHeight = true;

            left = Column(content.transform, "Left", 300, UiKit.Bg);
            center = Column(content.transform, "Center", -1, UiKit.Bg);
            right = Column(content.transform, "Right", 360, UiKit.Panel);

            // toast and reopen window float over everything
            var t = UiKit.Box(content.transform, "Toast", new Color(0.79f, 0.23f, 0.25f, 0.95f));
            t.raycastTarget = false;
            t.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var trt = t.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0f);
            trt.pivot = new Vector2(0.5f, 0f);
            trt.anchoredPosition = new Vector2(0, 40);
            trt.sizeDelta = new Vector2(420, 56);
            toast = UiKit.Label(t.transform, "", 20, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            UiKit.Stretch(toast.rectTransform);
            t.gameObject.SetActive(false);

            var dim = UiKit.Box(content.transform, "Modal", new Color(0, 0, 0, 0.75f));
            dim.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            UiKit.Stretch(dim.rectTransform);
            modal = dim.gameObject;
            var box = UiKit.Box(modal.transform, "Box", UiKit.Panel);
            var brt = box.rectTransform;
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(620, 0);
            UiKit.V(brt, 12, 24);
            box.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            modalTitle = UiKit.Label(box.transform, "", 22, UiKit.Text, FontStyles.Bold);
            modalList = UiKit.Node(box.transform, "Options");
            UiKit.V(modalList, 10);
            UiKit.TextButton(box.transform, "Отмена", UiKit.Panel2, UiKit.Text, () => modal.SetActive(false), 44);
            modal.SetActive(false);
            built = true;
        }

        static RectTransform Column(Transform parent, string name, float width, Color bg)
        {
            var img = UiKit.Box(parent, name, bg);
            UiKit.V(img.rectTransform, 12, 20);
            UiKit.Size(img, w: width, flexW: width < 0 ? 1 : 0, flexH: 1);
            return img.rectTransform;
        }

        // The build button covers "Закончить день" in Release, as in the mockup; Р3's TopBar prefab stays untouched.
        void DockBuildButton()
        {
            var end = (RectTransform)topBar.endDayButton.transform;
            var go = Instantiate(topBar.endDayButton.gameObject, end.parent);
            go.name = "BuildButton";
            var rt = (RectTransform)go.transform;
            buildButton = go.GetComponent<Button>();
            buildButton.onClick.RemoveAllListeners();
            buildButton.onClick.AddListener(Build);
            buildLabel = go.GetComponentInChildren<TMP_Text>();
            go.SetActive(false);
        }

        void Update()
        {
            if (!built || controller.State == null) return;

            // Reopened board: after the new pitch (stage Production) the game goes straight back to Release.
            if (boardReopened && S.stage == Stage.Production)
            {
                S.stage = Stage.Release;
                boardReopened = false;
                controller.Refresh();
                Refresh();
            }

            bool release = S.stage == Stage.Release;
            if (buildButton.gameObject.activeSelf != release) buildButton.gameObject.SetActive(release);
            if (release) buildLabel.text = "Собрать v0." + (S.versions.Count + 1) + " и отправить";

            bool want = controller.versionsTab.activeInHierarchy;
            if (want) FitToTab();
            if (want != shown || (want && lastVersions != S.versions.Count))
            {
                shown = want;
                content.SetActive(want);
                if (want) Refresh();
            }
            if (toast.transform.parent.gameObject.activeSelf && Time.unscaledTime > toastUntil)
                toast.transform.parent.gameObject.SetActive(false);
        }

        void FitToTab()
        {
            var tab = (RectTransform)controller.versionsTab.transform;
            var rt = (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = tab.rect.size;
            rt.position = tab.TransformPoint(tab.rect.center);
        }

        void Toast(string text)
        {
            toast.text = text;
            toast.transform.parent.gameObject.SetActive(true);
            toastUntil = Time.unscaledTime + ToastSeconds;
        }

        // ---- build and review ----

        void Build()
        {
            if (S.stage != Stage.Release) return;
            if (!dbCloned)
            {
                // The boss reply is a runtime message; work on a copy so the asset on disk stays untouched.
                controller.db = Instantiate(controller.db);
                dbCloned = true;
            }
            var v = ReviewSystem.Review(S, Db);
            if (v.approved || DayClock.IsOverDeadline(S)) S.stage = Stage.Done;
            selVersion = S.versions.Count - 1;
            selQuote = null;
            PostBossReply(v);
            controller.Refresh();
            controller.tabVersions.onClick.Invoke();
            Refresh();
        }

        void PostBossReply(VersionRecord v)
        {
            var m = ScriptableObject.CreateInstance<MessageSO>();
            m.id = "review_v" + v.number;
            m.sender = Sender.Boss;
            m.stage = Stage.Release;
            m.day = S.day;   // put into the inbox here; DayClock skips ids already in the inbox
            m.text = v.approved
                ? $"Версия v0.{v.number} одобрена. Берём в релиз!"
                : $"Версия v0.{v.number}: идея крутая, но играть пока тяжело. Разбери отзывы во вкладке «Версии», найди причину каждого и пересобери.";
            Db.messages = Db.messages.Concat(new[] { m }).ToArray();
            S.inbox.Add(m.id);
        }

        // ---- rendering ----

        void Refresh()
        {
            if (!built || S == null) return;
            lastVersions = S.versions.Count;
            if (S.versions.Count > 0 && (selVersion < 0 || selVersion >= S.versions.Count)) selVersion = S.versions.Count - 1;
            RefreshLeft();
            RefreshCenter();
            RefreshRight();
            topBar.Refresh(S);
        }

        VersionRecord Selected => selVersion >= 0 && selVersion < S.versions.Count ? S.versions[selVersion] : null;

        // Decisions made after the previous version up to this one (day window), by option text.
        string Changed(int fromDay, int toDay)
        {
            var names = S.log.Where(r => r.day > fromDay && r.day <= toDay).Select(r => Short(Ending.DecisionText(S, Db, r))).ToList();
            return names.Count == 0 ? "" : "изменено: " + string.Join(", ", names.Take(2)) + (names.Count > 2 ? "..." : "");
        }

        static string Short(string s) => s.Length > 22 ? s.Substring(0, 21) + "..." : s;

        void RefreshLeft()
        {
            Format.Clear(left);
            UiKit.Label(left, "История версий", 24, UiKit.Text, FontStyles.Bold);
            for (int i = 0; i < S.versions.Count; i++)
            {
                int idx = i;
                var v = S.versions[i];
                int prevDay = i == 0 ? 0 : S.versions[i - 1].day;
                int count = S.log.Count(r => r.day <= v.day && !r.replaced) + 1;
                string info = "день " + v.day + " · " + count + " решений";
                string changed = i == 0 ? "" : Changed(prevDay, v.day);
                VersionCard(left, "v0." + v.number, v.approved ? "одобрено" : "доработать",
                    v.approved ? UiKit.Green : UiKit.Red, info, changed, i == selVersion, () => { selVersion = idx; selQuote = null; Refresh(); });
            }
            var last = S.versions.Count > 0 ? S.versions[S.versions.Count - 1] : null;
            if (S.stage == Stage.Release && (last == null || !last.approved))
            {
                string changed = last == null ? "первая сборка" : Changed(last.day, S.day);
                VersionCard(left, "v0." + (S.versions.Count + 1), "черновик", UiKit.Dim, "день " + S.day,
                    changed, false, () => { });
            }
            var spacer = UiKit.Node(left, "Spacer");
            UiKit.Size(spacer, flexH: 1);
            UiKit.Label(left, "Первая версия почти всегда уходит на доработку. Это нормально – так работают все студии.", 15, UiKit.Dim);
        }

        static void VersionCard(Transform parent, string title, string status, Color statusColor, string info, string changed,
            bool selected, System.Action click)
        {
            var img = UiKit.ButtonBox(parent, "Version", UiKit.Panel2, click);
            if (selected) UiKit.Border(img, UiKit.Accent);
            UiKit.V(img.rectTransform, 4, 14);
            var row = UiKit.Node(img.transform, "Row");
            UiKit.H(row, 8).childAlignment = TextAnchor.MiddleLeft;
            UiKit.Label(row, title, 22, UiKit.Text, FontStyles.Bold);
            UiKit.Size(row.GetChild(0), flexW: 1);
            UiKit.Label(row, status, 15, statusColor);
            UiKit.Label(img.transform, info, 15, UiKit.Dim);
            if (changed != "") UiKit.Label(img.transform, changed, 15, UiKit.Dim);
        }

        void RefreshCenter()
        {
            Format.Clear(center);
            var v = Selected;
            if (v == null)
            {
                UiKit.Label(center, "Версий пока нет", 24, UiKit.Text, FontStyles.Bold);
                UiKit.Label(center, S.stage == Stage.Release
                    ? "Нажми «Собрать v0.1 и отправить» вверху, чтобы получить первый фидбек."
                    : "Версии собираются на этапе «Релиз». Сначала закончи разработку.", 18, UiKit.Dim);
                return;
            }

            UiKit.Label(center, "Фидбек на v0." + v.number + " · Борис Петрович", 24, UiKit.Text, FontStyles.Bold);
            UiKit.Label(center, v.approved ? "«Берём в релиз!»" : "«Идея крутая, но играть пока тяжело. Разбери.»", 18, UiKit.Dim);

            var axes = UiKit.Node(center, "Axes");
            UiKit.H(axes, 12).childForceExpandWidth = true;
            for (int i = 0; i < 4; i++) AxisCard(axes, i, v.axes[i], S.goals[i]);

            UiKit.Label(center, "Отзывы плейтестеров – найди причину каждого", 20, UiKit.Text, FontStyles.Bold);
            var list = UiKit.Scroll(center, 10);
            UiKit.Size(list.parent, flexH: 1);
            foreach (var id in v.quoteIds)
            {
                var q = Db.quotes.FirstOrDefault(x => x != null && x.id == id);
                if (q != null) QuoteRow(list, v, q);
            }
        }

        static void AxisCard(Transform parent, int axis, int value, int goal)
        {
            var img = UiKit.Box(parent, "Axis", UiKit.Panel);
            UiKit.V(img.rectTransform, 2, 12);
            UiKit.Size(img, flexW: 1);
            bool ok = value >= goal;
            Color c = ok ? UiKit.Green : goal - value <= 10 ? UiKit.Yellow : UiKit.Red;
            UiKit.Label(img.transform, Ending.AxisNames[axis], 16, UiKit.Dim);
            var row = UiKit.Node(img.transform, "Row");
            UiKit.H(row, 6).childAlignment = TextAnchor.LowerLeft;
            UiKit.Label(row, value.ToString(), 36, c, FontStyles.Bold);
            UiKit.Label(row, "/ цель " + goal, 16, UiKit.Dim);
            UiKit.Label(img.transform, ok ? "цель достигнута" : "ниже цели", 14, c);
        }

        void QuoteRow(Transform parent, VersionRecord v, QuoteSO q)
        {
            bool blamed = v.blamed.Exists(b => b.quoteId == q.id);
            bool picked = selQuote == q.id;
            string status = q.praise ? "похвала" : blamed ? "найдена причина" : picked ? "выбран" : "не разобран";
            Color sc = q.praise || blamed ? UiKit.Green : picked ? UiKit.Accent : UiKit.Dim;
            string id = q.id;
            var img = UiKit.ButtonBox(parent, "Quote", blamed ? new Color32(0x1E, 0x35, 0x2C, 255) : picked ? new Color32(0x4A, 0x32, 0x24, 255) : UiKit.Panel, () => { selQuote = selQuote == id ? null : id; RefreshCenter(); RefreshRight(); },
                !q.praise && !blamed);
            UiKit.H(img.rectTransform, 12, 14).childAlignment = TextAnchor.MiddleLeft;
            var texts = UiKit.Node(img.transform, "Texts");
            UiKit.V(texts, 2);
            UiKit.Size(texts, flexW: 1);
            UiKit.Label(texts, "«" + q.text + "»", 19, UiKit.Text);
            UiKit.Label(texts, q.author, 15, UiKit.Dim);
            UiKit.Label(img.transform, status, 15, sc, FontStyles.Bold, TextAlignmentOptions.Right);
        }

        void RefreshRight()
        {
            Format.Clear(right);
            UiKit.Label(right, "Журнал решений", 24, UiKit.Text, FontStyles.Bold);
            var v = Selected;
            var quote = selQuote == null ? null : Db.quotes.FirstOrDefault(x => x != null && x.id == selQuote);
            string hint = v == null ? "Здесь будут твои решения после первой версии."
                : quote == null ? "Выбери отзыв, потом решение, которое его вызвало."
                : "Выбран отзыв: «" + quote.text + "». Какое решение виновато?";
            UiKit.Label(right, hint, 16, UiKit.Dim);

            var list = UiKit.Scroll(right, 10);
            UiKit.Size(list.parent, flexH: 1);
            var board = ReviewSystem.BoardRecord(S, Db);
            Row(list, v, board, "Идея");
            foreach (var r in S.log.Where(x => !x.replaced)) Row(list, v, r, r.stage == Stage.Idea ? "Идея" : "Разработка");

            if (v != null && quote != null)
            {
                var blame = v.blamed.Find(b => b.quoteId == quote.id);
                if (blame != null) ReopenBlock(blame.messageId);
            }
        }

        void Row(Transform parent, VersionRecord v, DecisionRecord r, string stageName)
        {
            bool culprit = v != null && v.blamed.Exists(b => b.messageId == r.messageId);
            string id = r.messageId;
            var img = UiKit.ButtonBox(parent, "Decision", culprit ? new Color32(0x1E, 0x35, 0x2C, 255) : UiKit.Panel2, () => Guess(id), v != null);
            if (culprit) UiKit.Border(img, UiKit.Green);
            UiKit.V(img.rectTransform, 2, 12);
            var head = UiKit.Node(img.transform, "Head");
            UiKit.H(head, 8);
            UiKit.Label(head, "день " + r.day, 14, UiKit.Dim);
            UiKit.Size(head.GetChild(0), flexW: 1);
            UiKit.Label(head, stageName, 14, UiKit.Dim, align: TextAlignmentOptions.Right);
            UiKit.Label(img.transform, Ending.DecisionText(S, Db, r), 17, UiKit.Text);
        }

        void Guess(string messageId)
        {
            var v = Selected;
            if (v == null) return;
            if (selQuote == null) { Toast("Сначала выбери отзыв"); return; }
            if (ReviewSystem.CheckBlame(S, v, selQuote, messageId, Db)) Toast("Верно! Причина найдена");
            else
            {
                Toast("Не то. –1 день");
                if (DayClock.IsOverDeadline(S)) { S.stage = Stage.Done; }
            }
            controller.Refresh();
            Refresh();
        }

        DecisionRecord Culprit(string messageId)
            => messageId == ReviewSystem.BoardId
                ? ReviewSystem.BoardRecord(S, Db)
                : S.log.Find(r => r.messageId == messageId && !r.replaced);

        void ReopenBlock(string messageId)
        {
            var rec = Culprit(messageId);
            if (rec == null) return;
            var cost = ReviewSystem.ReopenCost(rec);
            var box = UiKit.Box(right, "Reopen", UiKit.Panel2);
            UiKit.V(box.rectTransform, 6, 14);
            UiKit.Label(box.transform, "Переоткрыть «" + Short(Ending.DecisionText(S, Db, rec)) + "»?", 18, UiKit.Text);
            UiKit.Label(box.transform, "Решение этапа " + (rec.stage == Stage.Idea ? 1 : 2) + ": " + Format.Cost(cost), 15, UiKit.Dim);
            bool board = messageId == ReviewSystem.BoardId;
            bool ok = board ? CanPay(cost.days, cost.budget) : true;
            UiKit.TextButton(box.transform, "Переоткрыть решение", new Color32(0xF6, 0xF0, 0xE3, 255), UiKit.Bg,
                () => { if (board) ReopenBoard(rec); else OpenModal(rec); }, 48, ok);
            if (!ok) UiKit.Label(box.transform, "Не хватает дней или бюджета", 14, UiKit.Red);
        }

        bool CanPay(int days, int budget) => S.budget >= budget && S.day + days <= S.deadline;

        void ReopenBoard(DecisionRecord rec)
        {
            ReviewSystem.Reopen(S, rec, null, Db);
            boardReopened = true;
            selQuote = null;
            controller.Refresh();
            controller.tabBoard.onClick.Invoke();
        }

        void OpenModal(DecisionRecord rec)
        {
            var msg = MessengerController.Find(Db, rec.messageId);
            if (msg == null) return;
            var cost = ReviewSystem.ReopenCost(rec);
            modalTitle.text = "Переделать: выбери другое решение\nЦена переоткрытия: " + Format.Cost(cost);
            Format.Clear(modalList);
            foreach (var o in msg.options.Where(x => x != null && x.id != rec.optionId))
            {
                var opt = o;
                bool ok = CanPay(cost.days + opt.cost.days, cost.budget + opt.cost.budget);
                var b = UiKit.ButtonBox(modalList, "Option", UiKit.Panel2, () => Reopen(rec, opt), ok);
                UiKit.V(b.rectTransform, 2, 12);
                UiKit.Label(b.transform, opt.text, 18, ok ? UiKit.Text : UiKit.Dim);
                UiKit.Label(b.transform, ok ? Format.Cost(opt.cost) : "не хватает дней или бюджета", 14, ok ? UiKit.Dim : UiKit.Red);
            }
            modal.SetActive(true);
        }

        void Reopen(DecisionRecord rec, OptionSO opt)
        {
            ReviewSystem.Reopen(S, rec, opt, Db);
            modal.SetActive(false);
            selQuote = null;
            Toast("Решение переоткрыто");
            controller.Refresh();
            Refresh();
        }
    }
}
