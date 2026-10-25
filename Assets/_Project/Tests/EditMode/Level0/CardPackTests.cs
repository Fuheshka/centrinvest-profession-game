using System.Collections.Generic;
using NUnit.Framework;
using Studio.Core;
using UnityEditor;

namespace Studio.Tests
{
    public class CardPackTests
    {
        LevelSO level;

        [SetUp]
        public void SetUp()
        {
            level = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
            Assert.IsNotNull(level);
        }

        [Test]
        public void AllTwentyPackagesAndDistinctBonusesHavePlayableCardChoices()
        {
            Assert.AreEqual(22, level.cardPacks.Count, "20 packages plus two distinct bonus combinations");
            CollectionAssert.AreEqual(new[]
            {
                "psych_horror", "slasher", "cozy_farm", "mobile_puzzle", "soulslike",
                "metroidvania", "roguelike_deckbuilder", "battle_royale", "tycoon", "visual_novel",
                "cozy_twist", "coop_horror", "pixel_platformer", "rts", "idle_clicker",
                "sandbox_survival", "party_minigames", "stealth_action", "arcade_racing", "sports_manager",
            }, level.cardPacks.GetRange(0, 20).ConvertAll(pack => pack.id));
            var ids = new HashSet<string>();
            foreach (var pack in level.cardPacks)
            {
                Assert.IsTrue(ids.Add(pack.id), "Duplicate package: " + pack.id);
                var state = LevelState.New(level, "Тест");
                string previous = null;
                int greenLinks = 0;
                for (var type = BlockType.Genre; type <= BlockType.Platform; type++)
                {
                    var paramId = pack.ParamId(type);
                    var param = level.Param(paramId);
                    Assert.IsNotNull(param, pack.title + ": missing " + type + " parameter " + paramId);
                    Assert.AreEqual(type, param.type, pack.title + ": wrong parameter type");
                    var blockId = ConceptRules.Place(state, level, type, 0, 0);
                    Assert.IsTrue(ConceptRules.SetParam(state, level, blockId, paramId));
                    if (previous != null)
                    {
                        Assert.IsTrue(ConceptRules.Connect(state, previous, blockId), pack.title);
                        var from = state.concept.blocks.Find(b => b.id == previous);
                        var effects = ConceptRules.PairEffects(level, from.paramId, paramId);
                        if (effects.FindAll(effect => effect.sign > 0).Count >
                            effects.FindAll(effect => effect.sign < 0).Count) greenLinks++;
                    }
                    previous = blockId;
                }
                Assert.GreaterOrEqual(greenLinks, 4, pack.title + ": the supplied cards do not form a compatible chain");
                Assert.AreSame(pack, ConceptRules.MatchPack(state, level));
                Assert.IsTrue(ConceptRules.CanApprove(state, level, out var reason), pack.title + ": " + reason);
            }
        }

        [Test]
        public void PackageNeedsAllSixExactChoices()
        {
            var pack = level.cardPacks[0];
            var state = LevelState.New(level, "Тест");
            for (var type = BlockType.Genre; type <= BlockType.Platform; type++)
            {
                var id = ConceptRules.Place(state, level, type, 0, 0);
                Assert.IsTrue(ConceptRules.SetParam(state, level, id, pack.ParamId(type)));
            }
            Assert.AreSame(pack, ConceptRules.MatchPack(state, level));
            Assert.IsTrue(ConceptRules.SetParam(state, level, state.BlockOf(BlockType.Audience).id, "kids"));
            Assert.IsNull(ConceptRules.MatchPack(state, level));
        }

        [Test]
        public void NewMatchesAndExplicitConflictsHaveOppositeEffects()
        {
            Assert.IsTrue(ConceptRules.PairEffects(level, "simulator", "kids").Exists(e => e.sign > 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "phone", "resource_management").Exists(e => e.sign < 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "visual_novel", "pvp_loot").Exists(e => e.sign < 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "idle", "realism").Exists(e => e.sign < 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "pixel", "adult").Exists(e => e.sign < 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "party", "solo_story").Exists(e => e.sign < 0));
            Assert.IsTrue(ConceptRules.PairEffects(level, "horror", "kids").Exists(e => e.sign < 0));
        }
    }
}
