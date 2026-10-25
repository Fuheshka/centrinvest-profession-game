using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// P-30: puts ButtonMotion (docs/UI_MOTION.md) on every Button of the menu, the MVP screen and its row prefabs.
// Buttons placed by a LayoutGroup get no lift, only scale, so the layout keeps their position.
public static class MotionKit
{
    static readonly string[] Prefabs =
    {
        "Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab",
        "Assets/_Project/Prefabs/UI/Mvp/MvpScreen.prefab",
        "Assets/_Project/Prefabs/UI/Mvp/Option.prefab",
        "Assets/_Project/Prefabs/UI/Mvp/PopupItem.prefab",
        "Assets/_Project/Prefabs/UI/Mvp/CandidateCard.prefab",
        "Assets/_Project/Prefabs/UI/Common/SettingsPanel.prefab",
    };

    [MenuItem("Tools/UI/P-30 Add button motion")]
    public static void Apply()
    {
        int added = 0;
        foreach (var path in Prefabs)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            foreach (var b in root.GetComponentsInChildren<Button>(true))
            {
                if (b.GetComponent<ButtonMotion>() != null || PrefabUtility.IsPartOfPrefabInstance(b)) continue;
                var m = b.gameObject.AddComponent<ButtonMotion>();
                if (b.transform.parent != null && b.transform.parent.GetComponent<LayoutGroup>() != null) m.hoverLift = 0;
                added++;
            }
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }
        // fade-in for the four windows of the MVP screen
        var screen = PrefabUtility.LoadPrefabContents(Prefabs[1]);
        // an earlier run saved PanelFade from a file with another name: drop those missing-script leftovers first
        foreach (var t in screen.GetComponentsInChildren<Transform>(true)) GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        var bar = screen.GetComponentInChildren<TopBarMvp>(true);
        int faded = 0;
        foreach (var w in bar.windows)
            if (w.GetComponent<PanelFade>() == null) { if (w.GetComponent<CanvasGroup>() == null) w.AddComponent<CanvasGroup>(); w.AddComponent<PanelFade>(); faded++; }
        // soft accent on a new shift: the whole game screen fades in a little slower than a tab
        if (screen.GetComponent<PanelFade>() == null)
        {
            if (screen.GetComponent<CanvasGroup>() == null) screen.AddComponent<CanvasGroup>();
            screen.AddComponent<PanelFade>().duration = 0.45f;
        }
        PrefabUtility.SaveAsPrefabAsset(screen, Prefabs[1]);
        PrefabUtility.UnloadPrefabContents(screen);
        Debug.Log("MotionKit: ButtonMotion added to " + added + " buttons, PanelFade to " + faded + " windows");
    }
}
