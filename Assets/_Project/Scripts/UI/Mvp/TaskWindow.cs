using System.Collections.Generic;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    public class TaskWindow : MonoBehaviour
    {
        public GameLoop game;
        public Transform list;
        public RowView rowPrefab;
        public Button build;
        public TMP_Text buildReason, summary;
        public RowView[] team = new RowView[4];
        float redrawTimer;

        void Awake()
        {
            game.Changed += Tick;
            build.onClick.AddListener(Build);
        }

        void OnEnable() => Redraw();

        void Tick()
        {
            if (!isActiveAndEnabled) return;
            redrawTimer -= Time.unscaledDeltaTime;
            if (redrawTimer > 0) return;
            redrawTimer = 0.25f;
            Redraw();
        }

        void Redraw()
        {
            if (game.S == null) return;
            var s = game.S; var l = game.L;
            for (int i = 0; i < s.tasks.Count; i++)
            {
                var t = s.tasks[i];
                var row = RowView.At(list, rowPrefab, i);
                var st = WorkSim.Status(s, l, t.id, out var why);
                var who = t.assigneeId == null ? null : l.Candidate(t.assigneeId);
                row.Set(Names.Of(t.d) + " · " + t.title,
                        "Трудность " + t.tier + " (нужно энергии " + l.tuning.TierEnergy(t.tier) + ") · " + StatusText(s, l, t, st, why),
                        Mathf.RoundToInt(100 * t.progress / t.effort) + " %");
                row.Progress(t.done ? 1 : t.progress / t.effort);
                row.background.color = t.done ? Palette.Dim : st == WorkStatus.Working ? Palette.Olive : Palette.Mustard;
                string id = t.id;
                row.Button1(t.done ? null : who == null ? "Назначить" : who.name + " · сменить", !t.done, () => { CycleAssignee(id); });
                int up = WorkSim.HigherSlot(s, id);
                row.Button2(up < 0 ? null : "Выше", true, () => { WorkSim.SetPriority(s, id, up); game.Commit(); Redraw(); });
            }
            bool ok = Review.CanBuild(s, l, out var reason);
            build.interactable = ok;
            buildReason.text = ok ? "Все файлы готовы – можно собирать" : reason ?? "";
            TeamCards.Fill(game, team);
            int done = s.tasks.FindAll(x => x.done).Count;
            summary.text = "Задачи\n" + done + " из " + s.tasks.Count + "\n\nСборок\n" + s.builds.Count;
        }

        static string StatusText(LevelState s, LevelSO l, TaskState t, WorkStatus st, WaitReason why)
        {
            switch (st)
            {
                case WorkStatus.Done: return "готово";
                case WorkStatus.Working: return "в работе";
                case WorkStatus.Queued: return why == WaitReason.NotFirst ? "в очереди" : "ждёт начала дня";
                case WorkStatus.Waiting:
                    if (why == WaitReason.NoBudget) return "нет денег на день";
                    var e = s.Employee(t.assigneeId);
                    return "устал(а): нужно " + l.tuning.TierEnergy(t.tier) + ", есть " + Mathf.RoundToInt(e.energy);
                default: return "нет исполнителя";
            }
        }

        void CycleAssignee(string taskId)
        {
            var s = game.S;
            var t = s.Task(taskId);
            var pool = new List<string>();
            foreach (var e in s.staff) if (e.d == t.d) pool.Add(e.id);
            pool.Sort((x, y) => Rank(game.L.Candidate(y)).CompareTo(Rank(game.L.Candidate(x))));   // best first
            if (pool.Count == 0)
            {
                game.modal.Show("Некому поручить", "Нужен сотрудник: " + Names.Of(t.d) + ". Найми его в хедхантере.", ("Понятно", null));
                return;
            }
            int i = pool.IndexOf(t.assigneeId);
            string pick;
            if (i >= 0) pick = pool[(i + 1) % pool.Count];
            else
            {
                // first assignment: the best employee who has no other unfinished task; if all are busy, the best overall
                pick = pool.Find(id => !s.tasks.Exists(o => !o.done && o.id != taskId && o.assigneeId == id)) ?? pool[0];
            }
            WorkSim.Assign(s, taskId, pick);
            game.Commit();
            Redraw();
        }

        static int Rank(CandidateDef c) => c.quality * 10 + c.speed;

        void Build()
        {
            var s = game.S; var l = game.L;
            var r = Review.Build(s, l);
            if (r == null) return;
            game.Commit();
            if (!r.passed) game.ShowReview(r);   // a passed build is shown by Commit
        }
    }
}
