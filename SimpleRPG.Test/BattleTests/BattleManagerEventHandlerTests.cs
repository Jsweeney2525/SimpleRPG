using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests
{
    [TestFixture]
    class BattleManagerEventHandlerTests
    {
        private TestHumanFighter _humanPlayer1, _humanPlayer2;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemyPlayer1, _enemyPlayer2;
        private Team _enemyTeam;

        private readonly Spell _fireball = SpellFactory.GetSpell(MagicType.Fire, 1);
        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _mockChanceService;
        private BattleManager _battleManager;

        private EventLogger _logger;

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _logger = new EventLogger();
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);

            _battleManager = new BattleManager(_mockChanceService, _input, _output);

            _humanPlayer1 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _humanPlayer2 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jed");

            _humanPlayer2.AddSpell(_fireball);
            _humanPlayer2.SetMana(_fireball.Cost);

            _humanPlayer2.TurnEnded += TurnEndedEvents.RestoreManaOnTurnEnd;

            _enemyPlayer1 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _enemyPlayer2 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");

            _humanTeam = new TestTeam(_menuManager, _humanPlayer1, _humanPlayer2);

            _enemyTeam = new Team(_menuManager, _enemyPlayer1, _enemyPlayer2);
        }

        private void PhysicalDamageEvent_Setup(int expectedDamage, bool showPhysicalDamageMessages)
        {
            Team mixedPlayerTeam = new Team(_menuManager, _humanPlayer1, _enemyPlayer1);
            TestEnemyFighter enemy3 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team enemyTeam = new Team(_menuManager, _enemyPlayer2, enemy3);

            _humanPlayer1.SetSpeed(2);
            _humanPlayer1.SetStrength(expectedDamage);
            _humanPlayer1.SetMove(_basicAttackMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer2);
            _enemyPlayer2.SetHealth(expectedDamage + 1);

            _enemyPlayer1.SetSpeed(1);
            _enemyPlayer1.SetStrength(expectedDamage);
            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetMoveTarget(enemy3);
            enemy3.SetHealth(expectedDamage + 1);

            _mockChanceService.PushEventsOccur(true, false, true, false); //both attacks hit, neither are crits
            _logger.Subscribe(EventType.DamageTaken, _enemyPlayer2);
            _logger.Subscribe(EventType.DamageTaken, enemy3);

            _enemyPlayer2.SetMove(_doNothingMove);
            enemy3.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowDeathMessages = false,
                ShowAttackMessages = false,
                ShowPhysicalDamageMessages = showPhysicalDamageMessages
            };

            _battleManager.Battle(mixedPlayerTeam, enemyTeam, config: config);
        }

        [Test]
        public void PhysicalDamageEvent_CorrectlyPrintsMessage([Values(2, 4)] int expectedDamage)
        {
            PhysicalDamageEvent_Setup(expectedDamage, true);

            Assert.AreEqual(2, _logger.Logs.Count);

            MockOutputMessage[] messages = _output.GetOutputs();

            Assert.AreEqual(2, messages.Length);

            Assert.AreEqual($"It did {expectedDamage} damage!\n", messages[0].Message);
            Assert.AreEqual($"It did {expectedDamage} damage!\n", messages[1].Message);
        }

        [Test]
        public void PhysicalDamageEvent_CorrectlySuppressesMessage_ShowMessageConfigValueSetToFalse(
            [Values(2, 4)] int expectedDamage)
        {
            PhysicalDamageEvent_Setup(expectedDamage, false);

            Assert.AreEqual(2, _logger.Logs.Count);

            MockOutputMessage[] messages = _output.GetOutputs();

            Assert.AreEqual(0, messages.Length);
        }

        private void CriticalHitEvent_Setup(bool showAttackMessages)
        {
            Team mixedPlayerTeam = new Team(_menuManager, _humanPlayer1, _enemyPlayer1);
            TestEnemyFighter enemy3 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team enemyTeam = new Team(_menuManager, _enemyPlayer2, enemy3);

            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer2);
            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetMoveTarget(enemy3);
            _mockChanceService.PushEventsOccur(true, true, true, true); //both attacks hit, both are crits
            _logger.Subscribe(EventType.CriticalAttack, _humanPlayer1);
            _logger.Subscribe(EventType.CriticalAttack, _enemyPlayer1);

            _enemyPlayer2.SetMove(_doNothingMove);
            enemy3.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = false,
                ShowAttackMessages = showAttackMessages
            };

            _battleManager.Battle(mixedPlayerTeam, enemyTeam, config: config);
        }

        [Test]
        public void CriticalHitEvent_CorrectlyPrintsMessage()
        {
            CriticalHitEvent_Setup(true);

            Assert.AreEqual(2, _logger.Logs.Count);

            MockOutputMessage[] messages = _output.GetOutputs();

            Assert.AreEqual(4, messages.Length);
            //both "A attacked B" and critical hit messages, they are currently coupled to the same config value

            Assert.AreEqual("Critical hit!\n", messages[1].Message);
            Assert.AreEqual("Critical hit!\n", messages[3].Message);
        }

        [Test]
        public void CriticalHitEvent_CorrectlySuppressesMessage_ShowMessageConfigValueSetToFalse()
        {
            CriticalHitEvent_Setup(false);

            Assert.AreEqual(2, _logger.Logs.Count);

            MockOutputMessage[] messages = _output.GetOutputs();

            Assert.AreEqual(0, messages.Length);
        }

        private void KilledEvent_Setup(bool showDeathMessages, out TestEnemyFighter enemy3)
        {
            Team mixedPlayerTeam = new Team(_menuManager, _humanPlayer1, _enemyPlayer1);
            enemy3 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team enemyTeam = new Team(_menuManager, _enemyPlayer2, enemy3);

            _humanPlayer1.SetSpeed(2);
            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer2);
            _enemyPlayer1.SetSpeed(1);
            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetMoveTarget(enemy3);
            _mockChanceService.PushEventsOccur(true, false, true, false); //both attacks hit, neither are crits
            _logger.Subscribe(EventType.Killed, _enemyPlayer2);
            _logger.Subscribe(EventType.Killed, enemy3);

            _enemyPlayer2.SetMove(_doNothingMove);
            enemy3.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = showDeathMessages,
                ShowAttackMessages = false
            };

            _battleManager.Battle(mixedPlayerTeam, enemyTeam, config: config);
        }

        [Test]
        public void KilledEvent_CorrectlyPrintsMessage()
        {
            TestEnemyFighter enemy3;
            KilledEvent_Setup(true, out enemy3);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);
            Assert.AreEqual(_enemyPlayer2, logs[0].Sender);
            Assert.AreEqual(enemy3, logs[1].Sender);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Length);

            Assert.AreEqual($"{_enemyPlayer2.DisplayName} has been defeated!\n", outputs[0].Message);
            Assert.AreEqual($"{enemy3.DisplayName} has been defeated!\n", outputs[1].Message);
        }

        [Test]
        public void KilledEvent_CorrectlySuppressesMessage_ShowMessageConfigValueSetToFalse()
        {
            TestEnemyFighter enemy3;
            KilledEvent_Setup(false, out enemy3);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);
            Assert.AreEqual(_enemyPlayer2, logs[0].Sender);
            Assert.AreEqual(enemy3, logs[1].Sender);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void KilledEvent_CorrectlyThrowsException_BattleManagerSubscribestoNonFighrerEntity()
        {
            TestBattleManager testBattleManager = new TestBattleManager(_mockChanceService, _input, _output);
            testBattleManager.SetConfig(new BattleManagerBattleConfiguration());

            Assert.Throws<InvalidOperationException>(
                () => testBattleManager.TestPrintKilledMessage(_input, new KilledEventArgs()));
        }

        private void MissedEvent_Setup(bool showAttackMessages)
        {
            Team mixedPlayerTeam = new Team(_menuManager, _humanPlayer1, _enemyPlayer1);
            TestEnemyFighter enemy3 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team enemyTeam = new Team(_menuManager, _enemyPlayer2, enemy3);

            _humanPlayer1.SetSpeed(2);
            _humanPlayer1.SetMove(_basicAttackMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer2);
            _enemyPlayer1.SetSpeed(1);
            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetMoveTarget(enemy3);
            _mockChanceService.PushEventsOccur(false, false); //both attacks miss
            _logger.Subscribe(EventType.AttackMissed, _humanPlayer1);
            _logger.Subscribe(EventType.AttackMissed, _enemyPlayer1);

            _enemyPlayer2.SetMove(_doNothingMove);
            enemy3.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowAttackMessages = showAttackMessages
            };

            _battleManager.Battle(mixedPlayerTeam, enemyTeam, config: config);
        }

        [Test]
        public void MissedEvent_CorrectlyPrintsMessage()
        {
            MissedEvent_Setup(true);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);
            Assert.AreEqual(EventType.AttackMissed, logs[0].Type);
            Assert.AreEqual(_humanPlayer1, logs[0].Sender);
            Assert.AreEqual(EventType.AttackMissed, logs[1].Type);
            Assert.AreEqual(_enemyPlayer1, logs[1].Sender);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(4, outputs.Length); //will have both "A attacked B" and "it missed" messages for each attack

            Assert.AreEqual("But it missed!\n", outputs[1].Message);
            Assert.AreEqual("But it missed!\n", outputs[3].Message);
        }

        [Test]
        public void MissedEvent_CorrectlySuppressesMessage_ShowMessageConfigValueSetToFalse()
        {
            MissedEvent_Setup(false);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);
            Assert.AreEqual(EventType.AttackMissed, logs[0].Type);
            Assert.AreEqual(_humanPlayer1, logs[0].Sender);
            Assert.AreEqual(EventType.AttackMissed, logs[1].Type);
            Assert.AreEqual(_enemyPlayer1, logs[1].Sender);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void AttackEvents_OccurInCorrectOrder()
        {
            _logger.Subscribe(_humanPlayer1, EventType.CriticalAttack, EventType.AttackSuccessful, EventType.EnemyKilled);
            _logger.Subscribe(_enemyPlayer1, EventType.DamageTaken, EventType.Killed);

            _humanTeam = new TestTeam(TestMenuManager.GetTestMenuManager(), _humanPlayer1);
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1);

            _enemyPlayer1.SetMove(_doNothingMove);

            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);
            _mockChanceService.PushEventsOccur(true, true); //attack hits, and is a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(5, logs.Count);

            int i = 0;
            Assert.AreEqual(EventType.CriticalAttack, logs[i++].Type);
            Assert.AreEqual(EventType.DamageTaken, logs[i++].Type);
            Assert.AreEqual(EventType.Killed, logs[i++].Type);
            Assert.AreEqual(EventType.AttackSuccessful, logs[i++].Type);
            Assert.AreEqual(EventType.EnemyKilled, logs[i].Type);
        }

        private void SpecialAttackExecutedEvent_Setup(string executionText)
        {
            StatusMove moveToExecute = new StatusMove("foo", TargetType.Self, new AutoEvadeStatus(1, false),
                executionText);

            _logger.Subscribe(_humanPlayer1, EventType.StatusAdded, EventType.SpecialMoveExecuted);

            _humanPlayer1.SetMove(moveToExecute, 1);
            _mockChanceService.PushEventOccurs(true); //status move hits
            _humanPlayer1.SetMove(_runawayMove);

            _enemyPlayer1.SetMove(_doNothingMove);


            _humanTeam = new TestTeam(_humanPlayer1);

            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        [Test]
        public void SpecialAttackExecutedEvent_CorrectlyPrintsMessage()
        {
            string executionText = "jumps up and down";
            SpecialAttackExecutedEvent_Setup(executionText);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            Assert.AreEqual(EventType.SpecialMoveExecuted, logs[0].Type);
            Assert.AreEqual(EventType.StatusAdded, logs[1].Type);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Length); //1 for special move execution, one for status added

            string expectedText = $"{_humanPlayer1.DisplayName} {executionText}!\n";

            Assert.AreEqual(expectedText, outputs[0].Message);
        }

        [Test]
        public void SpecialAttackExecutedEvent_CorrectlySuppressesMessage_NoExecutionText()
        {
            SpecialAttackExecutedEvent_Setup(null);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            Assert.AreEqual(EventType.SpecialMoveExecuted, logs[0].Type);
            Assert.AreEqual(EventType.StatusAdded, logs[1].Type);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length); //one for status added

            string expectedText = $"{_humanPlayer1.DisplayName} will evade all attacks for 1 turn!\n";

            Assert.AreEqual(expectedText, outputs[0].Message);
        }

        [Test]
        public void NewlyAddedEnemyTeam_CorrectlyWiredIntoEvents()
        {
            //Assert
            _enemyTeam = new TestTeam(_enemyPlayer1);
            _enemyPlayer1.SetMove(_doNothingMove, 2);
            _enemyPlayer1.SetMove(_runawayMove);
            _enemyPlayer2.SetMove(_doNothingMove);

            bool alreadyAdded = false;
            _enemyTeam.RoundEnded += delegate(object sender, RoundEndedEventArgs args) 
            {
                if (!alreadyAdded) {
                    args.Team.Add(_enemyPlayer2);
                    alreadyAdded = true;
                }
            };

            _humanPlayer1.SetMove(_doNothingMove, 1);
            _humanPlayer1.SetMove(_basicAttackMove, 1);
            _humanPlayer1.SetMove(_doNothingMove);
            _mockChanceService.PushAttackHitsNotCrit();
            _humanPlayer1.SetMoveTarget(_enemyPlayer2);

            _humanPlayer2.SetMove(_doNothingMove);
            _humanPlayer2.SetMoveTarget(_humanPlayer2);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowExpAndLevelUpMessages = false,
                ShowIntroAndOutroMessages = false
            };

            //Act
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            //Assert
            MockOutputMessage output = _output.GetOutputs()[1];
            Assert.AreEqual($"It did 1 damage!\n", output.Message);
        }

        [Test]
        public void MessageDisplayed_OnFighterHealed([Values(5, 10, 25)] int healAmount, [Values] bool shouldTryToOverheal)
        {
            //arrange
            _humanPlayer1.SetHealth(100, shouldTryToOverheal ? 99 : 1);
            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer2.SetMove(_doNothingMove);

            _humanTeam.SetDeathsOnRoundEndEvent();

            _enemyPlayer1.SetMove(_doNothingMove);
            _enemyPlayer2.SetMove(_doNothingMove);
            
            _humanPlayer1.TurnEnded += delegate { _humanPlayer1.Heal(healAmount); };

            //Act
            _battleManager.Battle(_humanTeam, _enemyTeam);

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedHealAmount = shouldTryToOverheal ? 1 : healAmount;
            string expectedMessage = $"{_humanPlayer1.DisplayName} was healed for {expectedHealAmount} HP!\n";
            MockOutputMessage output = outputs.FirstOrDefault(o => o.Message == expectedMessage);

            Assert.NotNull(output);
        }

        [Test]
        public void MessageDisplayed_OnFighterFullyHealed()
        {
            //arrange
            _humanPlayer1.SetHealth(100, 1);
            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer2.SetMove(_doNothingMove);

            _humanTeam.SetDeathsOnRoundEndEvent();

            _enemyPlayer1.SetMove(_doNothingMove);
            _enemyPlayer2.SetMove(_doNothingMove);

            _humanPlayer1.TurnEnded += delegate { _humanPlayer1.FullyHeal(); };

            //Act
            _battleManager.Battle(_humanTeam, _enemyTeam);

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();
            
            string expectedMessage = $"{_humanPlayer1.DisplayName}'s HP was fully restored!\n";
            MockOutputMessage output = outputs.FirstOrDefault(o => o.Message == expectedMessage);

            Assert.NotNull(output);
        }

        [Test]
        public void MessageDisplayed_OnStatIncrease([Values] StatType raisedStat, [Range(1,5)] int boostAmount)
        {
            //arrange
            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer2.SetMove(_doNothingMove);

            _humanTeam.SetDeathsOnRoundEndEvent();

            _enemyPlayer1.SetMove(_doNothingMove);
            _enemyPlayer2.SetMove(_doNothingMove);

            StatRaisedEventArgs e = new StatRaisedEventArgs(raisedStat, boostAmount);
            _humanPlayer1.TurnEnded += delegate { _humanPlayer1.OnStatRaised(e); };

            //Act
            _battleManager.Battle(_humanTeam, _enemyTeam);

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            string statTypeString = raisedStat == StatType.Evade ? "evasion" : raisedStat.ToString().ToLower();

            string expectedMessage = $"{_humanPlayer1.DisplayName}'s {statTypeString} was raised by {boostAmount}!\n";
            MockOutputMessage output = outputs.FirstOrDefault(o => o.Message == expectedMessage);

            Assert.NotNull(output);
        }
    }
}
