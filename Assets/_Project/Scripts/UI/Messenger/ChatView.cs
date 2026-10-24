using System;
using Studio.Core;
using TMPro;
using UnityEngine;

namespace Studio.UI
{
    public class ChatView : MonoBehaviour
    {
        public TMP_Text headerName, headerStatus, emptyText;
        public Transform messages, options;
        public BubbleView bubblePrefab;
        public OptionButtonView optionPrefab;
        public GameObject optionBar;

        public void Refresh(GameState s, ContentDB db, Sender sender, Action<MessageSO, OptionSO> pick)
        {
            Format.Clear(messages);
            Format.Clear(options);
            headerName.text = Format.SenderName(sender);

            MessageSO current = null;
            foreach (var id in s.inbox)
            {
                var m = MessengerController.Find(db, id);
                if (m == null || m.sender != sender) continue;
                Instantiate(bubblePrefab, messages).Setup(s, m, current == null, pick);
                if (current == null) current = m;
            }
            headerStatus.text = current != null ? "ждёт ответа" : "свободна";
            emptyText.gameObject.SetActive(current == null);
            optionBar.SetActive(current != null && current.options.Length > 0);
            if (current == null) return;
            foreach (var o in current.options)
            {
                var opt = o;
                Instantiate(optionPrefab, options).Setup(s, opt, () => pick(current, opt));
            }
        }
    }
}
