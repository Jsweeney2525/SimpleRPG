using System.Collections.Generic;
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
    public class CounterAttackStatusTests
    {
        private EventLogger _logger;
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private CounterAttackStatus _status;

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

            _status = new CounterAttackStatus(1);

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothing = new DoNothingMove();
        }

        [Test]
        public void CorrectlyPrintsMessages_WhenAdded([Values(1, 3)] int statusDuration)
        {
            CounterAttackStatus status = new CounterAttackStatus(statusDuration);
            StatusMove statusMove = new StatusMove("foo", TargetType.Self, status);

            _humanFighter.SetMove(statusMove, 1);
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
            string expectedOutput = $"{_humanFighter.DisplayName} will counter any attack for {statusDuration} {turnOrTurns}!\n";

            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void CorrectlyCountersAttack()
        {
            _humanFighter.AddStatus(_status);
            
            _humanFighter.SetDefense(_enemy.Strength);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _enemy.SetHealth(_humanFighter.Strength + 1);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit
            _chanceService.PushEventsOccur(true, false); //counter hits, not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _enemy.CurrentHealth);
        }

        [Test]
        public void CorrectlyRaisesCounterAttackEvent()
        {
            _humanFighter.AddStatus(_status);
            _logger.Subscribe(EventType.EnemyAttackCountered, _humanFighter);

            _humanFighter.SetDefense(_enemy.Strength);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _enemy.SetHealth(_humanFighter.Strength + 1);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit
            _chanceService.PushEventsOccur(true, false); //counter hits, not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.EnemyAttackCountered, log.Type);
            Assert.AreEqual(_humanFighter, log.Sender);

            EnemyAttackCounteredEventArgs e = log.E as EnemyAttackCounteredEventArgs;
            Assert.NotNull(e);

            Assert.AreEqual(_humanFighter, e.Counterer);
            Assert.AreEqual(_enemy, e.Enemy);
        }

        [Test]
        public void CorrectScreenOutputsPrinted([Values(true, false)] bool enemyDies)
        {
            _humanFighter.AddStatus(_status);

            _humanFighter.SetDefense(_enemy.Strength);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            int enemyHealth = _humanFighter.Strength + (enemyDies ? 0 : 1);
            _enemy.SetHealth(enemyHealth);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit
            _chanceService.PushEventsOccur(true, false); //counter hits, not a crit

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedOutputsLength = 4; //enemy attacked, 0 damage taken, counter, damage taken
            if (enemyDies)
            {
                ++expectedOutputsLength;
            }
            Assert.AreEqual(expectedOutputsLength, outputs.Length); 

            Assert.AreEqual($"{_humanFighter.DisplayName} counter attacks!\n", outputs[2].Message);
        }

        [Test]
        public void DoesNotCounter_MagicAttacks()
        {
            Spell fireball = new Spell("fire", MagicType.Fire, SpellType.Attack, TargetType.SingleEnemy, 0, 1);

            _humanFighter.AddStatus(_status);

            _humanFighter.SetHealth(_enemy.MagicStrength + fireball.Power + 1);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.AddSpell(fireball);
            _enemy.SetMove(fireball);
            
            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _humanFighter.CurrentHealth);
            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth);
        }

        [Test]
        public void DoesNotCounter_EvadedMove()
        {
            _humanFighter.AddStatus(_status);
            _enemy.AddStatus(_status);

            _humanFighter.SetDefense(_enemy.Strength);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _enemy.SetHealth(_humanFighter.Strength + 1);
            _chanceService.PushEventsOccur(false); //attack misses

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth);
        }

        [Test]
        public void DoesNotCounter_AlreadyCounteredMove()
        {
            _humanFighter.AddStatus(_status);
            _enemy.AddStatus(_status);

            _humanFighter.SetDefense(_enemy.Strength);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _enemy.SetHealth(_humanFighter.Strength + 1);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit
            _chanceService.PushEventsOccur(true, false); //counter hits, not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_humanFighter.MaxHealth, _humanFighter.CurrentHealth);
        }
    }
}