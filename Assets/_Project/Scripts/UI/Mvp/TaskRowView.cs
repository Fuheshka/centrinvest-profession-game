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
        public Button assign, up, wait;   // wait: the employee is stopped by a question in the chat; click opens that chat
        public TMP_Text assignLabel;

        float shown = -1, target;   // displayed progress follows the real one; never runs ahead of it
        bool wasDone, inited;

        void Update()
        {
            if (shown < 0 || Mathf.Approximately(shown, target)) return;
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 0.6f);
            fill.anchorMax = new Vector2(shown, 1);
            percent.text = Mathf.RoundToInt(shown * 100) + "%";
        }

        public void Waiting(bool on, UnityEngine.Events.UnityAction open)
        {
            if (wait == null) return;
            wait.gameObject.SetActive(on);
            wait.onClick.RemoveAllListeners();
            if (on) wait.onClick.AddListener(open);
        }

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
            target = Mathf.Clamp01(progress);
            if (shown < 0 || target < shown) shown = target;   // first draw, a reassigned row or a reset: no tween
            fill.anchorMax = new Vector2(shown, 1);
            percent.text = Mathf.RoundToInt(shown * 100) + "%";
            if (inited && done && !wasDone) Pulse.Pop(this, transform, 1.03f, 0.4f);   // once, on the real transition to done
            wasDone = done; inited = true;
            background.color = done ? Palette.Hex("EEF4DC") : Palette.Hex("FBF8EA");
        }
    }
}
