using UnityEngine;
using UnityEngine.SceneManagement;

namespace Studio.UI
{
    public enum Track { Menu, Work, Review }

    // Survives scene loads. Menu scene plays the menu track, Game scene the workday track; GameLoop switches to Review.
    // Clips live in Resources/Music (menu, work, review). Volumes are stored in PlayerPrefs.
    public class AudioManager : MonoBehaviour
    {
        const float FadeSeconds = 1.2f;

        static AudioManager inst;
        public static float Master { get => PlayerPrefs.GetFloat("vol.master", 1f); set { PlayerPrefs.SetFloat("vol.master", value); inst?.Apply(); } }
        public static float Music { get => PlayerPrefs.GetFloat("vol.music", 0.7f); set { PlayerPrefs.SetFloat("vol.music", value); inst?.Apply(); } }

        public static void Play(Track t) { if (inst != null) inst.Switch(t); }

        AudioSource a, b;       // a is the audible one, b fades in
        Track? current;
        float fade = 1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            inst = go.AddComponent<AudioManager>();
        }

        void Awake()
        {
            a = NewSource(); b = NewSource();
            SceneManager.sceneLoaded += (s, _) => Play(s.name == "Menu" ? Track.Menu : Track.Work);
            Play(SceneManager.GetActiveScene().name == "Menu" ? Track.Menu : Track.Work);
        }

        AudioSource NewSource()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.loop = true; s.playOnAwake = false; s.volume = 0;
            return s;
        }

        void Switch(Track t)
        {
            if (current == t) return;
            var clip = Resources.Load<AudioClip>("Music/" + t.ToString().ToLower());
            if (clip == null) { Debug.LogWarning("No music clip for " + t); return; }
            current = t;
            (a, b) = (b, a);           // b becomes the old one, a the new one
            a.clip = clip; a.Play(); fade = 0;
        }

        void Apply() { float v = Master * Music; if (fade >= 1) { a.volume = v; } }

        void Update()
        {
            if (fade >= 1) return;
            fade = Mathf.Min(1, fade + Time.unscaledDeltaTime / FadeSeconds);
            float v = Master * Music;
            a.volume = v * fade;
            b.volume = v * (1 - fade);
            if (fade >= 1) b.Stop();
        }
    }
}
