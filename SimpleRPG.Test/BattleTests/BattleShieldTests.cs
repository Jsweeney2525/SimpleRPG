using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests
{
    [TestFixture]
    class BattleShieldTests
    {
        private TestHumanFighter _humanPlayer1, _humanPlayer2;
        private Team _humanTeam;
        private TestEnemyFighter _enemyPlayer1, _enemyPlayer2;
        private Team _enemyTeam;

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private EventLogger _logger;
        private MockChanceService _chanceService;

        private BattleManager _battleManager;

        private const int ShieldMagicResistance = 2;
        private IBattleShield _shield;

        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();

            _battleManager = new BattleManager(_chanceService, _input, _output);
            
            _humanPlayer1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _humanPlayer2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jed");

            _enemyPlayer1 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _enemyPlayer2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");

            _humanTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1, _enemyPlayer2);

            _shield = new ElementalBattleShield(5, 0, ShieldMagicResistance, MagicType.None);
            _logger.SubscribeAll(_shield);
        }

        #region .IncrementHealth() method

        private IronBattleShield IncrementHealth_Setup(int maxHealth, int damageAmount, int healAmount, out int returnAmount)
        {
            IronBattleShield ironShield = new IronBattleShield(maxHealth, 0, 0);
            _logger.Subscribe(EventType.ShieldHealed, ironShield);

            ironShield.DecrementHealth(damageAmount);
            returnAmount = ironShield.IncrementHealth(healAmount);

            return ironShield;
        }

        [Test]
        public void IncrementHealth_ProperlyHeals()
        {
            int returnAmount;
            IronBattleShield ironShield = IncrementHealth_Setup(10, 2, 1, out returnAmount);

            Assert.AreEqual(9, ironShield.CurrentHealth);
        }

        [Test]
        public void IncrementHealth_DoesNotExceedMaxHealth()
        {
            int returnAmount;
            IronBattleShield ironShield = IncrementHealth_Setup(10, 2, 7, out returnAmount);

            Assert.AreEqual(10, ironShield.CurrentHealth);
        }

        [Test]
        public void IncrementHealth_ReturnsAppropriateAmount([Values(1, 50)] int healAmount)
        {
            int returnAmount;
            int damageAmount = 9;
            IncrementHealth_Setup(10, 9, healAmount, out returnAmount);

            int expectedReturnAmount = healAmount <= damageAmount ? healAmount : damageAmount;
            Assert.AreEqual(expectedReturnAmount, returnAmount);
        }

        [Test]
        public void IncrementHealth_FiresHealedEvent([Values(1, 2)] int healAmount)
        {
            int returnAmount;
            IronBattleShield ironShield = IncrementHealth_Setup(10, healAmount + 1, healAmount, out returnAmount);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            EventLog log = logs[0];

            Assert.AreEqual(EventType.ShieldHealed, log.Type);
            Assert.AreEqual(ironShield, log.Sender);

            ShieldHealedEventArgs e = log.E as ShieldHealedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(healAmount, e.HealedAmount);
        }

        [Test]
        public void IncrementHealth_ThrowsException_NegativeHealAmount()
        {
            int returnAmount;
            Assert.Throws<ArgumentException>(() => IncrementHealth_Setup(10, 1, -1, out returnAmount));
        }

        [Test]
        public void IncrementHealth_ThrowsException_ShieldHasNoHealth()
        {
            int returnAmount;
            Assert.Throws<InvalidOperationException>(() => IncrementHealth_Setup(10, 10, 1, out returnAmount));
        }

        #endregion .IncrementHealth() method

        #region .FortifyDefense() method

        [Test]
        public void FortifyDefense_ProperlyRaisesDefense([Values(1, 3)] int fortifyAmount)
        {
            IronBattleShield shield = new IronBattleShield(5, 0, 0);

            shield.FortifyDefense(fortifyAmount);

            Assert.AreEqual(fortifyAmount, shield.Defense);
        }

        [Test]
        public void FortifyDefense_ThrowsFortifyEvent([Values(1, 3)] int fortifyAmount)
        {
            IronBattleShield shield = new IronBattleShield(5, 0, 0);

            _logger.Subscribe(EventType.ShieldFortified, shield);
            shield.FortifyDefense(fortifyAmount);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            ShieldFortifiedEventArgs e = logs[0].E as ShieldFortifiedEventArgs;
            
            Assert.NotNull(e);
            Assert.AreEqual(fortifyAmount, e.FortifyAmount);
        }

        #endregion

        [Test]
        public void OwnerPropertySet_WhenSetToPlayer()
        {
            BattleShield shield = new IronBattleShield(1, 0, 0);

            Assert.AreEqual(null, shield.Owner);

            _humanPlayer1.SetBattleShield(shield);

            Assert.AreEqual(null, shield.Owner);
            Assert.AreEqual(_humanPlayer1, _humanPlayer1.BattleShield.Owner);
        }

        [Test]
        public void BattleManager_CorrectlyPrintsDamageOutput()
        {
            int damage = (_shield.MaxHealth + _shield.Defense) - 1;

            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetStrength(damage);
            _enemyPlayer1.SetMoveTarget(_humanPlayer1);
            _chanceService.PushEventsOccur(true, false); //attack hits, not crit

            _humanPlayer1.SetBattleShield(_shield as BattleShield);
            BattleShield fighterShield = _humanPlayer1.BattleShield;
            _humanPlayer1.SetMove(_doNothingMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Length); //enemy attacks and shield took damage message

            MockOutputMessage output = outputs[1];
            Assert.AreEqual($"{fighterShield.Owner.DisplayName}'s {fighterShield.GetDisplayText(false)} took {damage} damage!\n", output.Message);
        }

        [Test]
        public void BattleManager_CorrectlyCalculatesDamageForShield()
        {
            int shieldDefense = 10;

            IronBattleShield shield = new IronBattleShield((shieldDefense * 2) + 1, shieldDefense, 0);
            _humanPlayer1.SetBattleShield(shield);

            _logger.SubscribeAll(_humanPlayer1.BattleShield);

            _humanPlayer1.SetDefense(shieldDefense);
            _humanPlayer1.SetStrength(_enemyPlayer1.MaxHealth + _enemyPlayer1.Defense);
            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);

            _enemyPlayer1.SetSpeed(1);
            _enemyPlayer1.SetStrength(shieldDefense * 2);
            _enemyPlayer1.SetMove(_basicAttackMove);

            _chanceService.PushEventsOccur(true, false, true, false); //attacks hit, not misses

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            PhysicalDamageTakenEventArgs e = logs[0].E as PhysicalDamageTakenEventArgs;
            
            Assert.NotNull(e);
            Assert.AreEqual(shieldDefense, e.Damage);
        }

        [Test]
        public void BattleManager_CorrectlyCalculatesMagicalDamageForShield([Values(MagicType.Fire, MagicType.Ice)] MagicType spellMagicType)
        {
            int shieldResistance = 10;

            IronBattleShield shield = new IronBattleShield((shieldResistance * 2) + 1, 0, shieldResistance);
            _humanPlayer1.SetBattleShield(shield);

            _logger.SubscribeAll(_humanPlayer1.BattleShield);

            _humanPlayer1.SetMagicResistance(shieldResistance);
            _humanPlayer1.SetStrength(_enemyPlayer1.MaxHealth + _enemyPlayer1.Defense);
            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);

            _enemyPlayer1.SetSpeed(1);
            _enemyPlayer1.SetMagicStrength(shieldResistance * 2);
            Spell spell = new Spell("foo", spellMagicType, SpellType.Attack, TargetType.SingleEnemy, 0, 0);
            _enemyPlayer1.AddSpell(spell);
            _enemyPlayer1.SetMove(spell);

            _chanceService.PushEventsOccur(true, false); //attack hits, not misses

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            MagicalDamageTakenEventArgs e = logs[0].E as MagicalDamageTakenEventArgs;

            Assert.NotNull(e);
            Assert.AreEqual(shieldResistance, e.Damage);
            Assert.AreEqual(spellMagicType, e.MagicType);
        }

        [Test]
        public void RemovedFromPlayer_WhenDestroyed_PhysicalDamage()
        {
            _humanPlayer1.SetBattleShield(_shield as BattleShield);
            IBattleShield shield = _humanPlayer1.BattleShield;
            _logger.SubscribeAll(shield);

            Assert.That(_humanPlayer1.BattleShield, Is.Not.Null);

            _humanPlayer1.PhysicalDamage(_shield.MaxHealth);
            
            Assert.That(_humanPlayer1.BattleShield, Is.Null);
            Assert.AreEqual(_humanPlayer1.MaxHealth, _humanPlayer1.CurrentHealth);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count); //damage taken and destroyed
            var log = logs[1];

            Assert.AreEqual(EventType.ShieldDestroyed, log.Type);
            Assert.IsInstanceOf<ShieldDestroyedEventArgs>(log.E);
            Assert.AreEqual(shield, log.Sender);
        }

        [Test]
        public void RemovedFromPlayer_WhenDestroyed_MagicalDamage()
        {
            _humanPlayer1.SetBattleShield(_shield as BattleShield);
            IBattleShield shield = _humanPlayer1.BattleShield;
            _logger.SubscribeAll(shield);

            Assert.That(_humanPlayer1.BattleShield, Is.Not.Null);

            _humanPlayer1.MagicalDamage(_shield.MaxHealth, MagicType.Fire);

            Assert.That(_humanPlayer1.BattleShield, Is.Null);
            Assert.AreEqual(_humanPlayer1.MaxHealth, _humanPlayer1.CurrentHealth);
        }

        [Test]
        public void ExcessDamage_NotTransferredToPlayer()
        {
            _humanPlayer1.SetBattleShield(_shield as BattleShield);

            var maxDamage = (_shield.MaxHealth + _shield.Defense);
            _humanPlayer1.PhysicalDamage(maxDamage * 2);
            
            Assert.That(_humanPlayer1.BattleShield, Is.Null);
            Assert.AreEqual(_humanPlayer1.MaxHealth, _humanPlayer1.CurrentHealth);
        }

        [Test]
        public void DecrementHealthMethod_ThrowsErrorIfGivenNegativeArgument([Values(-1, -5)] int damage)
        {
            Assert.Throws<ArgumentException>(() => _shield.DecrementHealth(damage));
        }

        #region events

        [Test]
        public void FiresProperEvents_PhysicalDamage_NotDestroyed()
        {
            int damage = (_shield.MaxHealth + _shield.Defense) - 1;
            _shield.DecrementHealth(damage);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count); //damage taken
            var log = logs[0];

            Assert.AreEqual(EventType.DamageTaken, log.Type);
            Assert.IsInstanceOf<PhysicalDamageTakenEventArgs>(log.E);
            Assert.AreEqual(_shield, log.Sender);
        }

        [Test]
        public void FiresProperEvents_PhysicalDamage_Destroyed()
        {
            int damage = (_shield.MaxHealth + _shield.Defense) + 1;
            _shield.DecrementHealth(damage);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count); //damage taken, destroyed
            var log = logs[0];

            Assert.AreEqual(EventType.DamageTaken, log.Type);
            Assert.IsInstanceOf<PhysicalDamageTakenEventArgs>(log.E);
            Assert.AreEqual(_shield, log.Sender);

            log = logs[1];

            Assert.AreEqual(EventType.ShieldDestroyed, log.Type);
            Assert.IsInstanceOf<ShieldDestroyedEventArgs>(log.E);
            Assert.AreEqual(_shield, log.Sender);
        }

        [Test]
        public void FiresProperEvents_MagicalDamage_NotDestroyed()
        {
            int damage = _shield.MaxHealth - 1;

            _shield.DecrementHealth(damage, MagicType.Fire);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count); //damage taken
            var log = logs[0];

            Assert.AreEqual(EventType.MagicalDamageTaken, log.Type);

            MagicalDamageTakenEventArgs e = log.E as MagicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(damage, e.Damage);
            Assert.AreEqual(_shield, log.Sender);
        }

        [Test]
        public void FiresDestroyedEvent_MagicalDamage_Destroyed()
        {
            _shield.DecrementHealth(_shield.MaxHealth, MagicType.Ice);

            Assert.AreEqual(0, _shield.CurrentHealth);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count); //damage taken and destroyed
            var log = logs[1];

            Assert.AreEqual(EventType.ShieldDestroyed, log.Type);
            Assert.IsInstanceOf<ShieldDestroyedEventArgs>(log.E);
            Assert.AreEqual(_shield, log.Sender);
        }

        [Test]
        public void BattleManager_CorrectlySubscribesToAddedShieldsEvents()
        {
            BattleShield shield = new IronBattleShield(1, 0, 0);
            ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, null, shield);

            _humanPlayer1.SetMove(shieldMove, 1);
            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);
            _humanPlayer1.SetSpeed(1);

            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetStrength(1000);
            _enemyPlayer1.SetMoveTarget(_humanPlayer1);
            _chanceService.PushEventsOccur(true, false, true, false); //two attacks to end the battle, both hit, neither are crits

            _logger.Subscribe(EventType.ShieldAdded, _humanPlayer1);

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowDeathMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowShieldAddedMessage = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedOutputCount = 2 * 2 + 1; //"enemy attacks!" and "it did ____ damage" message for both attacks, as well as "shield destroyed!"
            Assert.AreEqual(expectedOutputCount, outputs.Length);
        }

        [Test]
        public void BattleManager_CorrectlyUnsubscribes_OnceShieldRemovedFromPlayer()
        {
            BattleShield shield = new IronBattleShield(1, 0, 0);
            ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, null, shield);

            _humanPlayer1.SetMove(shieldMove, 1);
            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);
            _humanPlayer1.SetSpeed(1);

            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetStrength(1000);
            _enemyPlayer1.SetMoveTarget(_humanPlayer1);
            _chanceService.PushEventsOccur(true, false, true, false); //two attacks to end the battle, both hit, neither are crits

            _logger.Subscribe(EventType.ShieldAdded, _humanPlayer1);

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int outputCountBefore = _output.GetOutputs().Length;

            ShieldAddedEventArgs e = _logger.Logs[0].E as ShieldAddedEventArgs;
            Assert.NotNull(e);

            BattleShield shieldCopy = e.BattleShield;

            shieldCopy.OnDamageTaken(new PhysicalDamageTakenEventArgs(7));

            int outputCountAfter = _output.GetOutputs().Length;

            Assert.AreEqual(outputCountBefore, outputCountAfter);
        }

        #endregion

        [Test]
        public void DefaultDisplayName_ElementalBattleShield([Values(MagicType.Earth, MagicType.Fire)] MagicType shieldElementalType)
        {
            ElementalBattleShield shield = new ElementalBattleShield(1, 0, 0, shieldElementalType);

            string displayText = shield.GetDisplayText();
            string expectedText = $"{shieldElementalType.ToString().ToLower()} elemental battle shield";

            if (shieldElementalType == MagicType.Earth || shieldElementalType == MagicType.Ice)
            {
                expectedText = $"an {expectedText}";
            }
            else
            {
                expectedText = $"a {expectedText}";
            }

            Assert.AreEqual(expectedText, displayText);
        }

        [Test]
        public void DefaultDisplayName_IronBattleShield()
        {
            IronBattleShield shield = new IronBattleShield(1, 0, 0);

            string displayText = shield.GetDisplayText();
            string expectedText = "an iron battle shield";

            Assert.AreEqual(expectedText, displayText);
        }
    }
}
