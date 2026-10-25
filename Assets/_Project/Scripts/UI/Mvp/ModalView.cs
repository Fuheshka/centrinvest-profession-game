using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // One modal for confirmations, night report, review, choices. While it is open the workday clock stands.
    public class ModalView : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text title, body;
        public Transform options;
        public Button optionPrefab;

        public bool IsOpen => root.activeSelf;

        void Awake() => root.SetActive(false);

        public void Hide() => root.SetActive(false);

        public void Show(string head, string text, params (string label, Action action)[] buttons) => ShowSized(860, head, text, buttons);

        // Narrower panel and slimmer buttons for short menus such as the pause.
        public void ShowSized(float width, string head, string text, params (string label, Action action)[] buttons)
        {
            var panel = (RectTransform)options.parent;
            panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
            bool compact = width < 700;
            title.text = head;
            body.text = text;
            body.gameObject.SetActive(!string.IsNullOrEmpty(text));
            foreach (Transform c in options) Destroy(c.gameObject);
            int n = 0;
            foreach (var b in buttons)
            {
                var btn = Instantiate(optionPrefab, options);
                var img = btn.GetComponent<Image>(); var txt = btn.GetComponentInChildren<TMP_Text>();
                txt.text = b.label;
                bool quiet = b.label == "Отмена" || b.label == "Закрыть" || n > 0;                                  // the first button is the main action
                img.color = quiet ? Palette.Secondary : Palette.Primary;
                if (b.label.StartsWith("Удалить") || b.label.StartsWith("Уволить")) { img.color = Palette.Red; txt.color = Palette.Cream; }    // destructive
                var le = btn.GetComponent<LayoutElement>();
                if (compact && le != null) le.preferredHeight = Mathf.Min(le.preferredHeight, 56);
                var tag = btn.gameObject.AddComponent<SfxButton>();
                tag.fx = img.color == Palette.Primary ? Fx.Confirm : Fx.Click;
                n++;
                var act = b.action;
                btn.onClick.AddListener(() => { root.SetActive(false); act?.Invoke(); });
            }
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }
    }
}
