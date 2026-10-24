using NUnit.Framework;
using Studio.Core;
using UnityEditor;

namespace Studio.Tests
{
    public class ConceptReworkTests
    {
        LevelSO L;
        LevelState S;

        [SetUp]
        public void SetUp()
        {
            L = AssetDatabase.LoadAssetAtPath<LevelSO>("Assets/_Project/Data/Level0/Level0.asset");
            S = LevelState.New(L, "x");
            string[] ps = { "horror", "adult", "space", "pixel", "gravity", "pc" };   // wrong platform and audience for the brief
            string prev = null; var t = (BlockType[])System.Enum.GetValues(typeof(BlockType));
            for (int i = 0; i < 6; i++)
            {
                var id = ConceptRules.Place(S, L, t[i], i * 100, -100);
                ConceptRules.SetParam(S, L, id, ps[i]);
                if (prev != null) ConceptRules.Connect(S, prev, id);
                prev = id;
            }
            Assert.IsTrue(ConceptRules.Approve(S, L));
        }

        [Test]
        public void Frozen_UntilNegativeBuild()
        {
            var b = S.BlockOf(BlockType.Platform);
            Assert.IsFalse(ConceptRules.CanRework(S));
            Assert.IsFalse(ConceptRules.SetParam(S, L, b.id, "phone"));
        }

        [Test]
        public void AfterNegativeBuild_ParamsChange_AndRebuildAllowed()
        {
            var first = Review.Score(S, L);
            first.passed = false;
            S.builds.Add(first);
            var b = S.BlockOf(BlockType.Platform);
            Assert.IsTrue(ConceptRules.CanRework(S));
            Assert.IsFalse(ConceptRules.Remove(S, b.id), "blocks stay");
            Assert.IsTrue(ConceptRules.SetParam(S, L, b.id, "phone"));
            Assert.AreNotEqual(first.conceptSig, ConceptRules.Signature(S));
            foreach (var t in S.tasks) t.done = true;
            Assert.IsTrue(Review.CanBuild(S, L, out _), "a changed concept alone allows a rebuild");
            Assert.AreEqual(2, first.unmetRequirements.Count, "audience and platform were unmet");
            Assert.AreEqual(1, Review.Score(S, L).unmetRequirements.Count, "platform fixed, audience still wrong");
        }
    }
}
