using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Dropdown next to a block: a small card with a heading and a list of choices; a click outside closes it.
    public class PopupMenu : MonoBehaviour
    {
        public GameObject root;
        public RectTransform panel, bounds;
        public TMP_Text heading;
        public Transform items;
        public Button itemPrefab, backdrop;

        public bool IsOpen => root.activeSelf;

        void Awake() { backdrop.onClick.AddListener(Hide); root.SetActive(false); }

        public void Hide() => root.SetActive(false);

        // style: 0 plain, 1 current choice, 2 destructive
        public void Show(RectTransform anchor, string head, params (string label, Action action, int style)[] entries)
        {
            heading.text = head;
            foreach (Transform c in items) Destroy(c.gameObject);
            foreach (var e in entries)
            {
                var b = Instantiate(itemPrefab, items);
                var img = b.GetComponent<Image>(); var txt = b.GetComponentInChildren<TMP_Text>();
                txt.text = e.label;
                if (e.style == 1) img.color = Palette.Yellow;
                else if (e.style == 2) { img.color = Palette.Red; txt.color = Palette.Cream; }
                var act = e.action;
                b.onClick.AddListener(() => { Hide(); act?.Invoke(); });
            }
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            // top-left corner of the card at the block's right edge, then pushed back inside the board
            var ac = new Vector3[4]; anchor.GetWorldCorners(ac);
            var bc = new Vector3[4]; bounds.GetWorldCorners(bc);
            var size = Vector2.Scale(panel.rect.size, panel.lossyScale);
            float x = Mathf.Min(ac[2].x + 12 * panel.lossyScale.x, bc[2].x - size.x);
            float y = Mathf.Max(Mathf.Min(ac[1].y, bc[1].y), bc[0].y + size.y);
            panel.pivot = new Vector2(0, 1);
            panel.position = new Vector3(Mathf.Max(bc[0].x, x), y, panel.position.z);
        }
    }
}
