namespace Studio.Core
{
    // Headhunter: daily rates and hiring (docs/MVP_RULES.md 3.6).
    public static class Staffing
    {
        public static int DailyRate(CandidateDef c, LevelTuning t) =>
            t.ratePerPoint * (c.speed + c.quality);

        public static int Rate(LevelState s, LevelSO level, string employeeId) => DailyRate(level.Candidate(employeeId), level.tuning);

        // Fired people leave the team; their tasks stay with progress but without an assignee. No refund: today's pay is spent.
        public static bool Fire(LevelState s, string employeeId)
        {
            var e = s.Employee(employeeId);
            if (e == null || s.stage != LevelStage.Production) return false;
            foreach (var t in s.tasks) if (t.assigneeId == employeeId) t.assigneeId = null;
            s.pendingEvents.RemoveAll(p => p.employeeId == employeeId);
            s.staff.Remove(e);
            return true;
        }

        public static bool CanHire(LevelState s, LevelSO level, string candidateId, out string reason)
        {
            reason = null;
            if (s.stage != LevelStage.Production) reason = s.stage == LevelStage.Concept ? "Сначала утверди концепт" : "Уровень завершён";
            else if (level.Candidate(candidateId) == null) reason = "Нет такого кандидата";
            else if (s.Employee(candidateId) != null) reason = "Уже в команде";
            else if (s.staff.Count >= level.tuning.teamLimit) reason = "Команда полная: " + level.tuning.teamLimit + " человека";
            return reason == null;
        }

        public static bool Hire(LevelState s, LevelSO level, string candidateId)
        {
            if (!CanHire(s, level, candidateId, out _)) return false;
            var c = level.Candidate(candidateId);
            s.staff.Add(new EmployeeState { id = c.id, d = c.d, energy = 100 });
            s.Say(c.id, "text", level.Text("hello_" + c.id, "Привет! Я " + c.name + ". Жду задачу из таск-листа."));
            return true;
        }
    }
}
