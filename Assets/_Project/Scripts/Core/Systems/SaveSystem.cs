using UnityEngine;

namespace Studio.Core
{
    // One slot in PlayerPrefs (IndexedDB in WebGL, so it survives a page reload).
    public static class SaveSystem
    {
        public const string Key = "save_v1";

        public static void Save(GameState state)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        // null: no save or unreadable save (logged, never thrown).
        public static GameState Load()
        {
            if (!HasSave()) return null;
            try
            {
                var s = JsonUtility.FromJson<GameState>(PlayerPrefs.GetString(Key));
                if (s != null && s.saveVersion == GameState.CurrentSaveVersion) return s;
                Debug.LogWarning("save: unknown or empty save, ignored");
            }
            catch (System.ArgumentException e)
            {
                Debug.LogWarning("save: broken JSON, ignored (" + e.Message + ")");
            }
            return null;
        }

        public static bool HasSave() => PlayerPrefs.HasKey(Key);

        public static void Delete()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
