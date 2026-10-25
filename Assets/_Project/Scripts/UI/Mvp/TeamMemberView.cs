using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // One card of the "Команда" column (mockup 03): portrait, name, role, energy bar with number, what they are doing.
    public class TeamMemberView : MonoBehaviour
    {
        public Image avatar, energyFill;
        public RectTransform fillRect;
        public TMP_Text title, role, energy, state;
        public GameObject content;   // hidden for an empty seat
        public TMP_Text empty;

        float shown = -1, target;   // 0..1, the bar glides to the real energy like the task bars

        void Update()
        {
            if (shown < 0 || Mathf.Approximately(shown, target)) return;
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 0.6f);
            Draw();
        }

        void Draw()
        {
            float v = shown * 100;
            energy.text = Mathf.RoundToInt(v) + "/100";
            fillRect.anchorMax = new Vector2(shown, 1);
            energyFill.color = v < 25 ? Palette.Hex("C93A3F") : v < 55 ? Palette.Hex("E7B93A") : Palette.Hex("718733");
        }

        public static TeamMemberView At(Transform grid, TeamMemberView prefab, int i)
        {
            while (grid.childCount <= i) Instantiate(prefab, grid);
            return grid.GetChild(i).GetComponent<TeamMemberView>();
        }

        public void SetEmpty()
        {
            content.SetActive(false); empty.gameObject.SetActive(true);
        }

        public void Set(Sprite face, string name, string roleText, float energyValue, string stateText)
        {
            content.SetActive(true); empty.gameObject.SetActive(false);
            if (face != null) avatar.sprite = face;
            title.text = name; role.text = roleText; state.text = stateText;
            target = Mathf.Clamp01(energyValue / 100f);
            if (shown < 0) shown = target;   // first draw: no tween
            Draw();
        }
    }
}
