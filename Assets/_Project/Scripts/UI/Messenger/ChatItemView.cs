using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class ChatItemView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public TMP_Text nameText, lastText, timeText, badgeText;
        public GameObject badge;

        public void Setup(Sender sender, string last, int day, int unread, bool selected, Action<Sender> onClick)
        {
            nameText.text = Format.SenderName(sender);
            lastText.text = last;
            timeText.text = "день " + day;
            badge.SetActive(unread > 0);
            badgeText.text = unread.ToString();
            background.color = selected ? new Color(0.145f, 0.16f, 0.2f) : new Color(0.086f, 0.094f, 0.12f);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(sender));
        }
    }
}
