using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class HighlightRowView : MonoBehaviour
    {
        public TMP_Text title, note;
        public void Setup(Highlight h)
        {
            title.text = (h.good ? "+ " : "– ") + h.title; title.color = h.good ? Pal.Green : Pal.Red;
            note.text = h.note;
        }
    }
}
