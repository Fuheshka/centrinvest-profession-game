using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class ReviewCardView : MonoBehaviour
    {
        public TMP_Text quote, author;
        public void Setup(QuoteSO q) { quote.text = "«" + q.text + "»"; author.text = q.author; }
    }
}
