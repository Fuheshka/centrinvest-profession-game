using System.Collections;
using UnityEngine;

namespace Studio.UI.Mvp
{
    // Fade-in of a screen when its window opens (tab switch). Needs a CanvasGroup; MotionKit adds both.
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelFade : MonoBehaviour
    {
        public float duration = 0.18f;
        CanvasGroup group;

        void OnEnable()
        {
            group = GetComponent<CanvasGroup>();
            StopAllCoroutines();
            StartCoroutine(Fade());
        }

        IEnumerator Fade()
        {
            for (float x = 0; x < duration; x += Time.unscaledDeltaTime) { group.alpha = x / duration; yield return null; }
            group.alpha = 1;
        }
    }
}
