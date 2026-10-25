using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // First visit to the dashboard: text on the left, a looped mock of the real gestures on the right
    // (drag a block, press "Связать блоки", click two blocks). Everything is scene objects; this only animates them.
    public class DashboardIntro : MonoBehaviour
    {
        public static bool IsOpen { get; private set; }   // the workday clock stands while it is open

        public GameObject root;
        public Button ok;
        public RectTransform cursor, blockA, blockB, linkButton, line, tag;
        public Image linkButtonImage, blockAImage;
        public TMP_Text caption;

        Vector2 homeA, homeB, homeLink;
        Coroutine loop;

        void Awake()
        {
            homeA = blockA.anchoredPosition; homeB = blockB.anchoredPosition; homeLink = linkButton.anchoredPosition;
            ok.onClick.AddListener(Hide);   // the object starts inactive, so this runs on the first Show
        }

        public void Show()
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            IsOpen = true;
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(Loop());
        }

        void Hide() { root.SetActive(false); IsOpen = false; }
        void OnDisable() => IsOpen = false;

        IEnumerator Loop()
        {
            while (true)
            {
                ResetDemo();
                cursor.anchoredPosition = homeLink + new Vector2(0, -160);
                yield return Wait(0.6f);

                caption.text = "Блок можно перетащить";
                yield return Move(cursor, homeA + new Vector2(10, -20), 0.7f);
                blockA.localScale = Vector3.one * 1.06f;
                yield return Drag(blockA, homeA + new Vector2(0, -35), 0.5f);
                yield return Drag(blockA, homeA, 0.5f);
                blockA.localScale = Vector3.one;
                yield return Wait(0.4f);

                caption.text = "Нажми «Связать блоки»";
                yield return Move(cursor, homeLink + new Vector2(30, -10), 0.8f);
                yield return Press(linkButton);
                linkButtonImage.color = Palette.Green;
                yield return Wait(0.5f);

                caption.text = "Кликни первый блок";
                yield return Move(cursor, homeA + new Vector2(10, -20), 0.8f);
                yield return Press(blockA);
                blockAImage.color = new Color(1f, 0.8f, 0.55f);
                yield return Wait(0.5f);

                caption.text = "Кликни второй блок";
                yield return Move(cursor, homeB + new Vector2(10, -20), 0.8f);
                yield return Press(blockB);
                blockAImage.color = Color.white;
                linkButtonImage.color = Palette.Primary;
                PlaceLine();
                caption.text = "Готово: на линии видно, что даёт связь";
                yield return Pop(tag);
                yield return Wait(2f);
            }
        }

        void ResetDemo()
        {
            blockA.anchoredPosition = homeA; blockA.localScale = Vector3.one;
            blockAImage.color = Color.white;
            linkButtonImage.color = Palette.Primary;
            line.gameObject.SetActive(false); tag.gameObject.SetActive(false);
        }

        // Side ports sit at 41.1% of the sticker width from its centre, same as BlockView.PortOnSide.
        void PlaceLine()
        {
            float half = blockA.rect.width * 0.411f;
            var a = blockA.anchoredPosition + new Vector2(half, 0); var b = blockB.anchoredPosition - new Vector2(half, 0);
            line.anchoredPosition = (a + b) / 2;
            line.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), line.sizeDelta.y);
            tag.anchoredPosition = line.anchoredPosition + new Vector2(0, 34);
            line.gameObject.SetActive(true); tag.gameObject.SetActive(true);
        }

        static IEnumerator Wait(float s) { for (float t = 0; t < s; t += Time.unscaledDeltaTime) yield return null; }

        static IEnumerator Move(RectTransform r, Vector2 to, float dur)
        {
            var from = r.anchoredPosition;
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime) { r.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, t / dur)); yield return null; }
            r.anchoredPosition = to;
        }

        // The block follows the cursor by the same offset.
        IEnumerator Drag(RectTransform block, Vector2 to, float dur)
        {
            var delta = to - block.anchoredPosition;
            var start = cursor.anchoredPosition;
            var from = block.anchoredPosition;
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0, 1, t / dur);
                block.anchoredPosition = from + delta * k; cursor.anchoredPosition = start + delta * k;
                yield return null;
            }
            block.anchoredPosition = to; cursor.anchoredPosition = start + delta;
        }

        IEnumerator Press(RectTransform target)
        {
            cursor.localScale = Vector3.one * 0.85f; target.localScale = Vector3.one * 0.95f;
            yield return Wait(0.12f);
            cursor.localScale = Vector3.one; target.localScale = Vector3.one;
        }

        static IEnumerator Pop(RectTransform t)
        {
            for (float x = 0; x < 0.3f; x += Time.unscaledDeltaTime) { t.localScale = Vector3.one * (1 + 0.3f * Mathf.Sin(x / 0.3f * Mathf.PI)); yield return null; }
            t.localScale = Vector3.one;
        }
    }
}
