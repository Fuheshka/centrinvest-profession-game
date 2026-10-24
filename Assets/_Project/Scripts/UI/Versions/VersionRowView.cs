using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class VersionRowView : MonoBehaviour
    {
        public Button button;
        public Outline border;
        public TMP_Text title, status, info, changed;

        public void Setup(string t, string st, Color stColor, string inf, string chg, bool selected, Action click)
        {
            title.text = t; status.text = st; status.color = stColor; info.text = inf;
            changed.text = chg; changed.gameObject.SetActive(chg != "");
            border.enabled = selected;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => click());
        }
    }
}
