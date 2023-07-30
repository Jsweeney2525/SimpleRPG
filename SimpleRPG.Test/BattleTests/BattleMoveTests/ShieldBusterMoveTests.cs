using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    public class ShieldBusterMoveTests
    {
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;
        private EventLogger _logger;

        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);
            _logger = new EventLogger();

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);
        }

        [Test]
        public void MoveFactory_SmokeTest()
        {
            BattleMove ret = null;
            Assert.DoesNotThrow(() => ret = MoveFactory.Get(BattleMoveType.ShieldBuster));
            Assert.NotNull(ret);
        }

        [Test]
        public void BattleManager_CorrectlyPrintsMessages_TargetHasShield([Values("eats pudding", null)] string executionMessage)
        {
            const int shieldDefense = 5;
            const int shieldHealth = 1;
            IronBattleShield shield = new IronBattleShield(shieldHealth, shieldDefense, 0);
            ShieldBusterMove shieldBusterMove = new ShieldBusterMove("foo", TargetType.SingleEnemy, executionMessage);

            _humanFighter.SetSpeed(1);
            _humanFighter.SetMove(shieldBusterMove, 1);
            _humanFighter.SetMove(_runawayMove);
            _humanFighter.SetMoveTarget(_enemy);

            _enemy.SetBattleShield(shield);
            //_logger.SubscribeAll(_enemy.BattleShield);
            _enemy.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedLength = 1;
            if (executionMessage != null)
            {
                expectedLength++;
            }
            Assert.AreEqual(expectedLength, outputs.Length);

            int i = 0;

            if (executionMessage != null)
            {
                Assert.AreEqual($"{_humanFighter.DisplayName} {executionMessage}!\n", outputs[i++].Message);
            }
            Assert.AreEqual($"{_enemy.DisplayName}'s shield was destroyed!\n", outputs[i].Message);
        }

        [Test]
        public void ShieldBusterMove_CannotBustShieldsWithHigherShieldBusterDefense()
        {
            IronBattleShield shield = new IronBattleShield(5, 5, 3, 2);
            _enemy.SetBattleShield(shield);

            Assert.NotNull(_enemy.BattleShield);

            ShieldBusterMove shieldBuster = new ShieldBusterMove("shieldBuster", TargetType.SingleEnemy, null, 1);

            _humanFighter.SetMove(shieldBuster, 1);
            _humanFighter.SetMoveTarget(_enemy);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothingMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.NotNull(_enemy.BattleShield);
        }

        [Test]
        public void SpecialMoveFailedEvent_RaisedWhenShieldBusterFails_TargetHasNoShield()
        {
            _logger.Subscribe(_humanFighter, EventType.SpecialMoveFailed);

            ShieldBusterMove shieldBuster = new ShieldBusterMove("shieldBuster", TargetType.SingleEnemy, null, 1);

            _humanFighter.SetMove(shieldBuster, 1);
            _humanFighter.SetMoveTarget(_enemy);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothingMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.SpecialMoveFailed, log.Type);

            SpecialMoveFailedEventArgs e = log.E as SpecialMoveFailedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(SpecialMoveFailedReasonType.TargetHadNoShield, e.Reason);
        }

        [Test]
        public void SpecialMoveFailedEvent_RaisedWhenShieldBusterFails_BusterPowerLessThanShieldBusterDefense()
        {
            _logger.Subscribe(_humanFighter, EventType.SpecialMoveFailed);

            IronBattleShield shield = new IronBattleShield(5, 5, 3, 2);
            _enemy.SetBattleShield(shield);

            Assert.NotNull(_enemy.BattleShield);

            ShieldBusterMove shieldBuster = new ShieldBusterMove("shieldBuster", TargetType.SingleEnemy, null, 1);

            _humanFighter.SetMove(shieldBuster, 1);
            _humanFighter.SetMoveTarget(_enemy);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothingMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.SpecialMoveFailed, log.Type);

            SpecialMoveFailedEventArgs e = log.E as SpecialMoveFailedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(SpecialMoveFailedReasonType.ShieldBusterDefenseHigherThanShieldBusterPower, e.Reason);
        }
    }
}
