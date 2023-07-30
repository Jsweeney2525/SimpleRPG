using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests.ConditionalBattleMoveTests
{
    [TestFixture]
    public class NotEvadedBattleConditionTests
    {
        private MockOutput _output;
        private MockInput _input;
        private MockChanceService _chanceService;

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

            _team1Fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _team2Fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);

            _team1 = new Team(TestMenuManager.GetTestMenuManager(), _team1Fighter);
            _team2 = new Team(TestMenuManager.GetTestMenuManager(), _team2Fighter);

            _battleManager = new TestBattleManager(_chanceService, _input, _output);
        }

        private AttackBattleMove GetNotEvadeRestoreHealthAttack(int percentage)
        {
            NotEvadedBattleCondition notEvadedCondition = new NotEvadedBattleCondition();
            RestorationBattleMoveEffect restorationEffect = new RestorationBattleMoveEffect(RestorationType.Health, percentage, BattleMoveEffectActivationType.OnAttackHit, notEvadedCondition);
            CannotBeEvadedBattleMoveEffect cannotBeEvadedEffect = new CannotBeEvadedBattleMoveEffect();
            BattleMoveEffect[] effects = { restorationEffect, cannotBeEvadedEffect };
            AttackBattleMove attackMove = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: effects);

            return attackMove;
        }

        [Test]
        public void ConditionalEffect_AppropriatelyFired_WhenNotEvadedConditionMet([Values(10, 20)] int percentage)
        {
            AttackBattleMove conditionalHealMove = GetNotEvadeRestoreHealthAttack(percentage);
            
            _team1Fighter.SetHealth(100, 10);
            _team1Fighter.SetMove(conditionalHealMove, 1);
            _team1Fighter.SetMoveTarget(_team2Fighter);
            _team1Fighter.SetMove(_runawayMove);

            _chanceService.PushAttackHitsNotCrit();

            _team2Fighter.SetHealth(100);
            _team2Fighter.SetMove(_doNothingMove);

            _battleManager.Battle(_team1, _team2);

            int expectedRemainingHealth = 10 + percentage;

            Assert.AreEqual(expectedRemainingHealth, _team1Fighter.CurrentHealth, $"The attack wasn't evaded, so the user should have regained {percentage} HP");
        }

        [Test]
        public void ConditionalEffect_AppropriatelySuppressed_WhenNotEvadedConditionNotMet([Values(10, 20)] int percentage)
        {
            AttackBattleMove conditionalHealMove = GetNotEvadeRestoreHealthAttack(percentage);

            _team1Fighter.SetHealth(100, 10);
            _team1Fighter.SetMove(conditionalHealMove, 1);
            _team1Fighter.SetMoveTarget(_team2Fighter);
            _team1Fighter.SetMove(_runawayMove);

            _chanceService.PushAttackHitsNotCrit();

            AutoEvadeStatus autoEvadeStatus = new AutoEvadeStatus(1, false);
            _team2Fighter.AddStatus(autoEvadeStatus);
            _team2Fighter.SetHealth(100);
            _team2Fighter.SetMove(_doNothingMove);

            _battleManager.Battle(_team1, _team2);

            int expectedRemainingHealth = 10;
            
            Assert.AreEqual(expectedRemainingHealth, _team1Fighter.CurrentHealth, "The enemy had evade status, so the user should not have regained any HP");
        }
    }
}
