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
            energy.text = Mathf.RoundToInt(energyValue) + "/100";
            fillRect.anchorMax = new Vector2(Mathf.Clamp01(energyValue / 100f), 1);
            energyFill.color = energyValue < 25 ? Palette.Hex("C93A3F") : energyValue < 55 ? Palette.Hex("E7B93A") : Palette.Hex("718733");
        }
    }
}
