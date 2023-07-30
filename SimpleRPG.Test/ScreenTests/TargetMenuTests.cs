using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class TargetMenuTests : ScreenTestBase
    {
        private TargetMenu _menu;
        private MockInput _menuInput;
        private MockOutput _menuOutput;
        private TestMenuManager _menuManager;
        private HumanFighter _player;
        private IFighter _ally1;
        private IFighter _ally2;
        private Team _playerTeamNoAllies;
        private Team _playerTeamWithAllies;
        private Team _enemyTeam;

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();
            _menuManager = new TestMenuManager(_menuInput, _menuOutput);

            _player = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            _ally1 =  FighterFactory.GetFighter(FighterType.Fairy, 1);
            _ally2 =  FighterFactory.GetFighter(FighterType.Goblin, 1);

            _playerTeamNoAllies = new Team(_menuManager, _player);
            _playerTeamWithAllies = new Team(_menuManager, _player, _ally1, _ally2);

            _enemyTeam = CreateEnemyTeam();

            _menu = (TargetMenu)Globals.MenuFactory.GetMenu(MenuType.ChooseTargetMenu, _menuInput, _menuOutput);
            _menu.Build(_player, _playerTeamNoAllies, _enemyTeam, null);
        }

        [TearDown]
        public void TearDown()
        {
            _menu = null;
            _menuInput = null;
            _menuOutput = null;
        }

        private static Team CreateEnemyTeam()
        {
            var list = new List<IFighter>
            {
                FighterFactory.GetFighter(FighterType.Goblin, 1),
                FighterFactory.GetFighter(FighterType.Goblin, 1)
            };

            return new Team(TestMenuManager.GetTestMenuManager(), list);
        }

        [Test]
        public void CorrectlyRenamesEnemiesIfMultipleEnemiesOfSameType()
        {
            _menuInput.Push("Goblin A");

            var ret = _menu.GetInput(MoveFactory.Get(BattleMoveType.Attack), null);

            Assert.AreEqual(_enemyTeam.Fighters[0], ret.Target);
        }

        [Test]
        public void TargetMenu_DoesNotDisplayDefeatedEnemies()
        {
            var enemy = _enemyTeam.Fighters[0];
            enemy.PhysicalDamage(enemy.MaxHealth);

            //should result in an error
            _menuInput.Push("Goblin A");
            _menuInput.Push("Goblin B");

            var ret = _menu.GetInput(MoveFactory.Get(BattleMoveType.Attack), null);

            Assert.AreEqual(_enemyTeam.Fighters[1], ret.Target);

            var outputs = _menuOutput.GetOutputs();

            //5 for each time menu is printed (prompt, only one enemy displayed, back prompt, help prompt, status prompt)
            //1 for error message
            Assert.AreEqual(11, outputs.Length);

            Assert.AreEqual(MockOutputMessageType.Error, outputs[5].Type);
        }

        [Test]
        public void TargetMenu_CorrectlyHandlesBackOption()
        {
            //should result in an error
            _menuInput.Push("back");

            var ret = _menu.GetInput(MoveFactory.Get(BattleMoveType.Attack), null);

            Assert.AreEqual("back", ret.Description);
        }

        [Test]
        public void TargetMenu_CorrectlyHandlesTargetTypes_SelfTarget()
        {
            var selfTargetMove = TestMoveFactory.Get(TargetType.Self);

            _menuInput.Push("1");

            _menu.GetInput(selfTargetMove, null);

            var outputs = _menuOutput.GetOutputs();

            Assert.AreEqual(5, outputs.Length); //prompt text, option, status, back, help
        }

        private MenuSelection SingleAllyTargetType_Setup(string menuInput)
        {
            var targetAllyMove = TestMoveFactory.Get(TargetType.SingleAlly);

            _menu.Build(_player, _playerTeamWithAllies, _enemyTeam, null);

            _menuInput.Push(menuInput);

            return _menu.GetInput(targetAllyMove, null);
        }

        [Test]
        public void TargetMenu_CorrectlyBuildsMenuActions_SingleAllyTargetType()
        {
            SingleAllyTargetType_Setup("1");

            List<MenuAction> menuActions = _menu.MenuActions;

            int expectedCount = _playerTeamWithAllies.Fighters.Count - 1; //exclude the owner
            Assert.AreEqual(expectedCount, menuActions.Count);

            Assert.AreEqual(_ally1, menuActions[0].Fighter);
            Assert.AreEqual(_ally2, menuActions[1].Fighter);
        }

        [Test]
        public void TargetMenu_CorrectlyBuildsMenuActions_SingleAllyTargetType_OneAllyDead()
        {
            _ally1.PhysicalDamage(_ally1.MaxHealth);
            SingleAllyTargetType_Setup("1");

            List<MenuAction> menuActions = _menu.MenuActions;

            int expectedCount = _playerTeamWithAllies.Fighters.Count - 2; //exclude both the owner and the dead ally
            Assert.AreEqual(expectedCount, menuActions.Count);

            Assert.AreEqual(_ally2, menuActions[0].Fighter);
        }

        [Test]
        public void TargetMenu_PrintsCorrectOutputs_SingleAllyTargetType()
        {
            SingleAllyTargetType_Setup("1");

            var outputs = _menuOutput.GetOutputs();

            int expectedOutputLength = 4; //prompt text, status, back, help options
            expectedOutputLength += (_playerTeamWithAllies.Fighters.Count - 1);
            Assert.AreEqual(expectedOutputLength, outputs.Length); //prompt text, option, status, back, help
        }

        [Test]
        public void TargetMenu_ReturnsCorrectSelection_SingleAllyTargetType()
        {
            MenuSelection menuSelection = SingleAllyTargetType_Setup("1");

            IFighter targetedFighter = _playerTeamWithAllies.Fighters.First(f => f != _player);
            Assert.AreEqual(targetedFighter, menuSelection.Target);
        }

        private MenuSelection SingleAllyOrSelfTargetType_Setup(string menuInput)
        {
            var targetAllyOrSelfMove = TestMoveFactory.Get(TargetType.SingleAllyOrSelf);

            _menu.Build(_player, _playerTeamWithAllies, _enemyTeam, null);

            _menuInput.Push(menuInput);

            return _menu.GetInput(targetAllyOrSelfMove, null);
        }

        [Test]
        public void TargetMenu_CorrectlyBuildsMenuActions_SingleAllyOrSelfTargetType()
        {
            SingleAllyOrSelfTargetType_Setup("1");

            List<MenuAction> menuActions = _menu.MenuActions;

            int expectedCount = _playerTeamWithAllies.Fighters.Count;

            Assert.AreEqual(expectedCount, menuActions.Count);

            int i = 0;

            Assert.AreEqual(_player, menuActions[i++].Fighter);
            Assert.AreEqual(_ally1, menuActions[i++].Fighter);
            Assert.AreEqual(_ally2, menuActions[i].Fighter);
        }

        [Test]
        public void TargetMenu_CorrectlyBuildsMenuActions_SingleAllyOrSelfTargetType_OneAllyDead()
        {
            _ally1.PhysicalDamage(_ally1.MaxHealth);
            SingleAllyOrSelfTargetType_Setup("1");

            List<MenuAction> menuActions = _menu.MenuActions;

            int expectedCount = _playerTeamWithAllies.Fighters.Count - 1; //exclude the dead ally
            Assert.AreEqual(expectedCount, menuActions.Count);

            Assert.AreEqual(_player, menuActions[0].Fighter);
            Assert.AreEqual(_ally2, menuActions[1].Fighter);
        }

        [Test]
        public void TargetMenu_PrintsCorrectOutputs_SingleAllyOrSelfTargetType()
        {
            SingleAllyOrSelfTargetType_Setup("1");

            var outputs = _menuOutput.GetOutputs();

            int expectedOutputLength = 4; //prompt text, status, back, help options
            expectedOutputLength += _playerTeamWithAllies.Fighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length); //prompt text, an option for each fighter, status, back, help
        }

        [Test]
        public void TargetMenu_ReturnsCorrectSelection_SingleAllyOrSelfTargetType()
        {
            MenuSelection menuSelection = SingleAllyOrSelfTargetType_Setup("1");

            IFighter targetedFighter = _playerTeamWithAllies.Fighters[0];
            Assert.AreEqual(targetedFighter, menuSelection.Target);
        }
    }
}
