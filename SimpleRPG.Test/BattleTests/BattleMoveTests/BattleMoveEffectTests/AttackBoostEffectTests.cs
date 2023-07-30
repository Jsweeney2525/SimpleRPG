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
    public class AttackBoostEffectTests
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
        public void MoveWithAttackBoostEffect_CorrectlyCalculatesDamage([Values(0.25, 0.5, 2.0, 3.0)] double multiplier)
        {
            const int humanStrength = 4;
            AttackBoostBattleMoveEffect attackBoostEffect = new AttackBoostBattleMoveEffect(multiplier);
            AttackBattleMove attackBoostAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 10, effects: attackBoostEffect);

            _human.SetStrength(humanStrength);
            _human.SetMove(attackBoostAttack, 1);
            _human.SetMove(_runawayMove);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushAttackHitsNotCrit(); //attack hits, not a crit
            
            _enemy.SetMove(_doNothingMove);
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedDamage = (int)(humanStrength*multiplier);
            int expectedRemainingHealth = 100 - expectedDamage;
            int actualDamage = 100 - _enemy.CurrentHealth;
            Assert.AreEqual(expectedRemainingHealth, _enemy.CurrentHealth, $"attack should have hit for {expectedDamage} damage, isntead of {actualDamage}!");
        }
    }
}
