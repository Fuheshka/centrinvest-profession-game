using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class TopBarView : MonoBehaviour
    {
        public Button[] tabs = new Button[3];   // Board, Chats, Versions
        public TMP_Text deadlineText;
        public Image deadlineFill;
        public TMP_Text budgetText;
        public Button endDayButton;
        public Image statusBackground, statusDivider;
        public TMP_Text budgetLabel;
        public Color statusOlive = new Color32(0x61, 0x78, 0x2B, 255);
        public Color statusPale = new Color32(0xD8, 0xE2, 0xA6, 255);
        public Color statusInk = new Color32(0x26, 0x09, 0x00, 255);
        public Color statusLight = new Color32(0xED, 0xF4, 0xE4, 255);
        public Color tabActive = new Color32(0xD5, 0xC0, 0x4D, 255);
        public Color tabIdle = new Color32(0x9C, 0x8B, 0x30, 255);
        public Vector2 tabActiveSize = new Vector2(250, 68), tabIdleSize = new Vector2(190, 52);

        public event Action EndDayClicked;
        public event Action<int> TabClicked;

        void Awake()
        {
            endDayButton.onClick.AddListener(() => EndDayClicked?.Invoke());
            for (int i = 0; i < tabs.Length; i++)
            {
                int n = i;
                tabs[i].onClick.AddListener(() => TabClicked?.Invoke(n));
            }
        }

        public void SetTab(int active)
        {
            bool chat = active == 1;
            if (statusBackground != null) statusBackground.color = chat ? statusPale : statusOlive;
            if (statusDivider != null) statusDivider.color = chat ? statusInk : statusPale;
            deadlineText.color = budgetText.color = chat ? statusInk : statusLight;
            if (budgetLabel != null) budgetLabel.color = chat ? statusInk : statusPale;
            for (int i = 0; i < tabs.Length; i++)
            {
                bool on = i == active;
                tabs[i].targetGraphic.color = on ? tabActive : tabIdle;
                ((RectTransform)tabs[i].transform).sizeDelta = on ? tabActiveSize : tabIdleSize;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)tabs[0].transform.parent);
        }

        public void Refresh(GameState s)
        {
            deadlineText.text = $"До дедлайна: {Mathf.Max(0, s.deadline - s.day)} дн. · день {s.day}/{s.deadline}";
            deadlineFill.fillAmount = s.deadline > 0 ? Mathf.Clamp01((float)s.day / s.deadline) : 0;
            budgetText.text = Format.Money(s.budget);
        }
    }
}
