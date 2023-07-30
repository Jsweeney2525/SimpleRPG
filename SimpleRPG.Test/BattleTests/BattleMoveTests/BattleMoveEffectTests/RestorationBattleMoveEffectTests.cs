using System;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.BattleMoves;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests.BattleMoveEffectTests
{
    [TestFixture]
    public class RestorationBattleMoveEffectTests
    {
        private RestorationBattleMoveEffect _restoreHealthEffect;
        private BattleMove _attackWithRestoreHealthEffect;

        private RestorationBattleMoveEffect _restoreManaEffect;
        private BattleMove _attackWithRestoreManaEffect;

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

            _restoreHealthEffect = new RestorationBattleMoveEffect(RestorationType.Health, 5, BattleMoveEffectActivationType.OnAttackHit);
            _attackWithRestoreHealthEffect = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 10, effects: _restoreHealthEffect);

            _restoreManaEffect = new RestorationBattleMoveEffect(RestorationType.Mana, 5, BattleMoveEffectActivationType.OnAttackHit);
            _attackWithRestoreManaEffect = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 10, effects: _restoreManaEffect);

        }

        #region constructor logic

        [Test]
        public void BattleMoveConstructor_AppropriatelyAssignsEffect()
        {
            Assert.AreEqual(_restoreHealthEffect, _attackWithRestoreHealthEffect.BattleMoveEffects[0]); 
        }

        private RestorationBattleMoveEffect CreateRestorationBattleMoveEffect(int percentage)
        {
            return new RestorationBattleMoveEffect(RestorationType.Health, percentage, BattleMoveEffectActivationType.OnAttackHit);
        }

        [Test]
        public void RestoreHealthEffect_ConstructorAppropriatelyDisallowsNegativePercentages([Values(-5, -15)] int percentage)
        {
            Assert.Throws<ArgumentException>(() => CreateRestorationBattleMoveEffect(percentage), "RestoreHealthEffect cannot be initialized with a negative percentage!");
        }

        [Test]
        public void RestoreHealthEffect_ConstructorAppropriatelyDisallowsZeroPercentage()
        {
            Assert.Throws<ArgumentException>(() => CreateRestorationBattleMoveEffect(0), "RestoreHealthEffect cannot be initialized with a percentage of 0!");
        }

        [Test]
        public void RestoreHealthEffect_ConstructorAppropriatelyDisallowsPercentagesExceeding100Percent([Values(110, 150)] int percentage)
        {
            Assert.Throws<ArgumentException>(() => CreateRestorationBattleMoveEffect(percentage), "RestoreHealthEffect cannot be initialized with a percentage that exceeds 100!");
        }

        #endregion

        [Test]
        public void RestoreHealthEffect_AppropriatelyExecuted_AttackHits()
        {
            int initialHealth = 100 - _restoreHealthEffect.Percentage;
            _human.SetHealth(100, initialHealth);
            _human.SetMana(100, 0);
            _human.SetMove(_attackWithRestoreHealthEffect);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_human.MaxHealth, _human.CurrentHealth, "humman player's health should have been restored when the attack hit!");
            Assert.AreEqual(0, _human.CurrentMana, "humman player's mana should have been unaffected by the restore effect!");
        }

        [Test]
        public void RestoreHealthEffect_DoesNotActivate_AttackMisses()
        {
            int initialHealth = 100 - _restoreHealthEffect.Percentage;
            _human.SetHealth(100, initialHealth);
            _human.SetMove(_attackWithRestoreHealthEffect, 1);
            _human.SetMove(_runawayMove);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(false); //attack misses

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(initialHealth, _human.CurrentHealth, "humman player's health should not have been restored because the attack missed!");
        }

        [Test]
        public void RestoreManaEffect_AppropriatelyExecuted_AttackHits([Values(10, 20)] int restoreAmount)
        {
            _human.SetHealth(100, 10);
            _human.SetMana(100, 0);
            _restoreManaEffect = new RestorationBattleMoveEffect(RestorationType.Mana, restoreAmount, BattleMoveEffectActivationType.OnAttackHit);
            _attackWithRestoreManaEffect = new AttackBattleMove("", TargetType.SingleEnemy, 100, 0, effects: _restoreManaEffect);
            _human.SetMove(_attackWithRestoreManaEffect);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushAttackHitsNotCrit(); //attack hits, not a crit

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);
            
            Assert.AreEqual(restoreAmount, _human.CurrentMana, $"humman player's mana should have been restored by {restoreAmount} when the attack hit!");
            Assert.AreEqual(10, _human.CurrentHealth, "humman player's health should have been unaffected by the restore effect!");
        }

        [Test]
        public void RestoreManaEffect_DoesNotActivate_AttackMisses()
        {
            _human.SetMana(100, 0);
            _human.SetMove(_attackWithRestoreManaEffect, 1);
            _human.SetMove(_runawayMove);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushEventOccurs(false); //attack misses

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(0, _human.CurrentMana, "humman player's mana should not have been restored because the attack missed!");
        }

        [Test]
        public void MultipleEffects_Fired_OnAttackHit([Values(10, 20)] int restoreAmount)
        {
            _human.SetHealth(100, 10);
            _human.SetMana(100, 10);
            _restoreManaEffect = new RestorationBattleMoveEffect(RestorationType.Mana, restoreAmount, BattleMoveEffectActivationType.OnAttackHit);
            _restoreHealthEffect = new RestorationBattleMoveEffect(RestorationType.Health, restoreAmount, BattleMoveEffectActivationType.OnAttackHit);
            BattleMove attackWithRestoreEffects = new AttackBattleMove("", TargetType.SingleEnemy, 100, 0, effects: new BattleMoveEffect[] { _restoreManaEffect, _restoreHealthEffect } );
            _human.SetMove(attackWithRestoreEffects);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushAttackHitsNotCrit(); //attack hits, not a crit

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedCurrentAmount = 10 + restoreAmount;
            Assert.AreEqual(expectedCurrentAmount, _human.CurrentMana, $"humman player's mana should have been restored by {restoreAmount} when the attack hit!");
            Assert.AreEqual(expectedCurrentAmount, _human.CurrentHealth, $"humman player's health should have been restored by {restoreAmount} when the attack hit!");
        }

        [Test]
        public void MultipleEffects_OnlyEffectsWhoseConditionsAreMet_AreFired([Values(10, 20)] int restoreAmount)
        {
            _human.SetHealth(100, 10);
            _human.SetMana(100, 10);
            _restoreManaEffect = new RestorationBattleMoveEffect(RestorationType.Mana, restoreAmount, BattleMoveEffectActivationType.OnAttackHit, new DanceBattleCondition(DanceEffectType.Soul));
            _restoreHealthEffect = new RestorationBattleMoveEffect(RestorationType.Health, restoreAmount, BattleMoveEffectActivationType.OnAttackHit, new DanceBattleCondition(DanceEffectType.Fire));
            BattleMove attackWithRestoreEffects = new AttackBattleMove("", TargetType.SingleEnemy, 100, 0, effects: new BattleMoveEffect[] { _restoreManaEffect, _restoreHealthEffect });

            _human.SetMove(attackWithRestoreEffects);
            _human.SetMoveTarget(_enemy);
            _chanceService.PushAttackHitsNotCrit(); //attack hits, not a crit

            TestHumanFighter dancer =
                (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "dancer");

            TestDanceMove danceMove = new TestDanceMove("bar", TargetType.Field, 2);
            danceMove.SetDanceEffect(DanceEffectType.Fire);
            danceMove.AddMove(_doNothingMove);

            dancer.SetSpeed(1);
            dancer.SetMove(danceMove);
            dancer.SetMoveTarget(dancer);

            _humanTeam.Add(dancer, false);

            _enemy.SetMove(_doNothingMove);
            _enemy.SetMoveTarget(_enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedCurrentAmount = 10 + restoreAmount;
            Assert.AreEqual(10, _human.CurrentMana, "humman player's mana should not have been restored, the dance condition wasn't met!");
            Assert.AreEqual(expectedCurrentAmount, _human.CurrentHealth, $"humman player's health should have been restored by {restoreAmount} when the attack hit!");
        }
    }
}
