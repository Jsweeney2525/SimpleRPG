using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.BattleMoves;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    class FieldEffectTests
    {
        private TestEnemyFighter _enemy1, _enemy2;
        private Team _enemyTeam;
        private TestHumanFighter _human1, _human2;
        private TestTeam _humanTeam;
        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        private TestFieldEffectMove _testTechnique;
        private DoNothingMove _doNothingMove;
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
        private List<FieldEffect> _fieldEffects;

        private MagicMultiplierStatus _doubleIceMagicFieldEffectStatus;

        private readonly RestoreHealthPercentageFieldEffect _heal10Percent = 
            new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "foo", 10);

        private readonly StatMultiplierFieldEffect _raiseTeamAttack50Percent =
            new StatMultiplierFieldEffect(TargetType.OwnTeam, "foo", StatType.Strength, 1.5);

        private readonly StatMultiplierFieldEffect _raiseFieldAttack50Percent =
            new StatMultiplierFieldEffect(TargetType.Field, "field", StatType.Strength, 1.5);

        private readonly StatMultiplierFieldEffect _raiseTeamDefense50Percent =
            new StatMultiplierFieldEffect(TargetType.OwnTeam, "foo", StatType.Defense, 1.5);

        private readonly StatMultiplierFieldEffect _lowerEnemyAttack50Percent =
            new StatMultiplierFieldEffect(TargetType.EnemyTeam, "bar", StatType.Strength, 0.5);

        private readonly StatMultiplierFieldEffect _lowerEnemyDefense50Percent =
            new StatMultiplierFieldEffect(TargetType.EnemyTeam, "bar", StatType.Defense,  0.5);

        [SetUp]
        public void SetUp()
        {
            _output = new MockOutput();
            _input = new MockInput();
            _menuManager = new TestMenuManager(_input, _output);

            _chanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_chanceService);

            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            _enemy1 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Test A");
            _enemy2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Test B");

            _enemyTeam = new Team(_menuManager, _enemy1, _enemy2);

            _human1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _human2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            _humanTeam = new TestTeam(_human1, _human2);

            _testTechnique = (TestFieldEffectMove)TestMoveFactory.Get(TargetType.Field, moveType: BattleMoveType.Field);
            _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);

            _doubleIceMagicFieldEffectStatus = new MagicMultiplierStatus(1, MagicType.Ice, 2.0);

            _fieldEffects = new List<FieldEffect>
            {
                new CriticalChanceMultiplierFieldEffect(TargetType.OwnTeam, "test", 2, 3)
                ,new MagicAttackFieldEffect(TargetType.EnemyTeam, "test", MagicType.Fire, 2, 3)
                ,new MagicMultiplierFieldEffect(TargetType.OwnTeam, "test", MagicType.Fire, 2, 3)
                ,new MagicResistanceMultiplierFieldEffect(TargetType.EnemyTeam, "test", MagicType.Earth, 0.5, 3)
                ,new ReflectFieldEffect(TargetType.OwnTeam, "test", MagicType.Fire, 3)
                ,new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "test", 10, 3)
                ,new RestoreManaPercentageFieldEffect(TargetType.OwnTeam, "test", 10, 3)
                ,new ShieldFieldEffect(TargetType.OwnTeam, "test", new ElementalBattleShield(10, 3, 0, MagicType.Fire))
                ,new SpellCostMultiplierFieldEffect(TargetType.EnemyTeam, "test", 3, 3)
                ,new StatMultiplierFieldEffect(TargetType.OwnTeam, "test", StatType.Strength, 2, 3)
                ,new StatusFieldEffect(TargetType.OwnTeam, "test", _doubleIceMagicFieldEffectStatus)
                ,new UndoDebuffsFieldEffect(TargetType.OwnTeam, "test")
            };
        }

        [TearDown]
        public void TearDown()
        {
            _human1 = null;
            _human2 = null;
            _humanTeam = null;

            _enemy1 = null;
            _enemy2 = null;
            _enemyTeam = null;

            _testTechnique = null;
            _fieldEffects = null;

            _chanceService = null;
            _output = null;
        }

        #region generic field effect tests

        [Test]
        public void TestMoveFactory_ReturnsFieldEffectTechnique()
        {
            Assert.AreEqual(BattleMoveType.Field, _testTechnique.MoveType);
            Assert.AreEqual(2, _testTechnique.EffectDuration);
        }

        [Test]
        public void FieldEffectMoveConstructor_AppropriatelyChangesEffectDuration([Values(1, 3)] int moveDuration, [Values(true, false)] bool shouldEffectHaveOwnDuration)
        {
            int? effectDuration = shouldEffectHaveOwnDuration ? (int?)moveDuration + 2 : null;
            FieldEffect effect = new CriticalChanceMultiplierFieldEffect(TargetType.Field, "foo", 75, effectDuration);

            Assert.AreEqual(effectDuration, effect.EffectDuration);

            FieldEffectMove move = new FieldEffectMove("foo", TargetType.Field, moveDuration, null, effect);

            FieldEffect alteredEffect = move.FieldEffects[0];

            Assert.AreEqual(moveDuration, alteredEffect.EffectDuration);
        }

        [Test]
        public void FieldEffectMoveConstructor_CopiesEffects()
        {
            FieldEffect effect = new CriticalChanceMultiplierFieldEffect(TargetType.Field, "foo", 75, 2);
            FieldEffectMove move = new FieldEffectMove("foo", TargetType.Field, 2, null, effect);

            FieldEffect moveEffect = move.FieldEffects[0];

            Assert.IsTrue(effect.AreEqual(moveEffect), "the fieldEffectMove's effect should have the same values as the original effect");
            Assert.AreNotEqual(effect, moveEffect, "effects should be copied by value, not by reference");
        }

        [Test]
        public void BattleManager_AppropriatelyExecutesFieldTechnique_FieldTargetType()
        {
            StatMultiplierStatus raiseAttackStatus = new StatMultiplierStatus(2, StatType.Strength, 1.5);
            StatusFieldEffect statusFieldEffect = new StatusFieldEffect(TargetType.Field, "foo dance!", raiseAttackStatus);
            _testTechnique.AddEffect(statusFieldEffect);

            _human1.SetHealth(10);
            _human1.SetMove(_testTechnique, 1);
            _human1.SetMoveTarget(_human1);
            _human1.SetMove(_runawayMove);

            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);

            _human2.SetStrength(2); //should do 3 damage
            _human2.SetMove(attack);
            _chanceService.PushEventsOccur(true, false); //hits but not crit
            _human2.SetMoveTarget(_enemy1);

            foreach (TestEnemyFighter enemy in _enemyTeam.Fighters)
            {
                enemy.SetHealth(5);
                enemy.SetMove(attack);
                _chanceService.PushEventsOccur(true, false); //hits but not crit
                enemy.SetStrength(2);
                enemy.SetMoveTarget(_human1);
            }

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(4, _human1.CurrentHealth);
            Assert.AreEqual(2, _enemy1.CurrentHealth);
        }

        [Test]
        public void BattleManager_AppropriatelyTicksDownFieldEffectCounter()
        {
            RestorePercentageStatus healStatus = new RestorePercentageStatus(2, RestorationType.Health, .1);
            StatusFieldEffect statusFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "love dance", healStatus);
            _testTechnique.AddEffect(statusFieldEffect);

            foreach (var enemy in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                enemy.SetHealth(100, 10);
            }

            _human1.SetMove(_doNothingMove, 5);
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_human1);

            _human2.SetMove(_doNothingMove);
            _human2.SetMoveTarget(_human2);

            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _enemy2.SetMove(_testTechnique, 1);
            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            foreach (var enemy in _enemyTeam.Fighters)
            {
                Assert.AreEqual(30, enemy.CurrentHealth, $"enemies should have 30 health after the turn's healing, but they have {enemy.CurrentHealth} health instead");
            }
        }

        [Test]
        public void BattleManager_AppropriatelyResetsEffectCounterIfMoveUsedAgain([Values(RestorationType.Health, RestorationType.Mana)] RestorationType restorationType)
        {
            RestorePercentageStatus healStatus = new RestorePercentageStatus(2, restorationType, .1);
            StatusFieldEffect statusFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "love dance", healStatus);
            _testTechnique.AddEffect(statusFieldEffect);

            foreach (var enemy in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                switch (restorationType)
                {
                    case RestorationType.Health:
                        enemy.SetHealth(100, 10);
                        break;
                    case RestorationType.Mana:
                        enemy.SetMana(100, 10);
                        break;
                }
            }

            _human1.SetMove(_doNothingMove, 5);
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_human1);

            _human2.SetMove(_doNothingMove);
            _human2.SetMoveTarget(_human2);

            _enemy1.SetMove(_testTechnique, 2); //will use the technique on both turn 1 and turn 2, for a total of 3 heals
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            string healthOrMana = restorationType == RestorationType.Health ? "health" : "mana";

            foreach (var enemy in _enemyTeam.Fighters)
            {
                int actualValue = restorationType == RestorationType.Health ? enemy.CurrentHealth : enemy.CurrentMana;
                Assert.AreEqual(40, actualValue, $"enemies should have 40 {healthOrMana} after the turn's healing, but they have {actualValue} {healthOrMana} instead");
            }
        }

        [Test]
        public void BattleManager_AppropriatelyResetsEffectCounterIfMoveUsedAgain_FieldTargetType([Values(5, 10)] int healPercentage)
        {
            _testTechnique.SetDuration(4);

            RestoreHealthPercentageFieldEffect healMove = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "foo", healPercentage);
            _testTechnique.AddEffect(healMove);
            
            _human1.SetHealth(100, 10);
            _human1.SetMoveTarget(_human1);
            _human1.SetMove(_testTechnique, 1);
            _human1.SetMove(_doNothingMove, 1); //let the heal effect be in place for 2 turns before resetting
            _human1.SetMove(_testTechnique, 1);
            _human1.SetMove(_doNothingMove, 8);
            _human1.SetMove(_runawayMove);

            _human2.SetHealth(100, 10);
            _human2.SetMove(_doNothingMove);
            _human2.SetMoveTarget(_human2);

            _enemy1.SetMove(_doNothingMove);
            _enemy2.SetMove(_doNothingMove);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedHealth = (healPercentage*6) + 10; //starts at 10, 6 total rounds of being healed
            Assert.AreEqual(expectedHealth, _human1.CurrentHealth);
            Assert.AreEqual(expectedHealth, _human2.CurrentHealth);
        }

        #endregion generic field effect tests

        #region testing each effect individually

        [Test]
        public void TestRestoreHealthEffect([Values(20, 40, 60)] int restoreHealthPercentage, [Values(true, false)] bool testOverHeal)
        {
            int initialHealth = testOverHeal ? (100 - restoreHealthPercentage + 10) : 10;
            RestoreHealthPercentageFieldEffect healHealthEffect = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "foo", restoreHealthPercentage);
            _testTechnique.AddEffect(healHealthEffect);

            int i = 0;
            foreach (var enemy in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                enemy.SetHealth(100, initialHealth);
                BattleMove moveToExecute = (i == 0) ? (BattleMove)_testTechnique : _doNothingMove;
                enemy.SetMove(moveToExecute);
                enemy.SetMoveTarget(enemy);
            }

            foreach (TestHumanFighter humanFighter in _humanTeam.Fighters.Cast<TestHumanFighter>())
            {
                humanFighter.SetMove(_doNothingMove);
                humanFighter.SetMoveTarget(humanFighter);
            }

            _humanTeam.SetDeathsOnRoundEndEvent();

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedCurrentHealth = Math.Min(100, initialHealth + restoreHealthPercentage);

            foreach (var enemy in _enemyTeam.Fighters)
            {
                Assert.AreEqual(expectedCurrentHealth, enemy.CurrentHealth);
            }
        }

        [Test]
        public void TestRestoreHealthEffect_ScreenOutputs([Values(true, false)] bool isHumanEffect, [Values(1, 2)] int effectDuration)
        {
            _testTechnique.SetDuration(effectDuration);
            RestoreHealthPercentageFieldEffect healEffect = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "", 10, effectDuration);
            _testTechnique.AddEffect(healEffect);

            IFighter techniqueExecutor = isHumanEffect ? (IFighter)_human1 : _enemy1;
            ITestFighter testFighterExecutor = (ITestFighter) techniqueExecutor;

            if (testFighterExecutor == null)
            {
                throw new InvalidOperationException("impossible! A testFighter could not be cast to type ITestFighter");
            }

            testFighterExecutor.SetMove(_testTechnique);
            testFighterExecutor.SetMoveTarget(techniqueExecutor);

            if (isHumanEffect)
            {
                _enemy1.SetMove(_doNothingMove);
            }
            else
            {
                _human1.SetMove(_doNothingMove);
                _human1.SetMoveTarget(_human1);
            }

            _human2.SetMove(_doNothingMove, 1);
            _human2.SetMove(_runawayMove);
            _human2.SetMoveTarget(_human2);

            _enemy2.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            //initial status message, then 1 for each player who is healed
            int expectedOutputLength = 1 + _humanTeam.Fighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length); 

            string teamString = isHumanEffect ? "Your team" : "Enemy team";
            string durationString = effectDuration == 1 ? "1 turn" : $"{effectDuration} turns";

            string expectedOutput = $"{teamString} has gained regen status for {durationString}!\n";

            //use testTechnique's effect Duration because heal10Percent has null for its duration
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void TestRestoreManaEffect([Values(15, 20, 40)] int percentageAmount, [Values(true, false)] bool testOverHeal)
        {
            int startingMana = testOverHeal ? 100 - (percentageAmount - 10) : 0;
            var restoreManaEffect = new RestoreManaPercentageFieldEffect(TargetType.OwnTeam, "foo", percentageAmount);
            _testTechnique.AddEffect(restoreManaEffect);

            int i = 0;
            foreach (var enemy in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                enemy.SetMana(100, startingMana);
                BattleMove moveToExecute = (i == 0) ? (BattleMove)_testTechnique : _doNothingMove;
                enemy.SetMove(moveToExecute);
                enemy.SetMoveTarget(enemy);
            }

            foreach (TestHumanFighter humanFighter in _humanTeam.Fighters.Cast<TestHumanFighter>())
            {
                humanFighter.SetMove(_doNothingMove);
                humanFighter.SetMoveTarget(humanFighter);
            }

            _humanTeam.SetDeathsOnRoundEndEvent();

            _battleManager.Battle(_humanTeam, _enemyTeam);

            int expectedCurrentMana = Math.Min(startingMana + percentageAmount, 100);
            foreach (var enemy in _enemyTeam.Fighters)
            {
                Assert.AreEqual(expectedCurrentMana, enemy.CurrentMana);
            }
        }

        [Test]
        public void TestRestoreManaEffect_ScreenOutputs([Values(true, false)] bool isHumanEffect,
            [Values(1, 2)] int effectDuration)
        {
            _testTechnique.SetDuration(effectDuration);
            RestoreManaPercentageFieldEffect restoreEffect = new RestoreManaPercentageFieldEffect(TargetType.OwnTeam,
                "", 10, effectDuration);
            _testTechnique.AddEffect(restoreEffect);

            IFighter techniqueExecutor = isHumanEffect ? (IFighter) _human1 : _enemy1;
            ITestFighter testFighterExecutor = (ITestFighter) techniqueExecutor;

            if (testFighterExecutor == null)
            {
                throw new InvalidOperationException("impossible! A testFighter could not be cast to type ITestFighter");
            }

            testFighterExecutor.SetMove(_testTechnique);
            testFighterExecutor.SetMoveTarget(techniqueExecutor);

            if (isHumanEffect)
            {
                _enemy1.SetMove(_doNothingMove);
            }
            else
            {
                _human1.SetMove(_doNothingMove);
                _human1.SetMoveTarget(_human1);
            }

            _human2.SetMove(_doNothingMove, 1);
            _human2.SetMove(_runawayMove);
            _human2.SetMoveTarget(_human2);

            _enemy2.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            string teamString = isHumanEffect ? "Your team" : "Enemy team";
            string durationString = effectDuration == 1 ? "1 turn" : $"{effectDuration} turns";

            string expectedOutput = $"{teamString} has gained mana regen status for {durationString}!\n";

            //use testTechnique's effect Duration because heal10Percent has null for its duration
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        /// <summary>
        /// A helper method used in both the set up and the assertions for StatMultiplierFieldEffect
        /// </summary>
        /// <param name="humanTeamEffect"></param>
        /// <param name="baseSpeed"></param>
        /// <param name="multiplier"></param>
        private bool TestSpeedMultiplierEffect_IsHumanFaster(bool humanTeamEffect, int baseSpeed, double multiplier)
        {
            int calculatedHumanSpeed = humanTeamEffect ? (int)(baseSpeed * multiplier) : baseSpeed;
            int calculatedEnemySpeed = humanTeamEffect ? baseSpeed : (int)(baseSpeed * multiplier);

            return calculatedHumanSpeed > calculatedEnemySpeed;
        }

        private void TestStatMultiplierEffect_Setup(double multiplier, bool humanTeamEffect, StatType statMultiplierType, int attackerStrength, int defenseStrength, int baseSpeedStat)
        {
            StatMultiplierStatus status = new StatMultiplierStatus(1, statMultiplierType, multiplier);
            TargetType statusTargetType = humanTeamEffect ? TargetType.OwnTeam : TargetType.EnemyTeam;
            StatusFieldEffect fieldEffect = new StatusFieldEffect(statusTargetType, "foo", status);
            _testTechnique.AddEffect(fieldEffect);

            _human1.SetSpeed(baseSpeedStat + 1);
            _human1.SetMove(_testTechnique, 1);
            _human1.SetMove(_runawayMove);
            _human1.SetMoveTarget(_human1);
            _human1.SetStrength(attackerStrength);

            int humanHealth = 100;
            int enemyHealth = 100;
            int humanDefense = defenseStrength;
            int enemyDefense = defenseStrength;

            if (statMultiplierType == StatType.Speed)
            {
                if (TestSpeedMultiplierEffect_IsHumanFaster(humanTeamEffect, baseSpeedStat, multiplier))
                {
                    enemyHealth = 1;
                    enemyDefense = 0;
                }
                else
                {
                    humanHealth = 1;
                    humanDefense = 0;
                }
            }

            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);
            _human2.SetHealth(humanHealth);
            _human2.SetStrength(attackerStrength);
            _human2.SetDefense(humanDefense);
            _human2.SetSpeed(baseSpeedStat);
            _human2.SetMove(attack);
            _human2.SetMoveTarget(_enemy1);
            _chanceService.PushEventsOccur(true, false); //attack hits, doesn't miss

            _enemy1.SetHealth(enemyHealth);
            _enemy1.SetStrength(attackerStrength);
            _enemy1.SetDefense(enemyDefense);
            _enemy1.SetSpeed(baseSpeedStat);
            _enemy1.SetMove(attack);
            _enemy1.SetMoveTarget(_human2);
            _chanceService.PushEventsOccur(true, false); //attack hits, doesn't miss

            _enemyTeam.Remove(_enemy2);

            _battleManager.SuppressBattleIntroAndOutroMessages();

            _battleManager.Battle(_humanTeam, _enemyTeam);
        }

        [Test]
        public void TestStatMultiplierEffect([Values(1.0/3.0, 3.0)] double multiplier, [Values(true, false)] bool humanTeamEffect, 
            [Values(StatType.Strength, StatType.Defense, StatType.Speed)] StatType statType)
        {
            const int attackStrength = 15;
            const int defenseStrength = 3;
            const int baseSpeedStat = 3;

            TestStatMultiplierEffect_Setup(multiplier, humanTeamEffect, statType, attackStrength, defenseStrength, baseSpeedStat);

            int calculatedHumanAttackStrength = attackStrength;
            int calculatedEnemyAttackStrength = attackStrength;

            if (statType == StatType.Strength)
            {
                if (humanTeamEffect)
                {
                    calculatedHumanAttackStrength = (int) (attackStrength*multiplier);
                }
                else
                {
                    calculatedEnemyAttackStrength = (int)(attackStrength * multiplier);
                }
            }

            int calculatedHumanDefenseStrength = defenseStrength;
            int calculatedEnemyDefenseStrength = defenseStrength;

            if (statType == StatType.Defense)
            {
                if (humanTeamEffect)
                {
                    calculatedHumanDefenseStrength = (int)(defenseStrength * multiplier);
                }
                else
                {
                    calculatedEnemyDefenseStrength = (int)(defenseStrength * multiplier);
                }
            }
            

            int expectedDamageToEnemy = Math.Max(0, calculatedHumanAttackStrength - calculatedEnemyDefenseStrength);
            int expectedEnemyHealth = _enemy1.MaxHealth - expectedDamageToEnemy;

            int expectedDamageToHuman = Math.Max(0, calculatedEnemyAttackStrength - calculatedHumanDefenseStrength);
            int expectedHumanHealth = _human2.MaxHealth - expectedDamageToHuman;

            if (statType == StatType.Speed)
            {
                if (TestSpeedMultiplierEffect_IsHumanFaster(humanTeamEffect, baseSpeedStat, multiplier))
                {
                    expectedHumanHealth = _human2.MaxHealth;
                    expectedEnemyHealth = 0;
                }
                else
                {
                    expectedHumanHealth = 0;
                    expectedEnemyHealth = _enemy1.MaxHealth;
                }
            }

            Assert.AreEqual(expectedEnemyHealth, _enemy1.CurrentHealth);
            Assert.AreEqual(expectedHumanHealth, _human2.CurrentHealth);
        }

        [Test]
        public void TestStatMultiplierEffect_ScreenOutputs([Values(1, 3)] int statusDuration, 
            [Values(.5, 2)] double multiplier, 
            [Values(true, false)] bool humanTeamEffect,
            [Values(StatType.Strength, StatType.Defense, StatType.Speed)] StatType statType)
        {
            StatMultiplierStatus status = new StatMultiplierStatus(statusDuration, statType, multiplier);
            StatusFieldEffect increaseAttackFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", status);
            _testTechnique.AddEffect(increaseAttackFieldEffect);

            BattleMove enemyMove = humanTeamEffect ? (BattleMove)_doNothingMove : _testTechnique;

            _enemyTeam = new Team(_menuManager, _enemy1);
            _enemy1.SetMove(enemyMove);
            _enemy1.SetMoveTarget(_enemy1);

            _humanTeam = new TestTeam(_human1);
            _humanTeam.SetDeathsOnRoundEndEvent();

            BattleMove humanMove = humanTeamEffect ? (BattleMove)_testTechnique : _doNothingMove;
            _human1.SetMove(humanMove);
            _human1.SetMoveTarget(_human1);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            List<IFighter> fighters = humanTeamEffect ? _humanTeam.Fighters : _enemyTeam.Fighters;

            int expectedOutputsLength = fighters.Count;
            Assert.AreEqual(expectedOutputsLength, outputs.Length);
            
            string expectedVerb = multiplier > 1 ? "raised" : "lowered";
            string turnOrTurns = statusDuration == 1 ? "turn" : "turns";

            int i = 0;
            foreach (IFighter fighter in fighters)
            {
                string expectedOutput =
                    $"{fighter.DisplayName} had their {statType.ToString().ToLower()} {expectedVerb} for {statusDuration} {turnOrTurns}!\n";

                Assert.AreEqual(expectedOutput, outputs[i++].Message);
            }
        }

        [Test]
        public void TestCriticalChanceEffect()
        {
            CriticalChanceMultiplierStatus status = new CriticalChanceMultiplierStatus(1, 2);
            StatusFieldEffect fieldEffect = new StatusFieldEffect(TargetType.EnemyTeam, "foo", status);
            _testTechnique.AddEffect(fieldEffect);

            _enemyTeam = new Team(_menuManager, _enemy1);
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetMoveTarget(_enemy1);
            //assure enemy goes first so when the human attacks, its attack is already raised
            _enemy1.SetSpeed(_human1.Speed + 1);
            _enemy1.SetHealth(10);

            _humanTeam = new TestTeam(_human1);
            AttackBattleMove attack = (AttackBattleMove)MoveFactory.Get(BattleMoveType.Attack);
            double expectedCritChance = (attack.CritChance*2)/100.0;

            _human1.SetStrength(1);
            _human1.SetMove(attack);
            _human1.SetMoveTarget(_enemy1);
            _human1.SetDeathOnTurnEndEvent();

            _chanceService.PushEventOccurs(true); //attack hits
            _chanceService.PushEventOccurs(false); //attack is not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            List<double> chanceValues = _chanceService.LastChanceVals;

            Assert.AreEqual(2, chanceValues.Count); //first for whether the attack hits, second for if it was a crit
            Assert.AreEqual(expectedCritChance, chanceValues[1]);
        }

        [Test]
        public void TestCriticalChanceEffect_ScreenOutputs()
        {
            const int statusDuration = 3;
            CriticalChanceMultiplierStatus increaseCritStatus = new CriticalChanceMultiplierStatus(statusDuration, 2);
            StatusFieldEffect increaseCritFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", increaseCritStatus);
            _testTechnique.AddEffect(increaseCritFieldEffect);
            CriticalChanceMultiplierStatus decreaseCritStatus = new CriticalChanceMultiplierStatus(statusDuration, 0.5);
            StatusFieldEffect decreaseCritFieldEffect = new StatusFieldEffect(TargetType.EnemyTeam, "foo", decreaseCritStatus);
            _testTechnique.AddEffect(decreaseCritFieldEffect);

            _enemyTeam = new Team(_menuManager, _enemy1);
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetMoveTarget(_enemy1);
            _enemy1.SetSpeed(_human1.Speed + 1);

            _humanTeam = new TestTeam(_human1);
            
            _human1.SetMove(_doNothingMove);
            _human1.SetMoveTarget(_human1);
            _human1.SetDeathOnTurnEndEvent();

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowDeathMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            List<IFighter> humanFighters = _humanTeam.Fighters;
            List<IFighter> enemyFighters = _enemyTeam.Fighters;

            int expectedMessagesCount = humanFighters.Count + enemyFighters.Count;
            Assert.AreEqual(expectedMessagesCount, outputs.Length);

            int i = 0;

            foreach (IFighter fighter in enemyFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has an increased chance for scoring critical hits for {statusDuration} turns!\n", outputs[i++].Message);
            }

            //Assert.AreEqual($"Your team has a decreased chance for scoring critical hits for {statusDuration} turns!\n", outputs[1].Message);
        }
        
        private void TestMagicalMultiplierEffect_Setup(MagicType multiplierType, int statusDuration, int spellPower, double teamMultiplier, double enemyMultiplier, IEnumerable<MagicType> magicTypes)
        {
            Status increaseTeamMagicStatus = new MagicMultiplierStatus(statusDuration, multiplierType, teamMultiplier);
            var raiseTeamMagic = new StatusFieldEffect(TargetType.OwnTeam, "foo", increaseTeamMagicStatus);
            _testTechnique.AddEffect(raiseTeamMagic);
            Status decreaseEnemyTeamMagicStatus = new MagicMultiplierStatus(statusDuration, multiplierType, enemyMultiplier);
            var lowerEnemyMagic = new StatusFieldEffect(TargetType.EnemyTeam, "foo", decreaseEnemyTeamMagicStatus);
            _testTechnique.AddEffect(lowerEnemyMagic);

            List<HumanFighter> humanFighters = new List<HumanFighter>();
            List<IFighter> enemyFighters = new List<IFighter>();
            TestHumanFighter fighter;

            int i = 0;

            foreach (MagicType mType in magicTypes)
            {
                string mTypeString = mType.ToString();
                fighter = new TestHumanFighter("Bill" + i++, 1);
                TestEnemyFighter enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);

                Spell spell = new Spell(mTypeString, mType, SpellType.Attack, TargetType.SingleEnemy, 0, spellPower);

                enemy.SetAppendText(mTypeString);
                enemy.SetHealth(10);
                enemy.SetMagicStrength(0);
                enemy.AddSpell(spell);
                enemy.SetMove(spell);
                enemy.SetMoveTarget(fighter);

                fighter.SetHealth(10);
                fighter.SetMagicStrength(0);
                fighter.AddSpell(spell);
                if (i == 1) //i was already incremented above, 1 is the first iteration through the loop
                {
                    fighter.SetMove(spell, 1);
                    fighter.SetMove(_runawayMove);
                }
                else
                {
                    fighter.SetMove(spell);
                }
                fighter.SetMoveTarget(enemy);


                enemyFighters.Add(enemy);
                humanFighters.Add(fighter);
            }

            fighter = new TestHumanFighter("Tom", 1);
            fighter.SetMove(_testTechnique);
            fighter.SetSpeed(2);
            humanFighters.Add(fighter);

            _humanTeam = new TestTeam(humanFighters);
            _enemyTeam = new Team(_menuManager, enemyFighters);
            
            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowCastSpellMessages = false,
                ShowMagicalDamageMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        [Test]
        public void TestMagicalMultiplierEffect_IndividualElements([Values(MagicType.Lightning, MagicType.Earth, MagicType.Ice)] MagicType multiplierType)
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();
            const double enemyTeamMultiplier = (3.0 / 4.0);
            const double humanTeamMultiplier = 2;
            const int spellPower = 4;

            TestMagicalMultiplierEffect_Setup(multiplierType, 1, spellPower, humanTeamMultiplier, enemyTeamMultiplier, magicTypes);

            int magicTypesLength = magicTypes.Count;

            for (int i = 0; i < magicTypesLength; ++i)
            {
                MagicType magicType = magicTypes[i];
                double humanMultiplierForElement = (magicType == multiplierType) ? humanTeamMultiplier : 1;
                int expectedDamageDealtByHuman = (int) (spellPower*humanMultiplierForElement);
                int expectedEnemyHealth = 10 - expectedDamageDealtByHuman;
                IFighter enemy = _enemyTeam.Fighters[i];

                Assert.AreEqual(expectedEnemyHealth, enemy.CurrentHealth, $"enemy {enemy.DisplayName} should have {expectedEnemyHealth} health after being attacked, but had {enemy.CurrentHealth} instead");
            }
        }

        [Test]
        public void TestMagicalMultiplierEffect_IndividualElements_ScreenOutputs(
            [Values(MagicType.Lightning, MagicType.Earth, MagicType.Ice)] MagicType multiplierType,
            [Values(1, 3)] int numberOfTurns)
        {
            IEnumerable<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes();

            const double enemyTeamMultiplier = (3.0/4.0);
            const double humanTeamMultiplier = 2;
            const int spellPower = 4;

            TestMagicalMultiplierEffect_Setup(multiplierType, numberOfTurns, spellPower, humanTeamMultiplier,
                enemyTeamMultiplier, magicTypes);

            List<IFighter> humanFighters = _humanTeam.Fighters;
            List<IFighter> enemyFighters = _enemyTeam.Fighters;

            var outputs = _output.GetOutputs();

            int expectedOutputLength = humanFighters.Count + enemyFighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string multiplierTypeString = multiplierType.ToString().ToLower();
            string turnOrTurnsString = numberOfTurns == 1 ? "turn" : "turns";

            int i = 0;

            foreach (IFighter fighter in humanFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has strengthened {multiplierTypeString} magic for {numberOfTurns} {turnOrTurnsString}!\n", outputs[i++].Message);
            }
            foreach (IFighter fighter in enemyFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has weakened {multiplierTypeString} magic for {numberOfTurns} {turnOrTurnsString}!\n", outputs[i++].Message);
            }
        }

        [Test]
        public void TestMagicalMultiplierEffect_AllElements()
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();
            const double enemyMultiplier = (3.0 / 4.0);
            const double teamMultiplier = 2;
            const int spellPower = 4;

            TestMagicalMultiplierEffect_Setup(MagicType.All, 1, spellPower, teamMultiplier, enemyMultiplier, magicTypes);

            for (var i = 0; i < magicTypes.Count; ++i)
            {
                HumanFighter fighter = _humanTeam.Fighters[i] as HumanFighter;
                EnemyFighter enemy = _enemyTeam.Fighters[i] as EnemyFighter;

                int expectedDamageDealtByHumanFighter = spellPower*2;
                int expectedEnemyHealth = enemy.MaxHealth - expectedDamageDealtByHumanFighter;

                Assert.AreEqual(expectedEnemyHealth, enemy.CurrentHealth, $"EnemyFighter {enemy.DisplayName} had {enemy.CurrentHealth} health instead of the expected {expectedEnemyHealth}");

                int expectedDamageDealtByEnemyFighter = (int)(spellPower* enemyMultiplier);
                int expectedHumanHealth = fighter.MaxHealth - expectedDamageDealtByEnemyFighter;

                Assert.AreEqual(expectedHumanHealth, fighter.CurrentHealth, $"HumanFighter {fighter.DisplayName} had {fighter.CurrentHealth} health instead of the expected {expectedHumanHealth}");
            }
        }

        [Test]
        public void TestMagicalMultiplierEffect_AllElement_ScreenOutputs([Values(1, 3)] int statusDuration)
        {
            IEnumerable<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes();
            const double enemyMultiplier = (3.0/4.0);
            const double teamMultiplier = 2;
            const int spellPower = 2;

            TestMagicalMultiplierEffect_Setup(MagicType.All, statusDuration, spellPower, teamMultiplier, enemyMultiplier,
                magicTypes);

            var outputs = _output.GetOutputs();

            List<IFighter> humanFighters = _humanTeam.Fighters;
            List<IFighter> enemyFighters = _enemyTeam.Fighters;

            int expectedOutputLength = humanFighters.Count + enemyFighters.Count;
                //2 + (4*magicTypes.Count); //2 status messages, plus a message for attack and damage dealt for each element twice (enemy and human spells)

            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string turnOrTurns = statusDuration == 1 ? "turn" : "turns";

            int i = 0;

            foreach (IFighter fighter in humanFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has strengthened magic for {statusDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }

            foreach (IFighter fighter in enemyFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has weakened magic for {statusDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }
        }

        private void TestMagicalResistanceMultiplierEffect_Setup(MagicType multiplierType, int statusDuration, int spellPower,
            int humanResistance, double teamMultiplier, int enemyResistance, double enemyMultiplier,
           IEnumerable<MagicType> magicTypes)
        {
            MagicResistanceMultiplierStatus teamResistanceMultiplierStatus = new MagicResistanceMultiplierStatus(statusDuration, multiplierType, teamMultiplier);
            StatusFieldEffect teamResistanceMultiplierEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", teamResistanceMultiplierStatus);
            _testTechnique.AddEffect(teamResistanceMultiplierEffect);

            if (Math.Abs(enemyMultiplier - 1.0) > .01)
            {
                MagicResistanceMultiplierStatus enemyResistanceMultiplierStatus =
                    new MagicResistanceMultiplierStatus(statusDuration, multiplierType, enemyMultiplier);
                StatusFieldEffect enemyResistanceMultiplierEffect = new StatusFieldEffect(TargetType.EnemyTeam, "foo",
                    enemyResistanceMultiplierStatus);
                _testTechnique.AddEffect(enemyResistanceMultiplierEffect);
            }

            List<HumanFighter> humanFighters = new List<HumanFighter>();
            List<IFighter> enemyFighters = new List<IFighter>();
            TestHumanFighter fighter;
            TestEnemyFighter enemy;
            int i = 0;

            foreach (MagicType mType in magicTypes)
            {
                string mTypeString = mType.ToString();
                fighter = new TestHumanFighter("Bill" + i, 1);
                enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
                Spell spell = new Spell(mTypeString, mType, SpellType.Attack, TargetType.SingleEnemy, 0, spellPower);

                enemy.SetAppendText(mTypeString);
                enemy.SetHealth(100);
                enemy.SetMagicStrength(0);
                enemy.SetResistanceBonus(mType, enemyResistance);
                enemy.AddSpell(spell);
                enemy.SetMove(spell);
                enemy.SetMoveTarget(fighter);

                fighter.SetHealth(100);
                fighter.SetMagicStrength(0);
                fighter.SetResistanceBonus(mType, humanResistance);
                fighter.AddSpell(spell);
                fighter.SetMoveTarget(enemy);

                if (i == 0)
                {
                    fighter.SetMove(spell, 1);
                    fighter.SetMove(_runawayMove);
                }
                else
                {
                    fighter.SetMove(spell);
                }

                enemyFighters.Add(enemy);
                humanFighters.Add(fighter);

                ++i;
            }

            fighter = new TestHumanFighter("Bill" + i, 1);
            fighter.SetSpeed(1);
            fighter.SetMove(_testTechnique);
            fighter.SetMoveTarget(fighter);
            humanFighters.Add(fighter);

            _humanTeam = new TestTeam(humanFighters);
            _enemyTeam = new Team(_menuManager, enemyFighters);

            _battleManager.SuppressBattleIntroAndOutroMessages();
            _battleManager.SuppressCastSpellMessages();
            _battleManager.SuppressMagicalDamageMessages();
            _battleManager.Battle(_humanTeam, _enemyTeam);

        }

        [Test]
        public void TestMagicalResistanceMultiplierEffect_IndividualElements([Values(MagicType.Fire, MagicType.Water, MagicType.Wind)] MagicType magicType,
            [Values(2.0, 0.5)] double resistanceMultiplier)
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();
            const int spellPower = 10;
            const int humanResistance = 2;
            const int enemyResistance = 1;
            const double enemyResistanceMultiplier = 1.0;

            TestMagicalResistanceMultiplierEffect_Setup(magicType, 1, spellPower, humanResistance, resistanceMultiplier, enemyResistance, enemyResistanceMultiplier, magicTypes);

            for (var idx = 0; idx < magicTypes.Count; idx++)
            {
                MagicType mType = magicTypes[idx];
                IFighter fighter = _humanTeam.Fighters[idx];

                double calculatedMultiplier = (mType == magicType) ? resistanceMultiplier : 1.0;
                int calculatedResistance = (int) (humanResistance*calculatedMultiplier);
                int expectedDamage = spellPower - calculatedResistance;
                int expectedRemainingHealth = fighter.MaxHealth - expectedDamage;

                Assert.AreEqual(expectedRemainingHealth, fighter.CurrentHealth, $"humanFighter {fighter.DisplayName} was left with {fighter.CurrentHealth} HP instead of {expectedRemainingHealth} after a {mType} magic attack!");
            }
        }

        [Test]
        public void TestMagicalResistanceMultiplierEffect_IndividualElements_ScreenOutputs(
            [Values(MagicType.Earth, MagicType.Ice, MagicType.Lightning)] MagicType magicType,
            [Values(2.0, 0.5)] double resistanceMultiplier, [Values(1, 4)] int numberOfTurns)
        {
            IEnumerable<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes();

            TestMagicalResistanceMultiplierEffect_Setup(magicType, numberOfTurns, 5, 2, resistanceMultiplier, 1, 1.0,
                magicTypes);

            MockOutputMessage[] outputs = _output.GetOutputs();

            List<IFighter> humanFighters = _humanTeam.Fighters;

            int expectedOutputLength = humanFighters.Count;

            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string turnOrTurns = numberOfTurns == 1 ? "turn" : "turns";
            string strengthenedOrWeakened = resistanceMultiplier > 1 ? "strengthened" : "weakened";

            int i = 0;

            foreach (IFighter fighter in humanFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has {strengthenedOrWeakened} {magicType.ToString().ToLower()} resistance for {numberOfTurns} {turnOrTurns}!\n", outputs[i++].Message);
            }
        }

        [Test]
        public void TestMagicalResistanceMultiplierEffect_AllElements()
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();

            int spellPower = 6;
            int humanResistance = 2;
            int enemyResistance = 3;
            double humanResistanceMultiplier = 2;
            double enemyResistanceMultiplier = 1.0/3;

            TestMagicalResistanceMultiplierEffect_Setup(MagicType.All, 1, spellPower, humanResistance, humanResistanceMultiplier, enemyResistance, enemyResistanceMultiplier, magicTypes);

            for (var i = 0; i < magicTypes.Count; i++)
            {
                IFighter enemy = _enemyTeam.Fighters[i];
                int calculatedEnemyResistance = (int) (enemyResistance*enemyResistanceMultiplier);
                int calculatedDamageToEnemy = spellPower - calculatedEnemyResistance;
                int expectedEnemyRemainingHealth = enemy.MaxHealth - calculatedDamageToEnemy;

                Assert.AreEqual(expectedEnemyRemainingHealth, enemy.CurrentHealth, $"enemy {enemy.DisplayName} should be left with {expectedEnemyRemainingHealth} health, but had {enemy.CurrentHealth} instead");
            }
        }

        [Test]
        public void TestMagicalResistanceMultiplierEffect_AllElements_ScreenOutputs([Values(1, 4)] int statusDuration)
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();

            TestMagicalResistanceMultiplierEffect_Setup(MagicType.All, statusDuration, 2, 0, 3, 10, 1.0/5, magicTypes);

            var outputs = _output.GetOutputs();

            List<IFighter> humanFighters = _humanTeam.Fighters;
            List<IFighter> enemyFighters = _enemyTeam.Fighters;

            int expectedOutputLength = humanFighters.Count + enemyFighters.Count;

            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string turnOrTurns = (statusDuration == 1) ? "turn" : "turns";

            int i = 0;

            foreach (IFighter fighter in humanFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has strengthened resistance for {statusDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }

            foreach (IFighter fighter in enemyFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has weakened resistance for {statusDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }
        }

        private void SpellCostMultiplierEffect_Setup(int baseCost, double costMultiplier, int effectDuration)
        {
            var lowerTeamCost = new SpellCostMultiplierStatus(effectDuration, costMultiplier);
            StatusFieldEffect lowerTeamCostEffect = new StatusFieldEffect(TargetType.OwnTeam, "lower", lowerTeamCost);
            _testTechnique.AddEffect(lowerTeamCostEffect);

            var raiseEnemyCost = new SpellCostMultiplierStatus(effectDuration, costMultiplier);
            StatusFieldEffect raiseEnemyCostEffect = new StatusFieldEffect(TargetType.EnemyTeam, "raise", raiseEnemyCost);
            _testTechnique.AddEffect(raiseEnemyCostEffect);

            TestHumanFighter human3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam.Add(human3);
            human3.SetSpeed(1);
            human3.SetMove(_testTechnique, 1);
            human3.SetMoveTarget(human3);
            human3.SetMove(_runawayMove, 1);

            Spell spell = new Spell("foo", MagicType.None, SpellType.HealHealth, TargetType.Self, baseCost, 0);

            List<ITestFighter> spellCasters = new List<ITestFighter>
            {
                _human1,
                _human2,
                _enemy1,
                _enemy2
            };

            foreach (ITestFighter spellCaster in spellCasters)
            {
                var fighter = spellCaster as Fighter;
                if (fighter == null)
                {
                    throw new Exception("A TestHumanFighter or TestEnemyFighter could not be cast to type 'Fighter'");
                }
                fighter.AddSpell(spell);
                spellCaster.SetMagicStrength(0);
                spellCaster.SetMove(spell);
                spellCaster.SetMoveTarget(fighter);
                spellCaster.SetMana(10);
            }

            _battleManager.SuppressBattleIntroAndOutroMessages();
            _battleManager.SuppressCastSpellMessages();
            _battleManager.SuppressMagicalDamageMessages();
            _battleManager.Battle(_humanTeam, _enemyTeam);
        }

        [Test]
        public void TestSpellCostMultiplierEffect([Values(0.5, 2.0)] double costMultiplier)
        {
            const int cost = 2;
            
            SpellCostMultiplierEffect_Setup(cost, costMultiplier, 1);

            int expectedCost = (int) (cost*costMultiplier);
            int expectedRemainingMana = 10 - expectedCost;

            Assert.AreEqual(expectedRemainingMana, _human1.CurrentMana);
            Assert.AreEqual(expectedRemainingMana, _human2.CurrentMana);
            Assert.AreEqual(expectedRemainingMana, _enemy1.CurrentMana);
            Assert.AreEqual(expectedRemainingMana, _enemy2.CurrentMana);
        }

        [Test]
        public void TestSpellCostMultiplierEffect_ScreenOutputs([Values(0.5, 2.0)] double costMultiplier, [Values(1, 8)] int effectDuration)
        {
            const int cost = 2;

            SpellCostMultiplierEffect_Setup(cost, costMultiplier, effectDuration);

            var outputs = _output.GetOutputs();

            List<IFighter> allFighters = new List<IFighter>(_humanTeam.Fighters);
            allFighters = allFighters.Concat(_enemyTeam.Fighters).ToList();

            int expectedOutputLength = allFighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string increaseOrDecrease = costMultiplier < 1 ? "decreased" : "increased";
            string turnOrTurns = effectDuration == 1 ? "turn" : "turns";

            int i = 0;
            foreach (IFighter fighter in allFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has {increaseOrDecrease} cost for magic spells for {effectDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }
        }
        
        private void TestMagicAttackEffect_Setup(MagicType magicAttackType, int magicPower)
        {
            var magicAttack = new MagicAttackFieldEffect(TargetType.EnemyTeam, "foo", magicAttackType, magicPower, 1, true);
            _testTechnique.AddEffect(magicAttack);

            _humanTeam = new TestTeam(_human1);

            _human1.SetMove(_testTechnique, 1);
            _human1.SetMove(_runawayMove);

            foreach (TestEnemyFighter enemy in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                enemy.SetMove(_doNothingMove);
                enemy.SetHealth(magicPower + 1);
            }

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        [Test]
        public void TestMagicAttackEffect([Values(MagicType.Fire, MagicType.Water, MagicType.Wind)] MagicType magicAttackType,
            [Values(2, 4)] int magicPower)
        {
            TestMagicAttackEffect_Setup(magicAttackType, magicPower);

            foreach (var enemy in _enemyTeam.Fighters)
            {
                Assert.AreEqual(1, enemy.CurrentHealth);
            }
        }

        //TODO: What if the team or individual fighters have reflect status?
        [Test]
        public void TestMagicAttackEffect_ScreenOutputs([Values(MagicType.Fire, MagicType.Water, MagicType.Wind)] MagicType magicAttackType,
            [Values(2, 4)] int magicPower)
        {
            TestMagicAttackEffect_Setup(magicAttackType, magicPower);

            var outputs = _output.GetOutputs();

            int numberEnemyFighters = _enemyTeam.Fighters.Count;
            //"Enemy team has been attacked", 
            //and a message for each enemy when hit for damage
            int expectedMessageCount = 1 + numberEnemyFighters;   
            Assert.AreEqual(expectedMessageCount, outputs.Length);
            
            Assert.AreEqual($"Enemy team has been attacked with {magicAttackType.ToString().ToLower()}!\n", outputs[0].Message);
            
            for (int j = 0; j < numberEnemyFighters; ++j)
            {
                EnemyFighter enemy = _enemyTeam.Fighters[j] as EnemyFighter;
                Assert.AreEqual($"{enemy?.DisplayName} took {magicPower} damage!\n", outputs[j + 1].Message);
            }
        }

        private void TestReflectEffect_Setup(MagicType reflectMagicType, int statusDuration, double reflectBonus, int spellPower, IList<MagicType> magicTypes)
        {
            ReflectStatus reflectStatus = new ReflectStatus(statusDuration, reflectMagicType, reflectBonus);
            StatusFieldEffect statusFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", reflectStatus);
            _testTechnique.AddEffect(statusFieldEffect);

            List<HumanFighter> humanFighters = new List<HumanFighter>();
            List<IFighter> enemyFighters = new List<IFighter>();
            int i = 0;

            foreach (MagicType mType in magicTypes)
            {
                string mTypeString = mType.ToString();
                TestHumanFighter fighter = new TestHumanFighter("Bill" + i, 1);
                TestEnemyFighter enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);

                enemy.SetAppendText(mTypeString);
                enemy.SetHealth(10);
                enemy.SetMoveTarget(enemy);

                Spell spell = new Spell(mTypeString, mType, SpellType.Attack, TargetType.SingleEnemy, 0, spellPower); //will do 2, 4, or 8 damage depending 
                fighter.SetHealth(10);
                fighter.AddSpell(spell);
                fighter.SetMove(spell, 1);
                fighter.SetMove(_runawayMove);

                fighter.SetMoveTarget(enemy);

                if (i == 0)
                {
                    enemy.SetSpeed(1);
                    enemy.SetMove(_testTechnique);
                }
                else
                {
                    enemy.SetMove(_doNothingMove);
                }

                enemyFighters.Add(enemy);
                humanFighters.Add(fighter);

                ++i;
            }

            _humanTeam = new TestTeam(humanFighters);
            _enemyTeam = new Team(_menuManager, enemyFighters);

            _battleManager.SuppressBattleIntroAndOutroMessages();
            _battleManager.SuppressCastSpellMessages();
            _battleManager.SuppressMagicalDamageMessages();
            _battleManager.Battle(_humanTeam, _enemyTeam);
        }

        [Test]
        public void TestReflectEffect([Values(MagicType.Earth, MagicType.Ice, MagicType.Lightning, MagicType.Water, MagicType.All)] MagicType reflectMagicType,
            [Values(0.5, 1.0, 2.0)] double reflectBonus)
        {
            const int spellPower = 4;
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();

            TestReflectEffect_Setup(reflectMagicType, 1, reflectBonus, spellPower, magicTypes);            

            for (var idx = 0; idx < magicTypes.Count; idx++)
            {
                EnemyFighter enemy = _enemyTeam.Fighters[idx] as EnemyFighter;
                HumanFighter fighter = _humanTeam.Fighters[idx] as HumanFighter;
                MagicType magicType = magicTypes[idx];

                int fighterExpectedRemainingHealth = 10;
                int enemyExpectedRemainingHealth = 10;
                int reflectDamage = (int) (spellPower*reflectBonus);

                if (magicType == reflectMagicType || reflectMagicType == MagicType.All)
                {
                    fighterExpectedRemainingHealth -= reflectDamage;
                }
                else
                {
                    enemyExpectedRemainingHealth -= spellPower;
                }

                Assert.AreEqual(fighterExpectedRemainingHealth, fighter.CurrentHealth, $"fighter {fighter.DisplayName} should have {fighterExpectedRemainingHealth} HP left after {magicType} spell!");
                Assert.AreEqual(enemyExpectedRemainingHealth, enemy.CurrentHealth, $"EnemyFighter {enemy.DisplayName} should have {enemyExpectedRemainingHealth} HP left after {magicType} spell!");
            }
        }

        [Test]
        public void TestReflectEffect_ScreenOutputs([Values(MagicType.Fire, MagicType.Ice, MagicType.Lightning, MagicType.Wind, MagicType.All)] MagicType reflectMagicType,
            [Values(1, 7)] int statusDuration)
        {
            IList<MagicType> magicTypes = MagicTypes.GetBasicMagicTypes().ToList();

            TestReflectEffect_Setup(reflectMagicType, statusDuration, 1, 1, magicTypes);

            var outputs = _output.GetOutputs();

            List<IFighter> enemyFighters = _enemyTeam.Fighters;

            int expectedOutputLength = enemyFighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string againstStringStatus = reflectMagicType == MagicType.All ? "" : $" against {reflectMagicType.ToString().ToLower()} magic";
            string turnOrTurns = statusDuration == 1 ? "turn" : "turns";

            int i = 0;
            foreach (IFighter fighter in enemyFighters)
            {
                Assert.AreEqual($"{fighter.DisplayName} has gained reflect status{againstStringStatus} for {statusDuration} {turnOrTurns}!\n", outputs[i++].Message);
            }
        }

        private void TestShieldEffect_Setup(bool isHumanTeamEffect, IBattleShield shield, bool shouldEnemyAttack = false, int attackStrength = 0)
        {
            int i = 0;

            ShieldFieldEffect shieldEffect = new ShieldFieldEffect(TargetType.OwnTeam, "foo", shield);
            _testTechnique.AddEffect(shieldEffect);

            BattleMove attackMove = TestMoveFactory.Get(TargetType.SingleEnemy, "attack", BattleMoveType.Attack);

            foreach (TestHumanFighter humanFighter in _humanTeam.Fighters.Cast<TestHumanFighter>())
            {
                humanFighter.SetMoveTarget(humanFighter);

                if (i == 0)
                {
                    if (isHumanTeamEffect)
                    {
                        humanFighter.SetSpeed(1);
                        humanFighter.SetMove(_testTechnique, 1);
                    }
                    else if (shouldEnemyAttack)
                    {
                        humanFighter.SetStrength(attackStrength);

                        humanFighter.SetMove(attackMove, 1);
                        IFighter target = _enemyTeam.Fighters[0];
                        humanFighter.SetMoveTarget(target);
                        _chanceService.PushEventsOccur(true, false); //attack hits, but not a crit
                    }
                    else
                    {
                        humanFighter.SetMove(_doNothingMove, 1);
                    }

                    humanFighter.SetMove(_runawayMove);
                }
                else
                {
                    humanFighter.SetMove(_doNothingMove);
                }
                
                ++i;
            }
            _humanTeam.SetDeathsOnRoundEndEvent();

            i = 0;
            foreach (TestEnemyFighter enemyFighter in _enemyTeam.Fighters.Cast<TestEnemyFighter>())
            {
                enemyFighter.SetMoveTarget(enemyFighter);

                if (i == 0)
                {
                    if (!isHumanTeamEffect)
                    {
                        enemyFighter.SetSpeed(1);
                        enemyFighter.SetMove(_testTechnique, 1);
                    }
                    else if (shouldEnemyAttack)
                    {
                        enemyFighter.SetStrength(attackStrength);

                        enemyFighter.SetMove(attackMove, 1);
                        IFighter target = _humanTeam.Fighters[0];
                        enemyFighter.SetMoveTarget(target);
                        _chanceService.PushEventsOccur(true, false); //attack hits, but not a crit
                    }
                    else
                    {
                        enemyFighter.SetMove(_doNothingMove, 1);
                    }

                    enemyFighter.SetMove(_doNothingMove);
                }
                else
                {
                    enemyFighter.SetMove(_doNothingMove);
                }

                ++i;
            }

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        private void TestShieldEffect_CompareShields(IBattleShield expectedShield, IBattleShield actualShield)
        {
            Assert.That(actualShield, Is.Not.Null);
            Assert.IsTrue(expectedShield.AreEqual(actualShield));
        }

        [Test]
        public void TestShieldEffect([Values(true, false)] bool isHumanTeamEffect)
        {
            ElementalBattleShield shield = new ElementalBattleShield(5, 2, 0, MagicType.Water);
            TestShieldEffect_Setup(isHumanTeamEffect, shield);

            foreach (var fighterShield in _humanTeam.Fighters.Select(fighter => fighter.BattleShield))
            {
                if (isHumanTeamEffect)
                {
                    TestShieldEffect_CompareShields(shield, fighterShield);
                }
                else
                {
                    Assert.That(fighterShield, Is.Null);
                }
            }


            foreach (var fighterShield in _enemyTeam.Fighters.Select(fighter => fighter.BattleShield))
            {
                if (!isHumanTeamEffect)
                {
                    TestShieldEffect_CompareShields(shield, fighterShield);
                }
                else
                {
                    Assert.That(fighterShield, Is.Null);
                }
            }
        }

        [Test]
        public void TestShieldEffect_ScreenOutputs([Values(true, false)] bool isHumanTeamEffect, [Values(MagicType.Water, MagicType.Fire, MagicType.Lightning)] MagicType elementalShieldType)
        {
            ElementalBattleShield shield = new ElementalBattleShield(5, 2, 0, elementalShieldType);
            TestShieldEffect_Setup(isHumanTeamEffect, shield);

            var outputs = _output.GetOutputs();

            int expectedOutputLength = 1 + (isHumanTeamEffect ? _humanTeam.Fighters.Count : _enemyTeam.Fighters.Count);
            Assert.AreEqual(expectedOutputLength, outputs.Length);

            string teamString = isHumanTeamEffect ? "Your" : "Enemy";
            string shieldType = elementalShieldType.ToString().ToLower();

            Assert.AreEqual($"{teamString} team has gained {shieldType} elemental shields!\n", outputs[0].Message);
        }

        [Test]
        public void TestShieldEffect_ShieldCopiedByValueNotByReference()
        {
            const int shieldHealth = 5;
            const int shieldDefense = 2;
            const int attackStrength = 3;
            
            var shield = new ElementalBattleShield(shieldHealth, shieldDefense, 0, MagicType.Water);
            TestShieldEffect_Setup(false, shield, true, attackStrength);

            foreach (var fighterShield in _enemyTeam.Fighters.Select(fighter => fighter.BattleShield))
            {
                Assert.AreNotSame(shield, fighterShield);
            }

            const int totalShieldDamage = attackStrength - shieldDefense;
            const int expectedHealthAfterAttack = shieldHealth - totalShieldDamage;

            Assert.AreEqual(expectedHealthAfterAttack, _enemy1.BattleShield.CurrentHealth); //_enemy1's shield took damage
            Assert.AreEqual(shieldHealth, _enemy2.BattleShield.CurrentHealth); //_enemy2's shield did not
            Assert.AreEqual(shieldHealth, shield.CurrentHealth); //effect's shield did not take damage
        }

        [Test]
        public void TestUndoDebuffsEffect_TeamEffect()
        {
            const int humanStrength = 4;
            const int enemyDefense = 1;
            const int enemyHealth = 10;
            const int enemyDefenseMultiplier = 2;
            _human2.SetStrength(humanStrength);
            _enemy1.SetDefense(enemyDefense);
            _enemy1.SetHealth(enemyHealth);
            
            //have enemy go first- use a fieldEffect that will raise allies' defense, lower enemy attack
            
            StatMultiplierStatus raiseDefenseStatus = new StatMultiplierStatus(2, StatType.Defense, enemyDefenseMultiplier);
            StatusFieldEffect raiseDefenseEffect = new StatusFieldEffect(TargetType.OwnTeam, "defense up", raiseDefenseStatus);
            _testTechnique.AddEffect(raiseDefenseEffect);
            StatMultiplierStatus lowerAttackStatus = new StatMultiplierStatus(2, StatType.Strength, 0.5);
            StatusFieldEffect lowerAttackEffect = new StatusFieldEffect(TargetType.EnemyTeam, "attack down", lowerAttackStatus);
            _testTechnique.AddEffect(lowerAttackEffect);

            _enemy1.SetSpeed(2);
            _enemy1.SetMove(_testTechnique);
            _enemy1.SetMoveTarget(_enemy1);

            //have a human cast undo debuff
            TestFieldEffectMove undoDebuffTechnique = new TestFieldEffectMove("bar", TargetType.OwnTeam, 1);
            undoDebuffTechnique.AddEffect(new UndoDebuffsFieldEffect(TargetType.OwnTeam, "bar"));

            _human1.SetSpeed(1);
            _human1.SetMove(undoDebuffTechnique);
            _human1.SetMoveTarget(_human1);
            _human1.SetDeathOnTurnEndEvent();

            //have second human attack
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);
            _human2.SetMove(attack);
            _human2.SetMoveTarget(_enemy1);
            _chanceService.PushEventsOccur(true, false); //attack hits and is not a crit
            _human2.SetDeathOnTurnEndEvent();

            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            //damage output will be normal
            const int damage = humanStrength - (enemyDefense * enemyDefenseMultiplier);
            const int expectedHealth = enemyHealth - damage;

            Assert.AreEqual(expectedHealth, _enemy1.CurrentHealth);
        }

        [Test]
        public void TestUndoDebuffsEffect_ScreenOutputs()
        {
            var undoDebuff = new UndoDebuffsFieldEffect(TargetType.OwnTeam, "foo");
            _testTechnique.AddEffect(undoDebuff);

            _human1.SetMove(_testTechnique);
            _human1.SetMoveTarget(_human1);
            _human2.SetMove(_doNothingMove);
            _human2.SetMoveTarget(_human2);

            _humanTeam.SetDeathsOnRoundEndEvent();

            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);
            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowPhysicalDamageMessages = false, //technically used for "SetDeathsOnTurnEndEvent()" method
                ShowDeathMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            //use testTechnique's effect Duration because restoreMana has null for its duration
            Assert.AreEqual("Your team has had all stat changes removed!\n", outputs[0].Message);
        }

        #endregion testing each effect individually

        #region .AreEqual() method

        [Test]
        public void AreEqualMethod_CriticalChanceEffect()
        {
            var criticalChance =
                _fieldEffects.First(fe => fe is CriticalChanceMultiplierFieldEffect) as
                    CriticalChanceMultiplierFieldEffect;

            if (criticalChance == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is CriticalChanceMultiplierFieldEffect)))
            {
                Assert.IsFalse(criticalChance.AreEqual(effect));
            }

            var notEqual = new CriticalChanceMultiplierFieldEffect(TargetType.OwnTeam, "test", criticalChance.Percentage + .25);
            Assert.IsFalse(criticalChance.AreEqual(notEqual));

            var equal = new CriticalChanceMultiplierFieldEffect(TargetType.OwnTeam, "bar", criticalChance.Percentage);
            Assert.IsTrue(criticalChance.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_MagicAttackEffect()
        {
            var magicalAttack =
                _fieldEffects.First(fe => fe is MagicAttackFieldEffect) as
                    MagicAttackFieldEffect;

            if (magicalAttack == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is MagicAttackFieldEffect)))
            {
                Assert.IsFalse(magicalAttack.AreEqual(effect));
            }

            MagicAttackFieldEffect notEqual;
            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();
            foreach (MagicType magicType in magicTypes)
            {
                if (magicType == magicalAttack.MagicType)
                {
                    continue;
                }

                notEqual = new MagicAttackFieldEffect(TargetType.OwnTeam, "test", magicType, magicalAttack.Power);
                Assert.IsFalse(magicalAttack.AreEqual(notEqual));
            }

            notEqual = new MagicAttackFieldEffect(TargetType.OwnTeam, "test", magicalAttack.MagicType, magicalAttack.Power + 2);
            Assert.IsFalse(magicalAttack.AreEqual(notEqual));

            var equal = new MagicAttackFieldEffect(TargetType.OwnTeam, "bar", magicalAttack.MagicType, magicalAttack.Power);
            Assert.IsTrue(magicalAttack.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_MagicMultiplierEffect()
        {
            var multiplierEffect =
                _fieldEffects.First(fe => fe is MagicMultiplierFieldEffect) as
                    MagicMultiplierFieldEffect;

            if (multiplierEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is MagicMultiplierFieldEffect)))
            {
                Assert.IsFalse(multiplierEffect.AreEqual(effect));
            }

            MagicMultiplierFieldEffect notEqual;
            var magicTypes = Enum.GetValues(typeof(MagicType));
            foreach (MagicType magicType in magicTypes)
            {
                if (magicType == multiplierEffect.MagicType)
                {
                    continue;
                }

                notEqual = new MagicMultiplierFieldEffect(TargetType.OwnTeam, "test", magicType, multiplierEffect.Percentage);
                Assert.IsFalse(multiplierEffect.AreEqual(notEqual));
            }

            notEqual = new MagicMultiplierFieldEffect(TargetType.OwnTeam, "test", multiplierEffect.MagicType, multiplierEffect.Percentage + .25);
            Assert.IsFalse(multiplierEffect.AreEqual(notEqual));

            var equal = new MagicMultiplierFieldEffect(TargetType.OwnTeam, "test", multiplierEffect.MagicType, multiplierEffect.Percentage);
            Assert.IsTrue(multiplierEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_MagicResistanceMultiplierEffect()
        {
            var multiplierEffect =
                _fieldEffects.First(fe => fe is MagicResistanceMultiplierFieldEffect) as
                    MagicResistanceMultiplierFieldEffect;

            if (multiplierEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is MagicResistanceMultiplierFieldEffect)))
            {
                Assert.IsFalse(multiplierEffect.AreEqual(effect));
            }

            MagicResistanceMultiplierFieldEffect notEqual;
            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();
            foreach (MagicType magicType in magicTypes)
            {
                if (magicType == multiplierEffect.MagicType)
                {
                    continue;
                }

                notEqual = new MagicResistanceMultiplierFieldEffect(TargetType.OwnTeam, "test", magicType, multiplierEffect.Percentage);
                Assert.IsFalse(multiplierEffect.AreEqual(notEqual));
            }

            notEqual = new MagicResistanceMultiplierFieldEffect(TargetType.OwnTeam, "test", multiplierEffect.MagicType, multiplierEffect.Percentage + .25);
            Assert.IsFalse(multiplierEffect.AreEqual(notEqual));

            notEqual = new MagicResistanceMultiplierFieldEffect(TargetType.OwnTeam, "test", multiplierEffect.MagicType, multiplierEffect.Percentage - .25);
            Assert.IsFalse(multiplierEffect.AreEqual(notEqual));

            var equal = new MagicResistanceMultiplierFieldEffect(TargetType.OwnTeam, "test", multiplierEffect.MagicType, multiplierEffect.Percentage);
            Assert.IsTrue(multiplierEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_ReflectFieldEffect()
        {
            var reflectEffect =
                _fieldEffects.First(fe => fe is ReflectFieldEffect) as
                    ReflectFieldEffect;

            if (reflectEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is ReflectFieldEffect)))
            {
                Assert.IsFalse(reflectEffect.AreEqual(effect));
            }

            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();
            foreach (MagicType magicType in magicTypes)
            {
                if (magicType == reflectEffect.MagicType)
                {
                    continue;
                }

                var notEqual = new ReflectFieldEffect(TargetType.OwnTeam, "test", magicType);
                Assert.IsFalse(reflectEffect.AreEqual(notEqual));
            }

            var equal = new ReflectFieldEffect(TargetType.OwnTeam, "bar", reflectEffect.MagicType);
            Assert.IsTrue(reflectEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_RestoreHealthPercentageFieldEffect()
        {
            var healthEffect =
                _fieldEffects.First(fe => fe is RestoreHealthPercentageFieldEffect) as
                    RestoreHealthPercentageFieldEffect;

            if (healthEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is RestoreHealthPercentageFieldEffect)))
            {
                Assert.IsFalse(healthEffect.AreEqual(effect));
            }

            var notEqual = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "test", healthEffect.Percentage + 25);
            Assert.IsFalse(healthEffect.AreEqual(notEqual));

            var equal = new RestoreHealthPercentageFieldEffect(TargetType.OwnTeam, "test", healthEffect.Percentage);
            Assert.IsTrue(healthEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_RestoreManaPercentageFieldEffect()
        {
            var manaEffect =
                _fieldEffects.First(fe => fe is RestoreManaPercentageFieldEffect) as
                    RestoreManaPercentageFieldEffect;

            if (manaEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is RestoreManaPercentageFieldEffect)))
            {
                Assert.IsFalse(manaEffect.AreEqual(effect));
            }

            var notEqual = new RestoreManaPercentageFieldEffect(TargetType.OwnTeam, "test", manaEffect.Percentage + 25);
            Assert.IsFalse(manaEffect.AreEqual(notEqual));

            var equal = new RestoreManaPercentageFieldEffect(TargetType.OwnTeam, "test", manaEffect.Percentage);
            Assert.IsTrue(manaEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_ShieldFieldEffect()
        {
            var shieldEffect =
                _fieldEffects.First(fe => fe is ShieldFieldEffect) as
                    ShieldFieldEffect;

            if (shieldEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is ShieldFieldEffect)))
            {
                Assert.IsFalse(shieldEffect.AreEqual(effect));
            }

            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();
            var shield = shieldEffect.BattleShield;
            ElementalBattleShield newShield;

            var elementalShield = shield as ElementalBattleShield;
            if (elementalShield != null)
            {
                foreach (MagicType magicType in magicTypes)
                {
                    if (magicType == elementalShield.ElementalType)
                    {
                        newShield = new ElementalBattleShield(shield.MaxHealth, shield.Defense, 0, elementalShield.ElementalType);
                        var equal = new ShieldFieldEffect(TargetType.OwnTeam, "test", newShield);
                        Assert.IsTrue(shieldEffect.AreEqual(equal));
                    }
                    else
                    {
                        newShield = new ElementalBattleShield(shield.MaxHealth, shield.Defense, 0, magicType);
                        var notEqual = new ShieldFieldEffect(TargetType.OwnTeam, "test", newShield);
                        Assert.IsFalse(shieldEffect.AreEqual(notEqual));
                    }
                }
            }
        }

        [Test]
        public void AreEqualMethod_SpellCostMultiplierFieldEffect()
        {
            var costEffect =
                _fieldEffects.First(fe => fe is SpellCostMultiplierFieldEffect) as
                    SpellCostMultiplierFieldEffect;

            if (costEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is SpellCostMultiplierFieldEffect)))
            {
                Assert.IsFalse(costEffect.AreEqual(effect));
            }

            var notEqual = new SpellCostMultiplierFieldEffect(TargetType.OwnTeam, "bar", costEffect.Multiplier + .25);
            Assert.IsFalse(costEffect.AreEqual(notEqual));

            notEqual = new SpellCostMultiplierFieldEffect(TargetType.OwnTeam, "bar", costEffect.Multiplier - .25);
            Assert.IsFalse(costEffect.AreEqual(notEqual));

            var equal = new SpellCostMultiplierFieldEffect(TargetType.OwnTeam, "bar", costEffect.Multiplier);
            Assert.IsTrue(costEffect.AreEqual(equal));
        }

        [Test]
        public void AreEqualMethod_StatMultiplierFieldEffect()
        {
            var statEffect =
                _fieldEffects.First(fe => fe is StatMultiplierFieldEffect) as
                    StatMultiplierFieldEffect;

            if (statEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is StatMultiplierFieldEffect)))
            {
                Assert.IsFalse(statEffect.AreEqual(effect));
            }

            IEnumerable<StatType> statTypes = EnumHelperMethods.GetAllValuesForEnum<StatType>();

            var notEqual = new StatMultiplierFieldEffect(TargetType.OwnTeam, "bar", statEffect.Stat, statEffect.Percentage + .25);
            Assert.IsFalse(statEffect.AreEqual(notEqual));

            notEqual = new StatMultiplierFieldEffect(TargetType.OwnTeam, "bar", statEffect.Stat, statEffect.Percentage - .25);
            Assert.IsFalse(statEffect.AreEqual(notEqual));

            foreach (StatType statType in statTypes)
            {
                if (statType == statEffect.Stat)
                {
                    var equal = new StatMultiplierFieldEffect(TargetType.OwnTeam, "bar", statType, statEffect.Percentage);
                    Assert.IsTrue(statEffect.AreEqual(equal));
                }
                else
                {
                    notEqual = new StatMultiplierFieldEffect(TargetType.OwnTeam, "bar", statType, statEffect.Percentage);
                    Assert.IsFalse(statEffect.AreEqual(notEqual));
                }
            }
        }

        [Test]
        public void AreEqualMethod_StatusFieldEffect()
        {
            StatusFieldEffect statusEffect =
                _fieldEffects.First(fe => fe is StatusFieldEffect) as
                    StatusFieldEffect;

            if (statusEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            MagicMultiplierStatus status = statusEffect.Status as MagicMultiplierStatus;

            if (status == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is StatusFieldEffect)))
            {
                Assert.IsFalse(statusEffect.AreEqual(effect));
            }

            MagicSealedStatus magicSealedStatus = new MagicSealedStatus(1);
            StatusFieldEffect notEqual = new StatusFieldEffect(TargetType.OwnTeam, "bar", magicSealedStatus);
            Assert.IsFalse(statusEffect.AreEqual(notEqual));

            MagicMultiplierStatus notEqualStatus = new MagicMultiplierStatus(1, status.MagicType, status.Multiplier - .5);
            notEqual = new StatusFieldEffect(TargetType.OwnTeam, "bar", notEqualStatus);
            Assert.IsFalse(statusEffect.AreEqual(notEqual));

            notEqualStatus = new MagicMultiplierStatus(1, status.MagicType, status.Multiplier + .5);
            notEqual = new StatusFieldEffect(TargetType.OwnTeam, "bar", notEqualStatus);
            Assert.IsFalse(statusEffect.AreEqual(notEqual));

            IEnumerable<MagicType> magicTypes = EnumHelperMethods.GetAllValuesForEnum<MagicType>();

            foreach (MagicType magicType in magicTypes)
            {
                MagicMultiplierStatus newStatus = new MagicMultiplierStatus(1, magicType, status.Multiplier);
                StatusFieldEffect newFieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "bar", newStatus);
                bool shouldBeEqual = magicType == status.MagicType;
                bool areEqualReturnValue = statusEffect.AreEqual(newFieldEffect);

                Assert.AreEqual(shouldBeEqual, areEqualReturnValue);
            }
        }

        [Test]
        public void AreEqualMethod_UndoDebuffsFieldEffect()
        {
            var undoEffect =
                _fieldEffects.First(fe => fe is UndoDebuffsFieldEffect) as
                    UndoDebuffsFieldEffect;

            if (undoEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            foreach (var effect in _fieldEffects.Where(fe => !(fe is UndoDebuffsFieldEffect)))
            {
                Assert.IsFalse(undoEffect.AreEqual(effect));
            }

            var equal = new UndoDebuffsFieldEffect(TargetType.OwnTeam, "bar");
            Assert.IsTrue(undoEffect.AreEqual(equal));
        }

        #endregion

        #region .Copy() method

        [Test]
        public void CopyMethod_CriticalChanceEffect()
        {
            var criticalChance =
                _fieldEffects.First(fe => fe is CriticalChanceMultiplierFieldEffect) as
                    CriticalChanceMultiplierFieldEffect;

            if (criticalChance == null)
            {
                throw new Exception("this code will never be reached");
            }

            CriticalChanceMultiplierFieldEffect copy = criticalChance.Copy() as CriticalChanceMultiplierFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(criticalChance.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_MagicAttackEffect()
        {
            var magicalAttack =
                _fieldEffects.First(fe => fe is MagicAttackFieldEffect) as
                    MagicAttackFieldEffect;

            if (magicalAttack == null)
            {
                throw new Exception("this code will never be reached");
            }

            MagicAttackFieldEffect copy = magicalAttack.Copy() as MagicAttackFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(magicalAttack.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_MagicMultiplierEffect()
        {
            var multiplierEffect =
                _fieldEffects.First(fe => fe is MagicMultiplierFieldEffect) as
                    MagicMultiplierFieldEffect;

            if (multiplierEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            MagicMultiplierFieldEffect copy = multiplierEffect.Copy() as MagicMultiplierFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(multiplierEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_MagicResistanceMultiplierEffect()
        {
            var multiplierEffect =
                _fieldEffects.First(fe => fe is MagicResistanceMultiplierFieldEffect) as
                    MagicResistanceMultiplierFieldEffect;

            if (multiplierEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            MagicResistanceMultiplierFieldEffect copy = multiplierEffect.Copy() as MagicResistanceMultiplierFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(multiplierEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_ReflectFieldEffect()
        {
            var reflectEffect =
                _fieldEffects.First(fe => fe is ReflectFieldEffect) as
                    ReflectFieldEffect;

            if (reflectEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            ReflectFieldEffect copy = reflectEffect.Copy() as ReflectFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(reflectEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_RestoreHealthPercentageFieldEffect()
        {
            var healthEffect =
                _fieldEffects.First(fe => fe is RestoreHealthPercentageFieldEffect) as
                    RestoreHealthPercentageFieldEffect;

            if (healthEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            RestoreHealthPercentageFieldEffect copy = healthEffect.Copy() as RestoreHealthPercentageFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(healthEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_RestoreManaPercentageFieldEffect()
        {
            var manaEffect =
                _fieldEffects.First(fe => fe is RestoreManaPercentageFieldEffect) as
                    RestoreManaPercentageFieldEffect;

            if (manaEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            RestoreManaPercentageFieldEffect copy = manaEffect.Copy() as RestoreManaPercentageFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(manaEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_ShieldFieldEffect()
        {
            var shieldEffect =
                _fieldEffects.First(fe => fe is ShieldFieldEffect) as
                    ShieldFieldEffect;

            if (shieldEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            ShieldFieldEffect copy = shieldEffect.Copy() as ShieldFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(shieldEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_SpellCostMultiplierFieldEffect()
        {
            var costEffect =
                _fieldEffects.First(fe => fe is SpellCostMultiplierFieldEffect) as
                    SpellCostMultiplierFieldEffect;

            if (costEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            SpellCostMultiplierFieldEffect copy = costEffect.Copy() as SpellCostMultiplierFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(costEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_StatMultiplierFieldEffect()
        {
            var statEffect =
                _fieldEffects.First(fe => fe is StatMultiplierFieldEffect) as
                    StatMultiplierFieldEffect;

            if (statEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            StatMultiplierFieldEffect copy = statEffect.Copy() as StatMultiplierFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(statEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_StatusFieldEffect()
        {
            StatusFieldEffect statusEffect =
                _fieldEffects.First(fe => fe is StatusFieldEffect) as
                    StatusFieldEffect;

            if (statusEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            StatusFieldEffect copy = statusEffect.Copy() as StatusFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(statusEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        [Test]
        public void CopyMethod_UndoDebuffsFieldEffect()
        {
            var undoEffect =
                _fieldEffects.First(fe => fe is UndoDebuffsFieldEffect) as
                    UndoDebuffsFieldEffect;

            if (undoEffect == null)
            {
                throw new Exception("this code will never be reached");
            }

            UndoDebuffsFieldEffect copy = undoEffect.Copy() as UndoDebuffsFieldEffect;

            Assert.IsNotNull(copy);
            Assert.IsTrue(undoEffect.AreEqual(copy), "The copy and the original should return AreEqual() true for each other");
        }

        #endregion

        [Test]
        public void Test_LowerEnemyAttack_RaiseAttack_DoubleEffects()
        {
            const int enemyHealth = 100;
            const int baseAttackStrength = 2;
            const int firstMultiplier = 2;
            const int secondMultiplier = 3;

            StatMultiplierStatus status = new StatMultiplierStatus(1, StatType.Strength, firstMultiplier);
            StatusFieldEffect fieldEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", status);
            _testTechnique.AddEffect(fieldEffect);
            _human1.SetMove(_testTechnique);
            _human1.SetMoveTarget(_human1);
            _human1.SetDeathOnTurnEndEvent();

            StatMultiplierStatus secondStatus = new StatMultiplierStatus(1, StatType.Strength, secondMultiplier);
            StatusFieldEffect secondStatusEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", secondStatus);
            TestFieldEffectMove testTechnique2 = new TestFieldEffectMove("bar", TargetType.OwnTeam, 3);
            testTechnique2.AddEffect(secondStatusEffect);
            _human2.SetMove(testTechnique2);
            _human2.SetMoveTarget(_human2);
            _human2.SetDeathOnTurnEndEvent();

            TestHumanFighter human3 = new TestHumanFighter("Jeff", 1);
            _humanTeam.Add(human3);
            human3.SetStrength(baseAttackStrength);
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);
            human3.SetMove(attack);
            human3.SetMoveTarget(_enemy1);
            _chanceService.PushEventsOccur(true, false); //attack hits but is not a crit
            human3.SetDeathOnTurnEndEvent();

            _enemy1.SetHealth(enemyHealth);
            _enemy1.SetMove(_doNothingMove);
            _enemy1.SetMoveTarget(_enemy1);

            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            const int expectedDamage = baseAttackStrength*firstMultiplier*secondMultiplier;
            const int expectedRemainingHealth = enemyHealth - expectedDamage;

            Assert.AreEqual(expectedRemainingHealth, _enemy1.CurrentHealth);
        }

        [Test]
        public void Test_LowerEnemyDefense_RaiseDefense_DoubleEffects()
        {
            const int enemyHealth = 100;
            const int baseAttackStrength = 10;
            const int baseEnemyDefense = 4; //calculated defense should be 6
            const double firstMultiplier = 0.5;
            const int secondMultiplier = 3;

            //lower enemy defenses
            StatMultiplierStatus lowerEnemyDefenseStatus = new StatMultiplierStatus(1, StatType.Defense, firstMultiplier);
            StatusFieldEffect fieldEffect = new StatusFieldEffect(TargetType.EnemyTeam, "foo", lowerEnemyDefenseStatus);
            _testTechnique.AddEffect(fieldEffect);
            _human1.SetMove(_testTechnique);
            _human1.SetMoveTarget(_human1);
            _human1.SetSpeed(2);
            _human1.SetDeathOnTurnEndEvent();

            StatMultiplierStatus raiseTeamDefenseStatus = new StatMultiplierStatus(1, StatType.Defense, secondMultiplier);
            StatusFieldEffect secondStatusEffect = new StatusFieldEffect(TargetType.OwnTeam, "foo", raiseTeamDefenseStatus);
            TestFieldEffectMove testTechnique2 = new TestFieldEffectMove("bar", TargetType.OwnTeam, 3);
            testTechnique2.AddEffect(secondStatusEffect);
            _enemy1.SetMove(testTechnique2);
            _enemy1.SetMoveTarget(_enemy1);
            _enemy1.SetSpeed(1);

            _human2.SetStrength(baseAttackStrength);
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);
            _human2.SetMove(attack);
            _human2.SetMoveTarget(_enemy2);
            _chanceService.PushEventsOccur(true, false); //attack hits but is not a crit
            _human2.SetDeathOnTurnEndEvent();

            _enemy2.SetHealth(enemyHealth);
            _enemy2.SetMove(_doNothingMove);
            _enemy2.SetMoveTarget(_enemy1);
            _enemy2.SetDefense(baseEnemyDefense);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            const int calculatedDefense = (int) (baseEnemyDefense*firstMultiplier*secondMultiplier);
            const int expectedDamage = baseAttackStrength - calculatedDefense;
            const int expectedRemainingHealth = enemyHealth - expectedDamage;

            Assert.AreEqual(expectedRemainingHealth, _enemy2.CurrentHealth);
        }
    }
}