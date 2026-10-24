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
        public Button tabChats, tabBoard, tabVersions;
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
            State = GameState.New(db, "Игрок", difficulty);
            // Until DayClock (P-6) delivers mail, day-1 messages of the stage land in the inbox at start.
            foreach (var m in db.messages)
                if (m != null && m.day <= State.day && m.stage == State.stage) State.inbox.Add(m.id);

            tabChats.onClick.AddListener(() => ShowTab(0));
            tabBoard.onClick.AddListener(() => ShowTab(1));
            tabVersions.onClick.AddListener(() => ShowTab(2));
            ShowTab(0);
            RefreshAll();
        }

        void ShowTab(int i)
        {
            chatsTab.SetActive(i == 0);
            boardTab.SetActive(i == 1);
            versionsTab.SetActive(i == 2);
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

        void RefreshAll()
        {
            if (topBar != null) topBar.Refresh(State);
            chatList.Refresh(State, db, selected, Select);
            chatView.Refresh(State, db, selected, Pick);
        }
    }
}
