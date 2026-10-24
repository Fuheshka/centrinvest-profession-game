using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // One task of the task list (mockup 03): number, title with a coloured type chip, assignee, status chip, progress bar.
    public class TaskRowView : MonoBehaviour
    {
        public Image background, avatar, chipBg, statusBg;
        public TMP_Text number, title, chip, who, role, status, percent;
        public RectTransform fill;
        public Button assign, up;
        public TMP_Text assignLabel;

        public static TaskRowView At(Transform list, TaskRowView prefab, int i)
        {
            while (list.childCount <= i) Instantiate(prefab, list);
            return list.GetChild(i).GetComponent<TaskRowView>();
        }

        public void Set(int n, string titleText, string chipText, Color chipColor, Sprite face, string whoText, string roleText,
                        string statusText, Color statusColor, float progress, bool done)
        {
            number.text = n.ToString("00"); title.text = titleText;
            chip.text = chipText; chipBg.color = chipColor;
            avatar.transform.parent.gameObject.SetActive(face != null);
            if (face != null) avatar.sprite = face;
            who.text = whoText; role.text = roleText;
            status.text = statusText; statusBg.color = statusColor;
            percent.text = Mathf.RoundToInt(progress * 100) + "%";
            fill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1);
            background.color = done ? Palette.Hex("EEF4DC") : Palette.Hex("FBF8EA");
        }
    }
}
