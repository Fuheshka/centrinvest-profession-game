using System;
using System.Collections.Generic;

namespace Studio.Core
{
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
    }
}
