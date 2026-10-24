using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class AxisCardView : MonoBehaviour
    {
        public TMP_Text axisName, value, goal, state;

        public void Setup(string name, int v, int g)
        {
            bool ok = v >= g;
            Color c = ok ? Pal.Green : g - v <= 10 ? Pal.Yellow : Pal.Red;
            axisName.text = name; value.text = v.ToString(); value.color = c;
            goal.text = "/ цель " + g;
            state.text = ok ? "цель достигнута" : "ниже цели"; state.color = c;
        }
    }
}
