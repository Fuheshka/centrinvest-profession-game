using System;
using Studio.Core;
using TMPro;
using UnityEngine;

namespace Studio.UI
{
    public class BubbleView : MonoBehaviour
    {
        public TMP_Text text;
        public Transform imageGrid;
        public ImageCardView cardPrefab;

        // Images are selectable cards when each one maps to an option (hero variants).
        public void Setup(GameState s, MessageSO m, bool active, Action<MessageSO, OptionSO> pick)
        {
            text.text = m.text;
            imageGrid.gameObject.SetActive(m.images.Length > 0);
            for (int i = 0; i < m.images.Length; i++)
            {
                var opt = i < m.options.Length ? m.options[i] : null;
                var card = Instantiate(cardPrefab, imageGrid);
                bool ok = active && opt != null && DecisionResolver.CanAfford(s, opt);
                card.Setup(m.images[i], opt != null ? opt.text : "", opt != null ? opt.tags : null, ok,
                    () => pick(m, opt));
            }
        }
    }
}
