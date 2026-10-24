using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class QuoteRowView : MonoBehaviour
    {
        public Button button;
        public Image background;
        public TMP_Text quote, author, status;

        public void Setup(QuoteSO q, bool blamed, bool picked, Action click)
        {
            status.text = q.praise ? "похвала" : blamed ? "найдена причина" : picked ? "выбран" : "не разобран";
            status.color = q.praise || blamed ? Pal.Green : picked ? Pal.Accent : Pal.Dim;
            background.color = blamed ? Pal.GreenTint : picked ? Pal.AccentTint : Pal.Panel;
            quote.text = "«" + q.text + "»"; author.text = q.author;
            button.interactable = !q.praise && !blamed;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => click());
        }
    }
}
