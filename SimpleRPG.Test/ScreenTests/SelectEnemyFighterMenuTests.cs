using NUnit.Framework;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{

    public class SelectEnemyFighterMenuTests
    {
        private MockInput _input;
        private MockOutput _output;

        private SelectEnemyFighterMenu _menu;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();

            _menu = new SelectEnemyFighterMenu(_input, _output, false);
        }

        [Test]
        public void ReturnsCorrectMenuSelection()
        {
            _input.Push("Goblin", "1");

            MenuSelection menuSelection = _menu.GetInput();

            SelectEnemyFighterMenuSelection selectEnemyMenuSelection = menuSelection as SelectEnemyFighterMenuSelection;
            Assert.NotNull(selectEnemyMenuSelection);
            Assert.AreEqual(FighterType.Goblin, selectEnemyMenuSelection.FighterType);
            Assert.AreEqual(1, selectEnemyMenuSelection.FighterLevel);
        }
    }
}
