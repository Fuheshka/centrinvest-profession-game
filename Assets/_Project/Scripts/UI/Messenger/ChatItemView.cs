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
            background.color = selected ? new Color32(0xD8, 0xE2, 0xA6, 255) : new Color32(0xED, 0xF4, 0xE4, 255);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(sender));
        }
    }
}
