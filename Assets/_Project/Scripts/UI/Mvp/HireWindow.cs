using Studio.Core;
using UnityEngine;

namespace Studio.UI.Mvp
{
    public class HireWindow : MonoBehaviour
    {
        public GameLoop game;
        public Transform list;
        public RowView rowPrefab;
        public RowView[] team = new RowView[4];
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
            TeamCards.Fill(game, team);
            int n = 0;
            foreach (var c in l.candidates)
            {
                var row = RowView.At(list, rowPrefab, n++);
                var e = s.Employee(c.id);
                row.Set(c.name + " · " + Names.Of(c.d),
                        "Скорость " + c.speed + "/3 · Качество " + c.quality + "/3"
                        + (e != null ? " · Энергия " + Mathf.RoundToInt(e.energy) : ""),
                        Names.Money(Staffing.DailyRate(c, l.tuning)) + " / день");
                row.Progress(-1);
                row.background.color = e != null ? Palette.Dim : Palette.Olive;
                string id = c.id;
                if (e == null)
                {
                    bool ok = Staffing.CanHire(s, l, id, out var why);
                    row.Button1(ok ? "Нанять" : why, ok, () => { Staffing.Hire(s, l, id); game.Commit(); Redraw(); });
                    row.Button2(null, false, null);
                }
                else
                {
                    row.Button1("В команде", false, null);
                    row.Button2("Уволить", true, () => game.modal.Show("Уволить: " + c.name + "?",
                        "Задачи " + c.name + " останутся без исполнителя, прогресс сохранится. Деньги за сегодня не вернутся. Нанять заново можно здесь же.",
                        ("Уволить", () => { Staffing.Fire(s, id); game.Commit(); Redraw(); }), ("Отмена", null)));
                }
            }
        }
    }
}
