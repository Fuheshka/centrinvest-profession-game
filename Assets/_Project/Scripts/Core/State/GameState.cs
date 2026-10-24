using System;
using System.Collections.Generic;

namespace Studio.Core
{
    [Serializable]
    public class Pending
    {
        public string messageId;
        public int day;
    }

    [Serializable]
    public class GameState
    {
        public string nickname;
        public DifficultyLevel difficulty;
        public int day;
        public int deadline;
        public int budget;
        public Stage stage;
        public int[] axes = new int[4];
        public int[] goals = new int[4];
        public BoardState board = new BoardState();
        public List<DecisionRecord> log = new List<DecisionRecord>();
        public List<VersionRecord> versions = new List<VersionRecord>();
        public List<string> inbox = new List<string>();       // message ids
        public List<string> answered = new List<string>();    // message ids
        public List<Pending> pending = new List<Pending>();   // follow-ups waiting for their day

        public static GameState New(ContentDB db, string nickname, DifficultyLevel difficulty)
        {
            var s = new GameState
            {
                nickname = nickname,
                difficulty = difficulty,
                day = 1,
                deadline = db.deadline,
                budget = db.startBudget,
                stage = Stage.Idea,
                axes = new[] { 30, 30, 30, 30 },
                goals = new[] { 50, 50, 50, 50 },
            };
            if (db.brief != null) s.inbox.Add(db.brief.id);
            return s;
        }
    }
}
