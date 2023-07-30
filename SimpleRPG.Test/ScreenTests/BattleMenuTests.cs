using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class BattleMenuTests
    {
        private MockInput _input;
        private MockOutput _output;

        private BattleMenu _menu;

        private TestHumanFighter _fighter;
        private TestTeam _fighterTeam;

        private TestEnemyFighter _enemy;
        private TestTeam _enemyTeam;

        private int _fullMenuPromptLength;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();

            _fighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _fighterTeam = new TestTeam(_fighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new TestTeam(_enemy);
        }

        [TearDown]
        public void TearDown()
        {
            _input = null;
            _output = null;

            _fighter = null;
            _fighterTeam = null;
            _enemy = null;
            _enemyTeam = null;
        }

        private void BuildMenu(List<MenuAction> specialMenuActions = null)
        {
            _menu = new BattleMenu(false, null, _input, _output, new MenuFactory(), specialMenuActions);
            _menu.Build(_fighter, _fighterTeam, _enemyTeam, null);

            _fullMenuPromptLength = _menu.MenuActions.Count + 3; //prompt, plus 'help' and 'status' options
        }

        [Test]
        public void NoSpecialActionsMenuCreated_NoSpecialActionsSupplied()
        {
            BuildMenu();

            Assert.AreEqual(3, _menu.MenuActions.Count);
        }

        [Test]
        public void SpecialMenuActions_CreateSpecialActionsMenu()
        {
            List<string> specialMenuActionDisplays = new List<string>
            {
                "dance like no one's watching",
                "thumb wrestle"
            };

            List<MenuAction> specialMenuActions = specialMenuActionDisplays.Select(s => new MenuAction(s)).ToList();

            BuildMenu(specialMenuActions);

            Assert.AreEqual(4, _menu.MenuActions.Count);
            Assert.NotNull(_menu.MenuActions.FirstOrDefault(ma => ma.DisplayText.ToUpper() == "SPECIAL ACTIONS"));
        }


        [Test]
        public void ReturnsCorrectInput_FromSpecialMenu([Range(0,1)] int selectedIndex)
        {
            List<string> specialMenuActionDisplays = new List<string>
            {
                "dance like no one's watching",
                "thumb wrestle"
            };

            List<MenuAction> specialMenuActions = specialMenuActionDisplays.Select(s => new MenuAction(s)).ToList();

            BuildMenu(specialMenuActions);
            _input.Push("special actions", $"{selectedIndex + 1}");

            MenuSelection menuSelection = _menu.GetInput();

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedOutputLength = _fullMenuPromptLength + specialMenuActions.Count + 4;  //prompt, plus 'back', 'help' and 'status' options

            Assert.AreEqual(expectedOutputLength, outputs.Length);

            Assert.AreEqual(specialMenuActions[selectedIndex].DisplayText, menuSelection.Description);
        }
    }
}
