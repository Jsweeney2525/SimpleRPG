using System;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class BasicFighterTests
    {
        private TestHumanFighter _factoryFighter;
        private TestHumanFighter _fighter;
        private TestEnemyFighter _enemy;
        private TestEnemyFighter _armoredEnemy;
        private TestEnemyFighter _superArmoredEnemy;

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        private const int FighterHealth = 5;
        private const int FighterMana = 5;
        private const int FighterAttack = 3;
        private const int FighterDefense = 1;
        private const int FighterSpeed = 2;
        private const int FighterEvade = 0;
        private const int FighterLuck = 5;

        [SetUp]
        public void Setup()
        {
            _fighter = new TestHumanFighter("Hero", 1);
            _fighter.SetHealth(FighterHealth);
            _fighter.SetMana(FighterMana);
            _fighter.SetStrength(FighterAttack);
            _fighter.SetDefense(FighterDefense);
            _fighter.SetSpeed(FighterSpeed);
            _fighter.SetEvade(FighterEvade);
            _fighter.SetLuck(FighterLuck);

            _factoryFighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _factoryFighter.AddSpell(SpellFactory.GetSpell(MagicType.Fire, 1));
            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "enemy");
            _armoredEnemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "armored");
            _superArmoredEnemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "super armored");

            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();

            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            var humanTeam = new Team(_menuManager, _fighter);
            _battleManager.SetHumanTeam(humanTeam);

            var enemyTeam = new Team(_menuManager, _enemy);
            _battleManager.SetEnemyTeam(enemyTeam);

            _battleManager.SetConfig(new BattleManagerBattleConfiguration());
        }

        [Test]
        public void ConstructorAppropriatelyInitializesCurrentHealth()
        {
            Assert.AreEqual(FighterHealth, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);
        }

        [Test]
        public void ConstructorAppropriatelyInitializesCurrentMana()
        {
            Assert.AreEqual(FighterMana, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);
        }

        [Test]
        public void PhysicalDamageMethod_AppropriatelyAltersCurrentHealth()
        {
            const int damage = FighterHealth - 1;
            var ret = _fighter.PhysicalDamage(damage);

            Assert.AreEqual(1, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(damage, ret);
        }

        [Test]
        public void PhysicalDamageMethod_AppropriatelySetsCurrentHealthToZeroIfDamageExceedsCurrentHealth()
        {
            const int damage = FighterHealth + 1;
            var ret = _fighter.PhysicalDamage(damage);

            Assert.AreEqual(0, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(FighterHealth, ret);
        }

        [Test]
        public void PhysicalDamageMethod_AppropriatelyThrowsError_NegativeDamageAmount([Values(-2, -8)] int damageAmount)
        {
            Assert.Throws<ArgumentException>(() => _fighter.PhysicalDamage(damageAmount));
        }

        [Test]
        public void HealMethod_AppropriatelyAltersCurrentHealth()
        {
            const int damage = FighterHealth - 1;
            _fighter.PhysicalDamage(damage);
            var ret = _fighter.Heal(damage - 2);

            Assert.AreEqual(FighterHealth - 2, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(2, ret);
        }

        [Test]
        public void HealMethod_AppropriatelySetsCurrentHealthToMaxHealthIfHealingExceedsMaxHealth()
        {
            _fighter.PhysicalDamage(1);
            var ret = _fighter.Heal(5);

            Assert.AreEqual(FighterHealth, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(1, ret);
        }

        [Test]
        public void HealMethod_DoesNotHealDefeatedPlayer()
        {
            _fighter.PhysicalDamage(_fighter.MaxHealth);
            var ret = _fighter.Heal(1);

            Assert.AreEqual(0, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(0, ret);
        }

        [Test]
        public void SpendManaMethod_AppropriatelyAltersCurrentMana()
        {
            const int manaSpent = FighterMana - 1;
            var ret = _fighter.SpendMana(manaSpent);

            Assert.AreEqual(1, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);

            Assert.AreEqual(manaSpent, ret);
        }

        [Test]
        public void SpendManaMethod_AppropriatelyRaisesException_IfCostExceedsCurrentMana()
        {
            const int manaSpent = FighterMana + 1;
            Assert.Throws<ArgumentException>(() => _fighter.SpendMana(manaSpent));
        }

        [Test]
        public void DainManaMethod_AppropriatelyAltersCurrentMana()
        {
            const int manaSpent = FighterMana - 1;
            var ret = _fighter.DrainMana(manaSpent);

            Assert.AreEqual(1, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);

            Assert.AreEqual(manaSpent, ret);
        }

        [Test]
        public void DrainManaMethod_AppropriatelySetsCurrentManaToZero_IfCostExceedsCurrentMana()
        {
            const int manaSpent = FighterMana + 2;
            var ret = _fighter.DrainMana(manaSpent);

            Assert.AreEqual(0, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);

            Assert.AreEqual(FighterMana, ret);
        }

        [Test]
        public void RestoreManaMethod_AppropriatelyAltersCurrentMana()
        {

            const int manaSpent = FighterMana - 1;
            _fighter.DrainMana(manaSpent);
            var ret = _fighter.RestoreMana(manaSpent - 2);
        
            Assert.AreEqual(FighterMana - 2, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);
        
            Assert.AreEqual(2, ret);
        }
        
        [Test]
        public void RestoreManaMethod_AppropriatelyLimitsToMaxMana()
        {
            _fighter.DrainMana(1);
            var ret = _fighter.RestoreMana(5);
        
            Assert.AreEqual(FighterMana, _fighter.CurrentMana);
            Assert.AreEqual(FighterMana, _fighter.MaxMana);
        
            Assert.AreEqual(1, ret);
        }

        [Test]
        public void MagicalDamageMethod_AppropriatelyAltersCurrentHealth([Values(1, 3)] int remainingHealth)
        {
            int damage = FighterHealth - remainingHealth;
            var ret = _fighter.MagicalDamage(damage, MagicType.Lightning);

            Assert.AreEqual(remainingHealth, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(damage, ret);
        }

        [Test]
        public void MagicalDamageMethod_AppropriatelySetsCurrentHealthToZeroIfDamageExceedsCurrentHealth()
        {
            const int damage = FighterHealth + 1;
            var ret = _fighter.MagicalDamage(damage, MagicType.Fire);

            Assert.AreEqual(0, _fighter.CurrentHealth);
            Assert.AreEqual(FighterHealth, _fighter.MaxHealth);

            Assert.AreEqual(FighterHealth, ret);
        }

        [Test]
        public void MagicalDamageMethod_AppropriatelyThrowsError_NegativeDamageAmount([Values(-3, -7)] int damageAmount)
        {
            Assert.Throws<ArgumentException>(() => _fighter.MagicalDamage(damageAmount, MagicType.Earth));
        }

        //TODO: add logic to check attack multiplier
        [Test]
        public void AttackMethod_AppropriatelyCorrectlyCalculatesDamage()
        {
            var expectedDamageDone = _factoryFighter.Strength;
            _enemy.SetDefense(0);
            _enemy.SetHealth(_factoryFighter.Strength + 1);
            var damageDone = _factoryFighter.Attack(_enemy);

            Assert.AreEqual(_enemy.MaxHealth - (expectedDamageDone), _enemy.CurrentHealth);
            Assert.AreEqual(_enemy.MaxHealth - damageDone, _enemy.CurrentHealth);

            _armoredEnemy.SetDefense(_factoryFighter.Strength - 1);
            _armoredEnemy.SetHealth(2);
            damageDone = _factoryFighter.Attack(_armoredEnemy);
            expectedDamageDone = 1;

            Assert.AreEqual(_armoredEnemy.MaxHealth - (expectedDamageDone), _armoredEnemy.CurrentHealth);
            Assert.AreEqual(expectedDamageDone, damageDone);

            _superArmoredEnemy.SetDefense(_factoryFighter.Strength + 2);
            damageDone = _factoryFighter.Attack(_superArmoredEnemy);
            expectedDamageDone = 0;

            Assert.AreEqual(_superArmoredEnemy.MaxHealth, _superArmoredEnemy.CurrentHealth);
            Assert.AreEqual(expectedDamageDone, damageDone);
        }

        [Test]
        public void GetStatMethod_ReturnsCorrectStatValue([Values] StatType stat)
        {
            int expectedValue;

            switch (stat)
            {
                case StatType.Strength:
                    expectedValue = _fighter.Strength;
                    break;
                case StatType.Defense:
                    expectedValue = _fighter.Defense;
                    break;
                case StatType.Speed:
                    expectedValue = _fighter.Speed;
                    break;
                case StatType.Evade:
                    expectedValue = _fighter.Evade;
                    break;
                case StatType.Luck:
                    expectedValue = _fighter.Luck;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }

            Assert.AreEqual(expectedValue, _fighter.GetStatValue(stat));
        }

        [Test, Pairwise]
        public void RaiseStatMethod_CorrectlyRaisesStat([Values] StatType statType, [Range(1,4)] int statIncreaseAmount)
        {
            int valueBefore = _fighter.GetStatValue(statType);

            int returnedValue = _fighter.RaiseStat(statType, statIncreaseAmount);
            int valueAfter = _fighter.GetStatValue(statType);

            Assert.AreEqual(valueAfter, returnedValue);

            int valueDiff = valueAfter - valueBefore;

            Assert.AreEqual(statIncreaseAmount, valueDiff);
        }
    }
}
