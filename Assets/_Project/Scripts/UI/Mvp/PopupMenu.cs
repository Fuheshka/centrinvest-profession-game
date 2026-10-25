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
        const int PageSize = 5;
        (string label, Action action, int style)[] entries;
        RectTransform anchor;
        string title;
        int page;

        public bool IsOpen => root.activeSelf;

        void Awake() { backdrop.onClick.AddListener(Hide); root.SetActive(false); }

        public void Hide() => root.SetActive(false);

        // style: 0 plain, 1 current choice, 2 destructive
        public void Show(RectTransform anchor, string head, params (string label, Action action, int style)[] entries)
        {
            title = head;
            this.anchor = anchor;
            this.entries = entries;
            page = 0;
            ShowPage();
        }

        void ShowPage()
        {
            int pages = Mathf.Max(1, (entries.Length + PageSize - 1) / PageSize);
            heading.text = pages == 1 ? title : title + " · " + (page + 1) + "/" + pages;
            foreach (Transform c in items) { c.gameObject.SetActive(false); Destroy(c.gameObject); }
            if (page > 0) AddButton("Назад", () => { page--; ShowPage(); }, 0, false);
            int end = Mathf.Min(entries.Length, (page + 1) * PageSize);
            for (int i = page * PageSize; i < end; i++)
            {
                var e = entries[i];
                AddButton(e.label, e.action, e.style, true);
            }
            if (end < entries.Length) AddButton("Далее", () => { page++; ShowPage(); }, 0, false);
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

        void AddButton(string label, Action action, int style, bool close)
        {
            var b = Instantiate(itemPrefab, items);
            var img = b.GetComponent<Image>(); var txt = b.GetComponentInChildren<TMP_Text>();
            txt.text = label;
            if (style == 1) img.color = Palette.Yellow;
            else if (style == 2) { img.color = Palette.Red; txt.color = Palette.Cream; }
            b.onClick.AddListener(() => { if (close) Hide(); action?.Invoke(); });
        }
    }
}
