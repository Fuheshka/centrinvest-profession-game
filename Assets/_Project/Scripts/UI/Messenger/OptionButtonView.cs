using System;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class OptionButtonView : MonoBehaviour
    {
        public Button button;
        public TMP_Text label, cost, hint;

        public void Setup(GameState s, OptionSO o, Action onClick)
        {
            label.text = o.text;
            cost.text = Format.Cost(o.cost);
            bool ok = DecisionResolver.CanAfford(s, o);
            button.interactable = ok;
            label.color = ok ? label.color : new Color(0.55f, 0.57f, 0.62f);
            hint.gameObject.SetActive(!ok);
            if (!ok)
                hint.text = s.budget < o.cost.budget
                    ? $"Не хватает бюджета: нужно {Format.Money(o.cost.budget)}, есть {Format.Money(s.budget)}"
                    : "Не хватает дней до дедлайна";
            button.onClick.AddListener(() => onClick());
        }
    }
}
