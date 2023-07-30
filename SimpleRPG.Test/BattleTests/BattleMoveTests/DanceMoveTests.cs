using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.BattleMoves;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    /// <summary>
    /// A dance effect is a specialized field effect. It offers a few differences:
    ///  - A dance effect is immediately canceled if its caster is defeated
    ///  - The executor of a dance effect cannot perform another move until the dance is completed
    ///  - Two separately performed dances can "combine" their effects. That is, one ally might perform a dance of fire and another might perform a dance of wind,
    ///    which may create a new dance- the dance of storms
    ///  - Certain Moves can have additional effects when performed during a particular dance
    /// </summary>
    [TestFixture]
    class DanceMoveTests
    {
        private TestEnemyFighter _enemy1, _enemy2;
        private Team _enemyTeam;
        private TestHumanFighter _human1, _human2;
        private TestTeam _humanTeam;
        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;
        private TestFieldEffectCombiner _combiner;
        private EventLogger _logger;

        private const int _testTechniqueDefaultDuration = 2;
        private TestDanceMove _testTechnique;

        private readonly StatMultiplierFieldEffect _raiseTeamDefense50Percent =
            new StatMultiplierFieldEffect(TargetType.OwnTeam, "foo", StatType.Defense, 1.5);

        private readonly StatMultiplierFieldEffect _lowerEnemyDefense50Percent =
            new StatMultiplierFieldEffect(TargetType.EnemyTeam, "foo", StatType.Defense, 0.5);

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
        private readonly BattleMove _basicAttack = MoveFactory.Get(BattleMoveType.Attack);

        private const string FirstTurnMessage = "Performs the Defense dance!";

        private const int OriginalDefense = 10;
        private const int EnemyDefenseAfterDanceMove = (int) (OriginalDefense*1.5);

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);
            _combiner = new TestFieldEffectCombiner();
            _logger = new EventLogger();

            _enemy1 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemy1.SetDefense(OriginalDefense);
            _enemy2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemy2.SetDefense(OriginalDefense);

            _enemyTeam = new Team(_menuManager, _enemy1, _enemy2);

            _human1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _human1.SetDefense(OriginalDefense);
            _human2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _human2.SetDefense(OriginalDefense);

            _humanTeam = new TestTeam(_human1, _human2);

            _testTechnique = (TestDanceMove)TestMoveFactory.Get(TargetType.Field, moveType: BattleMoveType.Dance);
            _testTechnique.AddEffect(_raiseTeamDefense50Percent);
            _testTechnique.AddEffect(_lowerEnemyDefense50Percent);
            _testTechnique.SetDuration(_testTechniqueDefaultDuration);
            _testTechnique.SetDanceEffect(DanceEffectType.Fire);

            var firstTurn = (TestDoNothingMove) TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            firstTurn.SetMessage(FirstTurnMessage);
            _testTechnique.AddMove(firstTurn);
            var secondTurn = (TestDoNothingMove)TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            secondTurn.SetMessage("Continue the Defense dance");
            _testTechnique.AddMove(secondTurn);
        }

        [TearDown]
        public void TearDown()
        {
            _testTechnique = null;
        }

        //TODO: test that if EffectDuration and moves do not have same length, an exception is thrown

        [Test]
        public void TestMoveFactory_ReturnsDanceEffectTechnique()
        {
            Assert.AreEqual(BattleMoveType.Dance, _testTechnique.MoveType);
            Assert.AreEqual(2, _testTechnique.EffectDuration);
        }

        [Test]
        public void BattleManager_AppropriatelyExecutesDanceTechnique_DoesNotThrowException()
        {
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetSpeed(10);
            _enemy2.SetMove(_doNothingMove);

            _human1.SetMove(_basicAttack);
            _human1.SetMoveTarget(_enemy1);
            _human1.SetStrength(EnemyDefenseAfterDanceMove + _enemy1.MaxHealth);

            _human2.SetMove(_basicAttack);
            _human2.SetMoveTarget(_enemy2);
            _human2.SetStrength(EnemyDefenseAfterDanceMove + _enemy2.MaxHealth);
            _chanceService.PushEventsOccur(true, false, true, false); //set up attack hits, attack crits for both attacks

            Assert.DoesNotThrow(() => { _battleManager.Battle(_humanTeam, _enemyTeam); });
        }

        [Test]
        public void BattleManager_AppropriatelyExecutesDanceTechnique_TriggersAppropriateEvents()
        {
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetSpeed(10);
            _enemy2.SetMove(_doNothingMove);

            _human1.SetMove(_basicAttack);
            _human1.SetMoveTarget(_enemy1);
            _human1.SetStrength(EnemyDefenseAfterDanceMove + _enemy1.MaxHealth);
            _human2.SetMove(_basicAttack);
            _human2.SetMoveTarget(_enemy2);
            _human2.SetStrength(EnemyDefenseAfterDanceMove + _enemy2.MaxHealth);
            _chanceService.PushEventsOccur(true, false, true, false); //set up attack hits, attack crits for both attacks

            _logger.Subscribe(EventType.FieldEffectExecuted, _battleManager);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            EventLog log = logs[0];
            FieldEffectExecutedEventArgs e = log.E as FieldEffectExecutedEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(_raiseTeamDefense50Percent, e.Effect);
                Assert.AreEqual(1, e.EffectOwners.Count);
                Assert.AreEqual(_enemy1, e.EffectOwners[0]);
            }

            log = logs[1];
            e = log.E as FieldEffectExecutedEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(_lowerEnemyDefense50Percent, e.Effect);
                Assert.AreEqual(1, e.EffectOwners.Count);
                Assert.AreEqual(_enemy1, e.EffectOwners[0]);
            }
        }

        [Test]
        public void BattleManager_AppropriatelyExecutesDanceTechnique_ScreenOutputs()
        {
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetSpeed(10);
            _enemy2.SetMove(_doNothingMove);

            _human1.SetMove(_basicAttack);
            _human1.SetMoveTarget(_enemy1);
            _human1.SetStrength(EnemyDefenseAfterDanceMove + _enemy1.MaxHealth);
            _human2.SetMove(_basicAttack);
            _human2.SetMoveTarget(_enemy2);
            _human2.SetStrength(EnemyDefenseAfterDanceMove + _enemy2.MaxHealth);
            _chanceService.PushEventsOccur(true, false, true, false); //set up attack hits, attack crits for both attacks

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowAttackMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = false,
                ShowExpAndLevelUpMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(3, outputs.Length); //dance move has been performed, enemy's stats raised, human's stats lowered

            Assert.AreEqual($"{_enemy1.DisplayName} {FirstTurnMessage}\n", outputs[0].Message);
        }

        private RestorationBattleMoveEffect GetRestoreHealthEffect(DanceEffectType danceEffectType)
        {
            DanceBattleCondition battleCondition = new DanceBattleCondition(danceEffectType);
            return new RestorationBattleMoveEffect(RestorationType.Health, 10, BattleMoveEffectActivationType.OnAttackHit, battleCondition);
        }

        private AttackBoostBattleMoveEffect GetAttackBoostEffect(double attackMultiplier, DanceEffectType danceEffectType)
        {
            DanceBattleCondition battleCondition = new DanceBattleCondition(danceEffectType);
            return new AttackBoostBattleMoveEffect(attackMultiplier, battleCondition);
        }

        [Test]
        public void ConditionalDanceMove_ExecutesAppropriateEffectWhenDanceInEffect([Values(DanceEffectType.Fire, DanceEffectType.Danger)] DanceEffectType danceEffectType)
        {
            _testTechnique.ClearEffects();
            _testTechnique.SetDanceEffect(danceEffectType);
            
            RestorationBattleMoveEffect conditionalHealEffect = GetRestoreHealthEffect(danceEffectType);
            AttackBattleMove attackWithHeal = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: conditionalHealEffect);

            _human2.SetMove(_testTechnique, 1);
            _human2.SetSpeed(1);

            _human1.SetHealth(100, 10);
            _human1.SetMove(attackWithHeal, 1);
            _chanceService.PushEventsOccur(true, false); //attack hits, is not crit
            _human1.SetStrength(_enemy1.MaxHealth);
            _human1.SetMoveTarget(_enemy1);

            _enemyTeam = new Team(_menuManager, _enemy1);

            _enemy1.SetDefense(0);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(20, _human1.CurrentHealth, "Human1 should have regained 10 HP when the attack was successful and the dance effect was active");
        }

        [Test]
        public void ConditionalDanceMove_ExecutesAppropriateEffectWhenDanceNotInEffect([Values(DanceEffectType.Water, DanceEffectType.Wind)] DanceEffectType danceEffectType)
        {
            _testTechnique.ClearEffects();
            _testTechnique.SetDanceEffect(DanceEffectType.Soul);

            RestorationBattleMoveEffect conditionalHealEffect = GetRestoreHealthEffect(danceEffectType);
            AttackBattleMove attackWithHeal = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: conditionalHealEffect);

            _human2.SetMove(_testTechnique, 1);
            _human2.SetSpeed(1);

            const int initialHealth = 10;
            _human1.SetHealth(100, initialHealth);
            _human1.SetMove(attackWithHeal, 1);
            _chanceService.PushEventsOccur(true, false); //attack hits, is not crit
            _human1.SetStrength(_enemy1.MaxHealth);
            _human1.SetMoveTarget(_enemy1);

            _enemyTeam = new Team(_menuManager, _enemy1);

            _enemy1.SetDefense(0);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(initialHealth, _human1.CurrentHealth, "Human1 should not have had their health altered, the active dance effect's didn't match the conditional heal!");
        }

        [Test]
        public void ConditionalAttackBoostMove_ExecutesAppropriateEffectWhenDanceInEffect([Values(DanceEffectType.Fire, DanceEffectType.Danger)] DanceEffectType danceEffectType)
        {
            _testTechnique.ClearEffects();
            _testTechnique.SetDanceEffect(danceEffectType);

            const int attackBoostMultiplier = 2;
            AttackBoostBattleMoveEffect conditionalAttackBoostEffect = GetAttackBoostEffect(attackBoostMultiplier, danceEffectType);
            AttackBattleMove attackWithBoost = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: conditionalAttackBoostEffect);

            _human2.SetMove(_testTechnique, 1);
            _human2.SetMove(_doNothingMove);
            _human2.SetSpeed(1);

            const int initialAttackStrength = 2;
            const int expectedDamage = initialAttackStrength * attackBoostMultiplier;
            _human1.SetStrength(initialAttackStrength);
            _human1.SetMove(attackWithBoost, 1);
            _chanceService.PushAttackHitsNotCrit();
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_enemy1);

            _enemyTeam = new Team(_menuManager, _enemy1);

            _enemy1.SetDefense(0);
            _enemy1.SetHealth(100);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            const int expectedRemainingHealth = 100 - expectedDamage;
            Assert.AreEqual(expectedRemainingHealth, _enemy1.CurrentHealth, "Human1's strength should have been mutliplied, since the dance effect condition was met!");
        }

        [Test]
        public void ConditionalAttackBoostMove_ExecutesAppropriateEffectWhenDanceNotInEffect([Values(DanceEffectType.Water, DanceEffectType.Wind)] DanceEffectType danceEffectType)
        {
            _testTechnique.ClearEffects();
            _testTechnique.SetDanceEffect(DanceEffectType.Soul);

            const int attackBoostMultiplier = 2;
            AttackBoostBattleMoveEffect conditionalAttackBoostEffect = GetAttackBoostEffect(attackBoostMultiplier, danceEffectType);
            AttackBattleMove attackWithBoost = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: conditionalAttackBoostEffect);

            _human2.SetMove(_testTechnique, 1);
            _human2.SetMove(_doNothingMove);
            _human2.SetSpeed(1);

            const int initialAttackStrength = 2;
            const int expectedDamage = initialAttackStrength;
            _human1.SetStrength(initialAttackStrength);
            _human1.SetMove(attackWithBoost, 1);
            _chanceService.PushAttackHitsNotCrit();
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_enemy1);

            _enemyTeam = new Team(_menuManager, _enemy1);

            _enemy1.SetDefense(0);
            _enemy1.SetHealth(100);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            const int expectedRemainingHealth = 100 - expectedDamage;
            Assert.AreEqual(expectedRemainingHealth, _enemy1.CurrentHealth, "Human1's strength should not have been mutliplied, since the dance effect condition was not met!");
        }

        /// <summary>
        /// The only effect of a dance effect is a) the field
        /// </summary>
        [Test]
        public void BattleManager_AppropriatelyTicksDownDanceEffectCounter([Values(DanceEffectType.Heart, DanceEffectType.Danger)] DanceEffectType danceEffectType)
        {
            const int danceDuration = 4;
            
            RestorationBattleMoveEffect conditionalHealEffect = GetRestoreHealthEffect(danceEffectType);
            AttackBattleMove attackWithHeal = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, effects: conditionalHealEffect);

            _testTechnique.ClearEffects();
            _testTechnique.SetDanceEffect(danceEffectType);
            _testTechnique.SetDuration(danceDuration);

            _human1.SetMove(_testTechnique, 1);
            _human1.SetMove(_doNothingMove);
            _human1.SetMoveTarget(_human1);
            _human1.SetSpeed(1);

            _human2.SetHealth(100, 10);
            _human2.SetMove(attackWithHeal, 6);
            for (var i = 0; i < 6; ++i)
            {
                _chanceService.PushEventsOccur(true, false); //attack hits, does not miss
            }
            _human2.SetMove(_runawayMove);
            _human2.SetMoveTarget(_enemy1);
            _human2.SetStrength(_enemy1.Defense);

            _enemy1.SetHealth(100);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _enemy2.SetHealth(100);
            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedHealth = 10 + (danceDuration*10);
            Assert.AreEqual(expectedHealth, _human2.CurrentHealth); //will be 20 higher if the effect doesn't wear off
        }

        [Test]
        public void BattleManager_StopsDanceEffectsIfOwnerDies()
        {
            const int expectedHuman2RemainingHealth = 2;
            const int human2Health = 4;
            int enemy2Strength = (human2Health + _human2.Defense) - expectedHuman2RemainingHealth;
            TestDoNothingMove doNothing1 = (TestDoNothingMove)TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            string dangerDanceMessage = "danced the danger dance";
            doNothing1.SetMessage(dangerDanceMessage);
            TestDanceMove dangerDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            dangerDance.SetDuration(2);
            dangerDance.AddEffect(new StatMultiplierFieldEffect(TargetType.EnemyTeam, "danger dance", StatType.Strength, 2));
            dangerDance.SetDanceEffect(DanceEffectType.Danger);

            dangerDance.AddMove(doNothing1);
            _human1.SetMove(dangerDance);
            _human1.SetMoveTarget(_human1);
            _human1.SetSpeed(2);
            _human1.SetHealth(10);

            _human2.SetHealth(human2Health);
            _human2.SetMove(_doNothingMove, 1);
            //end the battle after the first round
            _human2.SetMove(_runawayMove);
            _human2.SetMoveTarget(_human2);
            _human2.SetSpeed(3);

            _enemy1.SetMove(_basicAttack);
            _enemy1.SetMoveTarget(_human1);
            int human1DefenseAndHealth = _human1.Defense + _human1.MaxHealth;
            int enemy1Strength = human1DefenseAndHealth/2;
            _enemy1.SetStrength(enemy1Strength);
            _enemy1.SetSpeed(1);

            _enemy2.SetMove(_basicAttack);
            _enemy2.SetMoveTarget(_human2);
            _enemy2.SetStrength(enemy2Strength);

            _chanceService.PushEventsOccur(true, false, true, false); //set up attacks hitting and not crit'ing

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(0, _human1.CurrentHealth, "enemy1 attack should have doubled, causing enough damage to kill human1");
            Assert.AreEqual(expectedHuman2RemainingHealth, _human2.CurrentHealth, "The effect raising enemy attack should have been lifted when _human1 died");
        }

        private void CombineDanceMove_Setup(string doNothingMessage = "")
        {
            var doNothing = (TestDoNothingMove)TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            doNothing.SetMessage(doNothingMessage);
            var windDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            windDance.SetDuration(2);
            windDance.SetDanceEffect(DanceEffectType.Wind);
            windDance.AddMove(doNothing);
            windDance.AddMove(doNothing);
            _human1.SetMove(windDance, 1);
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_human1);
            _human1.SetHealth(100, 10);

            var fireDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            fireDance.SetDuration(2);
            fireDance.SetDanceEffect(DanceEffectType.Fire);
            fireDance.AddMove(doNothing);
            fireDance.AddMove(doNothing);
            _human2.SetMove(fireDance);
            _human2.SetMoveTarget(_human2);
            _human2.SetHealth(100, 10);

            _enemy1.SetMove(_doNothingMove);
            _enemy2.SetMove(_doNothingMove);

            CombinedFieldEffect fakeCombinedEffect = new CombinedFieldEffect("fwee", new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "fwee", 80, immediatelyExecuted: true));
            _combiner.AddFakeCombination(DanceEffectType.Wind, DanceEffectType.Fire, fakeCombinedEffect);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                FieldEffectCombiner = _combiner,
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        [Test]
        public void BattleManager_AppropriatelyCombinesDanceMoves()
        {
            CombineDanceMove_Setup();

            Assert.AreEqual(90, _human1.CurrentHealth);
            Assert.AreEqual(90, _human2.CurrentHealth);
        }

        [Test]
        public void TestCombineScreenOutputs()
        {
            const string doNothingMessage = "danced the foo dance";
            CombineDanceMove_Setup(doNothingMessage);
           
            var outputs = _output.GetOutputs();
            
            CombinedFieldEffect combined = _combiner.Combine(DanceEffectType.Wind, DanceEffectType.Fire);
            //The dances go for 2 turns each making 4 "danced the foo dance" messages, 
            //1 more combined field effect "combined to become ____", 1 for field effect message, then 2 more for each "player was healed!"
            int expectedOutputMessages = 5 + combined.Effects.Count + 2;
            Assert.AreEqual(expectedOutputMessages, outputs.Length);

            Assert.AreEqual($"{_human1.DisplayName} {doNothingMessage}\n", outputs[0].Message);
            Assert.AreEqual($"{_human2.DisplayName} {doNothingMessage}\n", outputs[1].Message);

            Assert.AreEqual($"They combined to become {combined.Description}\n", outputs[2].Message);
        }

        [Test]
        public void BattleManager_DoesNotError_IfDanceMovesCannotCombine()
        {
            TestDoNothingMove doNothing1 = (TestDoNothingMove)TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            string dangerDanceMessage = "danced the danger dance";
            doNothing1.SetMessage(dangerDanceMessage);
            var dangerDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            dangerDance.SetDuration(2);
            dangerDance.SetDanceEffect(DanceEffectType.Danger);
            dangerDance.AddMove(doNothing1);
            _human1.SetMove(dangerDance);
            _human1.SetMoveTarget(_human1);
            _human1.SetSpeed(1);

            TestDoNothingMove doNothing2 = (TestDoNothingMove)TestMoveFactory.Get(moveType: BattleMoveType.DoNothing);
            var mindDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            string mindDanceMessage = "danced the mind dance";
            doNothing2.SetMessage(mindDanceMessage);
            mindDance.SetDuration(2);
            mindDance.SetDanceEffect(DanceEffectType.Mind);
            mindDance.AddMove(doNothing2);
            _human2.SetMove(mindDance);
            _human2.SetMoveTarget(_human2);
            _human2.SetSpeed(1);

            _enemy1.SetMove(_basicAttack);
            _enemy1.SetMoveTarget(_human1);
            _enemy1.SetStrength(_human1.Defense + _human1.MaxHealth + 1);
            _enemy2.SetMove(_basicAttack);
            _enemy2.SetMoveTarget(_human2);
            _enemy2.SetStrength(_human2.Defense + _human2.MaxHealth + 1);
            _chanceService.PushEventsOccur(true, false, true, false); //attacks hit, don't crit
            
            var combine = _combiner.Combine(DanceEffectType.Danger, DanceEffectType.Mind);
            Assert.That(combine, Is.Null);
            
            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                FieldEffectCombiner = _combiner,
                ShowIntroAndOutroMessages = false,
                ShowAttackMessages = false,
                ShowDeathMessages = false,
                ShowExpAndLevelUpMessages = false,
                ShowPhysicalDamageMessages = false
            };

            Assert.DoesNotThrow(() => _battleManager.Battle(_humanTeam, _enemyTeam, config: config));

            //ensure dance moves were used before human team was defeated
            MockOutputMessage[] outputs = _output.GetOutputs();
            Assert.AreEqual(2, outputs.Length);
            Assert.AreEqual($"{_human1.DisplayName} {dangerDanceMessage}\n", outputs[0].Message);
            Assert.AreEqual($"{_human2.DisplayName} {mindDanceMessage}\n", outputs[1].Message);
        }

        [Test]
        public void BattleManager_ImmediatelyExecutesFieldEffectsAfterCombining()
        {
            _enemy1.SetHealth(100, 10);
            _enemy2.SetHealth(100, 10);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration();
            _battleManager.SetConfig(config);
            _battleManager.SetHumanTeam(_humanTeam);
            _battleManager.SetEnemyTeam(_enemyTeam);

            var effect = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "foo", 10, immediatelyExecuted: true);
            var combinedEffect = new Battle.FieldEffects.CombinedFieldEffect("foo", effect);
            _battleManager.ImplementEffects(combinedEffect, 10, _enemy1, _enemy2);

            //these values will be 10 if the heal effect wasn't immediately executed
            Assert.AreEqual(20, _enemy1.CurrentHealth);
            Assert.AreEqual(20, _enemy2.CurrentHealth);

            var effects = _battleManager.GetFieldEffects();
            Assert.AreEqual(0, effects.Count);

            _battleManager.TestOnTurnEnd();

            Assert.AreEqual(20, _enemy1.CurrentHealth);
            Assert.AreEqual(20, _enemy2.CurrentHealth);
            effects = _battleManager.GetFieldEffects();
            Assert.AreEqual(0, effects.Count);
        }

        [Test]
        public void BattleManager_AppropriatelyCancelsCombinationEffect_IfEitherOwnerDies()
        {
            var windDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            windDance.SetDuration(2);
            windDance.SetDanceEffect(DanceEffectType.Wind);
            windDance.AddMove(_doNothingMove);
            _human1.SetHealth(100, 10);
            _human1.SetSpeed(1);
            _human1.SetMove(windDance);
            _human1.SetMoveTarget(_human1);

            var fireDance = (TestDanceMove)TestMoveFactory.Get(moveType: BattleMoveType.Dance);
            fireDance.SetDuration(2);
            fireDance.SetDanceEffect(DanceEffectType.Fire);
            fireDance.AddMove(_doNothingMove);
            fireDance.AddMove(_runawayMove);
            _human2.SetHealth(100, 10);
            _human2.SetSpeed(1);
            _human2.SetMove(fireDance);
            _human2.SetMoveTarget(_human2);

            _enemy1.SetStrength(_human1.Defense + _human1.MaxHealth + 1);
            _enemy1.SetMove(_basicAttack);
            _enemy1.SetMoveTarget(_human1);
            _chanceService.PushEventsOccur(true, false); //attack hits, not a crit

            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemy1);

            RestoreHealthPercentageFieldEffect restoreHealthEffect =
                new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "heal dance", 20, 10);
            
            CombinedFieldEffect testCombo = new CombinedFieldEffect("heal dance", restoreHealthEffect);

            _combiner.AddFakeCombination(DanceEffectType.Wind, DanceEffectType.Fire, testCombo);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                FieldEffectCombiner = _combiner
            };
            
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            //will be 30 if the dance effect isn't interrupted
            Assert.AreEqual(10, _human2.CurrentHealth);
        }
    }
}