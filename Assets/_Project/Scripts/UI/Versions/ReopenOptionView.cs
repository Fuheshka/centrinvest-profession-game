using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // One alternative in the reopen window.
    public class ReopenOptionView : MonoBehaviour
    {
        public Button button;
        public TMP_Text text, cost;

        public void Setup(string t, string c, bool ok, Action click)
        {
            text.text = t; text.color = ok ? Pal.Text : Pal.Dim;
            cost.text = c; cost.color = ok ? Pal.Dim : Pal.Red;
            button.interactable = ok;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => click());
        }
    }
}
