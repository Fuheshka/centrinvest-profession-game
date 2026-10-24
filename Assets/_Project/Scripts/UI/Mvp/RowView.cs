using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Generic list row prefab: background, up to three texts and two buttons. Each window fills what it needs.
    public class RowView : MonoBehaviour
    {
        public Image background, avatar;
        public TMP_Text title, subtitle, right;
        public Button main, button1, button2;
        public TMP_Text button1Label, button2Label;
        public GameObject progressBar;       // optional thin bar under the subtitle (task rows)
        public RectTransform progressFill;   // stretched child; its right anchor is the progress
        bool refilled;   // set by At: the caller rebinds both buttons every time, so Set must not toggle them (toggling reset hover and blinked)

        // Rows are built once and refilled in place: rebuilding them every redraw flickered and reset the scroll.
        public static RowView At(Transform list, RowView prefab, int i)
        {
            while (list.childCount <= i) Instantiate(prefab, list).refilled = true;
            return list.GetChild(i).GetComponent<RowView>();
        }

        public void Set(string t, string sub = null, string r = null)
        {
            title.text = t;
            if (subtitle) { subtitle.text = sub ?? ""; subtitle.gameObject.SetActive(!string.IsNullOrEmpty(sub)); }
            if (right) { right.text = r ?? ""; right.gameObject.SetActive(!string.IsNullOrEmpty(r)); }
            if (refilled) return;
            if (button1) button1.gameObject.SetActive(false);
            if (button2) button2.gameObject.SetActive(false);
        }

        // 0..1 fills the bar, a negative value hides it.
        public void Progress(float v)
        {
            if (!progressBar) return;
            progressBar.SetActive(v >= 0);
            if (v >= 0) progressFill.anchorMax = new Vector2(Mathf.Clamp01(v), 1);
        }

        public void Button1(string label, bool on, UnityEngine.Events.UnityAction act) => Bind(button1, button1Label, label, on, act);
        public void Button2(string label, bool on, UnityEngine.Events.UnityAction act) => Bind(button2, button2Label, label, on, act);

        static void Bind(Button b, TMP_Text l, string label, bool on, UnityEngine.Events.UnityAction act)
        {
            if (!b) return;
            b.gameObject.SetActive(label != null);
            if (label == null) return;
            l.text = label;
            b.interactable = on;
            b.onClick.RemoveAllListeners();
            if (act != null) b.onClick.AddListener(act);
        }
    }
}
