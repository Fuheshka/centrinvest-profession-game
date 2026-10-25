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

    [MenuItem("Tools/UI/Debug: event chat")]
    public static void EventChat()
    {
        Production();
        var g = Object.FindFirstObjectByType<GameLoop>();
        var s = g.S;
        foreach (var e in s.staff) e.currentTaskId = s.tasks.Find(t => t.assigneeId == e.id)?.id;
        foreach (var (id, who) in new[] { ("art_pics", "lera"), ("croak", "kostya") })
        {
            s.pendingEvents.Add(new PendingEvent { eventId = id, employeeId = who });
            s.Say(who, "event", g.L.events.Find(x => x.id == id).text, id);
        }
        g.Commit();
        var chat = Object.FindFirstObjectByType<ChatWindow>();   // open Lera's chat
        var t = typeof(ChatWindow);
        t.GetField("open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(chat, "lera");
        t.GetMethod("Redraw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.Invoke(chat, null);
    }

    [MenuItem("Tools/UI/Debug: clear level save")]
    public static void ClearSave() { PlayerPrefs.DeleteKey(SaveSystem.LevelKey); PlayerPrefs.Save(); }

    [MenuItem("Tools/UI/Debug: negative review")]
    public static void NegativeReview()
    {
        Production();
        var g = Object.FindFirstObjectByType<GameLoop>(); var s = g.S;
        foreach (var t in s.tasks) { t.done = true; t.progress = t.effort; }
        foreach (var t in s.tasks)
        {
            var f = s.files.Find(x => x.taskId == t.id);
            if (f == null)
            {
                var author = s.staff.Find(e => e.id == t.assigneeId) ?? s.staff.Find(e => e.d == t.d);
                f = new FileState
                {
                    id = t.id + "_preview_file", taskId = t.id, d = t.d, title = t.title,
                    revision = 1, authorId = author != null ? author.id : null, day = s.day
                };
                s.files.Add(f);
            }
            f.quality = t.d == Discipline.Design ? 55 : t.d == Discipline.Mechanics ? 76 : 84;
        }
        var r = Review.Build(s, g.L);
        g.Commit();
        if (r != null) g.ShowReview(r);
    }

    [MenuItem("Tools/UI/Debug: passed release")]
    public static void PassedRelease()
    {
        var g = Object.FindFirstObjectByType<GameLoop>();
        if (g == null || !Application.isPlaying) { Debug.LogWarning("Passed release preview: needs Play mode in Game."); return; }
        var s = g.S; var l = g.L;
        if (s.stage != LevelStage.Concept) { Debug.LogWarning("Passed release preview: start a fresh level first."); return; }

        string[] ps = { "platformer", "teen", "space", "pixel", "gravity", "phone" };
        var types = (BlockType[])System.Enum.GetValues(typeof(BlockType));
        string prev = null;
        for (int i = 0; i < types.Length; i++)
        {
            var id = ConceptRules.Place(s, l, types[i], 160 + (i % 3) * 360, -130 - (i / 3) * 280);
            ConceptRules.SetParam(s, l, id, ps[i]);
            if (prev != null) ConceptRules.Connect(s, prev, id);
            prev = id;
        }
        if (!ConceptRules.Approve(s, l)) { Debug.LogWarning("Passed release preview: could not approve concept."); return; }

        foreach (var id in new[] { "lera", "kostya", "anya" }) Staffing.Hire(s, l, id);
        foreach (var t in s.tasks)
        {
            var who = s.staff.Find(e => e.d == t.d);
            if (who != null) WorkSim.Assign(s, t.id, who.id);
            t.done = true; t.progress = t.effort;
            var f = s.files.Find(x => x.taskId == t.id);
            if (f == null)
            {
                var author = s.staff.Find(e => e.id == t.assigneeId) ?? s.staff.Find(e => e.d == t.d);
                f = new FileState { id = t.id + "_release_preview", taskId = t.id, d = t.d, title = t.title, revision = 1, authorId = author?.id, day = s.day };
                s.files.Add(f);
            }
            f.quality = 96;
        }
        var r = Review.Build(s, l);
        g.Commit();
        if (r == null) Debug.LogWarning("Passed release preview: build did not complete.");
    }
}
