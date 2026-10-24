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
        public Button button;
        public RectTransform tag;
        public Button tagButton;
        public TMP_Text label;

        // Geometry only: called every frame while a block is dragged.
        public void Place(Vector2 a, Vector2 b)
        {
            var rt = (RectTransform)transform;
            var d = b - a;
            rt.anchoredPosition = (a + b) / 2;
            rt.sizeDelta = new Vector2(d.magnitude, 14);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            tag.anchoredPosition = rt.anchoredPosition;
        }

        public void Bind(DashboardWindow w, ConceptLink link, Vector2 a, Vector2 b, Color c, string text)
        {
            if (tag.parent != w.tagLayer) tag.SetParent(w.tagLayer, false);
            line.color = c;
            label.text = text;
            label.ForceMeshUpdate();
            tag.sizeDelta = new Vector2(label.preferredWidth + 28, 38);
            Place(a, b);
            foreach (var btn in new[] { button, tagButton })
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => w.LinkClicked(link));
            }
        }

        void OnDestroy() { if (tag) Destroy(tag.gameObject); }
    }
}
