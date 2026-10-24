using System.Collections.Generic;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // Stage 1 board. Lives as a root prefab in Game next to MessengerPanel; shows itself over the messenger's
    // "Доска" tab area and rebuilds from GameState on every change (the state is small, a rebuild is cheap).
    public class BoardPanelView : MonoBehaviour
    {
        public GameObject content;
        public SlotView[] slots;
        public RectTransform threadLayer, dragLayer, handGrid;
        public CardView cardPrefab;
        public ThreadView threadPrefab;
        public TMP_Text briefText, researchHint, researchLabel, askLabel;
        public Button researchButton, askButton, pitchButton;
        public RectTransform[] goalFills = new RectTransform[4];   // fill bars, scaled by anchorMax.x
        public TMP_Text[] goalTargets = new TMP_Text[4];
        public RectTransform[] legendSamples = new RectTransform[4];   // Match, Conflict, Unknown, Neutral

        public int researchPrice = 30000;

        MessengerController controller;
        TopBarView topBar;
        string selectedId, pinFirstId;
        bool shown;

        GameState State => controller.State;
        ContentDB Db => controller.db;
        bool Editable => State.stage == Stage.Idea;

        void Start()
        {
            controller = FindAnyObjectByType<MessengerController>();
            topBar = FindAnyObjectByType<TopBarView>();
            content.SetActive(false);
            pitchButton.gameObject.SetActive(false);
            researchButton.onClick.AddListener(Research);
            pitchButton.onClick.AddListener(Pitch);
            for (var i = 0; i < legendSamples.Length; i++)
                ThreadView.DrawLine(legendSamples[i], 44, (ThreadKind)new[] { 1, 2, 0, 3 }[i]);   // Match, Conflict, Unknown, Neutral
            DockPitchButton();
        }

        // The pitch button lives in the TopBar (card P-5) but is built here so Р3's TopBar prefab stays untouched.
        // As in the mockup it takes the place of "Закончить день" while the stage is Idea.
        void DockPitchButton()
        {
            if (topBar == null) return;
            var end = (RectTransform)topBar.endDayButton.transform;
            var rt = (RectTransform)pitchButton.transform;
            rt.SetParent(end.parent, false);
            rt.anchorMin = end.anchorMin;
            rt.anchorMax = end.anchorMax;
            rt.pivot = end.pivot;
            rt.sizeDelta = end.sizeDelta;
            rt.anchoredPosition = end.anchoredPosition;
        }

        void Update()
        {
            if (controller == null || controller.State == null) return;
            var want = controller.boardTab.activeInHierarchy;
            if (want) FitToTab();
            if (want != shown)
            {
                shown = want;
                content.SetActive(want);
                if (want) Refresh();
            }
            var idea = State.stage == Stage.Idea;
            pitchButton.gameObject.SetActive(idea);
            if (topBar != null) topBar.endDayButton.gameObject.SetActive(!idea);
        }

        // This root prefab sits on top of the placeholder tab: copy its rectangle.
        void FitToTab()
        {
            var tab = (RectTransform)controller.boardTab.transform;
            var rt = (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = tab.rect.size;
            rt.position = tab.TransformPoint(tab.rect.center);
        }

        public void Refresh()
        {
            if (controller == null || controller.State == null) return;
            foreach (var s in slots) { s.SetEmpty(true); Clear(s.cardAnchor); }
            Clear(handGrid);
            Clear(threadLayer);

            var board = State.board;
            briefText.text = Db.brief != null ? "<b>Бриф:</b> " + Db.brief.text : "";

            var placedIds = new HashSet<string>();
            foreach (var entry in board.placed)
            {
                var card = FindCard(entry.cardId);
                if (card == null) continue;
                placedIds.Add(card.id);
                var slot = SlotFor(card.slot);
                slot.SetEmpty(false);
                var view = NewCard(slot.cardAnchor, card, true);
                var rt = (RectTransform)view.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = new Vector2(8, 8);
                rt.offsetMax = new Vector2(-8, -8);
            }

            foreach (var card in Db.cards)
                if (card != null && !placedIds.Contains(card.id))
                    NewCard(handGrid, card, false);

            Canvas.ForceUpdateCanvases();
            foreach (var link in board.links) DrawThread(link);
            RefreshGoals();
            RefreshButtons();
            if (topBar != null) topBar.Refresh(State);
        }

        CardView NewCard(Transform parent, CardSO card, bool onBoard)
        {
            var view = Instantiate(cardPrefab, parent, false);
            view.dragLayer = dragLayer;
            view.Setup(card, Note(card), onBoard, card.id == selectedId, card.id == pinFirstId, Editable);
            view.Clicked = v => { selectedId = selectedId == v.Card.id ? null : v.Card.id; Refresh(); };
            view.PinClicked = OnPin;
            view.DropRequested = OnDrop;
            return view;
        }

        string Note(CardSO card)
            => State.board.revealed.Contains(card.id)
                ? "популярность: " + (card.popularity < 34 ? "низкая" : card.popularity < 67 ? "средняя" : "высокая")
                : "рынок не изучен";

        bool OnDrop(CardView view, UnityEngine.EventSystems.PointerEventData e)
        {
            foreach (var s in slots)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(s.Rect, e.position, e.pressEventCamera)) continue;
                if (s.slot != view.Card.slot) return false;
                BoardRules.Place(State, view.Card);
                Destroy(view.gameObject);
                Refresh();
                return true;
            }
            return false;
        }

        void OnPin(CardView view)
        {
            var id = view.Card.id;
            if (pinFirstId == null) pinFirstId = id;
            else
            {
                if (pinFirstId != id) BoardRules.Connect(State, pinFirstId, id);
                pinFirstId = null;
            }
            Refresh();
        }

        void DrawThread(Link link)
        {
            var a = FindCard(link.a);
            var b = FindCard(link.b);
            if (a == null || b == null) return;
            var kind = BoardRules.Thread(a, b, Db, State.board, Difficulty.For(State.difficulty).showThreads);
            string label = null;
            if (kind == ThreadKind.Match || kind == ThreadKind.Conflict)
            {
                var combo = FindCombo(a, b);
                if (combo != null) label = (kind == ThreadKind.Match ? "Комбо: " : "Конфликт: ") + combo.title;
            }
            var view = Instantiate(threadPrefab, threadLayer, false);
            view.Setup(PinPoint(a), PinPoint(b), kind, label, () =>
            {
                if (!Editable) return;
                BoardRules.Disconnect(State, link.a, link.b);
                Refresh();
            });
        }

        // Pin centre in threadLayer space.
        Vector2 PinPoint(CardSO card)
        {
            foreach (var cv in SlotFor(card.slot).cardAnchor.GetComponentsInChildren<CardView>())
                return threadLayer.InverseTransformPoint(cv.PinRect.position);
            return Vector2.zero;
        }

        void RefreshGoals()
        {
            var goals = BoardRules.Goals(State.board, Db);
            for (var i = 0; i < 4; i++)
            {
                goalFills[i].anchorMax = new Vector2(goals[i] / 100f, 1f);
                goalTargets[i].text = "цель " + goals[i];
            }
        }

        void RefreshButtons()
        {
            var price = new Cost { budget = researchPrice };
            researchLabel.text = "Исследовать рынок · " + Format.Money(price.budget);
            string hint = null;
            var ok = Editable;
            if (!Editable) hint = "доска зафиксирована";
            else if (selectedId == null) { ok = false; hint = "выбери карточку кликом"; }
            else if (State.board.revealed.Contains(selectedId)) { ok = false; hint = "эта карточка уже изучена"; }
            else if (State.budget < price.budget) { ok = false; hint = "не хватает бюджета"; }
            researchButton.interactable = ok;
            researchHint.text = hint ?? "откроет популярность и нитки с ней";
            askButton.interactable = false;
            askLabel.text = "Спросить команду · полдня";
        }

        void Research()
        {
            if (selectedId == null || !Editable) return;
            BoardRules.Research(State, selectedId, new Cost { budget = researchPrice });
            Refresh();
        }

        void Pitch()
        {
            if (State.stage != Stage.Idea) return;
            BoardRules.Commit(State, Db);
            pinFirstId = null;
            Refresh();
            controller.tabChats.onClick.Invoke();
        }

        SlotView SlotFor(Slot slot)
        {
            foreach (var s in slots) if (s.slot == slot) return s;
            return slots[0];
        }

        CardSO FindCard(string id)
        {
            foreach (var c in Db.cards) if (c != null && c.id == id) return c;
            return null;
        }

        ComboSO FindCombo(CardSO a, CardSO b)
        {
            foreach (var c in Db.combos)
                if (c != null && c.a != null && c.b != null &&
                    ((c.a.id == a.id && c.b.id == b.id) || (c.a.id == b.id && c.b.id == a.id))) return c;
            return null;
        }

        static void Clear(Transform t) => Format.Clear(t);
    }
}
