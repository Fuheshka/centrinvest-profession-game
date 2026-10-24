using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    public class ImageCardView : MonoBehaviour
    {
        public Button button;
        public Image preview;
        public TMP_Text title, tags;

        public void Setup(Sprite sprite, string name, string[] tagList, bool interactable, Action onClick)
        {
            preview.sprite = sprite;
            title.text = name;
            tags.text = tagList == null ? "" : string.Join(" · ", tagList);
            button.interactable = interactable;
            button.onClick.AddListener(() => onClick());
        }
    }
}
