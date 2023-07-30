using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.MagicTests
{
    [TestFixture]
    class MagicRelationshipTests
    {
        private TestEnemyFighter _enemy;
        private TestHumanFighter _human;

        [SetUp]
        public void SetUp()
        {
            _enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _human = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
        }

        //the keys are weak to their values in the magic relationship
        Dictionary<MagicType, MagicType> _strengths = new Dictionary<MagicType, MagicType>
        {
            { MagicType.Fire, MagicType.Wind }
            , { MagicType.Wind, MagicType.Earth }
            , { MagicType.Earth, MagicType.Water }
            , { MagicType.Water, MagicType.Fire }
        };

        [Test]
        public void MagicRelationshipCalculor_ReturnsCorrectRelationships()
        {
            foreach (var key in _strengths.Keys)
            {
                var value = _strengths[key];
                var relationship = MagicRelationshipCalculator.GetRelationship(key, value);
                Assert.AreEqual(MagicRelationshipType.Strong, relationship, $"{key} should deal increased damage to {value}");

                relationship = MagicRelationshipCalculator.GetRelationship(value, key);
                Assert.AreEqual(MagicRelationshipType.Weak, relationship, $"{value} should deal reduced damage to {key}");
            }
        }

        [Test]
        public void  FighterTakesCorrectDamage_ElementalWeakness([Values(MagicType.Fire, MagicType.Ice, MagicType.Earth)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalWeakness(type);
            _enemy.MagicalDamage(10, type);

            //The current health will be 90 if the resistance has no impact on damage done
            Assert.AreEqual(80, _enemy.CurrentHealth);
        }

        [Test]
        public void FighterTakesCorrectDamage_ElementalWeakness_ToAllMagicTypes([Values(MagicType.Fire, MagicType.Ice, MagicType.Earth)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalWeakness(MagicType.All);
            _enemy.MagicalDamage(10, type);

            //The current health will be 90 if the resistance has no impact on damage done
            Assert.AreEqual(80, _enemy.CurrentHealth);
        }

        [Test]
        public void FighterTakesCorrectDamage_ElementalResistance([Values(MagicType.Water, MagicType.Wind, MagicType.Lightning)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalResistance(type);
            _enemy.MagicalDamage(10, type);

            //The current health will be 90 if the resistance has no impact on damage done
            Assert.AreEqual(95, _enemy.CurrentHealth);
        }

        [Test]
        public void FighterTakesCorrectDamage_ElementalResistance_ToAllMagicTypes([Values(MagicType.Water, MagicType.Wind, MagicType.Lightning)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalResistance(MagicType.All);
            _enemy.MagicalDamage(10, type);

            //The current health will be 90 if the resistance has no impact on damage done
            Assert.AreEqual(95, _enemy.CurrentHealth);
        }

        [Test]
        public void FighterTakesCorrectDamage_ElementalImmunity([Values(MagicType.Fire, MagicType.Water, MagicType.Ice)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalImmunity(type);
            _enemy.MagicalDamage(10, type);

            //will have 90 health left if immunity not taken into account
            Assert.AreEqual(100, _enemy.CurrentHealth);
        }

        [Test]
        public void FighterTakesCorrectDamage_ElementalImmunity_ToAllMagicTypes([Values(MagicType.Fire, MagicType.Water, MagicType.Ice)] MagicType type)
        {
            _enemy.SetHealth(100);
            _enemy.SetElementalImmunity(MagicType.All);
            _enemy.MagicalDamage(10, type);

            //will have 90 health left if immunity not taken into account
            Assert.AreEqual(100, _enemy.CurrentHealth);
        }
    }
}
