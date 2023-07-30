using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.TerrainInteractableTests
{
    [TestFixture]
    public class BellTests
    {
        private MockChanceService _chanceService;
        private EventLogger _logger;
        private TestHumanFighter _fighter;

        private Bell _copperBell;
        private Bell _silverBell;
        private const string CopperBellDisplayName = "the copper bell";
        private const string SilverBellDisplayName = "the silver bell";

        private readonly BellMove _prayMove = MoveFactory.Get(BellMoveType.SealMove);
        private readonly BellMove _bloodMove = MoveFactory.Get(BellMoveType.ControlMove);

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _logger = new EventLogger();
            TestFighterFactory.SetChanceService(_chanceService);
            _copperBell = new Bell(CopperBellDisplayName, BellType.Copper, new MenuFactory(), _chanceService);
            _silverBell = new Bell(SilverBellDisplayName, BellType.Silver, new MenuFactory(), _chanceService);

            _fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
        }

        [Test]
        public void BasicBell_GeneratesCorrectMenuActions()
        {
            List<MenuAction> menuActions = _copperBell.GetInteractableMenuActions();

            Assert.NotNull(menuActions);
            Assert.AreEqual(2, menuActions.Count);

            //asserts related to the "offer blood" menu action
            string expectedBloodDisplayText = $"offer blood to {CopperBellDisplayName} ('blood {BellType.Copper.ToString().ToLower()}')";
            MenuAction offerBloodMenuAction = menuActions.FirstOrDefault(ma => ma.DisplayText.Equals(expectedBloodDisplayText));
            Assert.NotNull(offerBloodMenuAction);

            //"pray to bell" tests
            string expectedPrayDisplayText = $"pray to {CopperBellDisplayName} ('pray {BellType.Copper.ToString().ToLower()}')";
            MenuAction prayMenuAction = menuActions.FirstOrDefault(ma => ma.DisplayText.Equals(expectedPrayDisplayText));
            Assert.NotNull(prayMenuAction);
        }

        #region SealingMove

        [Test]
        public void CorrectlySealsShade_WhenExecutingPrayMove()
        {
            Shade shade = (Shade) TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
            _logger.Subscribe(EventType.FighterSealed, shade);
            TestHumanFighter fighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, fighter);

            _chanceService.PushEventOccurs(true);

            //act
            _copperBell.ExecuteMove(moveWithTarget);

            //assert
            Assert.AreEqual(0, shade.CurrentHealth);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            Assert.AreEqual(EventType.FighterSealed, logs[0].Type);
            Assert.AreEqual(shade, logs[0].Sender);
        }

        [Test]
        public void CorrectlyHealsMoveExecutor_SealingMoveSucceeds()
        {
            Shade shade = (Shade)TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
            _logger.Subscribe(EventType.FighterSealed, shade);
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            fighter.SetHealth(10, 9);

            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, fighter);

            _chanceService.PushEventOccurs(true);

            Assert.AreNotEqual(fighter.MaxHealth, fighter.CurrentHealth);

            //act
            _copperBell.ExecuteMove(moveWithTarget);

            //assert
            Assert.AreEqual(fighter.MaxHealth, fighter.CurrentHealth);
        }

        [Test]
        public void CorrectlyHealsMoveExecutor_SealingMoveSucceeds_OnlyHealsForShadesCurrentHealth()
        {
            Shade shade = (Shade)TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
            _logger.Subscribe(EventType.FighterSealed, shade);
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            fighter.SetHealth(shade.MaxHealth + 2, 1);

            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, fighter);

            _chanceService.PushEventOccurs(true);

            Assert.AreEqual(1, fighter.CurrentHealth);

            //act
            _copperBell.ExecuteMove(moveWithTarget);

            //assert
            Assert.AreEqual(shade.MaxHealth + 1, fighter.CurrentHealth);
        }

        [Test]
        public void SealMove_DifferentEnemyHealthLevels_CorrectlyGeneratesDifferentChanceSuccessRates()
        {
            double[] expectedChances = { 1.0/3, 0.5, 0.65, 0.8 };
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            for (var i = 0; i < 4; ++i)
            {
                Shade shade = (Shade) TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
                shade.PhysicalDamage(i);
                Assert.AreEqual(4, shade.MaxHealth);

                BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, fighter);
                
                _chanceService.PushEventOccurs(true);

                //act
                _copperBell.ExecuteMove(moveWithTarget);

                //assert
                Assert.AreEqual(expectedChances[i], _chanceService.LastChanceVals[i],
                    $"for index value {i}, shade health is {shade.CurrentHealth} out of {shade.MaxHealth}");
            }
        }

        [Test, Pairwise]
        public void SealMove_DifferentShadeLevels_CorrectlyGeneratesDifferentChanceSuccessRates(
            [Range(1, 5)] int shadeLevel, [Range(0, 1)] int damage)
        {
            double expectedChance = damage == 0 ? 1.0/3 : 0.5;
            int levelDiff = shadeLevel - 1;
            double chanceDecrease = levelDiff*0.1;

            expectedChance = Math.Max(expectedChance - chanceDecrease, 0.1);
            
            Shade shade = FighterFactory.GetShade(1, shadeLevel: shadeLevel);
            shade.PhysicalDamage(damage);

            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, _fighter);

            _chanceService.PushEventOccurs(true);

            //act
            _copperBell.ExecuteMove(moveWithTarget);


            //assert
            Assert.AreEqual(expectedChance, _chanceService.LastChanceVals[0]);
        }

        [Test]
        public void SealMove_MoreEffectiveForSilverBellThanCopperBell()
        {
            //arrange
            Shade shade = FighterFactory.GetShade(1);
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_prayMove, shade, _fighter);
            _chanceService.PushEventsOccur(false, false);

            //act
            _silverBell.ExecuteMove(moveWithTarget);
            _copperBell.ExecuteMove(moveWithTarget);
            
            //assert
            List<double> chances = _chanceService.LastChanceVals;

            Assert.Greater(chances[0], chances[1]);
        }

        #endregion

        #region blood Move

        [Test]
        public void CorrectlySealsShade_WhenExecutingControlMove()
        {
            Shade shade = (Shade)TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
            _logger.Subscribe(EventType.FighterSealed, shade);
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            BattleMoveWithTargetAndNumberInput moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, fighter, 0);

            _chanceService.PushEventOccurs(true);

            //act
            _copperBell.ExecuteMove(moveWithTarget);

            //assert
            Assert.AreEqual(0, shade.CurrentHealth);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            Assert.AreEqual(EventType.FighterSealed, logs[0].Type);
            Assert.AreEqual(shade, logs[0].Sender);
        }

        [Test]
        public void CorrectlyRaisesMoveExecutorsMaxHealth_ByShadeLevel_DominatingMoveSucceeds([Range(1,4)] int shadeLevel)
        {
            Shade shade = FighterFactory.GetShade(1, shadeLevel: shadeLevel);
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            const int initialMaxHealth = 10;
            fighter.SetHealth(initialMaxHealth);

            BattleMoveWithTargetAndNumberInput moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, fighter, 1);

            _chanceService.PushEventOccurs(true);

            //act
            _copperBell.ExecuteMove(moveWithTarget);

            //assert
            int expectedMaxHealth = initialMaxHealth + shadeLevel;
            Assert.AreEqual(expectedMaxHealth, fighter.MaxHealth);
        }

        [Test]
        public void BloodMove_DifferentEnemyHealthLevels_CorrectlyGeneratesDifferentChanceSuccessRates()
        {
            double[] expectedChances = { 1.0 / 3 + .05, 0.55, 0.7, 0.85 };
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            for (var i = 0; i < 4; ++i)
            {
                Shade shade = (Shade)TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
                shade.PhysicalDamage(i);
                int currentHealth = shade.CurrentHealth;
                Assert.AreEqual(4, shade.MaxHealth);

                BattleMoveWithTargetAndNumberInput moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, fighter, 1);

                _chanceService.PushEventOccurs(true);

                //act
                _silverBell.ExecuteMove(moveWithTarget);

                //assert
                Assert.LessOrEqual(Math.Abs(expectedChances[i] - _chanceService.LastChanceVals[i]), 0.0001,
                    $"for index value {i}, shade health was {currentHealth} out of {shade.MaxHealth}");
            }
        }

        [Test, Pairwise]
        public void BloodMove_DifferentShadeLevels_CorrectlyGeneratesDifferentChanceSuccessRates(
            [Range(1, 5)] int shadeLevel, [Range(0, 1)] int damage)
        {
            double expectedChance = damage == 0 ? 1.0 / 3 : 0.5;
            int levelDiff = shadeLevel - 1;
            double chanceDecrease = levelDiff * 0.1;

            expectedChance = Math.Max(expectedChance - chanceDecrease, 0.1);
            expectedChance += .05;

            Shade shade = FighterFactory.GetShade(1, shadeLevel: shadeLevel);
            shade.PhysicalDamage(damage);

            BattleMoveWithTargetAndNumberInput moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, _fighter, 1);

            _chanceService.PushEventOccurs(true);

            //act
            _silverBell.ExecuteMove(moveWithTarget);
            
            //assert
            Assert.AreEqual(expectedChance, _chanceService.LastChanceVals[0]);
        }

        [Test]
        public void BloodMove_DifferentOfferedBloodValues_CorrectlyGeneratesDifferentChanceSuccessRates()
        {
            TestHumanFighter fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            for (var i = 1; i <= 4; ++i)
            {
                Shade shade = (Shade)TestFighterFactory.GetFighter(TestFighterType.Shade, 1);
                shade.PhysicalDamage(1);

                BattleMoveWithTargetAndNumberInput moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, fighter, i);

                _chanceService.PushEventOccurs(true);

                //act
                _silverBell.ExecuteMove(moveWithTarget);

                //assert
                double expectedChance = 0.5 + (i*0.05);
                Assert.AreEqual(expectedChance, _chanceService.LastChanceVals[i - 1],
                    $"for index value {i}");
            }
        }

        [Test]
        public void BloodMove_MoreEffectiveForCopperBellThanCopperBell()
        {
            //arrange
            Shade shade = FighterFactory.GetShade(1);
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTargetAndNumberInput(_bloodMove, shade, _fighter, 0);
            _chanceService.PushEventsOccur(false, false);

            //act
            _copperBell.ExecuteMove(moveWithTarget);
            _silverBell.ExecuteMove(moveWithTarget);

            //assert
            List<double> chances = _chanceService.LastChanceVals;

            Assert.Greater(chances[0], chances[1]);
        }

        #endregion
    }
}
