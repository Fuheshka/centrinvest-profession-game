using System;

namespace Studio.Core
{
    public enum Axis { Interest, Coherence, Clarity, Tech }          // index into int[4]
    public enum Stage { Idea, Production, Release, Done }
    public enum Slot { Genre, Setting, Audience, Mechanic, Hook, Reference }
    public enum Sender { Boss, Artist, Programmer, Producer, Nephew, Ai, Team }
    public enum ThreadKind { Unknown, Match, Conflict, Neutral }
    public enum DifficultyLevel { Intern, Designer, Lead }

    [Serializable]
    public struct Cost
    {
        public int days;
        public int budget;
    }
}
