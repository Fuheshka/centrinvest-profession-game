using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Studio.UI
{
    // Logic of the main menu. The layout lives in Prefabs/UI/Menu/MenuScreen (docs/mockup/0-main.png); all references come from the inspector.
    public class MenuController : MonoBehaviour
    {
        public const string DefaultName = "Стажёр";
        public const int MaxName = 16;

        public TMP_InputField nameInput;
        public Button[] levelButtons = new Button[3];     // Intern, Designer, Lead
        public Image[] levelBoxes = new Image[3];
        public Outline[] levelFrames = new Outline[3];
        public Button startButton, continueButton, confirmYes, confirmNo;
        public TMP_Text continueLabel;
        public GameObject confirm;
        public Color levelIdle = new Color32(0x1D, 0x20, 0x28, 255);
        public Color levelSelected = new Color32(0x2B, 0x24, 0x20, 255);
        public Color frameIdle = new Color32(0x2C, 0x30, 0x3B, 255);
        public Color frameSelected = new Color32(0xFF, 0x8A, 0x4C, 255);

        DifficultyLevel level = DifficultyLevel.Designer;

        void Awake()
        {
            nameInput.characterLimit = MaxName;
            for (int i = 0; i < levelButtons.Length; i++)
            {
                var l = (DifficultyLevel)i;
                levelButtons[i].onClick.AddListener(() => SetLevel(l));
            }
            startButton.onClick.AddListener(OnStart);
            continueButton.onClick.AddListener(OnContinue);
            confirmYes.onClick.AddListener(StartNew);
            confirmNo.onClick.AddListener(() => confirm.SetActive(false));

            var save = SaveSystem.Load();
            continueButton.gameObject.SetActive(save != null);
            if (save != null) continueLabel.text = "Продолжить · день " + save.day;
            confirm.SetActive(false);
            SetLevel(level);
        }

        void SetLevel(DifficultyLevel l)
        {
            level = l;
            for (int i = 0; i < levelBoxes.Length; i++)
            {
                bool on = i == (int)l;
                levelBoxes[i].color = on ? levelSelected : levelIdle;
                levelFrames[i].effectColor = on ? frameSelected : frameIdle;
            }
        }

        void OnStart()
        {
            if (SaveSystem.HasSave()) confirm.SetActive(true);
            else StartNew();
        }

        void StartNew()
        {
            SaveSystem.Delete();
            var n = nameInput.text.Trim();
            Session.Nickname = n.Length == 0 ? DefaultName : n;
            Session.Level = level;
            Session.Continue = false;
            SceneManager.LoadScene("Game");
        }

        void OnContinue()
        {
            Session.Continue = true;
            SceneManager.LoadScene("Game");
        }
    }
}
