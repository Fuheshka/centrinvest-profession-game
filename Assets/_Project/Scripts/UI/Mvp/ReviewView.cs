using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // The customer's verdict after a build: overall score, the three criteria as bars, remarks, then what to do next.
    public class ReviewView : MonoBehaviour
    {
        public GameObject root;
        public TMP_Text title, subtitle, score, need, remarks;
        public TMP_Text[] names = new TMP_Text[3], values = new TMP_Text[3];   // Design, Mechanics, Sound
        public RectTransform[] fills = new RectTransform[3];
        public Transform options;
        public Button optionPrefab;

        public bool IsOpen => root.activeSelf;

        void Awake() => root.SetActive(false);

        public void Show(string head, BuildRecord r, LevelSO l, params (string label, Action action)[] buttons)
        {
            title.text = head;
            subtitle.text = "Сборка " + r.number + " · заказчик «Южный капитал»";
            score.text = r.overall.ToString();
            score.color = r.passed ? Palette.Green : Palette.Red;
            need.text = (r.passed ? "проходной балл взят" : "нужно " + l.passScore) + (r.unmetRequirements.Count > 0 ? " · штраф за ТЗ" : "");
            int weakest = Review.Weakest(r);
            for (int i = 0; i < 3; i++)
            {
                names[i].text = Names.Of((Discipline)i);
                values[i].text = r.scores[i] + (r.concept[i] != 0 ? "  · концепт " + (r.concept[i] > 0 ? "+" : "–") + Math.Abs(r.concept[i]) : "");
                fills[i].anchorMax = new Vector2(Mathf.Clamp01(r.scores[i] / 100f), 1);
                fills[i].GetComponent<Image>().color = i == weakest && !r.passed ? Palette.Red : r.scores[i] >= l.passScore ? Palette.Green : Palette.Primary;
            }
            remarks.text = Review.Remarks(r, l);
            foreach (Transform c in options) Destroy(c.gameObject);
            int n = 0;
            foreach (var b in buttons)
            {
                var btn = Instantiate(optionPrefab, options);
                btn.GetComponentInChildren<TMP_Text>().text = b.label;
                btn.GetComponent<Image>().color = n++ == 0 ? Palette.Primary : Palette.Secondary;   // the first button is the main action
                var act = b.action;
                btn.onClick.AddListener(() => { root.SetActive(false); act?.Invoke(); });
            }
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }
    }
}
