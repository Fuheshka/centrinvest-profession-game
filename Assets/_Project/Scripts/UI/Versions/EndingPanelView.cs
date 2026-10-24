using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Studio.UI
{
    // Final screen: appears over everything once the stage is Done. Visuals live in the prefab.
    public class EndingPanelView : MonoBehaviour
    {
        // shortcut: placeholder link, the real page comes with the presentation material
        const string ProfessionUrl = "https://example.com/game-designer";

        public GameObject content;   // hidden in the prefab
        public TMP_Text kicker, headline, scoreText, verdictText, verdictSub, professionText;
        public Image ring;
        public Transform reviewList, breakdownList;
        public ReviewCardView reviewPrefab;
        public HighlightRowView highlightPrefab;
        public Button newButton, moreButton;

        MessengerController controller;

        void Start()
        {
            controller = FindAnyObjectByType<MessengerController>();
            newButton.onClick.AddListener(() => SceneManager.LoadScene(0));
            moreButton.onClick.AddListener(() => Application.OpenURL(ProfessionUrl));
        }

        void Update()
        {
            if (controller == null || controller.State == null || content.activeSelf) return;
            if (controller.State.stage != Stage.Done) return;
            transform.SetAsLastSibling();
            content.SetActive(true);
            Fill(controller.State, controller.db);
        }

        void Fill(GameState s, ContentDB db)
        {
            int score = Ending.Score(s.axes);
            var verdict = Ending.VerdictFor(score);
            Color vc = verdict == Verdict.Flop ? Pal.Red : verdict == Verdict.Average ? Pal.Yellow : Pal.Green;

            kicker.text = "РЕЛИЗ · ДЕНЬ " + Mathf.Min(s.day, s.deadline);
            headline.text = Ending.Headline(verdict, Ending.GameTitle(s, db));
            ring.color = vc;
            scoreText.text = score.ToString();
            verdictText.text = Ending.VerdictName(verdict); verdictText.color = vc;
            verdictSub.text = "Оценка критиков и игроков · версий до релиза: " + s.versions.Count;

            Format.Clear(reviewList);
            foreach (var q in Ending.Reviews(s, db)) Instantiate(reviewPrefab, reviewList).Setup(q);
            Format.Clear(breakdownList);
            foreach (var h in Ending.Breakdown(s, db)) Instantiate(highlightPrefab, breakdownList).Setup(h);
            professionText.text = string.Format(Ending.ProfessionText, Mathf.Min(s.day, s.deadline));
        }
    }
}
