using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.UI
{
    // One prefab for both places: compact in the hand grid, full on the board. Drag works for mouse and touch
    // through the EventSystem. A hand card is dragged to a slot of its own type; anything else returns it.
    public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public TMP_Text slotText, titleText, noteText;
        public Image background, selection, pin;
        public Button pinButton;
        public CanvasGroup group;
        public LayoutElement layout;

        public CardSO Card { get; private set; }
        public bool OnBoard { get; private set; }

        public Action<CardView> Clicked, PinClicked;
        public Func<CardView, PointerEventData, bool> DropRequested;   // true: the drop was accepted
        public RectTransform dragLayer;

        bool draggable, dragging;
        Transform homeParent;
        int homeIndex;
        Vector2 homeSize;

        public void Setup(CardSO card, string note, bool onBoard, bool selected, bool pinPending, bool editable)
        {
            Card = card;
            OnBoard = onBoard;
            draggable = !onBoard && editable;
            slotText.text = SlotView.Name(card.slot).ToUpperInvariant();
            titleText.text = card.title;
            noteText.gameObject.SetActive(onBoard);
            noteText.text = note;
            selection.gameObject.SetActive(selected);
            pinButton.gameObject.SetActive(onBoard);
            pinButton.interactable = editable;
            pin.color = SlotView.PinColor(card.slot);
            pin.rectTransform.localScale = pinPending ? Vector3.one * 1.5f : Vector3.one;
            pinButton.onClick.RemoveAllListeners();
            pinButton.onClick.AddListener(() => PinClicked?.Invoke(this));
        }

        public RectTransform PinRect => pin.rectTransform;

        public void OnPointerClick(PointerEventData e)
        {
            if (!dragging) Clicked?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!draggable || dragLayer == null) { e.pointerDrag = null; return; }
            dragging = true;
            var rt = (RectTransform)transform;
            homeParent = rt.parent;
            homeIndex = rt.GetSiblingIndex();
            homeSize = rt.rect.size;
            layout.ignoreLayout = true;
            rt.SetParent(dragLayer, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = homeSize;
            group.blocksRaycasts = false;
            Follow(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (dragging) Follow(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!dragging) return;
            group.blocksRaycasts = true;
            if (DropRequested != null && DropRequested(this, e)) return;   // accepted: the board rebuilds and destroys this view
            dragging = false;
            layout.ignoreLayout = false;
            var rt = (RectTransform)transform;
            rt.SetParent(homeParent, false);
            rt.SetSiblingIndex(homeIndex);
        }

        void Follow(PointerEventData e)
        {
            var rt = (RectTransform)transform;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragLayer, e.position, e.pressEventCamera, out var world))
                rt.position = world;
        }
    }
}
