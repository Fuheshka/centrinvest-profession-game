using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Wheel and trackpad scroll that eases to a target instead of jumping; the stock ScrollRect wheel is switched off.
    [RequireComponent(typeof(ScrollRect))]
    public class SmoothWheel : MonoBehaviour, IScrollHandler
    {
        public float pixelsPerNotch = 70, speed = 14;

        ScrollRect sr;
        float target;
        bool moving;

        void Awake() { sr = GetComponent<ScrollRect>(); sr.scrollSensitivity = 0; }

        public void OnScroll(PointerEventData e)
        {
            var c = sr.content;
            float max = Mathf.Max(0, c.rect.height - sr.viewport.rect.height);
            if (!moving) target = c.anchoredPosition.y;
            target = Mathf.Clamp(target - Mathf.Clamp(e.scrollDelta.y, -3, 3) * pixelsPerNotch, 0, max);
            moving = true;
        }

        void Update()
        {
            if (!moving) return;
            var c = sr.content;
            float y = Mathf.Lerp(c.anchoredPosition.y, target, 1 - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            if (Mathf.Abs(y - target) < 0.5f) { y = target; moving = false; }
            c.anchoredPosition = new Vector2(c.anchoredPosition.x, y);
        }
    }
}
