using System.Linq;
using SimpleRPG.Test.MockClasses;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class SelectEnemyTeamMenuManagerTests : ScreenTestBase
    {
        private SelectEnemyTeamMenuManager _menuManager;
        private MockInput _menuInput;
        private MockOutput _menuOutput;

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();

            _menuManager = new SelectEnemyTeamMenuManager(_menuInput, _menuOutput);
        }

        [TearDown]
        public void TearDown()
        {
            _menuManager = null;
            _menuInput = null;
            _menuOutput = null;
        }

        [Test]
        public void ReturnsAppropriateTeam_SingleEnemy()
        {
            _menuInput.Push("yes", "goblin", "1", "no");

            BattleConfigurationSpecialFlag battleFlag;
            Team returnedTeam = _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);

            Assert.AreEqual(1, returnedTeam.Fighters.Count);

            HumanControlledEnemyFighter humanControlledFighter = returnedTeam.Fighters[0] as HumanControlledEnemyFighter;

            Assert.NotNull(humanControlledFighter);

            Assert.IsTrue(humanControlledFighter.Fighter is Goblin);
        }

        [Test]
        public void ReturnsAppropriateTeam_MultipleEnemies([Values(1, 2, 3)] int numberOfExtraEnemies)
        {
            _menuInput.Push("yes", "goblin", "1");

            for (var i = 0; i < numberOfExtraEnemies; ++i)
            {
                _menuInput.Push("yes", "fairy", "1");
            }

            _menuInput.Push("no");

            BattleConfigurationSpecialFlag battleFlag;
            Team returnedTeam = _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);

            Assert.AreEqual(numberOfExtraEnemies + 1, returnedTeam.Fighters.Count);

            HumanControlledEnemyFighter humanControlledFighter = returnedTeam.Fighters[0] as HumanControlledEnemyFighter;

            Assert.NotNull(humanControlledFighter);
            Assert.IsTrue(humanControlledFighter.Fighter is Goblin);
            Assert.AreEqual(1, humanControlledFighter.Fighter.Level);

            for (var i = 0; i < numberOfExtraEnemies; ++i)
            {
                humanControlledFighter = returnedTeam.Fighters[i + 1] as HumanControlledEnemyFighter;

                Assert.NotNull(humanControlledFighter);
                Assert.IsTrue(humanControlledFighter.Fighter is Fairy);
                Assert.AreEqual(1, humanControlledFighter.Fighter.Level);
            }
        }

        [Test]
        public void ReturnsAppropriateTeam_MaximumNumberOfEnemies()
        {
            _menuInput.Push("yes", "goblin", "3");

            for (var i = 0; i < 4; ++i)
            {
                _menuInput.Push("yes", "warrior", "3");
            }

            BattleConfigurationSpecialFlag battleFlag;
            Team returnedTeam = _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);

            Assert.AreEqual(5, returnedTeam.Fighters.Count);

            for (var i = 0; i < 5; ++i)
            {
                HumanControlledEnemyFighter humanControlledFighter = returnedTeam.Fighters[i] as HumanControlledEnemyFighter;

                Assert.NotNull(humanControlledFighter);
                if (i == 0)
                {
                    Assert.IsTrue(humanControlledFighter.Fighter is Goblin);
                }
                else
                {
                    Assert.IsTrue(humanControlledFighter.Fighter is Warrior);
                }
                Assert.AreEqual(3, humanControlledFighter.Fighter.Level);
            }
        }

        [Test]
        public void ReturnsAppropriateTeam_CorrectlyHandlesBackOption()
        {
            _menuInput.Push("yes", "goblin", "1", "yes", "back", "shieldGuy", "2", "no");

            BattleConfigurationSpecialFlag battleFlag;
            Team returnedTeam = _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);

            Assert.AreEqual(1, returnedTeam.Fighters.Count);

            HumanControlledEnemyFighter humanControlledFighter = returnedTeam.Fighters[0] as HumanControlledEnemyFighter;

            Assert.NotNull(humanControlledFighter);
            Assert.IsTrue(humanControlledFighter.Fighter is ShieldGuy);
            Assert.AreEqual(2, humanControlledFighter.Fighter.Level);
        }

        [Test]
        public void ReturnsAppropriateTeam_HandlesInitialPromptForHumanControlledOrComputerControlledTeam([Values("yes", "no")] string firstPrompt)
        {
            _menuInput.Push(firstPrompt, "goblin", "1", "yes", "fairy", "1", "no");

            BattleConfigurationSpecialFlag battleFlag;
            Team returnedTeam = _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);
            IFighter firstFighter, secondFighter;

            if (firstPrompt == "yes")
            {
                Assert.IsTrue(returnedTeam.Fighters.TrueForAll(f => f is HumanControlledEnemyFighter));

                HumanControlledEnemyFighter humanControlledFirstFighter = returnedTeam.Fighters[0] as HumanControlledEnemyFighter;
                HumanControlledEnemyFighter humanControlledSecondFighter = returnedTeam.Fighters[1] as HumanControlledEnemyFighter;

                firstFighter = humanControlledFirstFighter.Fighter;
                secondFighter = humanControlledSecondFighter.Fighter;
            }
            else
            {
                Assert.IsNull(returnedTeam.Fighters.FirstOrDefault(f => f is HumanControlledEnemyFighter));

                firstFighter = returnedTeam.Fighters[0];
                secondFighter = returnedTeam.Fighters[1];
            }

            Assert.IsInstanceOf<Goblin>(firstFighter);
            Assert.IsInstanceOf<Fairy>(secondFighter);
        }

        [Test]
        public void ReturnsAppropriateBattleConfigurationFlag([Values("yes", "no")] string humanControlledTeam)
        {
            //Arrange
            _menuInput.Push(humanControlledTeam, "barbarian", "1", "no");
        
            //Act
            BattleConfigurationSpecialFlag battleFlag;
            _menuManager.GetTeam(TestMenuManager.GetTestMenuManager(), _menuInput, _menuOutput, out battleFlag);

            //Assert
            Assert.AreEqual(BattleConfigurationSpecialFlag.FirstBarbarianBattle, battleFlag);
        }
    }
}
