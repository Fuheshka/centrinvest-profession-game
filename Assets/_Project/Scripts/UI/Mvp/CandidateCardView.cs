using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // One candidate card of the headhunter window (mockup 05): portrait, name, role, quirk, speed and quality dots, pay, hire button.
    // Cards are built once and refilled in place, like RowView rows.
    public class CandidateCardView : MonoBehaviour
    {
        public Image portrait;
        public TMP_Text title, role, quirk, rate, hireLabel;
        public Image[] speedDots = new Image[3], qualityDots = new Image[3];
        public Button hire, fire;
        public Color speedOn = Palette.Hex("E7CC38"), qualityOn = Palette.Hex("718733"), off = Palette.Hex("D9D6C3");

        void Awake()
        {
            rate.enableAutoSizing = true;
            rate.fontSizeMin = 14;
            rate.fontSizeMax = 20;
        }

        public static CandidateCardView At(Transform grid, CandidateCardView prefab, int i)
        {
            while (grid.childCount <= i) Instantiate(prefab, grid);
            return grid.GetChild(i).GetComponent<CandidateCardView>();
        }

        public void Set(Sprite face, string name, string roleText, string quirkText, int speed, int quality, string pay)
        {
            if (face != null) { portrait.sprite = face; portrait.color = Color.white; }
            title.text = name; role.text = roleText; quirk.text = quirkText; rate.text = pay;
            for (int i = 0; i < 3; i++) { speedDots[i].color = i < speed ? speedOn : off; qualityDots[i].color = i < quality ? qualityOn : off; }
        }

        // hired: the main button turns into a quiet "in the team" mark and a small fire button appears
        public void SetHire(bool hired, string label, bool canHire, UnityEngine.Events.UnityAction onHire, UnityEngine.Events.UnityAction onFire)
        {
            hireLabel.text = hired ? "В команде" : label;
            hire.interactable = !hired && canHire;
            var img = hire.targetGraphic as Image;
            img.color = hired ? Palette.Hex("718733") : Palette.Primary;
            hireLabel.color = hired ? Palette.Hex("EFF2DF") : Palette.Ink;
            hire.onClick.RemoveAllListeners();
            if (onHire != null) hire.onClick.AddListener(onHire);
            fire.gameObject.SetActive(hired);
            fire.onClick.RemoveAllListeners();
            if (onFire != null) fire.onClick.AddListener(onFire);
        }
    }
}
