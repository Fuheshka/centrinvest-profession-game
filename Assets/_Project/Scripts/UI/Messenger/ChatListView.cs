using System;
using System.Collections.Generic;
using Studio.Core;
using UnityEngine;

namespace Studio.UI
{
    public class ChatListView : MonoBehaviour
    {
        public Transform content;
        public ChatItemView itemPrefab;

        public void Refresh(GameState s, ContentDB db, Sender selected, Action<Sender> onSelect)
        {
            Format.Clear(content);
            var known = new List<Sender>();
            void Know(MessageSO m) { if (m != null && !known.Contains(m.sender)) known.Add(m.sender); }
            Know(db.brief);
            foreach (var m in db.messages) Know(m);
            foreach (var id in s.inbox) Know(MessengerController.Find(db, id));
            known.Sort();

            foreach (var sender in known)
            {
                MessageSO last = null; int unread = 0;
                foreach (var id in s.inbox)
                {
                    var m = MessengerController.Find(db, id);
                    if (m == null || m.sender != sender) continue;
                    last = m; unread++;
                }
                var item = Instantiate(itemPrefab, content);
                item.Setup(sender, last != null ? Format.Name(last.text, s) : "Нет новых сообщений", last != null ? last.day : s.day,
                    unread, sender == selected, onSelect);
            }
        }
    }
}
