using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Helpers;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class BasicMenuTests : ScreenTestBase
    {
        //TODO need a way to pagenate menus
        //TODO: "item" options currently explode the program. Oops!
        //TODO: help screen should show field effects, status effects, and help text for moves

        private Menu _menu;
        private MockInput _menuInput;
        private MockOutput _menuOutput;
        private TestMenuManager _menuManager;

        private HumanFighter _player;
        private EnemyFighter _enemy;
        private const string MenuPrompt = "What would you like to do?";
        private const string HelpText = "This is your basic battle menu. You are facing a monster and can either choose to do damage with the \"attack\" command,\n" + "use an item for various effect\nOr you could run, you coward.";
        private string _expectedHelpText;

        private List<string> _fullMenuPrompt;
        private int _fullMenuPromptLength;
        private const int FighterStatusPromptLength = 6; //Name, HP, Mana, Str, Def, Speed

        private bool _allowBack;
        private bool _allowHelp;
        private bool _allowStatus;
        
        private readonly List<MenuAction> _menuActions = new List<MenuAction> {
                new MenuAction("fight", altText: "f"),
                new MenuAction("item"),
                new MenuAction("run") };

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _expectedHelpText = HelpText + "\n";
        }

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();
            _menuManager = new TestMenuManager(_menuInput, _menuOutput); 
            SetUpAndBuildMenu(false);
        }

        [TearDown]
        public void TearDown()
        {
            _menu = null;
            _menuInput = null;
            _menuOutput = null;
        }

        private void SetUpAndBuildMenu(bool allowBackOption, 
            bool allowHelpOption = true, 
            bool allowStatusOption = true, 
            bool shuffleOutputsFlag = false, 
            IChanceService chanceService = null,
            List<MenuAction> specialMenuActions = null,
            List<TerrainInteractable> terrainInteractable = null )
        {
            _allowBack = allowBackOption;
            _allowHelp = allowHelpOption;
            _allowStatus = allowStatusOption;

            int i = 1;

            _fullMenuPrompt = new List<string>
            {
                MenuPrompt + "\n",
                $"{i++}. fight\n"
            };

            if (specialMenuActions != null && specialMenuActions.Any())
            {
                _fullMenuPrompt.Add($"{i++}. special actions");
            }

            _fullMenuPrompt.Add($"{i++}. item\n");
            _fullMenuPrompt.Add($"{i}. run\n");

            if (_allowStatus)
            {
                _fullMenuPrompt.Add(StatusPrompt);
            }

            if (_allowBack)
            {
                _fullMenuPrompt.Add(BackPrompt);
            }

            if (_allowHelp)
            {
                _fullMenuPrompt.Add(HelpPrompt);
            }

            _fullMenuPromptLength = _fullMenuPrompt.Count;

            _menu = new Menu(allowBackOption, allowHelpOption, 
                allowStatusOption, MenuPrompt, null, 
                HelpText, _menuActions, _menuInput, _menuOutput, 
                chanceService, shuffleOutputsFlag);

            BuildMenu(_menu, terrainInteractable);
        }

        private void BuildMenu(IMenu menu, List<TerrainInteractable> terrainInteractables = null)
        {
            _player = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            var playerTeam = new Team(_menuManager, _player);
            _enemy = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            var enemyTeam = new Team(_menuManager, _enemy);

            menu.Build(_player, playerTeam, enemyTeam, terrainInteractables ?? new List<TerrainInteractable>());
        }

        private void TestMenuOutput(MockOutputMessage[] outputs, int startingIndex, int[] clearIndices, int clearStartingIndex, bool showedError = false)
        {
            for (var i = 0; i < _fullMenuPromptLength; ++i)
            {
                Assert.AreEqual(_fullMenuPrompt[i], outputs[startingIndex].Message); //prompt
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);
            }

            var index = startingIndex;
            //ClearScreen() is not called until *after* the error has been displayed
            if (showedError) { ++index; }
            Assert.AreEqual(index, clearIndices[clearStartingIndex]);
        }

        [Test]
        public void GetInput_ReturnsInputIfValidTextSpecified([Values("fight", "item", "run")] string input)
        {
            _menuInput.Push(input);
            MenuSelection ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            Assert.AreEqual(_fullMenuPromptLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0);

            Assert.AreEqual(input, ret.Description);
        }

        [Test]
        public void GetInput_ReturnsInputIfValidAlternativeTextSpecified()
        {
            _menuInput.Push("f");
            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            Assert.AreEqual(_fullMenuPromptLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0);

            Assert.AreEqual("fight", ret.Description);
        }

        [Test, Sequential]
        public void GetInput_DisplaysErrorMessageIfInvalidTextSpecified(
            [Values("foo", "itemz", "run!")] string errorInput,
            [Values("fight", "item", "run")] string validInput)
        {
            _menuInput.Push(errorInput);
            _menuInput.Push(validInput);

            MenuSelection ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show error message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0, true);

            Assert.AreEqual(2, clearIndices.Length);

            Assert.AreEqual(ErrorMessage, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[_fullMenuPromptLength].Type);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 1);

            Assert.AreEqual(validInput, ret.Description);
        }

        [Test]
        public void GetInput_ReturnsInputIfValidIndexSpecified([Values(1, 2, 3)] int input)
        {
            _menuInput.Push(input.ToString());
            MenuSelection ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            Assert.AreEqual(_fullMenuPromptLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0);

            Assert.AreEqual(_menuActions[input - 1].DisplayText, ret.Description);
        }

        [Test, Sequential]
        public void GetInput_DisplaysErrorMessageIfInvalidIndexSpecified(
            [Values(-1, 4, 5)] int errorInput,
            [Values(1, 2, 3)] int validInput)
        {
            _menuInput.Push(errorInput.ToString());
            _menuInput.Push(validInput.ToString());

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();
            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show error message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0, true);

            Assert.AreEqual(ErrorMessage, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[_fullMenuPromptLength].Type);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 1);

            Assert.AreEqual(_menuActions[validInput - 1].DisplayText, ret.Description);
        }

        [Test]
        public void GetInput_CorrectlyRespondsToHelpPrompt_HelpIsValid(
            [Values("fight", "item", "run")] string input)
        {
            _menuInput.Push("help");
            _menuInput.Push(input);

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show help message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0);

            Assert.AreEqual(_expectedHelpText, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[_fullMenuPromptLength].Type);

            //clear is called both before and after the help text is displayed, makign for a total of 3 calls to .ClearScreen() for this test
            Assert.AreEqual(_fullMenuPromptLength + 1, clearIndices[1]);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 2);

            Assert.AreEqual(input, ret.Description);
        }

        [Test]
        public void GetInput_CorrectlyDisplaysBackOption_WhenAllowBackIsTrue()
        {
            SetUpAndBuildMenu(true);

            _menuInput.Push("back");

            _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();
            Assert.AreEqual(_fullMenuPromptLength, outputs.Length); 

            TestMenuOutput(outputs, 0, clearIndices, 0);
        }

        [Test]
        public void GetInput_CorrectlyReturnsBack_WhenAllowBackIsTrue()
        {
            SetUpAndBuildMenu(true);

            _menuInput.Push("back");

            var ret = _menu.GetInput();

            Assert.AreEqual("back", ret.Description);
        }

        [Test]
        public void GetInput_CorrectlyDisplaysStatusScreens()
        {
            SetUpAndBuildMenu(true);

            _menuInput.Push("status");
            _menuInput.Push("back");

            _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = _fullMenuPromptLength * 2; //times 2 because it's displayed twice
            expectedLength += FighterStatusPromptLength; //no spells
            expectedLength += 2; //"Foes" header and one foe listed

            Assert.AreEqual(expectedLength, outputs.Length);
            Assert.AreEqual(4, clearIndices.Length); //once for each main menu, once after each human fighter (just 1), and another after all enemies

            TestMenuOutput(outputs, 0, clearIndices, 0);

            var index = _fullMenuPromptLength;
            Assert.AreEqual($"Fighter: {_player.DisplayName}\n", outputs[index++].Message);
            Assert.AreEqual($"HP: {_player.CurrentHealth} / {_player.MaxHealth}\n", outputs[index++].Message);
            Assert.AreEqual($"Mana: {_player.CurrentMana} / {_player.MaxMana}\n", outputs[index++].Message);
            Assert.AreEqual($"Strength: {_player.Strength}\n", outputs[index++].Message);
            Assert.AreEqual($"Defense: {_player.Defense}\n", outputs[index++].Message);
            Assert.AreEqual($"Speed: {_player.Speed}\n", outputs[index++].Message);

            Assert.AreEqual(index, clearIndices[1]);

            Assert.AreEqual("Remaining Foes:\n", outputs[index++].Message);
            Assert.AreEqual($"{_enemy.DisplayName}\n", outputs[index++].Message);

            Assert.AreEqual(index, clearIndices[2]);

            TestMenuOutput(outputs, index, clearIndices, 3);
        }

        [Test]
        public void StatusScreenCorrectlyDisplaysTerrainInteractableInformation_Bells()
        { 
            ChanceService chanceService = new ChanceService();
            MenuFactory menuFactory = new MenuFactory();

            List<TerrainInteractable> bells = new List<TerrainInteractable>
            {
                new Bell("copper bell", BellType.Copper, menuFactory, chanceService),
                new Bell("silver bell", BellType.Silver, menuFactory, chanceService)
            };

            SetUpAndBuildMenu(true, terrainInteractable: bells);

            _menuInput.Push("status");
            _menuInput.Push("back");

            _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = _fullMenuPromptLength * 2; //times 2 because it's displayed twice
            expectedLength += FighterStatusPromptLength; //no spells
            expectedLength += 2; //"Foes" header and one foe listed
            expectedLength += 3; //"Other" header and both bells listed

            Assert.AreEqual(expectedLength, outputs.Length);
            Assert.AreEqual(5, clearIndices.Length); //once for each main menu, once after each human fighter (just 1), one after enemy display, one after bells displayed

            var index = _fullMenuPromptLength + FighterStatusPromptLength + 2; //2 for the foes and enemy display information

            Assert.AreEqual("Other details:\n", outputs[index++].Message);
            foreach (TerrainInteractable bell in bells)
            {
                Assert.AreEqual($"{bell.GetFullDisplayString()}\n", outputs[index++].Message);
            }
        }

        [Test]
        public void GetInput_CorrectlyDisplaysStatusScreens_WhenStatusIsSpecified_NumerousFighters()
        {
            SetUpAndBuildMenu(true);

            var player1 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Dante");
            var player2 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Arrokoh");
            var player3 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "Shadow");

            var playerTeam = new Team(_menuManager, player1, player2, player3);

            var enemy1 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            var enemy2 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Ogre, 1);
            var enemy3 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Fairy, 1);
            var enemy4 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Golem, 1);

            var enemyTeam = new Team(_menuManager, enemy1, enemy2, enemy3, enemy4);

            _menu.Build(player1, playerTeam, enemyTeam, new List<TerrainInteractable>());

            _menuInput.Push("status");
            _menuInput.Push("back");

            _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = _fullMenuPromptLength * 2; //times 2 because it's displayed twice
            expectedLength += FighterStatusPromptLength * 3; //no spells, but there are 3 human fighters
            expectedLength += 5; //"Foes" header and 4 foes listed

            Assert.AreEqual(expectedLength, outputs.Length);
            Assert.AreEqual(6, clearIndices.Length); //once for each times main menu displayed (2), once after each human fighter (3), and another after all enemies

            TestMenuOutput(outputs, 0, clearIndices, 0);

            var index = _fullMenuPromptLength;
            var clearIndexIndex = 1;

            foreach (var player in playerTeam.Fighters)
            {
                Assert.AreEqual($"Fighter: {player.DisplayName}\n", outputs[index++].Message);
                Assert.AreEqual($"HP: {player.CurrentHealth} / {player.MaxHealth}\n", outputs[index++].Message);
                Assert.AreEqual($"Mana: {player.CurrentMana} / {player.MaxMana}\n", outputs[index++].Message);
                Assert.AreEqual($"Strength: {player.Strength}\n", outputs[index++].Message);
                Assert.AreEqual($"Defense: {player.Defense}\n", outputs[index++].Message);
                Assert.AreEqual($"Speed: {player.Speed}\n", outputs[index++].Message);

                Assert.AreEqual(index, clearIndices[clearIndexIndex++]);
            }

            Assert.AreEqual("Remaining Foes:\n", outputs[index++].Message);

            foreach (var enemy in enemyTeam.Fighters)
            {
                Assert.AreEqual($"{enemy.DisplayName}\n", outputs[index++].Message);
            }

            Assert.AreEqual(index, clearIndices[clearIndexIndex++]);

            TestMenuOutput(outputs, index, clearIndices, clearIndexIndex);
        }

        [Test]
        public void GetInput_CorrectlyRespondsToBackPrompt_WhenAllowBackIsFalse()
        {
            _menuInput.Push("back");
            _menuInput.Push("item");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show error message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0, true);

            Assert.AreEqual(ErrorMessage, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[_fullMenuPromptLength].Type);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 1);

            Assert.AreEqual("item", ret.Description);
        }

        [Test]
        public void GetInput_CorrectlyRespondsToHelpPrompt_HelpIsInvalid()
        {
            SetUpAndBuildMenu(false, false);
            _menuInput.Push("help");
            _menuInput.Push("fight");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show error message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0, true);

            Assert.AreEqual(ErrorMessage, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[_fullMenuPromptLength].Type);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 1);

            Assert.AreEqual("fight", ret.Description);
        }

        [Test]
        public void GetInput_CorrectlyRespondsToStatusPrompt_WhenAllowStatusIsFalse()
        {
            SetUpAndBuildMenu(false, false, false);

            _menuInput.Push("status");
            _menuInput.Push("run");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();
            var clearIndices = _menuOutput.GetClearIndices();

            var expectedLength = (_fullMenuPromptLength * 2) + 1; //display menu twice, show error message
            Assert.AreEqual(expectedLength, outputs.Length);

            TestMenuOutput(outputs, 0, clearIndices, 0, true);

            Assert.AreEqual(ErrorMessage, outputs[_fullMenuPromptLength].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[_fullMenuPromptLength].Type);

            TestMenuOutput(outputs, _fullMenuPromptLength + 1, clearIndices, 1);

            Assert.AreEqual("run", ret.Description);
        }

        [Test]
        public void GetInput_ReturnsTypedMenuActionIfTypedMenuSelectionSelected([Range(0,2)] int selectedActionIndex)
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new TypedMenuAction<int>(10, "foo"),
                new TypedMenuAction<int>(20, "bar"),
                new TypedMenuAction<int>(30, "baz"),
            };

            Menu menu = new Menu(false, false, false, "what do you want?", null, null, menuActions, _menuInput, _menuOutput);

            int indexPlusOne = 1 + selectedActionIndex;
            _menuInput.Push($"{indexPlusOne}");

            menu.Build(null, null, null, null);
            TypedMenuSelection<int>  ret = menu.GetInput() as TypedMenuSelection<int>;

            Assert.NotNull(ret);
            Assert.AreEqual(indexPlusOne * 10, ret.Item);
        }

        [Test]
        public void ShuffleFlag_ProperlyShufflesMenuOptions([Values(new [] {2, 1, 0}, new [] {0, 2, 1})] int[] newOrder)
        {
            string[] expectedMenuActions = { ". fight\n", ". item\n", ". run\n" };

            MockChanceService chanceService = new MockChanceService();
            chanceService.SetShuffleIndices(newOrder);

            SetUpAndBuildMenu(false, false, false, true, chanceService);

            _menuInput.Push("1");
            _menu.GetInput();

            MockOutputMessage[] outputs = _menuOutput.GetOutputs();

            Assert.AreEqual(4, outputs.Length);

            for (var i = 1; i < 4; ++i)
            {
                int expectedIndex = newOrder[i - 1];
                string expectedMenuAction = expectedMenuActions[expectedIndex];

                MockOutputMessage output = outputs[i];
                Assert.AreEqual($"{i}{expectedMenuAction}", output.Message);
            }
        }

        [Test]
        public void ShuffleFlag_DoesNotShuffleBackStatusHelpOptions()
        {
            MockChanceService chanceService = new MockChanceService();
            chanceService.SetShuffleIndices(new [] {2, 1, 0});

            SetUpAndBuildMenu(true, true, true, true, chanceService);

            _menuInput.Push("1");
            _menu.GetInput();

            MockOutputMessage[] outputs = _menuOutput.GetOutputs();


            Assert.AreEqual(_fullMenuPromptLength, outputs.Length);

            for (int i = 4, j = 3; i < 7; ++i, --j)
            {
                string expectedOutput = _fullMenuPrompt[_fullMenuPromptLength - j];

                MockOutputMessage output = outputs[i];
                Assert.AreEqual(expectedOutput, output.Message);
            }
        }

        [Test]
        public void ShuffleFlag_MenuOptionsCorrectlyAccessibleByIndex()
        {
            //Arrange
            MockChanceService chanceService = new MockChanceService();
            chanceService.SetShuffleIndices(new[] { 2, 1, 0 });

            SetUpAndBuildMenu(true, true, true, true, chanceService);

            _menuInput.Push("1");

            //Act
            MenuSelection selectedOption = _menu.GetInput();

            //Assert
            MenuAction expectedAction = _menuActions[2];

            Assert.AreEqual(expectedAction.DisplayText, selectedOption.Description);
        }


        [Test]
        public void HiddenMenuActions_NotDisplayed()
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("foo", isHidden: true),
                new MenuAction("bar", isHidden: true),
                new MenuAction("fight"),
                new MenuAction("defend")
            };

            const string prompt = "pick one";

            Menu menu = new Menu(false, false, false, prompt, Globals.GenericErrorMessage, null, menuActions, _menuInput, _menuOutput);

            menu.Build(null, null, null, null);

            _menuInput.Push("1");
            menu.GetInput();

            MockOutputMessage[] outputs = _menuOutput.GetOutputs();

            Assert.AreEqual(3, outputs.Length);
            Assert.AreEqual(prompt + "\n", outputs[0].Message);
            Assert.AreEqual("1. fight" + "\n", outputs[1].Message);
            Assert.AreEqual("2. defend" + "\n", outputs[2].Message);
        }

        [Test]
        public void HiddenMenuActions_NotAccessibleByNumberInput()
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("fight"),
                new MenuAction("defend"),
                new MenuAction("foo", isHidden: true),
                new MenuAction("bar", isHidden: true)
            };

            const string prompt = "pick one";

            Menu menu = new Menu(false, false, false, prompt, Globals.GenericErrorMessage, null, menuActions, _menuInput, _menuOutput);

            menu.Build(null, null, null, null);

            _menuInput.Push("3", "1");
            MenuSelection selection = menu.GetInput();

            Assert.AreEqual("fight", selection.Description);

            MockOutputMessage[] outputs = _menuOutput.GetOutputs();

            int menuOutputLength = 1 + menuActions.Count(ma => !ma.IsHidden);
            int expectedOutputLengh = menuOutputLength*2 + 1; //menu displayed twice, error message displayed once.
            Assert.AreEqual(expectedOutputLengh, outputs.Length);

            MockOutputMessage output = outputs[menuOutputLength];
            Assert.AreEqual(Globals.GenericErrorMessage + "\n", output.Message);
            Assert.AreEqual(MockOutputMessageType.Error, output.Type);
        }

        [Test]
        public void NumberInput_SkipsHiddenInput()
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("foo", isHidden: true),
                new MenuAction("bar", isHidden: true),
                new MenuAction("fight"),
                new MenuAction("defend")
            };

            const string prompt = "pick one";

            Menu menu = new Menu(false, false, false, prompt, Globals.GenericErrorMessage, null, menuActions, _menuInput, _menuOutput);

            menu.Build(null, null, null, null);

            _menuInput.Push("1");
            MenuSelection selection = menu.GetInput();

            Assert.AreEqual("fight", selection.Description);
        }

        [Test]
        public void HiddenMenuActions_StillAccessibleByCommandText([Values("foo", "bar")] string input)
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("fight"),
                new MenuAction("defend"),
                new MenuAction("foo", isHidden: true),
                new MenuAction("bar", isHidden: true)
            };

            Menu menu = new Menu(false, false, false, "pick one", Globals.GenericErrorMessage, null, menuActions, _menuInput, _menuOutput);

            menu.Build(null, null, null, null);

            _menuInput.Push(input);
            MenuSelection selection = menu.GetInput();

            Assert.AreEqual(input, selection.Description);
        }

        [Test]
        public void CorrectlyPassesBattleMove_SubMenuRequiresBattleMove()
        {
            MockMenu mockSubMenu = new MockMenu(requiresBattleMoveInput: true, input: _menuInput, output: _menuOutput);
            mockSubMenu.SetNextSelection(new MenuSelection[] {null});
            MenuSelection expectedMenuSelection = new MenuSelection("foo", new DoNothingMove(), null);
            mockSubMenu.SetNextBattleMoveRequiredSelection(expectedMenuSelection);

            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("fight", move: new DoNothingMove(), subMenu: mockSubMenu)
            };

            Menu menu = new Menu(false, false, false, "pick one", Globals.GenericErrorMessage, null, menuActions, _menuInput, _menuOutput);

            BuildMenu(menu);

            _menuInput.Push("1");

            MenuSelection returnedSelection = menu.GetInput();

            Assert.NotNull(returnedSelection);
            Assert.AreEqual(expectedMenuSelection, returnedSelection);
        }
    }
}
