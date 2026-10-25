using System;
using System.Collections.Generic;
using Studio.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Studio.UI.Mvp
{
    // Owns the LevelState of the MVP: creates or loads it, ticks the workday, saves, and tells views to redraw.
    // Rules stay in Core; this class only decides when to call them (docs/MVP_RULES.md 3.1, 3.11).
    public class GameLoop : MonoBehaviour
    {
        public LevelSO level;
        public ModalView modal;
        public ReviewView review;

        public LevelState S { get; private set; }
        public LevelSO L => level;
        public event Action Changed;
        public bool Paused { get; private set; }

        float saveTimer;

        void Awake()
        {
            S = Session.Continue ? SaveSystem.LoadLevel() : null;
            if (S == null) S = LevelState.New(level, Session.Nickname);
            else Paused = true;   // a loaded game waits for the player
        }

        void Start()
        {
            Changed?.Invoke();
            if (Paused) ShowPause();
        }

        void Update()
        {
            AudioManager.Play(review.IsOpen ? Track.Review : Track.Work);
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) TogglePause();
            if (Paused || modal.IsOpen || review.IsOpen || S.stage != LevelStage.Production) return;
            var report = WorkSim.Tick(S, L, Time.unscaledDeltaTime * L.tuning.timeScale);
            EventRules.Tick(S, L, Time.unscaledDeltaTime * L.tuning.timeScale, n => UnityEngine.Random.Range(0, n));
            if (report != null) DayEnded(report);
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= L.tuning.autosaveSeconds) { saveTimer = 0; SaveSystem.SaveLevel(S); }
            Changed?.Invoke();
        }

        // Every view calls this after a Core operation.
        public void Commit()
        {
            SaveSystem.SaveLevel(S);
            Changed?.Invoke();
            if (S.stage == LevelStage.Passed || S.stage == LevelStage.Failed) ShowResult();
        }

        public void AskEndDay()
        {
            if (S.stage != LevelStage.Production) return;
            int day = S.day;
            modal.Show("Закончить день " + day + "?", "Сотрудники отдохнут до утра, энергия восстановится.",
                ("Закончить день", () => { var r = WorkSim.EndDay(S, L, day); if (r != null) DayEnded(r); }), ("Отмена", null));
        }

        void DayEnded(NightReport r)
        {
            Review.CheckBudgetDeadlock(S, L);
            SaveSystem.SaveLevel(S);
            Changed?.Invoke();
            if (S.stage == LevelStage.Failed) { ShowResult(); return; }
            modal.Show("Ночь после дня " + r.day, string.Join("\n", r.lines) ,
                ("Утро, день " + S.day, null));
        }

        void TogglePause()
        {
            if (SettingsPanel.IsOpen) { SettingsPanel.Close(); return; }
            if (Paused && modal.IsOpen) { modal.Hide(); Paused = false; }
            else if (!Paused && !modal.IsOpen && !review.IsOpen) ShowPause();
        }

        public void ShowPause()
        {
            Paused = true;
            modal.Show("Пауза", "День " + S.day + ". Время стоит.", ("Продолжить", () => Paused = false),
                ("Настройки", () => SettingsPanel.Open(ShowPause)), ("В меню", ToMenu));
        }

        // The customer's verdict screen; a failed build offers to send a weak file to fix.
        public void ShowReview(BuildRecord r)
        {
            string linkedFile = "";
            if (r.files != null && r.files.Count > 0)
            {
                var weakestFile = r.files[0];
                foreach (var file in r.files)
                    if (file.quality < weakestFile.quality) weakestFile = file;
                var projectFile = S.files.Find(file => file.id == weakestFile.fileId || file.taskId == weakestFile.fileId);
                if (projectFile != null) linkedFile = projectFile.title;
                if (string.IsNullOrWhiteSpace(linkedFile))
                {
                    var task = S.tasks.Find(item => item.id == weakestFile.fileId || item.fileId == weakestFile.fileId);
                    linkedFile = task != null ? task.title : "Файл проекта · " + Names.Of((Discipline)Review.Weakest(r));
                }
            }
            if (r.passed)
            {
                review.Show("Первый релиз готов!", r, L, S.budget, linkedFile, FinalDecisionSummary(r),
                    ("В меню", ToMenu), ("Пройти ещё раз", Restart));
                return;
            }
            var opts = new List<(string, Action)>();
            int w = Review.Weakest(r);
            foreach (var f in S.files)
                if ((int)f.d == w)
                {
                    string fid = f.id;
                    opts.Add(("Исправить: " + f.title + " (" + L.Candidate(f.authorId).name + ", " + Mathf.RoundToInt(f.quality) + ")", () => FixPicker.Open(this, fid)));
                }
            if (r.unmetRequirements.Count > 0) opts.Insert(0, ("Пересмотреть концепт", () => FindAnyObjectByType<TopBarMvp>()?.Show(1)));
            opts.Add(("Закрыть", null));
            review.Show("Отзыв заказчика", r, L, S.budget, linkedFile, null, opts.ToArray());
        }

        string FinalDecisionSummary(BuildRecord r)
        {
            string Param(BlockType type)
            {
                var block = S.BlockOf(type);
                return block == null ? "не выбран" : L.Param(block.paramId)?.title ?? "не выбран";
            }

            var team = new List<string>();
            foreach (var member in S.staff)
            {
                var candidate = L.Candidate(member.id);
                if (candidate != null) team.Add(candidate.name);
            }

            string teamText = team.Count > 0 ? string.Join(" · ", team) : "Команда не нанята";
            return "Твои решения:\n"
                + "Концепт: " + Param(BlockType.Genre) + " · " + Param(BlockType.Audience) + " · " + Param(BlockType.Platform) + "\n"
                + "Команда: " + teamText + "\n"
                + "Выпустил игру на день " + r.day + " из " + L.deadlineDays + " · сборка " + r.number + ".";
        }

        public void ShowResult()
        {
            bool win = S.stage == LevelStage.Passed;
            if (win && S.builds.Count > 0) { ShowReview(S.builds[S.builds.Count - 1]); return; }
            string body = win ? "Оценка игры " + S.score + " – это и оценка уровня." : ResultReasons();
            if (S.builds.Count > 0)
            {
                var r = S.builds[S.builds.Count - 1];
                body += "\n\nЗвук " + r.scores[2] + " · Дизайн " + r.scores[0] + " · Механики " + r.scores[1]
                     + "\nСборок: " + S.builds.Count + " · Осталось бюджета: " + Names.Money(S.budget) + " · День " + S.day + " из " + S.deadline;
            }
            modal.Show(win ? "Уровень пройден!" : "Уровень не пройден", body, ("Заново", Restart), ("В меню", ToMenu));
        }

        string ResultReasons()
        {
            string t = S.failReason == FailReason.NoBudget ? "Деньги кончились: платить команде нечем." : "Срок вышел.";
            t += S.builds.Count > 0 ? "\nПоследняя сборка: оценка " + S.score + " (нужно " + L.passScore + ")." : "\nИгра не выпущена: сборки не было.";
            foreach (var task in S.tasks)
                if (!task.done) t += "\nНе готово: " + task.title + " – " + Mathf.RoundToInt(100 * task.progress / task.effort) + " %";
            return t;
        }

        void Restart()
        {
            SaveSystem.DeleteLevel();
            Session.Continue = false;
            PixelTransition.Load(SceneManager.GetActiveScene().name);
        }

        void ToMenu()
        {
            SaveSystem.SaveLevel(S);
            PixelTransition.Load("Menu");
        }
    }
}
