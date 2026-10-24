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
        public Button startButton, backButton, continueButton, confirmYes, confirmNo;
        public TMP_Text continueLabel, popupTitle;
        public GameObject namePopup, confirm;
        public string[] levelNames = { "Стажёр", "Дизайнер", "Ведущий" };

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
            backButton.onClick.AddListener(() => namePopup.SetActive(false));
            continueButton.onClick.AddListener(OnContinue);
            confirmYes.onClick.AddListener(StartNew);
            confirmNo.onClick.AddListener(() => confirm.SetActive(false));

            var save = SaveSystem.Load();
            continueButton.gameObject.SetActive(save != null);
            if (save != null) continueLabel.text = "Продолжить · день " + save.day;
            confirm.SetActive(false);
            namePopup.SetActive(false);
        }

        // Picking a level opens the nickname window; the game starts from there.
        void SetLevel(DifficultyLevel l)
        {
            level = l;
            popupTitle.text = "Уровень: " + levelNames[(int)l];
            namePopup.SetActive(true);
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
