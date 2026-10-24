using Studio.Core;
using Studio.UI.Mvp;
using UnityEditor;
using UnityEngine;

// Agent tooling: in Play mode jumps to a working state (concept approved, three people hired, tasks assigned, some work done)
// so the task list, the chat and the build button can be looked at. Not used by the game.
public static class DebugState
{
    [MenuItem("Tools/UI/Debug: production state")]
    public static void Production()
    {
        var g = Object.FindFirstObjectByType<GameLoop>();
        if (g == null || !Application.isPlaying) { Debug.LogWarning("Debug state: needs Play mode in Game."); return; }
        var s = g.S; var l = g.L;
        string[] ps = { "platformer", "adult", "space", "pixel", "gravity", "pc" };   // wrong audience and platform on purpose: a negative review
        var types = (BlockType[])System.Enum.GetValues(typeof(BlockType));
        string prev = null;
        for (int i = 0; i < types.Length; i++)
        {
            var id = ConceptRules.Place(s, l, types[i], 160 + (i % 3) * 360, -130 - (i / 3) * 280);
            ConceptRules.SetParam(s, l, id, ps[i]);
            if (prev != null) ConceptRules.Connect(s, prev, id);
            prev = id;
        }
        ConceptRules.Approve(s, l);
        foreach (var id in new[] { "lera", "kostya", "anya" }) Staffing.Hire(s, l, id);
        foreach (var t in s.tasks)
        {
            var who = s.staff.Find(e => e.d == t.d);
            if (who != null) WorkSim.Assign(s, t.id, who.id);
        }
        WorkSim.Tick(s, l, 90);
        g.Commit();
    }

    [MenuItem("Tools/UI/Debug: negative review")]
    public static void NegativeReview()
    {
        Production();
        var g = Object.FindFirstObjectByType<GameLoop>(); var s = g.S;
        foreach (var t in s.tasks) { t.done = true; t.progress = t.effort; }
        foreach (var f in s.files) f.quality = 70;
        var r = Review.Build(s, g.L);
        g.Commit();
        if (r != null) g.ShowReview(r);
    }
}
