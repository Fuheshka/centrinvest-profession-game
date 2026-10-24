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
        public Transform list, teamGrid, slots;
        public TaskRowView rowPrefab;
        public TeamMemberView memberPrefab;
        public Image slotPrefab;
        public Sprite fileDesign, fileMechanics, fileSound;
        public Button build;
        public TMP_Text buildReason, filesLabel;
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

        static readonly Color Pink = Palette.Hex("F4B9B2"), Purple = Palette.Hex("CBC6F2"), Gold = Palette.Hex("F2DC8A");
        static readonly Color Blue = Palette.Hex("D5E6F7"), Green = Palette.Hex("C7E8B0"), Grey = Palette.Hex("E2DFD0"), Orange = Palette.Hex("F7D9B0");

        void Redraw()
        {
            if (game.S == null) return;
            var s = game.S; var l = game.L;
            for (int i = 0; i < s.tasks.Count; i++)
            {
                var t = s.tasks[i];
                var row = TaskRowView.At(list, rowPrefab, i);
                var st = WorkSim.Status(s, l, t.id, out var why);
                var who = t.assigneeId == null ? null : l.Candidate(t.assigneeId);
                Color chipColor = t.d == Discipline.Design ? Pink : t.d == Discipline.Mechanics ? Purple : Gold;
                Color stColor = st == WorkStatus.Done ? Green : st == WorkStatus.Working ? Blue : st == WorkStatus.Waiting ? Orange : Grey;
                row.Set(i + 1, t.title, Names.Of(t.d), chipColor, who?.character, who == null ? "Не назначено" : who.name, who == null ? "" : Names.Of(who.d),
                        StatusText(s, l, t, st, why), stColor, t.done ? 1 : t.progress / t.effort, t.done);
                string id = t.id;
                bool open = !t.done;
                row.assign.gameObject.SetActive(open);
                row.assignLabel.text = who == null ? "Назначить" : "Сменить";
                row.assign.onClick.RemoveAllListeners();
                row.assign.onClick.AddListener(() => CycleAssignee(id));
                int up = WorkSim.HigherSlot(s, id);
                row.up.gameObject.SetActive(open && up >= 0);
                row.up.onClick.RemoveAllListeners();
                row.up.onClick.AddListener(() => { WorkSim.SetPriority(s, id, up); game.Commit(); Redraw(); });
            }

            for (int i = 0; i < l.tuning.teamLimit; i++)
            {
                var card = TeamMemberView.At(teamGrid, memberPrefab, i);
                if (i >= s.staff.Count) { card.SetEmpty(); continue; }
                var e = s.staff[i]; var c = l.Candidate(e.id);
                card.Set(c.character, c.name, Names.Of(c.d), e.energy, e.currentTaskId == null ? "Свободен(на)" : "Работает");
            }

            int done = s.tasks.FindAll(x => x.done).Count;
            filesLabel.text = "Файлы для сборки: " + done + " / " + s.tasks.Count;
            for (int i = 0; i < s.tasks.Count; i++)
            {
                while (slots.childCount <= i) Instantiate(slotPrefab, slots);
                var slot = slots.GetChild(i).GetComponent<Image>();
                var t = s.tasks[i];
                var ic = slot.transform.GetChild(0).GetComponent<Image>();
                ic.sprite = t.d == Discipline.Design ? fileDesign : t.d == Discipline.Mechanics ? fileMechanics : fileSound;
                ic.gameObject.SetActive(t.done);
                slot.color = t.done ? Palette.Primary : Palette.Hex("3A2A20");
            }
            bool ok = Review.CanBuild(s, l, out var reason);
            build.interactable = ok;
            buildReason.text = ok ? "Все файлы готовы – можно собирать." : reason ?? "";
        }

        static string StatusText(LevelState s, LevelSO l, TaskState t, WorkStatus st, WaitReason why)
        {
            switch (st)
            {
                case WorkStatus.Done: return "готово";
                case WorkStatus.Working: return "в работе";
                case WorkStatus.Queued: return why == WaitReason.NotFirst ? "в очереди" : "ждёт дня";
                case WorkStatus.Waiting:
                    return why == WaitReason.NoBudget ? "нет денег" : "ждёт сил";
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
