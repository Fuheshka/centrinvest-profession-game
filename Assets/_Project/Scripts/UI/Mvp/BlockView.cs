using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    public class BlockView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TMP_Text type, value;
        public Image background;

        DashboardWindow owner;
        Block block;
        bool dragged;

        public Vector2 Center => ((RectTransform)transform).anchoredPosition;

        // The sticker's two side ports are centred at 41.1% of its width from the pivot.
        // Keep connections on the visible rings instead of routing through the card centre.
        public Vector2 PortToward(Vector2 target)
        {
            float side = target.x >= Center.x ? 1f : -1f;
            return PortOnSide(side);
        }

        public Vector2 PortOnSide(float side) => Center + new Vector2(Mathf.Sign(side) * ((RectTransform)transform).rect.width * 0.411f, 0f);

        public void Bind(DashboardWindow w, Block b, string t, string v, bool selected, bool dim)
        {
            owner = w; block = b;
            type.text = t; value.text = v;
            ((RectTransform)transform).anchoredPosition = Clamp(new Vector2(b.x, b.y));
            background.color = selected ? new Color(1f, 0.8f, 0.55f) : b.paramId == null ? new Color(0.85f, 0.85f, 0.8f) : Color.white;
            float a = dim ? 0.3f : 1;   // not linkable to the chosen block
            var bc = background.color; bc.a = a; background.color = bc;
            type.alpha = value.alpha = a;
        }

        // Blocks stay inside the board (anchored top-left, centre pivot).
        Vector2 Clamp(Vector2 p)
        {
            var rt = (RectTransform)transform;
            var area = ((RectTransform)rt.parent).rect;
            var h = rt.rect.size / 2;
            return new Vector2(Mathf.Clamp(p.x, h.x, Mathf.Max(h.x, area.width - h.x)), Mathf.Clamp(p.y, Mathf.Min(-h.y, h.y - area.height), -h.y));
        }

        public void OnPointerClick(PointerEventData e) { if (!dragged) owner.Clicked(block); }
        public void OnBeginDrag(PointerEventData e) { dragged = true; transform.localScale = Vector3.one * 1.06f; }   // lifted while held; the view is rebuilt on drop

        public void OnDrag(PointerEventData e)
        {
            var rt = (RectTransform)transform;
            rt.anchoredPosition = Clamp(rt.anchoredPosition + e.delta / GetComponentInParent<Canvas>().rootCanvas.scaleFactor);
            owner.Dragging(block, rt.anchoredPosition);
        }

        public void OnEndDrag(PointerEventData e)
        {
            owner.Moved(block, ((RectTransform)transform).anchoredPosition);
            dragged = false;
        }
    }
}
