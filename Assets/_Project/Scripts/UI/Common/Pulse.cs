using System.Collections;
using TMPro;
using UnityEngine;

namespace Studio.UI.Mvp
{
    // One-shot accents from docs/UI_MOTION.md: a short scale pop (and optional colour flash) that settles back. Unscaled time.
    public static class Pulse
    {
        public static void Pop(MonoBehaviour host, Transform t, float peak = 1.08f, float duration = 0.3f, TMP_Text flash = null, Color? flashColor = null)
        {
            if (host == null || !host.isActiveAndEnabled) return;
            host.StartCoroutine(Run(t, peak, duration, flash, flashColor));
        }

        // A short sideways shake for a refused action; applied as deltas so layout-driven positions survive.
        public static void Nudge(MonoBehaviour host, Transform t, float px = 7f, float duration = 0.25f)
        {
            if (host == null || !host.isActiveAndEnabled) return;
            host.StartCoroutine(Shake((RectTransform)t, px, duration));
        }

        static IEnumerator Shake(RectTransform t, float px, float duration)
        {
            float applied = 0;
            for (float x = 0; x < duration && t != null; x += Time.unscaledDeltaTime)
            {
                float k = x / duration, want = Mathf.Sin(k * Mathf.PI * 4) * (1 - k) * px;
                t.anchoredPosition += Vector2.right * (want - applied); applied = want;
                yield return null;
            }
            if (t != null) t.anchoredPosition += Vector2.left * applied;
        }

        static IEnumerator Run(Transform t, float peak, float duration, TMP_Text flash, Color? flashColor)
        {
            var baseScale = t.localScale; var baseColor = flash != null ? flash.color : default;
            for (float x = 0; x < duration; x += Time.unscaledDeltaTime)
            {
                if (t == null) yield break;   // the row was rebuilt meanwhile
                float k = x / duration, bump = Mathf.Sin(k * Mathf.PI);   // 0 -> 1 -> 0
                t.localScale = baseScale * (1 + (peak - 1) * bump);
                if (flash != null && flashColor.HasValue) flash.color = Color.Lerp(baseColor, flashColor.Value, bump);
                yield return null;
            }
            if (t == null) yield break;
            t.localScale = baseScale;
            if (flash != null) flash.color = baseColor;
        }
    }
}
