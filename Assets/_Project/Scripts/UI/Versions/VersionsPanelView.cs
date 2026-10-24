using System.Linq;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // Stage 3 tab: version history, boss feedback with quotes, decision journal, blame and reopen.
    // Root prefab in Game next to MessengerPanel; all visuals live in the prefab, the code fills rows and handles clicks.
    public class VersionsPanelView : MonoBehaviour
    {
        const float ToastSeconds = 2.5f;

        public GameObject content;                       // hidden in the prefab; shown while the Versions tab is open
        public Transform versionList;
        public VersionRowView versionRowPrefab;
        public GameObject emptyState, feedback;
        public TMP_Text emptyTitle, emptyText, feedbackTitle, feedbackSub;
        public AxisCardView[] axisCards = new AxisCardView[4];
        public Transform quoteList;
        public QuoteRowView quoteRowPrefab;
        public TMP_Text journalHint;
        public Transform journalList;
        public JournalRowView journalRowPrefab;
        public GameObject reopenBox, reopenWarn;
        public TMP_Text reopenTitle, reopenCost;
        public Button reopenButton;
        public GameObject toastRoot;
        public TMP_Text toastText;
        public GameObject modal;
        public TMP_Text modalTitle;
        public Transform modalList;
        public ReopenOptionView optionPrefab;
        public Button modalCancel;
        public Button buildButtonPrefab;

        MessengerController controller;
        TopBarView topBar;
        Button buildButton;
        TMP_Text buildLabel;

        int selVersion = -1;
        string selQuote;
        float toastUntil;
        bool boardReopened, dbCloned;
        int lastVersions = -1;

        GameState S => controller.State;
        ContentDB Db => controller.db;

        void Start()
        {
            controller = FindAnyObjectByType<MessengerController>();
            topBar = FindAnyObjectByType<TopBarView>();
            if (controller == null || topBar == null) { enabled = false; return; }
            modalCancel.onClick.AddListener(() => modal.SetActive(false));
            DockBuildButton();
        }

        // The build button sits over "Закончить день" in Release, as in the mockup; Р3's TopBar prefab stays untouched.
        void DockBuildButton()
        {
            var end = (RectTransform)topBar.endDayButton.transform;
            buildButton = Instantiate(buildButtonPrefab, end.parent);
            var rt = (RectTransform)buildButton.transform;
            rt.anchorMin = end.anchorMin; rt.anchorMax = end.anchorMax; rt.pivot = end.pivot;
            rt.anchoredPosition = end.anchoredPosition; rt.sizeDelta = end.sizeDelta;
            buildButton.onClick.AddListener(Build);
            buildLabel = buildButton.GetComponentInChildren<TMP_Text>();
            buildButton.gameObject.SetActive(false);
        }

        void Update()
        {
            if (controller.State == null) return;

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
            if (want != content.activeSelf || (want && lastVersions != S.versions.Count))
            {
                content.SetActive(want);
                if (want) Refresh();
            }
            if (toastRoot.activeSelf && Time.unscaledTime > toastUntil) toastRoot.SetActive(false);
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
            toastText.text = text;
            toastRoot.SetActive(true);
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
            var v = ReviewSystem.Review(S, Db, Difficulty.For(S.difficulty).reviewTolerance);
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
            if (S == null) return;
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
            Format.Clear(versionList);
            for (int i = 0; i < S.versions.Count; i++)
            {
                int idx = i;
                var v = S.versions[i];
                int prevDay = i == 0 ? 0 : S.versions[i - 1].day;
                int count = S.log.Count(r => r.day <= v.day && !r.replaced) + 1;
                Instantiate(versionRowPrefab, versionList).Setup("v0." + v.number, v.approved ? "одобрено" : "доработать",
                    v.approved ? Pal.Green : Pal.Red, "день " + v.day + " · " + count + " решений",
                    i == 0 ? "" : Changed(prevDay, v.day), i == selVersion, () => { selVersion = idx; selQuote = null; Refresh(); });
            }
            var last = S.versions.Count > 0 ? S.versions[S.versions.Count - 1] : null;
            if (S.stage == Stage.Release && (last == null || !last.approved))
                Instantiate(versionRowPrefab, versionList).Setup("v0." + (S.versions.Count + 1), "черновик", Pal.Dim, "день " + S.day,
                    last == null ? "первая сборка" : Changed(last.day, S.day), false, () => { });
        }

        void RefreshCenter()
        {
            var v = Selected;
            emptyState.SetActive(v == null);
            feedback.SetActive(v != null);
            if (v == null)
            {
                emptyTitle.text = "Версий пока нет";
                emptyText.text = S.stage == Stage.Release
                    ? "Нажми «Собрать v0.1 и отправить» вверху, чтобы получить первый фидбек."
                    : "Версии собираются на этапе «Релиз». Сначала закончи разработку.";
                return;
            }

            feedbackTitle.text = "Фидбек на v0." + v.number + " · Борис Петрович";
            feedbackSub.text = v.approved ? "«Берём в релиз!»" : "«Идея крутая, но играть пока тяжело. Разбери.»";
            for (int i = 0; i < axisCards.Length; i++) axisCards[i].Setup(Ending.AxisNames[i], v.axes[i], S.goals[i]);

            Format.Clear(quoteList);
            foreach (var id in v.quoteIds)
            {
                var q = Db.quotes.FirstOrDefault(x => x != null && x.id == id);
                if (q == null) continue;
                string qid = q.id;
                Instantiate(quoteRowPrefab, quoteList).Setup(q, v.blamed.Exists(b => b.quoteId == qid), selQuote == qid,
                    () => { selQuote = selQuote == qid ? null : qid; RefreshCenter(); RefreshRight(); });
            }
        }

        void RefreshRight()
        {
            var v = Selected;
            var quote = selQuote == null ? null : Db.quotes.FirstOrDefault(x => x != null && x.id == selQuote);
            journalHint.text = v == null ? "Здесь будут твои решения после первой версии."
                : quote == null ? "Выбери отзыв, потом решение, которое его вызвало."
                : "Выбран отзыв: «" + quote.text + "». Какое решение виновато?";

            Format.Clear(journalList);
            Row(v, ReviewSystem.BoardRecord(S, Db), "Идея");
            foreach (var r in S.log.Where(x => !x.replaced)) Row(v, r, r.stage == Stage.Idea ? "Идея" : "Разработка");

            reopenBox.SetActive(false);
            if (v != null && quote != null)
            {
                var blame = v.blamed.Find(b => b.quoteId == quote.id);
                if (blame != null) ReopenBlock(blame.messageId);
            }
        }

        void Row(VersionRecord v, DecisionRecord r, string stageName)
        {
            bool culprit = v != null && v.blamed.Exists(b => b.messageId == r.messageId);
            string id = r.messageId;
            Instantiate(journalRowPrefab, journalList).Setup(r.day, stageName, Ending.DecisionText(S, Db, r), culprit, v != null, () => Guess(id));
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
            bool board = messageId == ReviewSystem.BoardId;
            bool ok = board ? CanPay(cost.days, cost.budget) : true;
            reopenBox.SetActive(true);
            reopenTitle.text = "Переоткрыть «" + Short(Ending.DecisionText(S, Db, rec)) + "»?";
            reopenCost.text = "Решение этапа " + (rec.stage == Stage.Idea ? 1 : 2) + ": " + Format.Cost(cost);
            reopenButton.interactable = ok;
            reopenButton.onClick.RemoveAllListeners();
            reopenButton.onClick.AddListener(() => { if (board) ReopenBoard(rec); else OpenModal(rec); });
            reopenWarn.SetActive(!ok);
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
                Instantiate(optionPrefab, modalList).Setup(opt.text, ok ? Format.Cost(opt.cost) : "не хватает дней или бюджета", ok, () => Reopen(rec, opt));
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
