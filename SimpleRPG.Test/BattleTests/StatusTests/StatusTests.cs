using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
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
    public class StatusTests
    {
        private EventLogger _logger;
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

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

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothing = new DoNothingMove();
        }

        #region Generic Fighter Status Tests

        [Test]
        public void FighterStatusCounterTicksDown_OnTurnEnd()
        {
            Status status = new MagicMultiplierStatus(3, MagicType.Lightning, 1.25);

            _humanFighter.AddStatus(status);

            _humanFighter.OnTurnEnded(new TurnEndedEventArgs(_humanFighter));

            Status returnedStatus = _humanFighter.Statuses[0];

            Assert.AreEqual(2, returnedStatus.TurnCounter);
        }

        [Test]
        public void FighterStatusCounterRemovesStatuses_WhenCounterReaches0([Values(1,2,3)] int turnCount)
        {
            ReflectStatus status = new ReflectStatus(turnCount, MagicType.Ice);

            _humanFighter.AddStatus(status);

            for (var i = 0; i < turnCount; ++i)
            {
                _humanFighter.OnTurnEnded(new TurnEndedEventArgs(_humanFighter));
            }

            _humanFighter.OnRoundEnded(new RoundEndedEventArgs(_humanTeam, _humanFighter));

            Assert.AreEqual(0, _humanFighter.Statuses.Count);
        }

        [Test]
        public void FighterOnRoundEnd_CorrectlyRemovesStatusesWithTurnCounter0([Values(1, 2, 3)] int turnCount)
        {
            ReflectStatus status = new ReflectStatus(turnCount, MagicType.Ice);

            _humanFighter.AddStatus(status);

            _logger.Subscribe(EventType.StatusRemoved, _humanFighter);

            for (var i = 0; i < turnCount; ++i)
            {
                _humanFighter.OnTurnEnded(new TurnEndedEventArgs(_humanFighter));
            }

            _humanFighter.OnRoundEnded(new RoundEndedEventArgs(_humanTeam, _humanFighter));
            
            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];
            Assert.AreEqual(EventType.StatusRemoved, log.Type);
            Assert.AreEqual(_humanFighter, log.Sender);

            StatusRemovedEventArgs args = log.E as StatusRemovedEventArgs;
            Assert.NotNull(args);
            Assert.IsTrue(status.AreEqual(args.Status));
        }

        [Test]
        public void FighterOnRoundEnd_CorrectlyRemovesStatusesWithTurnCounter0_MultipleStatuses([Values(1, 2, 3)] int turnCount)
        {
            ReflectStatus firstStatus = new ReflectStatus(turnCount, MagicType.Ice);
            MagicMultiplierStatus secondStatus = new MagicMultiplierStatus(turnCount, MagicType.Ice, 2.0);

            _humanFighter.AddStatus(firstStatus);
            _humanFighter.AddStatus(secondStatus);

            _logger.Subscribe(EventType.StatusRemoved, _humanFighter);

            for (var i = 0; i < turnCount; ++i)
            {
                _humanFighter.OnTurnEnded(new TurnEndedEventArgs(_humanFighter));
            }

            _humanFighter.OnRoundEnded(new RoundEndedEventArgs(_humanTeam, _humanFighter));

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            EventLog log = logs[0];
            Assert.AreEqual(EventType.StatusRemoved, log.Type);
            Assert.AreEqual(_humanFighter, log.Sender);

            StatusRemovedEventArgs args = log.E as StatusRemovedEventArgs;
            Assert.NotNull(args);
            Assert.IsTrue(firstStatus.AreEqual(args.Status));

            log = logs[1];
            Assert.AreEqual(EventType.StatusRemoved, log.Type);
            Assert.AreEqual(_humanFighter, log.Sender);

            args = log.E as StatusRemovedEventArgs;
            Assert.NotNull(args);
            Assert.IsTrue(secondStatus.AreEqual(args.Status));
        }

        [Test]
        public void StatusesRemoved_OnRoundEnd()
        {
            StatMultiplierStatus boostDefenseStatus = new StatMultiplierStatus(1, StatType.Defense, 2);
            _humanFighter.SetDefense(1);
            _humanFighter.SetHealth(1);
            _humanFighter.AddStatus(boostDefenseStatus);
            _humanFighter.SetMove(_doNothing, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_basicAttackMove);
            _enemy.SetMoveTarget(_humanFighter);
            _enemy.SetStrength(2);

            _chanceService.PushEventsOccur(true, false);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _humanFighter.CurrentHealth);
        }

        [Test]
        public void FighterStatus_MultipleInstancesOfStatus_HaveSeparateCounters()
        {
            MagicMultiplierStatus status = new MagicMultiplierStatus(4, MagicType.All, 1.25);

            _humanFighter.AddStatus(status);

            for (var i = 0; i < 2; ++i)
            {
                _humanFighter.OnTurnEnded(new TurnEndedEventArgs(_humanFighter));
            }

            TestHumanFighter fighter2 = new TestHumanFighter("Bob", 1);

            fighter2.AddStatus(status);

            Status returnedStatus = fighter2.Statuses[0];

            Assert.AreEqual(4, returnedStatus.TurnCounter);
        }

        [Test]
        public void StatusTechnique_AppropriatelyAssignsStatusToTarget()
        {
            StatMultiplierStatus status = new StatMultiplierStatus(3, StatType.Strength, 1.5);
            StatusMove statusMove = new StatusMove("raise attack", TargetType.SingleAlly, status);

            TestHumanFighter fighter2 = new TestHumanFighter("foo 2", 1);
            _humanTeam = new TestTeam(_humanFighter, fighter2);

            _humanFighter.SetSpeed(1);
            _humanFighter.SetMove(statusMove);
            _chanceService.PushEventOccurs(true);
            _humanFighter.SetMoveTarget(fighter2);
            _logger.Subscribe(EventType.StatusAdded, fighter2);

            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);

            fighter2.SetStrength(2);
            fighter2.SetMove(attack);
            fighter2.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(true); //attack hits
            _chanceService.PushEventOccurs(false); //attack is not a crit

            //enemy won't be killed if the status isn't assigned to _fighter2
            _enemy.SetHealth(3);
            _enemy.SetMove(_doNothing);
            _enemy.SetMoveTarget(_enemy);

            //once Statuses are removed after battle, won't be able to 
            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.StatusAdded, log.Type);
            StatusAddedEventArgs e = log.E as StatusAddedEventArgs;

            Assert.NotNull(e);
            Assert.IsTrue(status.AreEqual(e.Status));
        }

        [Test]
        public void TestUndoDebuffsEffect_IndividualEffect()
        {
            StatMultiplierStatus lowerAttackStatus = new StatMultiplierStatus(3, StatType.Strength, 1.0/3);
            StatusMove lowerEnemyAttackMove = new StatusMove("raise attack", TargetType.SingleEnemy, lowerAttackStatus);

            UndoDebuffsStatus undoDebuffStatus = new UndoDebuffsStatus(1);
            StatusMove undoDebuffMove = new StatusMove("reset stats", TargetType.SingleAlly, undoDebuffStatus);

            TestHumanFighter fighter2 = new TestHumanFighter("foo 2", 1);
            _humanTeam = new TestTeam(_humanFighter, fighter2);

            //enemy won't be killed if the status isn't assigned to _fighter2
            _enemy.SetHealth(3);
            _enemy.SetSpeed(2);
            _enemy.SetMove(lowerEnemyAttackMove);
            _chanceService.PushEventOccurs(true); //status hits
            _enemy.SetMoveTarget(fighter2);

            _humanFighter.SetSpeed(1);
            _humanFighter.SetMove(undoDebuffMove);
            _chanceService.PushEventOccurs(true); //status hits
            _humanFighter.SetMoveTarget(fighter2);
            _logger.Subscribe(EventType.StatusAdded, fighter2);
            _logger.Subscribe(EventType.StatusRemoved, fighter2);

            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);

            fighter2.SetStrength(3);
            fighter2.SetMove(attack);
            fighter2.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(true); //attack hits
            _chanceService.PushEventOccurs(false); //attack is not a crit

            //once Statuses are removed after battle, won't be able to 
            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            EventLog log = logs[1];

            Assert.AreEqual(EventType.StatusRemoved, log.Type);
            StatusRemovedEventArgs e = log.E as StatusRemovedEventArgs;

            Assert.NotNull(e);
            Assert.IsTrue(lowerAttackStatus.AreEqual(e.Status));
        }

        #endregion

        #region Generic Team Status Tests

        [Test]
        public void Team_AddStatusMethod_CorrectlyAssignsStatusesToFighters([Values(2, 4)] int numberOfFighters)
        {
            IList<IFighter> fighters = new List<IFighter>();

            for (var i = 0; i < numberOfFighters; ++i)
            {
                fighters.Add(TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1));
            }

            Team team = new Team(TestMenuManager.GetTestMenuManager(), fighters.ToArray());

            Status status = new CriticalChanceMultiplierStatus(2, 1.5);

            team.AddStatus(status);

            foreach (IFighter fighter in fighters)
            {
                Assert.AreEqual(1, fighter.Statuses.Count);
                Assert.IsTrue(status.AreEqual(fighter.Statuses[0]));
            }
        }

        [Test]
        public void Team_AddStatusMethod_CorrectlyDoesNotAssignsStatusToDefeatedFighters([Values(3, 5)] int numberOfFighters)
        {
            IList<IFighter> fighters = new List<IFighter>();

            numberOfFighters += 2;
            for (var i = 0; i < numberOfFighters; ++i)
            {
                fighters.Add(TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1));
            }

            int lastIndex = numberOfFighters - 1;
            fighters[0].PhysicalDamage(fighters[0].MaxHealth);
            fighters[lastIndex].PhysicalDamage(fighters[lastIndex].MaxHealth);

            Team team = new Team(TestMenuManager.GetTestMenuManager(), fighters.ToArray());

            Status status = new CriticalChanceMultiplierStatus(2, 1.5);

            team.AddStatus(status);

            for (var i = 0; i < numberOfFighters; ++i)
            {
                IFighter fighter = fighters[i];

                if (i == 0 || i == lastIndex)
                {
                    Assert.AreEqual(0, fighter.Statuses.Count);
                }
                else
                {
                    Assert.AreEqual(1, fighter.Statuses.Count);
                    Assert.IsTrue(status.AreEqual(fighter.Statuses[0]));
                }
            }
        }

        #endregion

        #region battle manager screen outputs

        [Test]
        public void BlindnessStatusCorrectlyPrintsMessage([Values(1, 3)] int statusDuration)
        {
            StatusMove blindnessMove = new StatusMove("foo", TargetType.SingleEnemy, new BlindStatus(statusDuration));
            _humanFighter.SetMove(blindnessMove, 1);
            _chanceService.PushEventOccurs(true);
            _humanFighter.SetMoveTarget(_enemy);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetMove(_doNothing);

            SilentBattleConfiguration config = new SilentBattleConfiguration();

            //act
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            MockOutputMessage output = outputs[0];

            string turnOrTurns = statusDuration == 1 ? "turn" : "turns";
            string expectedMessage = $"{_enemy.DisplayName} has been afflicted with blindness for {statusDuration} {turnOrTurns}!\n";
            Assert.AreEqual(expectedMessage, output.Message);
        }

        #endregion
    }
}
