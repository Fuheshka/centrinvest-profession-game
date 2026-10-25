using System;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    [Serializable] public class Requirement { public BlockType type; public string paramId; }
    [Serializable] public class ParamDef { public string id; public BlockType type; public string title; public int tier; }
    [Serializable] public class Effect { public Discipline d; public int sign; }
    [Serializable] public class PairRule { public string a, b; public List<Effect> effects = new List<Effect>(); }
    [Serializable] public class TaskTemplate { public string id; public Discipline d; public string titleFormat; public BlockType tierFrom; public bool tierFromBlock; public int fixedTier = 1; }
    [Serializable] public class CandidateDef { public string id, name; public Discipline d; public int speed, quality; public Sprite portrait, character; public string quirk; }   // portrait: messenger mascot; character: drawn person for the headhunter
    [Serializable] public class TextDef { public string key, text; }
    // A random work event: an employee writes in the chat and offers two choices (docs/DECISIONS.md, 2026-10-24).
    // progress is a share of the task's effort, energy and budget are plain numbers.
    [Serializable] public class DiscShare { public Discipline d; public float share; }
    // progress is a share of the task's effort, energy and budget are plain numbers. Everything below the first line is optional.
    [Serializable]
    public class EventOption
    {
        public string label, reply; public float energy, progress; public int budget;
        public float teamEnergy;                 // added to every employee
        public float progressAll;                // share of effort added to every open task whose discipline is not in disc
        public List<DiscShare> disc = new List<DiscShare>();
        public float speed, failChance, failAll; // speed: x for the rest of the day (0 = none); failChance: odds that failAll hits every open task
        public bool speedAll;                    // speed hits the whole team instead of the sender's discipline
        public int rate;                         // raises the sender's daily pay for good
        public bool endDay, croak;               // endDay: the rest of today is lost
        public int fire = -1;                    // Discipline (as int) of the employee who leaves, -1 nobody
    }
    // A random work event: an employee (or the boss) writes in the chat and offers choices (docs/DECISIONS.md, 2026-10-24).
    [Serializable]
    public class EventDef
    {
        public string id; public Discipline d; public bool anyone, boss, must; public string text;   // must: comes before any other event, from the boss if nobody fits
        public string after;                                       // shown only once this event has been answered
        public List<Discipline> needs = new List<Discipline>();    // every listed discipline must be on the team
        public List<EventOption> options = new List<EventOption>();
    }

    // Every number of the MVP rules lives here, so balance is edited in the inspector (docs/MVP_RULES.md, section 3).
    [Serializable]
    public class LevelTuning
    {
        public float daySeconds = 300, maxTickSeconds = 0.25f, autosaveSeconds = 10, timeScale = 2, eventFirst = 40, eventEvery = 70, eventJitter = 30;   // event times in game seconds   // game seconds per real second
        public float drainPerSecond = 0.4f, idleRegenPerSecond = 0.1f, nightRest = 100;   // energy back overnight
        public int[] tierEnergy = { 10, 35, 60 };
        public float effortPerTier = 150, qualityBase = 40, qualityPerPoint = 15, fatigueBelow = 25, fatiguePenalty = 15;
        public int ratePerPoint = 1000, teamLimit = 4;
        public int conceptClamp = 3, conceptStep = 4, requirementPenalty = 10;
        public float fixEffortShare = 0.5f;

        public int TierEnergy(int tier) => tierEnergy[Mathf.Clamp(tier, 1, tierEnergy.Length) - 1];
    }

    [CreateAssetMenu(menuName = "Studio/Level")]
    public class LevelSO : ScriptableObject
    {
        public string id, title;
        [TextArea] public string briefText;
        public int deadlineDays = 3, startBudget = 100000, passScore = 70;
        public List<Requirement> requirements = new List<Requirement>();
        public List<ParamDef> parameters = new List<ParamDef>();
        public List<PairRule> pairRules = new List<PairRule>();
        public List<TaskTemplate> tasks = new List<TaskTemplate>();
        public List<CandidateDef> candidates = new List<CandidateDef>();
        public List<TextDef> texts = new List<TextDef>();
        public List<EventDef> events = new List<EventDef>();
        public LevelTuning tuning = new LevelTuning();

        public ParamDef Param(string pid) => parameters.Find(p => p.id == pid);
        public CandidateDef Candidate(string cid) => candidates.Find(c => c.id == cid);
        public string Text(string key, string fallback = "") => texts.Find(t => t.key == key)?.text ?? fallback;
    }
}
