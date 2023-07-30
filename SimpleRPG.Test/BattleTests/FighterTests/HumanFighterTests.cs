using System;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Test.MockClasses;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class HumanFighterTests
    {
        private HumanFighter _fighter;

        private readonly int _expToLevel2 = LevelUpManager.GetExpForLevel(2);
        private readonly int _expToLevel4 = LevelUpManager.GetExpForLevel(4);

        [SetUp]
        public void Setup()
        {
           _fighter = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
        }

        [TearDown]
        public void TearDown()
        {
            _fighter = null;
        }

        private readonly Dictionary<int, List<Spell>> _spellsByLevel = new Dictionary<int, List<Spell>>
        {
            {
                4, SpellFactory.GetSpellsByLevel<HumanFighter>(4, 4)
            }
            ,{
                5, SpellFactory.GetSpellsByLevel<HumanFighter>(5, 5)
            }
        };

        private List<Spell> GetSpellsByLevel(int level)
        {
            var ret = new List<Spell>();

            var keys = _spellsByLevel.Keys.Where(l => l <= level);
            foreach(var key in keys)
            { 
                ret.AddRange(_spellsByLevel[key]);
            }

            return ret;
        }

        [Test]
        public void LevelInitializater_CorrectlyInitializesStatsBasedOnLevel([Values(1, 2, 5)] int level)
        {
            var fighter = new HumanFighter("Foo the Magnificent", level);

            Assert.AreEqual(level, fighter.Level);
            Assert.AreEqual(LevelUpManager.GetHealthByLevel(level), fighter.CurrentHealth);
            Assert.AreEqual(LevelUpManager.GetHealthByLevel(level), fighter.MaxHealth);
            Assert.AreEqual(LevelUpManager.GetManaByLevel(level), fighter.CurrentMana);
            Assert.AreEqual(LevelUpManager.GetManaByLevel(level), fighter.MaxMana);
            Assert.AreEqual(LevelUpManager.GetStrengthByLevel(level), fighter.Strength);
            Assert.AreEqual(LevelUpManager.GetDefenseByLevel(level), fighter.Defense);
            Assert.AreEqual(LevelUpManager.GetSpeedByLevel(level), fighter.Speed);
            Assert.AreEqual(LevelUpManager.GetEvadeByLevel(level), fighter.Evade);
            Assert.AreEqual(LevelUpManager.GetLuckByLevel(level), fighter.Luck);
            var spells = GetSpellsByLevel(level);
            Assert.AreEqual(spells.Count, fighter.Spells.Count);
            Assert.IsTrue(spells.TrueForAll(s => fighter.Spells.SingleOrDefault(fs => fs.Description == s.Description) != null));
        }

        [Test]
        public void GainExpMethod_AppropriatelyRaisesCurrentExp()
        {
            var amount = _expToLevel2 - 1;
            Assert.AreEqual(0, _fighter.CurrentExp);

            _fighter.GainExp(amount);

            Assert.AreEqual(amount, _fighter.CurrentExp);
            Assert.AreEqual(1, _fighter.Level);
        }

        [Test]
        public void GainExpMethod_AppropriatelyRaisesLevel([Values(2, 4)] int level)
        {
            var expAmount = (level == 2) ? _expToLevel2 : _expToLevel4;
            Assert.AreEqual(0, _fighter.CurrentExp);

            _fighter.GainExp(expAmount);

            Assert.AreEqual(expAmount, _fighter.CurrentExp);
            Assert.AreEqual(level, _fighter.Level);
            Assert.AreEqual(LevelUpManager.GetHealthByLevel(level), _fighter.CurrentHealth);
            Assert.AreEqual(LevelUpManager.GetHealthByLevel(level), _fighter.MaxHealth);
            Assert.AreEqual(LevelUpManager.GetManaByLevel(level), _fighter.CurrentMana);
            Assert.AreEqual(LevelUpManager.GetManaByLevel(level), _fighter.MaxMana);
            Assert.AreEqual(LevelUpManager.GetStrengthByLevel(level), _fighter.Strength);
            Assert.AreEqual(LevelUpManager.GetDefenseByLevel(level), _fighter.Defense);
            Assert.AreEqual(LevelUpManager.GetSpeedByLevel(level), _fighter.Speed);
            Assert.AreEqual(LevelUpManager.GetEvadeByLevel(level), _fighter.Evade);
            Assert.AreEqual(LevelUpManager.GetLuckByLevel(level), _fighter.Luck);
            var spells = GetSpellsByLevel(level);
            Assert.AreEqual(spells.Count, _fighter.Spells.Count);
            Assert.IsTrue(spells.TrueForAll(s => _fighter.Spells.SingleOrDefault(fs => fs.Description == s.Description) != null));
        }

        [Test]
        public void GainExpMethod_AppropriatelyRaisesExceptionIfAmountIsNegative()
        {
            Assert.Throws<ArgumentException>(() => _fighter.GainExp(-1));
        }

        [Test]
        public void LevelUp_DoesNotOverwriteNonLevelUpSpells()
        {
            const int maxLevel = LevelUpManager.MAX_LEVEL;
            var expAmount = LevelUpManager.GetExpForLevel(maxLevel);

            var fooSpell = new Spell("Foo", MagicType.None, SpellType.Attack, TargetType.SingleEnemy, 10, 40);
            _fighter.AddSpell(fooSpell);
            var barSpell = new Spell("Bar", MagicType.None, SpellType.Attack, TargetType.SingleEnemy, 5, 10);
            _fighter.AddSpell(barSpell);

            _fighter.GainExp(expAmount);

            Assert.AreEqual(expAmount, _fighter.CurrentExp);
            Assert.AreEqual(maxLevel, _fighter.Level);

            var spells = GetSpellsByLevel(maxLevel);
            Assert.AreEqual(spells.Count + 2, _fighter.Spells.Count);
            Assert.IsTrue(spells.TrueForAll(s => _fighter.Spells.SingleOrDefault(fs => fs.Description == s.Description) != null));
            Assert.IsTrue(_fighter.Spells.SingleOrDefault(s => s.Description == fooSpell.Description) != null);
            Assert.IsTrue(_fighter.Spells.SingleOrDefault(s => s.Description == barSpell.Description) != null);
        }

        [Test]
        public void AllSpecialMoves_CorrectlyCombinesSpecialMoves_AndExtraSpecialMoveLists()
        {
            Assert.AreEqual(0, _fighter.SpecialMoves.Count);
            Assert.AreEqual(0, _fighter.AllSpecialMoves.Count);

            BattleMove doNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);
            BattleMove shieldBusterMove = MoveFactory.Get(BattleMoveType.ShieldBuster);

            _fighter.AddMove(doNothingMove); //will be added to "extra special move" property
            _fighter.AddMove(shieldBusterMove); //will be added to "special move" property

            Assert.AreEqual(1, _fighter.SpecialMoves.Count);
            Assert.Contains(shieldBusterMove, _fighter.SpecialMoves);

            Assert.AreEqual(2, _fighter.AllSpecialMoves.Count);
            Assert.Contains(doNothingMove, _fighter.AllSpecialMoves);
            Assert.Contains(shieldBusterMove, _fighter.AllSpecialMoves);
        }
    }
}
