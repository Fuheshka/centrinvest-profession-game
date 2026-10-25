using System;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI
{
    // Volume sliders. The layout is Prefabs/UI/Common/SettingsPanel; place one instance in each scene.
    public class SettingsPanel : MonoBehaviour
    {
        public GameObject root;
        public Slider master, music, sfx;
        public Button close;

        static SettingsPanel inst;
        Action onClose;

        void Awake()
        {
            inst = this;
            master.SetValueWithoutNotify(AudioManager.Master);
            music.SetValueWithoutNotify(AudioManager.Music);
            sfx.SetValueWithoutNotify(AudioManager.Sfx);
            master.onValueChanged.AddListener(v => AudioManager.Master = v);
            music.onValueChanged.AddListener(v => AudioManager.Music = v);
            sfx.onValueChanged.AddListener(v => AudioManager.Sfx = v);
            close.onClick.AddListener(Close);
            root.SetActive(false);
        }

        public static bool IsOpen => inst != null && inst.root.activeSelf;

        public static void Open(Action onClose = null)
        {
            if (inst == null) return;
            inst.onClose = onClose;
            inst.master.SetValueWithoutNotify(AudioManager.Master);
            inst.music.SetValueWithoutNotify(AudioManager.Music);
            inst.sfx.SetValueWithoutNotify(AudioManager.Sfx);
            inst.root.SetActive(true);
            inst.root.transform.SetAsLastSibling();
        }

        public static void Close()
        {
            if (!IsOpen) return;
            inst.root.SetActive(false);
            inst.onClose?.Invoke();
        }
    }
}
