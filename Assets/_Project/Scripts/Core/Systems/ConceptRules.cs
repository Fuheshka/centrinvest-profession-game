using System;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    // Dashboard: blocks, manual links, buffs/debuffs, approval -> tasks (docs/MVP_RULES.md 3.8-3.10).
    public static class ConceptRules
    {
        static bool Editable(LevelState s) => s.stage == LevelStage.Concept && !s.concept.approved;
        static Block Find(LevelState s, string id) => s.concept.blocks.Find(b => b.id == id);

        public static string Place(LevelState s, LevelSO level, BlockType type, float x, float y)
        {
            if (!Editable(s) || s.BlockOf(type) != null) return null;
            var b = new Block { id = "b" + (++s.concept.nextId), type = type, x = x, y = y };
            s.concept.blocks.Add(b);
            return b.id;
        }

        public static bool SetParam(LevelState s, LevelSO level, string blockId, string paramId)
        {
            var b = Find(s, blockId);
            var p = level.Param(paramId);
            if (!Editable(s) || b == null || p == null || p.type != b.type) return false;
            b.paramId = paramId;
            return true;
        }

        public static bool Move(LevelState s, string blockId, float x, float y)
        {
            var b = Find(s, blockId);
            if (!Editable(s) || b == null) return false;
            b.x = x; b.y = y;
            return true;
        }

        public static bool Remove(LevelState s, string blockId)
        {
            var b = Find(s, blockId);
            if (!Editable(s) || b == null) return false;
            s.concept.blocks.Remove(b);
            s.concept.links.RemoveAll(l => l.a == blockId || l.b == blockId);
            return true;
        }

        static int LinkIndex(LevelState s, string a, string b) =>
            s.concept.links.FindIndex(l => (l.a == a && l.b == b) || (l.a == b && l.b == a));

        // Which block types may be linked: the chain Genre-Audience-Setting-Style-Mechanics-Platform plus three chords.
        // The dashboard shows it by dimming the other blocks, never in words.
        static readonly (BlockType, BlockType)[] Linkable =
        {
            (BlockType.Genre, BlockType.Audience), (BlockType.Audience, BlockType.Setting), (BlockType.Setting, BlockType.Style),
            (BlockType.Style, BlockType.Mechanics), (BlockType.Mechanics, BlockType.Platform),
            (BlockType.Genre, BlockType.Mechanics), (BlockType.Genre, BlockType.Style), (BlockType.Audience, BlockType.Platform),
        };

        public const int MaxLinks = 6;   // a spanning chain needs 5: one spare link, no more

        public static bool CanLink(BlockType a, BlockType b)
        {
            foreach (var (x, y) in Linkable) if ((x == a && y == b) || (x == b && y == a)) return true;
            return false;
        }

        public static bool Connect(LevelState s, string a, string b)
        {
            if (!Editable(s) || a == b || Find(s, a) == null || Find(s, b) == null || LinkIndex(s, a, b) >= 0) return false;
            if (s.concept.links.Count >= MaxLinks || !CanLink(Find(s, a).type, Find(s, b).type)) return false;
            s.concept.links.Add(new ConceptLink { a = a, b = b });
            return true;
        }

        public static bool Disconnect(LevelState s, string a, string b)
        {
            int i = LinkIndex(s, a, b);
            if (!Editable(s) || i < 0) return false;
            s.concept.links.RemoveAt(i);
            return true;
        }

        public static List<Effect> PairEffects(LevelSO level, string pa, string pb)
        {
            var list = new List<Effect>();
            if (pa == null || pb == null) return list;
            foreach (var r in level.pairRules)
                if ((r.a == pa && r.b == pb) || (r.a == pb && r.b == pa)) list.AddRange(r.effects);
            return list;
        }

        public static Core.LinkColor LinkColor(LevelState s, LevelSO level, string a, string b)
        {
            var ba = Find(s, a); var bb = Find(s, b);
            if (ba == null || bb == null) return Core.LinkColor.Red;
            int plus = 0, minus = 0;
            foreach (var e in PairEffects(level, ba.paramId, bb.paramId)) if (e.sign > 0) plus++; else if (e.sign < 0) minus++;
            return plus > minus ? Core.LinkColor.Green : Core.LinkColor.Red;   // a tie is red too
        }

        // Only linked pairs count: the links are the player's choice of which combinations the game is built on.
        public static int[] Totals(LevelState s, LevelSO level)
        {
            var t = new int[3];
            foreach (var l in s.concept.links)
                foreach (var e in PairEffects(level, Find(s, l.a)?.paramId, Find(s, l.b)?.paramId)) t[(int)e.d] += Math.Sign(e.sign);
            int c = level.tuning.conceptClamp;
            for (int i = 0; i < 3; i++) t[i] = Mathf.Clamp(t[i], -c, c);
            return t;
        }

        public static bool CanApprove(LevelState s, LevelSO level, out string reason)
        {
            reason = null;
            if (!Editable(s)) { reason = "Концепт уже утверждён"; return false; }
            foreach (BlockType t in Enum.GetValues(typeof(BlockType)))
            {
                var b = s.BlockOf(t);
                if (b == null) { reason = "Нет блока: " + Names.Of(t); return false; }
                if (b.paramId == null) { reason = "Не выбран параметр: " + Names.Of(t); return false; }
            }
            // all blocks in one network of links
            var seen = new HashSet<string> { s.concept.blocks[0].id };
            for (bool grew = true; grew;)
            {
                grew = false;
                foreach (var l in s.concept.links)
                    if (seen.Contains(l.a) != seen.Contains(l.b)) { seen.Add(l.a); seen.Add(l.b); grew = true; }
            }
            foreach (var b in s.concept.blocks)
                if (!seen.Contains(b.id)) { reason = "Соедини все блоки линиями: " + Names.Of(b.type) + " не связан"; return false; }
            return true;
        }

        public static bool Approve(LevelState s, LevelSO level)
        {
            if (!CanApprove(s, level, out _)) return false;
            s.concept.approved = true;
            s.stage = LevelStage.Production;
            s.dayElapsed = 0;
            foreach (var tpl in level.tasks)
            {
                int tier = tpl.fixedTier;
                if (tpl.tierFromBlock) tier = level.Param(s.BlockOf(tpl.tierFrom).paramId).tier;
                tier = Mathf.Clamp(tier, 1, 3);
                string title = tpl.titleFormat;
                foreach (var b in s.concept.blocks) title = title.Replace("{" + b.type + "}", level.Param(b.paramId).title);
                s.tasks.Add(new TaskState { id = tpl.id, d = tpl.d, title = title, tier = tier, effort = level.tuning.effortPerTier * tier });
            }
            s.Say(LevelState.Boss, "text", s.Fill(level.Text("approved", "Концепт принят. Нанимай команду в хедхантере и раздай задачи."), level));
            return true;
        }
    }

    public static class Names
    {
        public static string Of(BlockType t) => t switch
        {
            BlockType.Genre => "Жанр", BlockType.Audience => "Аудитория", BlockType.Setting => "Сеттинг",
            BlockType.Style => "Стиль", BlockType.Mechanics => "Механики", _ => "Платформа",
        };

        public static string Of(Discipline d) => d switch
        {
            Discipline.Design => "Дизайн", Discipline.Mechanics => "Механики", _ => "Звук",
        };

        // "–2 000 руб., энергия +10, прогресс +15 %" for an event option.
        public static string Effects(EventOption o)
        {
            var p = new List<string>();
            if (o.budget != 0) p.Add((o.budget > 0 ? "+" : "–") + Money(Math.Abs(o.budget)) + " руб.");
            if (o.energy != 0) p.Add("энергия " + (o.energy > 0 ? "+" : "–") + Math.Abs(Mathf.RoundToInt(o.energy)));
            if (o.progress != 0) p.Add("прогресс " + (o.progress > 0 ? "+" : "–") + Mathf.RoundToInt(Math.Abs(o.progress) * 100) + " %");
            return string.Join(", ", p);
        }

        public static string Money(int v) => v.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}
