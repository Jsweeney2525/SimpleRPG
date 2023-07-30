using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.TerrainInteractableTests
{
    [TestFixture]
    public class BattleManagerTerrainInteractableTests
    {
        private MockInput _input;
        private MockOutput _output;
        private MockChanceService _chanceService;
        private BattleManager _battleManager;
        private TestMenuManager _menuManager;

        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;

        private ShadeFighterGrouping _shadeGrouping;
        private Team _shadeTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
        private readonly BattleMove _doNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _chanceService = new MockChanceService();
            _menuManager = new TestMenuManager(_input, _output, new MenuFactory());

            _battleManager = new BattleManager(_chanceService, _input, _output);

            _shadeGrouping = new ShadeFighterGrouping(_chanceService, new Shade(1, _chanceService, 1), new Shade(1, _chanceService, 1), new Shade(1, _chanceService, 1));
            _shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shadeGrouping);

            _humanFighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanFighter.SetHealth(5);
            _humanFighter.SetSpeed(_shadeTeam.Fighters[0].Speed + 1);
            _humanTeam = new TestTeam(_menuManager, _humanFighter);

            _enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemy);
        }

        [TearDown]
        public void TearDown()
        {
            _input = null;
            _output = null;
            _chanceService = null;

            _humanFighter = null;
            _humanTeam = null;
            _enemyTeam = null;
        }

        public List<Bell> GetBells(params BellType[] bellTypes)
        {
            List<Bell> generatedBells = new List<Bell>();

            foreach (BellType bellType in bellTypes)
            {
                generatedBells.Add(new Bell($"{bellType.ToString().ToLower()} bell", bellType, new MenuFactory(), _chanceService));
            }

            return generatedBells;
        }

        #region bell tests

        [Test]
        public void BattleManagerCorrectlyIdentifiesBellInIntro()
        {
            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _humanFighter.SetMove(_runawayMove);
            _enemy.SetMove(_doNothingMove);

            _battleManager.Battle(_humanTeam, _enemyTeam, bells.Cast<TerrainInteractable>().ToList());

            MockOutputMessage[] outputs = _output.GetOutputs();

            int bellIntroIndex = 1 + _enemyTeam.Fighters.Count; //"time for a battle" and then each "encountered ____"

            foreach (Bell bell in bells)
            {
                MockOutputMessage output = outputs[bellIntroIndex];

                Assert.AreEqual($"There is a {bell.DisplayName} on the field\n", output.Message);

                bellIntroIndex++;
            }

            Assert.AreEqual(bellIntroIndex, _output.GetClearIndices()[1]);
        }

        [Test]
        public void BattleManagerCorrectlyExecutesBellSealingMove()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);
            
            _input.Push("special", "pray copper", "1", "run", "y");
            
            _chanceService.PushEventOccurs(true); //sealing is effective
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false); //both remaining shades will attack

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert
            
            //1st shade was sealed
            Assert.AreEqual(0, shades[0].CurrentHealth);

            //other shades did not absorb its power
            Assert.AreEqual(1, shades[1].ShadeExperience);
            Assert.AreEqual(1, shades[2].ShadeExperience);
        }

        [Test]
        public void BattleManagerCorrectly_DisplaysSuccessMessage_BellSealingMoveWorked()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _input.Push("special", "pray copper", "1", "run", "y");

            _chanceService.PushEventOccurs(true); //sealing is effective
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false); //both remaining shades will attack and miss

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            string expectedOutputMessage = $"{shades[0].DisplayName} has been sealed!\n";
            MockOutputMessage outputWithSuccessMessage = outputs.FirstOrDefault(o => o.Message == expectedOutputMessage);
            Assert.NotNull(outputWithSuccessMessage);

            string expectedHealingMessage = $"{_humanFighter.DisplayName} has been healed for 0 HP!\n";
            MockOutputMessage outputWithHealingMessage = outputs.FirstOrDefault(o => o.Message == expectedHealingMessage);
            Assert.NotNull(outputWithHealingMessage);
        }

        [Test]
        public void BattleManagerCorrectly_DisplaysFailureMessage_BellSealingMoveFailed()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _input.Push("special", "pray copper", "1", "run", "y");

            _chanceService.PushEventOccurs(false); //sealing failed
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false, false); //all remaining shades will attack and miss

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            string expectedOutputMessage = $"but {shades[0].DisplayName} was too strong!\n";
            MockOutputMessage outputWithSuccessMessage = outputs.FirstOrDefault(o => o.Message == expectedOutputMessage);
            Assert.NotNull(outputWithSuccessMessage);
        }

        [Test]
        public void BattleManagerCorrectlyExecutesBellBloodMove()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);
            int previousMaxHealth = _humanFighter.MaxHealth;

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _input.Push("special", "blood copper", "1", "1", "run", "y");

            _chanceService.PushEventOccurs(true); //sealing is effective
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false); //both remaining shades will attack and miss

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert

            //1st shade was sealed
            Assert.AreEqual(0, shades[0].CurrentHealth);

            //other shades did not absorb its power
            Assert.AreEqual(1, shades[1].ShadeExperience);
            Assert.AreEqual(1, shades[2].ShadeExperience);

            Assert.AreEqual(previousMaxHealth + shades[0].ShadeExperience, _humanFighter.MaxHealth);
        }

        [Test]
        public void BattleManagerCorrectly_DisplaysSuccessMessage_BellBloodMoveWorked()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _input.Push("special", "blood copper", "1", "1", "run", "y");

            _chanceService.PushEventOccurs(true); //sealing is effective
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false); //both remaining shades will attack and miss

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            string expectedOutputMessage = $"{shades[0].DisplayName} has been absorbed!\n";
            MockOutputMessage outputWithSuccessMessage = outputs.FirstOrDefault(o => o.Message == expectedOutputMessage);
            Assert.NotNull(outputWithSuccessMessage);

            string expectedHealingMessage = $"{_humanFighter.DisplayName} has had their max HP increased by {shades[0].ShadeExperience}!\n";
            MockOutputMessage outputWithHealingMessage = outputs.FirstOrDefault(o => o.Message == expectedHealingMessage);
            Assert.NotNull(outputWithHealingMessage);
        }

        [Test]
        public void BattleManagerCorrectly_DisplaysSuccessMessage_BellBloodMoveFailed()
        {
            //Arrange
            List<Shade> shades = _shadeGrouping.GetShades();

            _humanFighter.SetSpeed(shades[0].Speed + 1);

            List<Bell> bells = GetBells(BellType.Copper, BellType.Silver);

            _input.Push("special", "blood copper", "1", "1", "run", "y");

            _chanceService.PushEventOccurs(false); //sealing failed
            int attackIndex = shades[0].AvailableMoves.FindIndex(am => am.MoveType == BattleMoveType.Attack);
            _chanceService.PushWhichEventsOccur(attackIndex, attackIndex, attackIndex);
            _chanceService.PushEventsOccur(false, false, false); //both remaining shades will attack and miss

            //Act
            _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            //Assert
            MockOutputMessage[] outputs = _output.GetOutputs();

            string expectedOutputMessage = $"but {shades[0].DisplayName} was too strong!\n";
            MockOutputMessage outputWithSuccessMessage = outputs.FirstOrDefault(o => o.Message == expectedOutputMessage);
            Assert.NotNull(outputWithSuccessMessage);
        }

        [Test]
        public void BattleCorrectlyEnds_AllShadesSealed([Range(1,3)] int numberShades)
        {
            List<Bell> bells = GetBells(BellType.Copper);

            List<Shade> shades = new List<Shade>();

            int chargeMoveIndex = -1;

            for (int i = numberShades; i > 0; --i)
            {
                shades.Add(new Shade(1, _chanceService, 1));
            }

            ShadeFighterGrouping shadeGrouping = new ShadeFighterGrouping(_chanceService, shades.ToArray());
            _shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), shadeGrouping);

            chargeMoveIndex =
                        shades[0].GetExecutableMoves(_humanTeam).FindIndex(am => am.MoveType == BattleMoveType.Special);

            for (int i = numberShades; i > 0; --i)
            {
                _input.Push("special", "pray copper", "1");
                _chanceService.PushEventOccurs(true); //sealing is successful

                for (int j = i; j > 0; --j)
                {
                    _chanceService.PushWhichEventOccurs(chargeMoveIndex);
                }
            }

            //act
            BattleEndStatus battleEndStatus = _battleManager.Battle(_humanTeam, _shadeTeam, bells.Cast<TerrainInteractable>().ToList());

            Assert.AreEqual(BattleEndStatus.Victory, battleEndStatus);
        }

        #endregion bell tests
    }
}
