using NUnit.Framework;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.MockClassesTests
{
    [TestFixture]
    public class TestFieldEffectCombinerTests
    {
        private TestFieldEffectCombiner _combiner;

        [SetUp]
        public void SetUp()
        {
            _combiner = new TestFieldEffectCombiner();
        }

        [Test]
        public void CorrectlyReturnsDefaultImplementation_NoTestCombosSetup()
        {
            CombinedFieldEffect returnedEffect = _combiner.Combine(DanceEffectType.Fire, DanceEffectType.Soul);

            Assert.AreEqual("courage dance", returnedEffect.Description);
        }

        [Test]
        public void CorrectlyReturnsTestCombo()
        {
            CombinedFieldEffect fakeCombo = new CombinedFieldEffect("foo", new CriticalChanceMultiplierFieldEffect(TargetType.EnemyTeam, "fwop", 0, 5));

            _combiner.AddFakeCombination(DanceEffectType.Fire, DanceEffectType.Soul, fakeCombo);
            CombinedFieldEffect returnedEffect = _combiner.Combine(DanceEffectType.Fire, DanceEffectType.Soul);

            Assert.AreEqual(fakeCombo, returnedEffect);
        }
    }
}
