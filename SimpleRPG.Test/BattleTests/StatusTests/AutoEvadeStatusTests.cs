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

namespace SimpleRPG.Test.BattleTests.StatusTests
{
    [TestFixture]
    public class AutoEvadeStatusTests
    {
        private EventLogger _logger;
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private AutoEvadeStatus _evadeButDoNotCounterStatus;
        private AutoEvadeStatus _evadeAndCounterStatus;

        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private DoNothingMove _doNothing;
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            _evadeButDoNotCounterStatus = new AutoEvadeStatus(1, false);
            _evadeAndCounterStatus = new AutoEvadeStatus(1, true);

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothing = new DoNothingMove();
        }

        #region basic evade- no counter attack

        [Test]
        public void AutoEvadeStatus_CorrectlyEvadesAttack_AndDoesNotAttackIfCounterFlagFalse()
        {
            _humanFighter.AddStatus(_evadeButDoNotCounterStatus);
            
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_humanFighter.MaxHealth, _humanFighter.CurrentHealth);
            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth);
        }

        [Test]
        public void AutoEvadeStatus_CorrectlyPrintsMessage_WhenAdded([Values(1, 4)] int statusDuration, [Values(true, false)] bool shouldCounter)
        {
            AutoEvadeStatus evadeStatus = new AutoEvadeStatus(statusDuration, shouldCounter);
            StatusMove statusMove = new StatusMove("foo", TargetType.Self, evadeStatus);

            _humanFighter.SetMove(statusMove, 1);
            //status move hits
            _chanceService.PushEventOccurs(true);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothing);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            string turnOrTurns = statusDuration == 1 ? "turn" : "turns";
            string andCounterString = shouldCounter ? " and counter" : "";
            string expectedOutput = $"{_humanFighter.DisplayName} will evade{andCounterString} all attacks for {statusDuration} {turnOrTurns}!\n";

            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void AutoEvadeStatus_CorrectlyRaisesEvadeEvent_WhenEvaded()
        {
            _humanFighter.AddStatus(_evadeButDoNotCounterStatus);

            _logger.Subscribe(_humanFighter, EventType.AutoEvaded, EventType.EnemyAttackCountered);

            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.AutoEvaded, logs[0].Type);

            AutoEvadedEventArgs e = logs[0].E as AutoEvadedEventArgs;

            Assert.NotNull(e);
            Assert.IsFalse(e.AlsoCountered);
        }

        [Test]
        public void AutoEvadeStatus_CorrectlyPrintsMessage_WhenEvaded()
        {
            _humanFighter.AddStatus(_evadeButDoNotCounterStatus);

            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Count());
            Assert.AreEqual($"{_humanFighter.DisplayName} evaded the attack!\n", outputs[1].Message);
        }

        [Test]
        public void AutoEvadeStatus_DoesNotEvadeMagic()
        {
            _humanFighter.AddStatus(_evadeButDoNotCounterStatus);

            _humanFighter.SetMove(_doNothing);

            Spell fireball = new Spell("fire", MagicType.Fire, SpellType.Attack, TargetType.SingleEnemy, 0, 1);
            _enemy.AddSpell(fireball);
            _enemy.SetMove(fireball);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(0, _humanFighter.CurrentHealth);
        }

        #endregion

        #region evade and counter status

        [Test]
        public void AutoEvadeStatus_CorrectlyEvadesAttack_AndAttacksIfCounterFlagFalse()
        {
            _humanFighter.AddStatus(_evadeAndCounterStatus);

            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _chanceService.PushEventsOccur(true, false); //counter hits, doesn't crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_humanFighter.MaxHealth, _humanFighter.CurrentHealth);

            int expectedHealth = _enemy.MaxHealth - _humanFighter.Strength;
            Assert.AreEqual(expectedHealth, _enemy.CurrentHealth);
        }

        [Test]
        public void AutoEvadeStatus_CorrectlyRaisesEvadeEvent_WhenEvadedAndCountered()
        {
            _humanFighter.AddStatus(_evadeAndCounterStatus);

            _logger.Subscribe(_humanFighter, EventType.AutoEvaded, EventType.EnemyAttackCountered);

            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.AutoEvaded, logs[0].Type);
            AutoEvadedEventArgs e = logs[0].E as AutoEvadedEventArgs;

            Assert.NotNull(e);
            Assert.IsTrue(e.AlsoCountered);
        }

        [Test]
        public void AutoEvadeStatus_CorrectlyPrintsMessage_WhenEvadedAndCountered()
        {
            _humanFighter.AddStatus(_evadeAndCounterStatus);

            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetHealth(_humanFighter.Strength + 1);
            _enemy.SetMove(_basicAttackMove);
            _chanceService.PushEventsOccur(true, false); //counter hits and is not a crit

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(3, outputs.Count()); //original attack, evaded and countered, damage done
            Assert.AreEqual($"{_humanFighter.DisplayName} evaded the attack and countered!\n", outputs[1].Message);
        }

        [Test]
        public void AutoEvadeStatus_DoesNotEvadeMagic_CounterFlagTrue()
        {
            _humanFighter.AddStatus(_evadeButDoNotCounterStatus);

            _humanFighter.SetMove(_doNothing);

            Spell fireball = new Spell("fire", MagicType.Fire, SpellType.Attack, TargetType.SingleEnemy, 0, 1);
            _enemy.AddSpell(fireball);
            _enemy.SetMove(fireball);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(0, _humanFighter.CurrentHealth);
            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth);
        }

        #endregion
    }
}
