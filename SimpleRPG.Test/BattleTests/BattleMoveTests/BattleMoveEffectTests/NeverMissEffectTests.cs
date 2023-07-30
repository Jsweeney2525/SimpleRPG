using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests.BattleMoveEffectTests
{
    [TestFixture]
    public class NeverMissEffectTests
    {
        private static readonly NeverMissBattleMoveEffect NeverMissEffect = new NeverMissBattleMoveEffect();
        private readonly AttackBattleMove _attackWithNeverMissEffect = new AttackBattleMove("never miss", TargetType.SingleEnemy, 0, 10, effects: NeverMissEffect);

        private DoNothingMove _doNothingMove;
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        private TestHumanFighter _human;
        private TestTeam _humanTeam;

        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private BattleManager _battleManager;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new BattleManager(_chanceService, _input, _output);

            _human = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam = new TestTeam(_menuManager, _human);

            _enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);

        }

        [Test]
        public void MoveWithNeverMissEffect_BypassesAccuracyCheck()
        {
            _human.SetMove(_attackWithNeverMissEffect);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushEventsOccur(false); //just check the crit

            _enemy.SetMove(_doNothingMove);

            //Will throw if both hit and crit chances checked
            Assert.DoesNotThrow(() => _battleManager.Battle(_humanTeam, _enemyTeam));
        }

        [Test]
        public void MoveWithNeverMissEffect_CanStillBeEvaded()
        {
            _human.SetMove(_attackWithNeverMissEffect,1);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushEventsOccur(false); //just check the crit
            _human.SetMove(_runawayMove);

            AutoEvadeStatus autoEvadeStatus = new AutoEvadeStatus(1, false);
            _enemy.AddStatus(autoEvadeStatus);
            _enemy.SetMove(_doNothingMove);

             _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth, "Enemy should have evaded the never-miss attack and escaped at full health!");
        }
    }
}
