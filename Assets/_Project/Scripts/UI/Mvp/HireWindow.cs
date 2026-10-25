using Studio.Core;
using UnityEngine;

namespace Studio.UI.Mvp
{
    // Headhunter: cards of every candidate with their characteristics and pay; hiring opens the employee's chat.
    public class HireWindow : MonoBehaviour
    {
        public GameLoop game;
        public Transform grid;
        public CandidateCardView cardPrefab;
        public UnityEngine.UI.Button toTasks;
        public TopBarMvp top;
        float redrawTimer;

        void Awake()
        {
            game.Changed += Tick;
            toTasks.onClick.AddListener(() => top.Show(3));
        }
        void OnEnable() => Redraw();

        void Tick()
        {
            if (!isActiveAndEnabled) return;
            redrawTimer -= Time.unscaledDeltaTime;
            if (redrawTimer > 0) return;
            redrawTimer = 0.5f;
            Redraw();
        }

        void Redraw()
        {
            if (game.S == null) return;
            var s = game.S; var l = game.L;
            int n = 0;
            foreach (var c in l.candidates)
            {
                var card = CandidateCardView.At(grid, cardPrefab, n++);
                var e = s.Employee(c.id);
                card.Set(c.character != null ? c.character : c.portrait, c.name, ProfessionOf(c.d), c.quirk, c.speed, c.quality, Names.Money(Staffing.DailyRate(c, l.tuning)) + " ₽ / день");
                string id = c.id;
                bool ok = Staffing.CanHire(s, l, id, out var why);
                card.SetHire(e != null, ok ? "Нанять" : why, ok,
                    () => { Staffing.Hire(s, l, id); game.Commit(); Redraw(); },
                    () => game.modal.Show("Уволить: " + c.name + "?",
                        "Задачи " + c.name + " останутся без исполнителя, прогресс сохранится. Деньги за сегодня не вернутся. Нанять заново можно здесь же.",
                        ("Уволить", () => { Staffing.Fire(s, id); game.Commit(); Redraw(); }), ("Отмена", null)));
            }
        }

        static string ProfessionOf(Discipline discipline) => discipline switch
        {
            Discipline.Design => "Художник",
            Discipline.Mechanics => "Разработчик",
            _ => "Звукорежиссёр"
        };
    }
}
