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
        Vector3 baseScale; Vector2 basePos;
        bool hover, down;
        Coroutine run;

        void Awake()
        {
            button = GetComponent<Selectable>();
            if (target == null) target = (RectTransform)transform;
            baseScale = target.localScale; basePos = target.anchoredPosition;
            if (focusOutline != null) focusOutline.enabled = false;
        }

        void OnDisable() { hover = down = false; target.localScale = baseScale; target.anchoredPosition = basePos; }

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
            float lift = live && hover && !down ? hoverLift : 0;
            if (run != null) StopCoroutine(run);
            if (isActiveAndEnabled) run = StartCoroutine(Tween(baseScale * s, basePos + Vector2.up * lift));
        }

        IEnumerator Tween(Vector3 scale, Vector2 pos)
        {
            Vector3 s0 = target.localScale; Vector2 p0 = target.anchoredPosition;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = 1 - (1 - t / duration) * (1 - t / duration);   // ease-out
                target.localScale = Vector3.LerpUnclamped(s0, scale, k);
                target.anchoredPosition = Vector2.LerpUnclamped(p0, pos, k);
                yield return null;
            }
            target.localScale = scale; target.anchoredPosition = pos;
        }
    }
}
