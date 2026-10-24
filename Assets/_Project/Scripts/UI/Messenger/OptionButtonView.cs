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

        public void Setup(GameState s, ContentDB db, OptionSO o, Action onClick)
        {
            label.text = Format.Name(o.text, s);
            cost.text = Format.Cost(o.cost);
            bool ok = DecisionResolver.CanAfford(s, o);
            button.interactable = ok;
            label.color = ok ? label.color : new Color32(0x9C, 0x8B, 0x30, 255);
            // The board from stage 1 is the yardstick of every answer: say which way this one leans.
            var fit = DecisionResolver.BoardFit(s, o, db);
            hint.gameObject.SetActive(!ok || fit != 0);
            if (!ok)
                hint.text = s.budget < o.cost.budget
                    ? $"Не хватает бюджета: нужно {Format.Money(o.cost.budget)}, есть {Format.Money(s.budget)}"
                    : "Не хватает дней до дедлайна";
            else if (fit != 0)
            {
                hint.text = fit > 0 ? "По доске: сочетается с твоей игрой" : "По доске: спорит с твоей игрой";
                if (fit > 0) hint.color = new Color32(0x3E, 0x7A, 0x2E, 255);   // prefab colour is the warning red
            }
            button.onClick.AddListener(() => onClick());
        }
    }
}
