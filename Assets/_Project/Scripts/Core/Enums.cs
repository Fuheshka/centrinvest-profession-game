using System;

namespace Studio.Core
{
    public enum Axis { Interest, Coherence, Clarity, Tech }          // index into int[4]
    public enum Stage { Idea, Production, Release, Done }
    public enum Slot { Genre, Setting, Audience, Mechanic, Hook, Reference }
    public enum Sender { Boss, Artist, Programmer, Producer, Nephew, Ai, Team }
    public enum ThreadKind { Unknown, Match, Conflict, Neutral }
    public enum DifficultyLevel { Intern, Designer, Lead }

    // MVP level 0 (docs/MVP_RULES.md)
    public enum BlockType { Genre, Audience, Setting, Style, Mechanics, Platform }
    public enum Discipline { Design, Mechanics, Sound }        // employee, task and score criterion; index into int[3]
    public enum LevelStage { Concept, Production, Passed, Failed }
    public enum FailReason { None, Deadline, NoBudget }
    public enum LinkColor { Green, Red }
    public enum WorkStatus { Open, Queued, Working, Waiting, Done }
    public enum WaitReason { None, NoEnergy, NoBudget, NoAssignee, NotFirst }

    [Serializable]
    public struct Cost
    {
        public int days;
        public int budget;
    }
}
