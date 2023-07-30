using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.BattleTests.MagicTests
{
    [TestFixture]
    class SpellFactoryTests
    {
        private static readonly List<Tuple<MagicType, string>> _level1SpellCombos = new List<Tuple<MagicType, string>>
        {
            new Tuple<MagicType, string>(MagicType.Fire, "fireball"),
            new Tuple<MagicType, string>(MagicType.Earth, "clay spike"),
            new Tuple<MagicType, string>(MagicType.Water,"splash"),
            new Tuple<MagicType, string>(MagicType.Wind,"gust")
        };

        private static readonly List<Tuple<MagicType, string>> _level2SpellCombos = new List<Tuple<MagicType, string>>
        {
            new Tuple<MagicType, string>(MagicType.Fire, "blaze"),
            new Tuple<MagicType, string>(MagicType.Earth, "boulder"),
            new Tuple<MagicType, string>(MagicType.Water, "angry splash"),
            new Tuple<MagicType, string>(MagicType.Wind, "air whip")
        };

        [Test]
        public void TestGetSpellMethod_Level1([Values(MagicType.Fire, MagicType.Water, MagicType.Wind, MagicType.Earth)] MagicType type)
        {
            var ret = SpellFactory.GetSpell(type, 1);
            var expected = _level1SpellCombos.First(sc => sc.Item1 == type);

            Assert.AreEqual(2, ret.Cost);
            Assert.AreEqual(type, ret.ElementalType);
            Assert.AreEqual(1, ret.Power);
            Assert.AreEqual(expected.Item2, ret.Description);
        }

        [Test]
        public void TestGetSpellMethod_Level2([Values(MagicType.Fire, MagicType.Water, MagicType.Wind, MagicType.Earth)] MagicType type)
        {
            var ret = SpellFactory.GetSpell(type, 2);
            var expected = _level2SpellCombos.First(sc => sc.Item1 == type);

            Assert.AreEqual(5, ret.Cost);
            Assert.AreEqual(type, ret.ElementalType);
            Assert.AreEqual(3, ret.Power);
            Assert.AreEqual(expected.Item2, ret.Description);
        }
    }
}
