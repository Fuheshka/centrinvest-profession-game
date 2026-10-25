using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Studio.UI.Mvp
{
    // Scene change as a pixel mosaic: patterned cells grow in a diagonal wave and cover the screen, the next scene loads under
    // the cover, then the cells shrink away. The prefab (Resources/UI/PixelTransition, built by TransitionKit) survives the load.
    public class PixelTransition : MonoBehaviour
    {
        public RectTransform[] cells;   // row-major, cols x rows
        public int cols = 16, rows = 9;
        public float cover = 0.5f, reveal = 0.5f, grow = 0.3f;   // seconds; grow = share of a phase one cell needs to open

        static PixelTransition running;

        public static void Load(string scene)
        {
            if (running != null) return;
            var prefab = Resources.Load<PixelTransition>("UI/PixelTransition");
            if (prefab == null) { SceneManager.LoadScene(scene); return; }   // no overlay prefab: plain load, never block the player
            running = Instantiate(prefab);
            DontDestroyOnLoad(running.gameObject);
            running.StartCoroutine(running.Run(scene));
        }

        IEnumerator Run(string scene)
        {
            Set(0);
            yield return Wave(cover, 0, 1);
            var load = SceneManager.LoadSceneAsync(scene);
            while (!load.isDone) yield return null;
            yield return null; yield return null;   // let the new scene's Awake/Start settle under the cover
            yield return Wave(reveal, 1, 0);
            running = null;
            Destroy(gameObject);
        }

        void Set(float v) { foreach (var c in cells) c.localScale = Vector3.one * v; }

        // Diagonal sweep from the top-left corner: each cell starts a little after its neighbour and grows (or shrinks) on its own.
        IEnumerator Wave(float seconds, float from, float to)
        {
            float spread = 1 - grow;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                for (int i = 0; i < cells.Length; i++)
                {
                    float order = (i % cols + i / cols) / (float)(cols + rows - 2);   // 0 at the corner, 1 at the opposite one
                    float local = Mathf.Clamp01((k - order * spread) / grow);
                    cells[i].localScale = Vector3.one * Mathf.Lerp(from, to, local * local * (3 - 2 * local));
                }
                yield return null;
            }
            Set(to);
        }
    }
}
