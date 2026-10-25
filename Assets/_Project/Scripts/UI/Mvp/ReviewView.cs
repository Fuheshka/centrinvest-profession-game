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
        public TMP_Text title, subtitle, score, need, quote, remarks;
        public Image coverImage, testerImage;
        public Sprite[] gameCovers = new Sprite[2], testerPortraits = new Sprite[3];
        public Sprite mentorPortrait;
        public TMP_Text[] names = new TMP_Text[3], values = new TMP_Text[3];   // Design, Mechanics, Sound
        public RectTransform[] fills = new RectTransform[3];
        public Transform options;
        public Button optionPrefab;

        public bool IsOpen => root.activeSelf;

        void Awake() => root.SetActive(false);

        public void Show(string head, BuildRecord r, LevelSO l, int remainingBudget, string linkedFile, string finalSummary, params (string label, Action action)[] buttons)
        {
            title.text = head;
            subtitle.text = "Сборка " + r.number + " · заказчик «Южный капитал» · день "
                + r.day + "/" + l.deadlineDays + " · осталось " + Money(remainingBudget) + " ₽";
            score.text = r.overall.ToString();
            score.color = r.passed ? Palette.Green : Palette.Red;
            int weakest = Review.Weakest(r);
            if (coverImage != null && gameCovers != null && gameCovers.Length > 0)
            {
                int coverIndex = r.passed && gameCovers.Length > 1 ? gameCovers.Length - 1 : Mathf.Clamp(r.number - 1, 0, gameCovers.Length - 1);
                coverImage.sprite = gameCovers[coverIndex];
            }
            if (testerImage != null)
            {
                if (r.passed && mentorPortrait != null) testerImage.sprite = mentorPortrait;
                else if (testerPortraits != null && weakest >= 0 && weakest < testerPortraits.Length) testerImage.sprite = testerPortraits[weakest];
            }
            if (quote != null) quote.text = r.passed
                ? "Наставник:\n«Геймдизайнер связывает идеи, людей и ограничения.»"
                : "Что заметил тестировщик:\n«" + WeakestComment(r, l, weakest) + "»";
            need.text = r.passed
                ? "Проходной балл " + l.passScore + " взят · требования заказчика выполнены"
                : "Нужно " + l.passScore + " · слабое место: " + Names.Of((Discipline)weakest) + " " + r.scores[weakest] + "/100"
                    + (r.unmetRequirements.Count > 0 ? " · ТЗ: " + r.unmetRequirements.Count + " не выполнено" : "");
            for (int i = 0; i < 3; i++)
            {
                names[i].text = Names.Of((Discipline)i);
                values[i].text = r.scores[i] + (r.concept[i] != 0 ? "  · концепт " + (r.concept[i] > 0 ? "+" : "–") + Math.Abs(r.concept[i]) : "");
                fills[i].anchorMax = new Vector2(Mathf.Clamp01(r.scores[i] / 100f), 1);
                fills[i].GetComponent<Image>().color = i == weakest && !r.passed ? Palette.Red : r.scores[i] >= l.passScore ? Palette.Green : Palette.Primary;
            }
            remarks.text = r.passed && !string.IsNullOrWhiteSpace(finalSummary)
                ? finalSummary
                : RequirementsChecklist(r, l) + FileHint(r, linkedFile);
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

        static string RequirementsChecklist(BuildRecord record, LevelSO level)
        {
            var lines = new System.Collections.Generic.List<string> { "Чек-лист требований заказчика:" };
            if (level.requirements != null)
            {
                foreach (var requirement in level.requirements)
                {
                    string name = Names.Of(requirement.type);
                    string unmetPrefix = name + ":";
                    bool unmet = record.unmetRequirements != null && record.unmetRequirements.Exists(x => x.StartsWith(unmetPrefix));
                    string value = level.Param(requirement.paramId)?.title ?? requirement.paramId;
                    lines.Add((unmet ? "Не выполнено" : "Выполнено") + " – " + name + ": «" + value + "»");
                }
            }
            lines.Add((record.overall >= level.passScore ? "Выполнено" : "Не выполнено")
                + " – Оценка: минимум " + level.passScore + " (получено " + record.overall + ")");
            return string.Join("\n", lines);
        }

        static string FileHint(BuildRecord record, string linkedFile)
        {
            if (record.files == null || record.files.Count == 0) return "";

            BuiltFile weakest = record.files[0];
            foreach (var file in record.files)
                if (file.quality < weakest.quality) weakest = file;

            return "\n\nСвязанный файл: " + (string.IsNullOrWhiteSpace(linkedFile) ? weakest.fileId : linkedFile)
                + " · качество " + Mathf.RoundToInt(weakest.quality) + "/100.";
        }

        static string WeakestComment(BuildRecord record, LevelSO level, int weakest)
        {
            string[] lines = Review.Remarks(record, level).Split('\n');
            if (weakest < 0 || weakest >= lines.Length) return "Проверьте слабый критерий и требования заказа.";
            int separator = lines[weakest].IndexOf(':');
            return separator >= 0 ? lines[weakest].Substring(separator + 1).Trim() : lines[weakest];
        }

        static string Money(int value) => value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")).Replace('\u00a0', ' ');
    }
}
