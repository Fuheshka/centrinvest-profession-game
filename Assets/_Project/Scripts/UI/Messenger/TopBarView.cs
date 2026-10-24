using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class TopBarView : MonoBehaviour
    {
        public Image[] stagePills = new Image[3];
        public TMP_Text deadlineText;
        public Image deadlineFill;
        public TMP_Text budgetText;
        public Button endDayButton;
        public Color pillActive = new Color(1f, 0.54f, 0.3f);
        public Color pillIdle = new Color(0.15f, 0.16f, 0.2f);

        public event Action EndDayClicked;

        void Awake() => endDayButton.onClick.AddListener(() => EndDayClicked?.Invoke());

        public void Refresh(GameState s)
        {
            int active = s.stage == Stage.Idea ? 0 : s.stage == Stage.Production ? 1 : 2;
            for (int i = 0; i < stagePills.Length; i++)
                stagePills[i].color = i == active ? pillActive : pillIdle;
            deadlineText.text = $"До дедлайна: {Mathf.Max(0, s.deadline - s.day)} дней · день {s.day} из {s.deadline}";
            deadlineFill.fillAmount = s.deadline > 0 ? Mathf.Clamp01((float)s.day / s.deadline) : 0;
            budgetText.text = Format.Money(s.budget);
        }
    }
}
