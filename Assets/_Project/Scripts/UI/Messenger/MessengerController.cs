using Studio.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class MessengerController : MonoBehaviour
    {
        public ContentDB db;
        public DifficultyLevel difficulty = DifficultyLevel.Designer;
        public ChatListView chatList;
        public ChatView chatView;
        public GameObject chatsTab, boardTab, versionsTab;

        TopBarView topBar;
        Sender selected = Sender.Boss;

        public GameState State { get; private set; }

        public static MessageSO Find(ContentDB db, string id)
        {
            if (db.brief != null && db.brief.id == id) return db.brief;
            foreach (var m in db.messages) if (m != null && m.id == id) return m;
            return null;
        }

        void Start()
        {
            topBar = FindAnyObjectByType<TopBarView>();
            if (Session.Continue) State = SaveSystem.Load();
            if (State == null)
            {
                State = GameState.New(db, Session.Nickname ?? MenuController.DefaultName, Session.Nickname != null ? Session.Level : difficulty);
                // DayClock delivers from day 2 on; day-1 messages land in the inbox at start.
                foreach (var m in db.messages)
                    if (m != null && m.day >= 1 && m.day <= State.day && m.stage == State.stage) State.inbox.Add(m.id);   // day 0 = follow-up, arrives only via pending
            }
            Session.Continue = false;

            if (topBar != null)
            {
                topBar.EndDayClicked += EndDay;
                topBar.TabClicked += OnTab;
            }
            ShowTab(0);
            RefreshAll();
        }

        void OnDestroy()
        {
            if (topBar == null) return;
            topBar.EndDayClicked -= EndDay;
            topBar.TabClicked -= OnTab;
        }

        // Top bar order is Board, Chats, Versions; ShowTab keeps the old order Chats, Board, Versions.
        void OnTab(int i) => ShowTab(i == 0 ? 1 : i == 1 ? 0 : 2);

        void EndDay()
        {
            DayClock.EndDay(State, db);
            RefreshAll();
        }

        public void ShowTab(int i)
        {
            SaveSystem.Save(State);   // after a pitch or a build the UI switches tab, so this covers Commit and Review too
            chatsTab.SetActive(i == 0);
            boardTab.SetActive(i == 1);
            versionsTab.SetActive(i == 2);
            if (topBar != null) topBar.SetTab(i == 0 ? 1 : i == 1 ? 0 : 2);
        }

        void Select(Sender s)
        {
            selected = s;
            RefreshAll();
        }

        void Pick(MessageSO m, OptionSO o)
        {
            if (!DecisionResolver.CanAfford(State, o)) return;
            DecisionResolver.Apply(State, m, o, db);
            RefreshAll();
        }

        // Called by the Versions tab after it changes the state (boss reply, reopen).
        public void Refresh() => RefreshAll();

        void RefreshAll()
        {
            SaveSystem.Save(State);
            if (topBar != null) topBar.Refresh(State);
            chatList.Refresh(State, db, selected, Select);
            chatView.Refresh(State, db, selected, Pick);
        }
    }
}
