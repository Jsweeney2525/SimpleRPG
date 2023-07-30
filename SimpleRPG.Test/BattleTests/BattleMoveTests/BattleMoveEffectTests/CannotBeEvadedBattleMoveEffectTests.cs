using System;
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
    public class CannotBeEvadedBattleMoveEffectTests
    {
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
        public void MoveWithCannotBeEvadedEffect_HitsOpponentThatEvaded()
        {
            CannotBeEvadedBattleMoveEffect cannotBeEvadedEffect = new CannotBeEvadedBattleMoveEffect();
            AttackBattleMove noEvadeAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 10, effects: cannotBeEvadedEffect);

            _human.SetMove(noEvadeAttack, 1);
            _human.SetMove(_runawayMove);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushAttackHitsNotCrit(); //attack hits, not a crit

            AutoEvadeStatus autoEvadeStatus = new AutoEvadeStatus(1, false);
            _enemy.AddStatus(autoEvadeStatus);
            _enemy.SetMove(_doNothingMove);
            _enemy.SetHealth(_human.Strength + 1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _enemy.CurrentHealth, "attack should have hit even though enemy had an AutoEvade status!");
        }

        [Test]
        public void MoveWithCannotBeEvadedEffect_AppropriatelyCancelsCounterEffect()
        {

        }
    }
}
