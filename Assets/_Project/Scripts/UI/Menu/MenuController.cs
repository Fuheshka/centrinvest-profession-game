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
        public Button[] levelButtons = new Button[3];     // MVP: only [0] = level 0 «Обучение»; the rest are hidden
        public Button startButton, backButton, continueButton, confirmYes, confirmNo;
        public TMP_Text continueLabel, popupTitle;
        public GameObject namePopup, confirm;
        const string LevelName = "0 · Обучение";

        DifficultyLevel level = DifficultyLevel.Designer;

        void Awake()
        {
            nameInput.characterLimit = MaxName;
            levelButtons[0].onClick.AddListener(() => SetLevel(DifficultyLevel.Intern));
            var labels = levelButtons[0].GetComponentsInChildren<TMP_Text>(true);
            labels[0].text = LevelName;
            if (labels.Length > 1) labels[1].text = "Первый заказ: 3 рабочих дня";
            for (int i = 1; i < levelButtons.Length; i++) levelButtons[i].gameObject.SetActive(false);
            startButton.onClick.AddListener(OnStart);
            backButton.onClick.AddListener(() => namePopup.SetActive(false));
            continueButton.onClick.AddListener(OnContinue);
            confirmYes.onClick.AddListener(StartNew);
            confirmNo.onClick.AddListener(() => confirm.SetActive(false));

            var save = SaveSystem.LoadLevel();
            continueButton.gameObject.SetActive(save != null && save.stage != LevelStage.Passed && save.stage != LevelStage.Failed);
            if (save != null) continueLabel.text = "Продолжить · день " + save.day;
            confirm.SetActive(false);
            namePopup.SetActive(false);
        }

        // Picking a level opens the nickname window; the game starts from there.
        void SetLevel(DifficultyLevel l)
        {
            level = l;
            popupTitle.text = "Уровень " + LevelName;
            namePopup.SetActive(true);
        }

        void OnStart()
        {
            if (SaveSystem.HasLevelSave()) confirm.SetActive(true);
            else StartNew();
        }

        void StartNew()
        {
            SaveSystem.DeleteLevel();
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
