using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class SpecialMoveSelectionMenuTests : ScreenTestBase
    {
        SpecialMoveSelectionMenu _menu;

        private MockInput _menuInput;
        private MockOutput _menuOutput;
        private TestMenuManager _menuManager;

        private TestHumanFighter _player;
        private Team _playerTeam;
        private Team _enemyTeam;

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();
            _menuManager = new TestMenuManager(_menuInput, _menuOutput);

            _enemyTeam = new Team(_menuManager, FighterFactory.GetFighter(FighterType.Goblin, 1));
            _player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _playerTeam = new Team(_menuManager, _player);

            _menu = (SpecialMoveSelectionMenu)Globals.MenuFactory.GetMenu(MenuType.ChooseSpecialAttackMenu, _menuInput, _menuOutput);
            _menu.Build(_player, _playerTeam, _enemyTeam, null);
        }

        [TearDown]
        public void TearDown()
        {
            _menu = null;
            _menuInput = null;
            _menuOutput = null;
            _enemyTeam = null;
            _player = null;
            _playerTeam = null;
        }

        [Test]
        public void CorrectlySetsUpMenuForHumanPlayer()
        {
            BattleMove doNothing = MoveFactory.Get(BattleMoveType.DoNothing);
            BattleMove shieldMove = MoveFactory.Get(BattleMoveType.Shield, "iron shield");

            _player.AddMove(doNothing);
            _player.AddMove(shieldMove);

            _menu.Build(_player, _playerTeam, _enemyTeam, null);

            List<MenuAction> menuActions = _menu.MenuActions;

            Assert.AreEqual(2, menuActions.Count);

            Assert.True(menuActions.Exists(ma => ma.BattleMove == doNothing));
            Assert.True(menuActions.Exists(ma => ma.BattleMove == shieldMove));
        }
    }
}
