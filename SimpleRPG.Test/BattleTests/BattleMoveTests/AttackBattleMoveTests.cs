using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    public class AttackBattleMoveTests
    {
        private MockOutput _output;
        private MockInput _input;
        private MockChanceService _chanceService;
        private EventLogger _logger;

        private TestEnemyFighter _team1Fighter;
        private TestEnemyFighter _team2Fighter;

        private Team _team1;
        private Team _team2;

        private TestBattleManager _battleManager;

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _output = new MockOutput();
            _input = new MockInput();
            _logger = new EventLogger();

            _team1Fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _team2Fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);

            _team1 = new Team(TestMenuManager.GetTestMenuManager(), _team1Fighter);
            _team2 = new Team(TestMenuManager.GetTestMenuManager(), _team2Fighter);

            _battleManager = new TestBattleManager(_chanceService, _input, _output);
        }

        [Test]
        public void AttackMovePower_AppropriatelyAffectsDamage()
        {
            _logger.Subscribe(_team2Fighter, EventType.DamageTaken);

            int weakAttackPower = 4;
            int strongAttackPower = 8;

            AttackBattleMove weakerAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, weakAttackPower);
            AttackBattleMove strongerAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, strongAttackPower);

            _team1Fighter.SetStrength(0);
            
            _team1Fighter.SetMove(weakerAttack, 1);
            _team1Fighter.SetMove(strongerAttack, 1);
            _chanceService.PushEventsOccur(true, false, true, false); //attacks hit, not crits

            _team2Fighter.SetHealth(weakAttackPower + strongAttackPower);
            _team2Fighter.SetMove(_doNothingMove);

            _battleManager.Battle(_team1, _team2);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            PhysicalDamageTakenEventArgs e = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(weakAttackPower, e.Damage);

            e = logs[1].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(strongAttackPower, e.Damage);
        }

        [Test]
        public void CalculatedAttackPowerCannotBeNegative()
        {
            _logger.Subscribe(_team2Fighter, EventType.DamageTaken);

            AttackBattleMove attack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, -1);

            _team1Fighter.SetStrength(0);
            _team1Fighter.SetMove(attack, 1);
            _team1Fighter.SetMove(_runawayMove, 1);
            _chanceService.PushEventsOccur(true, false); //attack hit, not crits
            
            _team2Fighter.SetMove(_doNothingMove);

            _battleManager.Battle(_team1, _team2);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            PhysicalDamageTakenEventArgs e = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(0, e.Damage);
        }
    }
}
