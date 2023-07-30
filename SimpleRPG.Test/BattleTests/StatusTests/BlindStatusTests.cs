using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.StatusTests
{
    [TestFixture]
    public class BlindStatusTests
    {
        private EventLogger _logger;
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private BlindStatus _blindnessStatus;

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

            _blindnessStatus = new BlindStatus(2);

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothing = new DoNothingMove();
        }

        [Test]
        public void BlindnessStatus_CorrectlyReducesMoveAccuracy([Values(10, 60, 100)] int orignalAccuracy)
        {
            _humanFighter.AddStatus(_blindnessStatus);
            
            AttackBattleMove attack = new AttackBattleMove("foo", TargetType.SingleEnemy, orignalAccuracy, 0);

            _humanFighter.SetMove(attack, 1);
            _humanFighter.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(false); //attack misses
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothing);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            double accuracyAsPercent = orignalAccuracy/100.0;
            double expectedModifiedAccuracy = accuracyAsPercent / 3;

            Assert.AreEqual(expectedModifiedAccuracy, _chanceService.LastChanceVals[0]);
        }

        [Test]
        public void BlindnessStatus_DoesNotAffectNeverMissAttack()
        {
            _logger.Subscribe(_humanFighter, EventType.AttackSuccessful);
            _humanFighter.AddStatus(_blindnessStatus);

            AttackBattleMove attack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: new NeverMissBattleMoveEffect());

            _humanFighter.SetMove(attack, 1);
            _humanFighter.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(false); //attack is not crit
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothing);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.NotNull(_logger.Logs.FirstOrDefault(log => log.Sender == _humanFighter && log.Type == EventType.AttackSuccessful));
        }
    }
}
