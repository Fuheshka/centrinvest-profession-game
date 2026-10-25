using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // A link is a stretched, rotated Image between two block centres (ROADMAP risk note: simpler than LineRenderer in uGUI).
    // Its tag (effects label) lives in a layer above the blocks so a block never hides it.
    public class LinkView : MonoBehaviour
    {
        public Image line;
        public CurvedImage curve;
        public Button button;
        public new RectTransform tag;
        public Button tagButton;
        public TMP_Text label;

        // Geometry only: called every frame while a block is dragged.
        public void Place(Vector2 a, Vector2 b, float bend = 0f)
        {
            var rt = (RectTransform)transform;
            var d = b - a;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = (a + b) / 2;
            rt.sizeDelta = new Vector2(d.magnitude, Mathf.Max(14f, Mathf.Abs(bend) * 2f + 14f));
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            if (line != null && line.rectTransform != rt)
            {
                var lineRect = line.rectTransform;
                lineRect.anchorMin = lineRect.anchorMax = lineRect.pivot = new Vector2(0.5f, 0.5f);
                lineRect.anchoredPosition = Vector2.zero;
                lineRect.sizeDelta = rt.sizeDelta;
            }
            var normal = d.sqrMagnitude > 0.01f ? new Vector2(-d.y, d.x).normalized : Vector2.zero;
            tag.anchoredPosition = rt.anchoredPosition + normal * bend;
            if (curve != null) curve.SetBend(bend);
        }

        public void Bind(DashboardWindow w, ConceptLink link, Vector2 a, Vector2 b, Color c, string text)
        {
            if (tag.parent != w.tagLayer) tag.SetParent(w.tagLayer, false);
            line.color = c;
            if (curve != null) curve.color = c;
            label.text = text;
            label.ForceMeshUpdate();
            tag.sizeDelta = new Vector2(label.preferredWidth + 28, 38);
            Place(a, b);
            foreach (var btn in new[] { button, tagButton })
            {
                if (btn == null) continue;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => w.LinkClicked(link));
            }
        }

        void OnDestroy() { if (tag) Destroy(tag.gameObject); }
    }
}
