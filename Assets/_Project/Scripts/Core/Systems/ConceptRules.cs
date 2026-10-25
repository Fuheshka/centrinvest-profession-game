using System;
using System.Collections.Generic;
using UnityEngine;

namespace Studio.Core
{
    // Dashboard: blocks, manual links, buffs/debuffs, approval -> tasks (docs/MVP_RULES.md 3.8-3.10).
    public static class ConceptRules
    {
        static bool Editable(LevelState s) => s.stage == LevelStage.Concept && !s.concept.approved;
        // After a negative review the parameters (not blocks or links) may be changed: the requirements of the brief
        // cannot be met by fixing files alone.
        public static bool CanRework(LevelState s) =>
            s.stage == LevelStage.Production && s.builds.Count > 0 && !s.builds[s.builds.Count - 1].passed;

        public static string Signature(LevelState s) => string.Join(",", s.concept.blocks.ConvertAll(b => b.paramId ?? "-"));

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
            return SetParam(s, level, blockId, paramId, out _);
        }

        public static bool SetParam(LevelState s, LevelSO level, string blockId, string paramId, out string reason)
        {
            reason = null;
            var b = Find(s, blockId);
            var p = level.Param(paramId);
            if (!(Editable(s) || CanRework(s))) { reason = "Параметры нельзя менять после принятого концепта без негативного отзыва"; return false; }
            if (b == null || p == null || p.type != b.type) { reason = "Параметр не подходит этому блоку"; return false; }
            if (b.paramId == paramId) return true;

            bool rework = CanRework(s);
            if (rework && s.reworkChargedBuild != s.builds.Count)
            {
                if (s.day >= s.deadline) { reason = "Дедлайн уже наступил: потратить день на пересмотр нельзя"; return false; }

                int payroll = 0;
                foreach (var employee in s.staff)
                    payroll += Staffing.Rate(s, level, employee.id);
                if (s.budget < payroll)
                {
                    reason = "На пересмотр не хватает бюджета: нужны дневные ставки команды (" + payroll + ")";
                    return false;
                }

                s.budget -= payroll;
                s.reworkChargedBuild = s.builds.Count;
                s.Say(LevelState.Boss, "text", "Пересмотр концепта: списаны дневные ставки команды и один игровой день.");
                WorkSim.EndDay(s, level, s.day);
            }

            b.paramId = paramId;
            if (rework) RefreshTasksForConceptChange(s, level, b.type);
            return true;
        }

        static void RefreshTasksForConceptChange(LevelState s, LevelSO level, BlockType changedType)
        {
            foreach (var task in s.tasks)
            {
                if (task.isFix) continue;
                var template = level.tasks.Find(x => x.id == task.id);
                if (template == null) continue;

                string title = template.titleFormat;
                foreach (var conceptBlock in s.concept.blocks)
                    title = title.Replace("{" + conceptBlock.type + "}", level.Param(conceptBlock.paramId).title);
                task.title = title;

                if (!template.tierFromBlock || template.tierFrom != changedType) continue;
                var block = s.concept.blocks.Find(x => x.type == changedType);
                var param = block != null ? level.Param(block.paramId) : null;
                if (param == null) continue;
                int tier = Mathf.Clamp(param.tier, 1, 3);
                if (task.tier == tier) continue;

                task.tier = tier;
                task.effort = level.tuning.effortPerTier * tier;
                task.progress = 0;
                task.qualitySum = 0;
                task.done = false;
                foreach (var employee in s.staff)
                    if (employee.currentTaskId == task.id) employee.currentTaskId = null;
                s.files.RemoveAll(file => file.id == task.id || file.taskId == task.id);
            }
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
            if (o.rate != 0) p.Add("расход в день +" + Money(o.rate) + " руб.");
            if (o.energy != 0) p.Add("энергия " + Signed(o.energy));
            if (o.teamEnergy != 0) p.Add("команда: энергия " + Signed(o.teamEnergy));
            if (o.progress != 0) p.Add("прогресс " + Pct(o.progress));
            if (o.progressAll != 0) p.Add("везде " + Pct(o.progressAll));
            foreach (var d in o.disc) p.Add(Of(d.d).ToLower() + " " + Pct(d.share));
            if (o.failChance > 0) p.Add(Mathf.RoundToInt(o.failChance * 100) + " %: везде " + Pct(o.failAll));
            if (o.speed > 0) p.Add("скорость сегодня ×" + o.speed.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            if (o.endDay) p.Add("день потерян");
            if (o.fire >= 0) p.Add("уходит сотрудник");
            if (o.croak) p.Add("ква");
            return string.Join(", ", p);
        }

        static string Signed(float v) => (v > 0 ? "+" : "–") + Math.Abs(Mathf.RoundToInt(v));
        static string Pct(float share) => (share > 0 ? "+" : "–") + Mathf.RoundToInt(Math.Abs(share) * 100) + " %";

        public static string Money(int v) => v.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', ' ');
    }
}
