using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class JournalRowView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public Outline border;
        public TMP_Text day, stage, text;

        public void Setup(int d, string stageName, string t, bool culprit, bool interactable, Action click)
        {
            day.text = "день " + d; stage.text = stageName; text.text = t;
            background.color = culprit ? Pal.GreenTint : Pal.Panel2;
            border.enabled = culprit;
            button.interactable = interactable;
            day.alpha = stage.alpha = text.alpha = interactable ? 1f : 0.3f;   // non-suspects fade out (B-021)
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => click());
        }
    }
}
