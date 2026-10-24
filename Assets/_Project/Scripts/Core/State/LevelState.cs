using System;
using System.Collections.Generic;

namespace Studio.Core
{
    [Serializable] public class Block { public string id; public BlockType type; public string paramId; public float x, y; }
    [Serializable] public class ConceptLink { public string a, b; }

    [Serializable]
    public class ConceptState
    {
        public List<Block> blocks = new List<Block>();
        public List<ConceptLink> links = new List<ConceptLink>();
        public bool approved;
        public int nextId;
    }

    [Serializable]
    public class EmployeeState
    {
        public string id;                 // CandidateDef.id, also the chat id
        public Discipline d;
        public float energy = 100;
        public int paidDay;
        public string currentTaskId;      // started task: continues below the tier threshold until energy is 0
    }

    [Serializable]
    public class TaskState
    {
        public string id;
        public Discipline d;
        public string title;
        public int tier;
        public float effort, progress, qualitySum;   // qualitySum = sum of quality x work points
        public string assigneeId;
        public bool isFix;
        public string fileId;
        public bool done;
    }

    [Serializable]
    public class FileState
    {
        public string id, taskId;
        public Discipline d;
        public string title;
        public int revision;
        public float quality;
        public string authorId;
        public int day;
    }

    [Serializable] public class BuiltFile { public string fileId; public int revision; public float quality; }

    [Serializable]
    public class BuildRecord
    {
        public int number, day;
        public string conceptSig;   // parameters of the concept at build time: changing them allows a rebuild
        public List<BuiltFile> files = new List<BuiltFile>();
        public int[] scores = new int[3];
        public int[] concept = new int[3];
        public int overall;
        public List<string> unmetRequirements = new List<string>();
        public bool passed;
    }

    [Serializable] public class ChatEntry { public string chatId; public int day; public string kind, text, refId; }

    [Serializable] public class PendingEvent { public string eventId, employeeId; }

    public class NightReport { public int day; public List<string> lines = new List<string>(); public int spent; }

    [Serializable]
    public class LevelState
    {
        public const int CurrentSaveVersion = 2;
        public const string Boss = "boss";

        public int saveVersion = CurrentSaveVersion;
        public string levelId, nickname;
        public int day;
        public float dayElapsed;
        public LevelStage stage;
        public int budget, deadline;
        public ConceptState concept = new ConceptState();
        public List<EmployeeState> staff = new List<EmployeeState>();
        public List<TaskState> tasks = new List<TaskState>();
        public List<FileState> files = new List<FileState>();
        public List<BuildRecord> builds = new List<BuildRecord>();
        public List<ChatEntry> chat = new List<ChatEntry>();
        public List<PendingEvent> pendingEvents = new List<PendingEvent>();
        public List<string> eventsSeen = new List<string>();
        public float eventTimer;
        public bool eventsStarted;
        public FailReason failReason;
        public int score;

        public static LevelState New(LevelSO level, string nickname)
        {
            nickname = string.IsNullOrWhiteSpace(nickname) ? "Стажёр" : nickname.Trim();
            if (nickname.Length > 16) nickname = nickname.Substring(0, 16);
            var s = new LevelState
            {
                levelId = level.id, nickname = nickname, day = 1, stage = LevelStage.Concept,
                budget = level.startBudget, deadline = level.deadlineDays,
            };
            s.Say(Boss, "text", s.Fill(level.briefText, level));
            return s;
        }

        public string Fill(string text, LevelSO level) => (text ?? "")
            .Replace("{name}", nickname).Replace("{days}", level.deadlineDays.ToString())
            .Replace("{budget}", level.startBudget.ToString("N0").Replace(',', ' ').Replace(' ', ' '));

        public void Say(string chatId, string kind, string text, string refId = null) =>
            chat.Add(new ChatEntry { chatId = chatId, day = day, kind = kind, text = text, refId = refId });

        public EmployeeState Employee(string id) => staff.Find(e => e.id == id);
        public TaskState Task(string id) => tasks.Find(t => t.id == id);
        public FileState File(string id) => files.Find(f => f.id == id);
        public Block BlockOf(BlockType t) => concept.blocks.Find(b => b.type == t);
    }
}
