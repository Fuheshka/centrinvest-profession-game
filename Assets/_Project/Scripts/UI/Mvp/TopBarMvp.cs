using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    public class TopBarMvp : MonoBehaviour
    {
        public GameLoop game;
        public TMP_Text dayText, budgetText;
        public Button endDay, pause;
        public Button[] tabs = new Button[4];          // Chat, Dashboard, Headhunter, Tasks
        public GameObject[] windows = new GameObject[4];
        public TMP_Text hint;
        public GameObject chatBadge;       // red circle on the Chat tab
        public TMP_Text chatBadgeText;
        public TMP_Text phaseText;         // optional: part of the day on its own chip; when set, dayText and budgetText are single-line
        public Color tabOnColor, tabOffColor;       // optional (alpha > 0): flat tabs tinted by state instead of the old yellow/mustard
        public RectTransform timeFill;     // day clock: right anchor is the share of the workday gone

        int current, seen = -1, lastCount = -1;
        float toastUntil, pulse;
        string toast;

        void Awake()
        {
            for (int i = 0; i < tabs.Length; i++) { int k = i; tabs[i].onClick.AddListener(() => Show(k)); }
            endDay.onClick.AddListener(() => game.AskEndDay());
            pause.onClick.AddListener(() => game.ShowPause());
            game.Changed += Redraw;
            Show(0);
        }

        // Unread count on the Chat tab and a short toast in the hint line when a message arrives elsewhere.
        void Notify(LevelState s, LevelSO l)
        {
            if (chatBadge == null) return;
            int n = s.chat.Count;
            if (seen < 0) seen = lastCount = n;   // a loaded game starts fully read
            if (current == 0) seen = n;
            else if (n > lastCount)
            {
                var from = l.Candidate(s.chat[n - 1].chatId);
                toast = "Новое сообщение: " + (from != null ? from.name : "Борис Петрович");
                toastUntil = Time.unscaledTime + 4;
                pulse = 1;
            }
            lastCount = n;
            int unread = n - seen;
            chatBadge.SetActive(unread > 0);
            chatBadgeText.text = unread > 99 ? "99+" : unread.ToString();
            pulse = Mathf.MoveTowards(pulse, 0, Time.unscaledDeltaTime * 2);
            chatBadge.transform.localScale = Vector3.one * (1 + 0.5f * pulse);
        }

        public void Show(int i)
        {
            current = i;
            for (int k = 0; k < windows.Length; k++) windows[k].SetActive(k == i);
            Redraw();
        }

        void Redraw()
        {
            if (game.S == null) return;
            var s = game.S; var l = game.L;
            float f = s.dayElapsed / l.tuning.daySeconds;   // no countdown: a ticking clock only rushed the player
            string phase = s.stage == LevelStage.Concept ? "план" : f < 0.34f ? "утро" : f < 0.67f ? "день" : "вечер";
            if (phaseText != null) { dayText.text = "День " + s.day + " / " + s.deadline; phaseText.text = phase; budgetText.text = Names.Money(s.budget) + " ₽"; }
            else { dayText.text = "День " + s.day + " из " + s.deadline + "\n" + phase; budgetText.text = "Бюджет\n" + Names.Money(s.budget); }
            bool working = s.stage == LevelStage.Production;
            endDay.interactable = working;
            tabs[2].interactable = tabs[3].interactable = s.stage != LevelStage.Concept;
            Notify(s, l);
            if (timeFill) timeFill.anchorMax = new Vector2(s.stage == LevelStage.Concept ? 0 : Mathf.Clamp01(f), 1);
            hint.text = Time.unscaledTime < toastUntil ? toast : s.stage == LevelStage.Concept ? "День начнётся после утверждения концепта" : "";
            for (int k = 0; k < tabs.Length; k++)
            {
                var img = tabs[k].GetComponent<Image>();
                if (tabOnColor.a > 0)
                {
                    img.color = k == current ? tabOnColor : tabOffColor;
                    tabs[k].GetComponentInChildren<TMP_Text>().color = k == current ? Palette.Ink : Palette.Cream;
                    continue;
                }
                if (img) img.color = k == current ? Palette.Yellow : Palette.Mustard;
                tabs[k].transform.localScale = Vector3.one * (k == current ? 1.08f : 1f);
            }
        }
    }

    // Colours of the author's mockups (docs/mockup, P-14 reference screens).
    public static class Palette
    {
        public static readonly Color Bg = Hex("1E1A04"), Olive = Hex("6A7837"), Light = Hex("D8E2A6"), Cream = Hex("ECF3E2"),
            Yellow = Hex("D5C04F"), Mustard = Hex("9C8C34"), Ink = Hex("2A0F02"), Green = Hex("2E9E6A"), Red = Hex("C93A3F"),
            Dim = Hex("5B6430"), Neutral = Hex("8A8F72"),
            Primary = Hex("E7CC38"), Secondary = Hex("DADCC6"), TabOff = Hex("4B3225");

        public static Color Hex(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }
    }
}
