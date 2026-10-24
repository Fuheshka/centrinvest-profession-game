using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Studio.UI
{
    // Final screen: appears over everything once the stage is Done.
    public class EndingPanelView : MonoBehaviour
    {
        // shortcut: placeholder link, the real page comes with the presentation material
        const string ProfessionUrl = "https://example.com/game-designer";

        MessengerController controller;
        GameObject content;

        void Start()
        {
            controller = FindAnyObjectByType<MessengerController>();
            var topBar = FindAnyObjectByType<TopBarView>();
            if (topBar != null) UiKit.Font = topBar.budgetText.font;
            var bg = UiKit.Box(transform, "Content", UiKit.Bg);
            content = bg.gameObject;
            UiKit.Stretch(bg.rectTransform);
            content.SetActive(false);
        }

        void Update()
        {
            if (controller == null || controller.State == null || content.activeSelf) return;
            if (controller.State.stage != Stage.Done) return;
            var rt = (RectTransform)transform;
            UiKit.Stretch(rt);
            rt.SetAsLastSibling();
            content.SetActive(true);
            Build(controller.State, controller.db);
        }

        void Build(GameState s, ContentDB db)
        {
            var root = (RectTransform)content.transform;
            Format.Clear(root);
            var h = UiKit.H(root, 40, 60);
            h.childForceExpandHeight = true;

            var leftCol = UiKit.Node(root, "Left");
            UiKit.V(leftCol, 20);
            UiKit.Size(leftCol, flexW: 1.4f);
            var rightImg = UiKit.Box(root, "Right", UiKit.Panel);
            UiKit.V(rightImg.rectTransform, 14, 28);
            UiKit.Size(rightImg, flexW: 1);

            int score = Ending.Score(s.axes);
            var verdict = Ending.VerdictFor(score);
            string title = Ending.GameTitle(s, db);
            Color vc = verdict == Verdict.Flop ? UiKit.Red : verdict == Verdict.Average ? UiKit.Yellow : UiKit.Green;

            UiKit.Label(leftCol, "РЕЛИЗ · ДЕНЬ " + Mathf.Min(s.day, s.deadline), 18, UiKit.Accent, FontStyles.Bold);
            UiKit.Label(leftCol, Ending.Headline(verdict, title), 44, UiKit.Text, FontStyles.Bold);

            var scoreRow = UiKit.Node(leftCol, "Score");
            UiKit.H(scoreRow, 24).childAlignment = TextAnchor.MiddleLeft;
            var ring = UiKit.Box(scoreRow, "Ring", vc);
            UiKit.Size(ring, w: 150, h: 150);
            var inner = UiKit.Box(ring.transform, "Inner", UiKit.Bg);
            inner.rectTransform.anchorMin = Vector2.zero;
            inner.rectTransform.anchorMax = Vector2.one;
            inner.rectTransform.offsetMin = new Vector2(10, 10);
            inner.rectTransform.offsetMax = new Vector2(-10, -10);
            var num = UiKit.Label(inner.transform, score.ToString(), 60, UiKit.Text, FontStyles.Bold, TextAlignmentOptions.Center);
            UiKit.Stretch(num.rectTransform);
            var verdictBox = UiKit.Node(scoreRow, "Verdict");
            UiKit.V(verdictBox, 4);
            UiKit.Label(verdictBox, Ending.VerdictName(verdict), 34, vc, FontStyles.Bold);
            UiKit.Label(verdictBox, "Оценка критиков и игроков · версий до релиза: " + s.versions.Count, 18, UiKit.Dim);

            var grid = UiKit.Node(leftCol, "Reviews");
            UiKit.V(grid, 10);
            foreach (var q in Ending.Reviews(s, db))
            {
                var card = UiKit.Box(grid, "Review", UiKit.Panel);
                UiKit.V(card.rectTransform, 2, 14);
                UiKit.Label(card.transform, "«" + q.text + "»", 18, UiKit.Text);
                UiKit.Label(card.transform, q.author, 14, UiKit.Dim);
            }

            var spacer = UiKit.Node(leftCol, "Spacer");
            UiKit.Size(spacer, flexH: 1);
            var buttons = UiKit.Node(leftCol, "Buttons");
            UiKit.H(buttons, 16).childForceExpandWidth = false;
            var newBtn = UiKit.TextButton(buttons, "Новый проект", UiKit.Accent, UiKit.Bg, () => SceneManager.LoadScene(0), 60);
            UiKit.Size(newBtn, w: 240, h: 60);
            var moreBtn = UiKit.TextButton(buttons, "Узнать о профессии больше", UiKit.Panel2, UiKit.Text, () => Application.OpenURL(ProfessionUrl), 60);
            UiKit.Size(moreBtn, w: 340, h: 60);

            UiKit.Label(rightImg.transform, "Разбор твоих решений", 26, UiKit.Text, FontStyles.Bold);
            foreach (var hl in Ending.Breakdown(s, db))
            {
                var row = UiKit.Node(rightImg.transform, "Highlight");
                UiKit.V(row, 2);
                UiKit.Label(row, (hl.good ? "+ " : "– ") + hl.title, 20, hl.good ? UiKit.Green : UiKit.Red, FontStyles.Bold);
                UiKit.Label(row, hl.note, 16, UiKit.Dim);
            }
            UiKit.Label(rightImg.transform, string.Format(Ending.ProfessionText, Mathf.Min(s.day, s.deadline)), 18, UiKit.Dim);
        }
    }
}
