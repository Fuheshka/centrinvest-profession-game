using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Shared button feedback from docs/UI_MOTION.md: lift on hover, squeeze on press, outline on keyboard focus.
    // Add to a button prefab; all numbers are inspector fields. Disabled buttons do not move.
    [RequireComponent(typeof(Selectable))]
    public class ButtonMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        public RectTransform target;
        public Graphic focusOutline;
        public float hoverScale = 1.03f, pressScale = 0.96f, hoverLift = 2f, duration = 0.12f;

        Selectable button;
        Vector3 baseScale;
        float lift;   // current applied lift in px; applied as a delta so the button's own position is never stored or overwritten
        bool hover, down;
        Coroutine run;

        void Awake()
        {
            button = GetComponent<Selectable>();
            if (target == null) target = (RectTransform)transform;
            baseScale = target.localScale;
            if (focusOutline != null) focusOutline.enabled = false;
        }

        void OnDisable() { hover = down = false; target.localScale = baseScale; Lift(0); }

        void Lift(float to) { if (to == lift) return; target.anchoredPosition += Vector2.up * (to - lift); lift = to; }

        public void OnPointerEnter(PointerEventData e) { hover = true; Play(); }
        public void OnPointerExit(PointerEventData e) { hover = down = false; Play(); }
        public void OnPointerDown(PointerEventData e) { down = true; Play(); }
        public void OnPointerUp(PointerEventData e) { down = false; Play(); }
        public void OnSelect(BaseEventData e) { if (focusOutline != null && !(e is PointerEventData)) focusOutline.enabled = true; }
        public void OnDeselect(BaseEventData e) { if (focusOutline != null) focusOutline.enabled = false; }

        void Play()
        {
            bool live = button.IsInteractable();
            float s = !live ? 1 : down ? pressScale : hover ? hoverScale : 1;
            float want = live && hover && !down ? hoverLift : 0;
            if (run != null) StopCoroutine(run);
            if (isActiveAndEnabled) run = StartCoroutine(Tween(baseScale * s, want));
        }

        IEnumerator Tween(Vector3 scale, float liftTo)
        {
            Vector3 s0 = target.localScale; float l0 = lift;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = 1 - (1 - t / duration) * (1 - t / duration);   // ease-out
                target.localScale = Vector3.LerpUnclamped(s0, scale, k);
                Lift(Mathf.LerpUnclamped(l0, liftTo, k));
                yield return null;
            }
            target.localScale = scale;
            Lift(liftTo);
        }
    }
}
