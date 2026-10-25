using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Studio.UI
{
    // Logic of the main menu. The layout lives in Prefabs/UI/Menu/MenuScreen (docs/mockup/generated-2026-10-24/01-menu.png); all references come from the inspector.
    // MVP: one level («0 · Обучение»), the nickname field is on the screen itself.
    public class MenuController : MonoBehaviour
    {
        public const string DefaultName = "Стажёр";
        public const int MaxName = 16;

        public TMP_InputField nameInput;
        public Button startButton, continueButton, confirmYes, confirmNo, settingsButton;
        public TMP_Text continueLabel;
        public GameObject confirm;

        void Awake()
        {
            nameInput.characterLimit = MaxName;
            startButton.onClick.AddListener(OnStart);
            continueButton.onClick.AddListener(OnContinue);
            if (settingsButton != null) settingsButton.onClick.AddListener(() => SettingsPanel.Open());
            confirmYes.onClick.AddListener(StartNew);
            confirmNo.onClick.AddListener(() => confirm.SetActive(false));

            var save = SaveSystem.LoadLevel();
            continueButton.gameObject.SetActive(save != null && save.stage != LevelStage.Passed && save.stage != LevelStage.Failed);
            if (save != null) continueLabel.text = "Продолжить · день " + save.day;
            confirm.SetActive(false);
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
            Session.Level = DifficultyLevel.Intern;
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
