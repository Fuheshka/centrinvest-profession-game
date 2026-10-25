using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Studio.UI.Mvp;

namespace Studio.UI
{
    public enum Track { Menu, Work, Review }
    public enum Fx { None, Click, Tab, Confirm, Error, Money, MsgNew, MsgOut, Stage, Frog }

    // Put on a button to give it its own sound instead of the default click. Fx.None keeps it silent.
    public class SfxButton : MonoBehaviour { public Fx fx = Fx.Click; }

    // Survives scene loads. Menu scene plays the menu track, Game scene the workday track; GameLoop switches to Review.
    // Clips live in Resources/Music (menu, work, review). Volumes are stored in PlayerPrefs.
    public class AudioManager : MonoBehaviour
    {
        const float FadeSeconds = 1.2f;

        static AudioManager inst;
        public static float Master { get => PlayerPrefs.GetFloat("vol.master", 1f); set { PlayerPrefs.SetFloat("vol.master", value); inst?.Apply(); } }
        public static float Music { get => PlayerPrefs.GetFloat("vol.music", 0.7f); set { PlayerPrefs.SetFloat("vol.music", value); inst?.Apply(); } }

        public static float Sfx { get => PlayerPrefs.GetFloat("vol.sfx", 0.8f); set => PlayerPrefs.SetFloat("vol.sfx", value); }

        // Quiet by design: high band cut offline, small gain, tiny pitch wander so repeats do not sound identical,
        // tonal sounds wander only between notes of a pentatonic set, and a cooldown stops machine-gunning.
        struct FxDef { public string clip; public float gain, cooldown; public int[] semitones; public float wander; }
        static readonly int[] Pent = { -3, 0, 2, 4, 7 };
        static readonly Dictionary<Fx, FxDef> Defs = new Dictionary<Fx, FxDef>
        {
            [Fx.Click]   = new FxDef { clip = "click",   gain = 0.7f, cooldown = 0.05f, wander = 0.06f },
            [Fx.Tab]     = new FxDef { clip = "tab",     gain = 0.4f, cooldown = 0.05f, wander = 0.05f },
            [Fx.Confirm] = new FxDef { clip = "confirm", gain = 0.8f, cooldown = 0.15f, semitones = Pent },
            [Fx.Error]   = new FxDef { clip = "error",   gain = 0.8f, cooldown = 0.3f,  wander = 0.03f },
            [Fx.Money]   = new FxDef { clip = "money",   gain = 0.8f, cooldown = 0.3f,  semitones = new[] { -2, 0, 2 } },
            [Fx.MsgNew]  = new FxDef { clip = "msg_new", gain = 0.8f, cooldown = 0.5f,  semitones = Pent },
            [Fx.MsgOut]  = new FxDef { clip = "msg_out", gain = 0.8f, cooldown = 0.2f,  semitones = new[] { -2, 0, 2 } },
            [Fx.Stage]   = new FxDef { clip = "stage",   gain = 0.9f, cooldown = 1.0f },
            [Fx.Frog]    = new FxDef { clip = "frog",    gain = 0.9f, cooldown = 1.0f,  wander = 0.05f },
        };

        public static void Play(Fx fx) { if (inst != null) inst.PlayFx(fx); }

        public static void Play(Track t) { if (inst != null) inst.Switch(t); }

        readonly AudioSource[] pool = new AudioSource[6];
        readonly Dictionary<Fx, float> last = new Dictionary<Fx, float>();
        readonly Dictionary<Fx, int> lastStep = new Dictionary<Fx, int>();
        int poolNext;
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
            for (int i = 0; i < pool.Length; i++) { pool[i] = NewSource(); pool[i].loop = false; }
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

        void PlayFx(Fx fx)
        {
            if (fx == Fx.None || !Defs.TryGetValue(fx, out var d)) return;
            if (last.TryGetValue(fx, out var t0) && Time.unscaledTime - t0 < d.cooldown) return;
            var clip = Resources.Load<AudioClip>("Sfx/" + d.clip);
            if (clip == null) return;
            last[fx] = Time.unscaledTime;
            float pitch = 1;
            if (d.semitones != null)
            {
                int i = Random.Range(0, d.semitones.Length);
                if (i == lastStep.GetValueOrDefault(fx, -1)) i = (i + 1) % d.semitones.Length;   // never the same note twice in a row
                lastStep[fx] = i;
                pitch = Mathf.Pow(2, d.semitones[i] / 12f);
            }
            else pitch = 1 + Random.Range(-d.wander, d.wander);
            var s = pool[poolNext]; poolNext = (poolNext + 1) % pool.Length;
            s.pitch = pitch;
            s.volume = d.gain * Master * Sfx * Random.Range(0.9f, 1f);
            s.clip = clip; s.Play();
        }

        // Every enabled button clicks; a SfxButton changes the sound, the top bar tabs get the tab sound.
        void ClickSound()
        {
            if (EventSystem.current == null) return;
            var data = new PointerEventData(EventSystem.current) { position = Pointer.current.position.ReadValue() };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, hits);
            if (hits.Count == 0) return;
            var btn = hits[0].gameObject.GetComponentInParent<Button>();
            if (btn == null || !btn.interactable) return;
            var tag = btn.GetComponent<SfxButton>();
            if (tag != null) { Play(tag.fx); return; }
            var top = btn.GetComponentInParent<TopBarMvp>();
            Play(top != null && System.Array.IndexOf(top.tabs, btn) >= 0 ? Fx.Tab : Fx.Click);
        }

        void Apply() { float v = Master * Music; if (fade >= 1) { a.volume = v; } }

        void Update()
        {
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) ClickSound();
            if (fade >= 1) return;
            fade = Mathf.Min(1, fade + Time.unscaledDeltaTime / FadeSeconds);
            float v = Master * Music;
            a.volume = v * fade;
            b.volume = v * (1 - fade);
            if (fade >= 1) b.Stop();
        }
    }
}
