using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class ChooseAttackTypeMenuTests : ScreenTestBase
    {
        Menu _menu;
        MockInput _menuInput;
        MockOutput _menuOutput;
        private static TestMenuManager _menuManager;

        TestHumanFighter _player;
        Team _playerTeam;
        Team _enemyTeam;
        List<string> _fullMenuPrompt;
        List<string> _fullSpellMenuPrompt;
        List<string> _fullTargetMenuPrompt;

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();
            _menuManager = new TestMenuManager(_menuInput, _menuOutput);

            _player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            Spell fireball = SpellFactory.GetSpell(MagicType.Fire, 1);
            Spell earthSpell = SpellFactory.GetSpell(MagicType.Earth, 1);
            _player.AddSpell(fireball);
            _player.AddSpell(earthSpell);
            _player.SetMana(fireball.Cost);

            _playerTeam = new Team(_menuManager, _player);
            _enemyTeam = new Team(_menuManager, (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1));

            _menu = (Menu)Globals.MenuFactory.GetMenu(MenuType.ChooseAttackTypeMenu, _menuInput, _menuOutput);
            _menu.Build(_player, _playerTeam, _enemyTeam, null);

            _fullSpellMenuPrompt = new List<string>
            {
                $"Which spell would you like to cast?\n{_player.DisplayName} currently has {_player.CurrentMana} / {_player.MaxMana} Mana\n",
            };

            var spellMenu = (SpellSelectionMenu)Globals.MenuFactory.GetMenu(MenuType.ChooseSpellMenu);
            spellMenu.Build(_player, _playerTeam, _enemyTeam, null);

            var spellMenuActions = spellMenu.MenuActions;
            for (var i = 0; i < spellMenuActions.Count; ++i)
            {
                _fullSpellMenuPrompt.Add((i + 1) + ". " + spellMenuActions[i].DisplayText + "\n");
            }

            _fullSpellMenuPrompt.Add(StatusPrompt);
            _fullSpellMenuPrompt.Add(HelpPrompt);

            _fullMenuPrompt = new List<string>
            {
                "How would you like to fight?\n",
                "1. attack\n",
                "2. magic\n",
                StatusPrompt,
                HelpPrompt
            };

            _fullTargetMenuPrompt = new List<string>
            {
                "Who will you target for this action?\n",
                "1. " + _enemyTeam.Fighters[0].DisplayName + "\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
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
            List<IFighter> list = new List<IFighter>
            {
                FighterFactory.GetFighter(FighterType.Goblin, 1),
                FighterFactory.GetFighter(FighterType.Goblin, 1)
            };

            return new Team(_menuManager, list);
        }

        private void TestChooseAttackTypeMenuOutput(MockOutputMessage[] outputs, int startingIndex, bool allowBack = false)
        {
            Assert.AreEqual(_fullMenuPrompt[0], outputs[startingIndex].Message); //prompt
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);

            Assert.AreEqual(_fullMenuPrompt[1], outputs[startingIndex].Message); //attack
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);

            Assert.AreEqual(_fullMenuPrompt[2], outputs[startingIndex].Message); //magic
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);

            Assert.AreEqual(_fullMenuPrompt[3], outputs[startingIndex].Message); //status
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);

            if (allowBack)
            {
                Assert.AreEqual(BackPrompt, outputs[startingIndex].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);
            }

            Assert.AreEqual(_fullMenuPrompt[4], outputs[startingIndex].Message);
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex].Type);
        }

        private void TestChooseSpellMenuOutput(MockOutputMessage[] outputs, int startingIndex, bool allowBack = false)
        {
            var length = _fullSpellMenuPrompt.Count - 1;
            for (var i = 0; i < length; ++i)
            {
                Assert.AreEqual(_fullSpellMenuPrompt[i], outputs[startingIndex].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);
            }

            if (allowBack)
            {
                Assert.AreEqual("('back' to return to the previous screen)\n", outputs[startingIndex].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex++].Type);
            }

            Assert.AreEqual(_fullSpellMenuPrompt[length], outputs[startingIndex].Message);
            Assert.AreEqual(MockOutputMessageType.Normal, outputs[startingIndex].Type);
        }

        [Test]
        public void HappyPath_Attack_SingleOpponent()
        {
            _menuInput.Push("attack");
            _menuInput.Push("1");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            TestChooseAttackTypeMenuOutput(outputs, 0, true);

            Assert.AreEqual("attack", ret.Move.Description);
            Assert.AreEqual(BattleMoveType.Attack, ret.Move.MoveType);
        }

        [Test]
        public void HappyPath_Attack_MultipleOpponents()
        {
            var enemyTeam = CreateEnemyTeam();
            _menu = (Menu)Globals.MenuFactory.GetMenu(MenuType.ChooseAttackTypeMenu, _menuInput, _menuOutput);
            _menu.Build(_player, _playerTeam, enemyTeam, null);
            
            _menuInput.Push("attack");
            _menuInput.Push("1");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            TestChooseAttackTypeMenuOutput(outputs, 0, true);

            Assert.AreEqual(enemyTeam.Fighters[0], ret.Target,
                $"Expected '{enemyTeam.Fighters[0].DisplayName}' to be selected but '{ret.Target.DisplayName}' was returned");
        }

        [Test]
        public void HappyPath_MagicSpell_SingleOpponent([Values("fireball", "clay spike")] string spell)
        {
            _menuInput.Push("magic");
            _menuInput.Push(spell);
            _menuInput.Push("1");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            TestChooseAttackTypeMenuOutput(outputs, 0, true);

            TestChooseSpellMenuOutput(outputs, _fullMenuPrompt.Count + 1, true);

            Assert.AreEqual(spell, ret.Move.Description);
        }

        [Test]
        public void HappyPath_MagicSpell_MultipleOpponents([Values("fireball", "clay spike")] string spell)
        {
            var enemyTeam = CreateEnemyTeam();
            _menu = (Menu)Globals.MenuFactory.GetMenu(MenuType.ChooseAttackTypeMenu, _menuInput, _menuOutput);
            _menu.Build(_player, _playerTeam, enemyTeam, null);

            _menuInput.Push("magic");
            _menuInput.Push(spell);
            _menuInput.Push("2");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            TestChooseAttackTypeMenuOutput(outputs, 0, true);

            Assert.AreEqual(enemyTeam.Fighters[1], ret.Target);
        }

        [Test]
        public void MenuCorrectlyHandlesBackCommand()
        {
            _menuInput.Push("back");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            TestChooseAttackTypeMenuOutput(outputs, 0, true);

            Assert.AreEqual("back", ret.Description);
            Assert.IsNull(ret.Move);
        }

        [Test]
        public void MenuCorrectlyHandlesBackCommand_FromSubMenu()
        {
            _menuInput.Push("magic");
            _menuInput.Push("back");
            _menuInput.Push("attack");
            _menuInput.Push("1");

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            var startingIndex = 0;
            TestChooseAttackTypeMenuOutput(outputs, startingIndex, true);

            startingIndex += _fullMenuPrompt.Count + 1;
            TestChooseSpellMenuOutput(outputs, startingIndex, true);

            startingIndex += _fullSpellMenuPrompt.Count + 1;
            TestChooseAttackTypeMenuOutput(outputs, startingIndex, true);

            Assert.AreEqual("attack", ret.Move.Description);
            Assert.AreEqual(BattleMoveType.Attack, ret.Move.MoveType);
        }
        
        [Test]
        public void CorrectlyOmitsSpellSelectionWhenPlayerHasNoSpells()
        {
            var player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var playerTeam = new Team(_menuManager, player);

            Assert.AreEqual(0, player.Spells.Count, "Menu factory incorrectly assigned spells to player fighter.");

            var fullMenuPrompt = new List<string>
            {
                "How would you like to fight?\n",
                "1. attack\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var fullMenuPromptLength = fullMenuPrompt.Count;

            _menu.Build(player, playerTeam, _enemyTeam, null);

            //should generate error
            _menuInput.Push("magic");
            _menuInput.Push("attack");
            _menuInput.Push("1");

            var ret = _menu.GetInput();

            Assert.AreEqual(BattleMoveType.Attack, ret.Move.MoveType);

            var outputs = _menuOutput.GetOutputs();

            //1 for error message, 5 for target menu
            var expectedLength = (fullMenuPromptLength * 2) + 1 + _fullTargetMenuPrompt.Count;

            Assert.AreEqual(expectedLength, outputs.Length);

            var i = 0;

            for (; i < fullMenuPromptLength; ++i)
            {
                Assert.AreEqual(fullMenuPrompt[i], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i].Type);
            }

            Assert.AreEqual(ErrorMessage, outputs[i].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[i++].Type);

            for (var j = 0; j < fullMenuPromptLength; ++j, ++i)
            {
                Assert.AreEqual(fullMenuPrompt[j], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i].Type);
            }

            for(var j = 0; j < fullMenuPromptLength; ++j, ++i)
            {
                Assert.AreEqual(_fullTargetMenuPrompt[j], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i].Type);
            }
        }

        [Test]
        public void CorrectlyOmitsSpecialMovesSelectionWhenPlayerHasNoSpecialMoves()
        {
            var player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var playerTeam = new Team(_menuManager, player);

            Assert.AreEqual(0, player.SpecialMoves.Count, "Menu factory incorrectly assigned special moves to player fighter.");

            var fullMenuPrompt = new List<string>
            {
                "How would you like to fight?\n",
                "1. attack\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var fullMenuPromptLength = fullMenuPrompt.Count;

            _menu.Build(player, playerTeam, _enemyTeam, null);

            //should generate error
            _menuInput.Push(new List<string> { "back" } );

            var ret = _menu.GetInput();

            Assert.AreEqual("back", ret.Description);

            var outputs = _menuOutput.GetOutputs();

            Assert.AreEqual(fullMenuPromptLength, outputs.Length);

            var i = 0;

            for (; i < fullMenuPromptLength; ++i)
            {
                Assert.AreEqual(fullMenuPrompt[i], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i].Type);
            }
        }

        [Test]
        public void CorrectlyDisplaysError_WhenPlayerHasNoSpecialMoves()
        {
            var player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var playerTeam = new Team(_menuManager, player);

            Assert.AreEqual(0, player.SpecialMoves.Count, "Menu factory incorrectly assigned special moves to player fighter.");

            var fullMenuPrompt = new List<string>
            {
                "How would you like to fight?\n",
                "1. attack\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var fullMenuPromptLength = fullMenuPrompt.Count;

            _menu.Build(player, playerTeam, _enemyTeam, null);

            //should generate error
            _menuInput.Push(new List<string> { "special", "back" });

            var ret = _menu.GetInput();

            Assert.AreEqual("back", ret.Description);

            var outputs = _menuOutput.GetOutputs();

            //display menu twice, display error
            var expectedLength = (fullMenuPromptLength*2) + 1;

            Assert.AreEqual(expectedLength, outputs.Length);

            var i = 0;

            while(i < fullMenuPromptLength)
            {
                Assert.AreEqual(fullMenuPrompt[i], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i++].Type);
            }

            Assert.AreEqual(ErrorMessage, outputs[i].Message);
            Assert.AreEqual(MockOutputMessageType.Error, outputs[i++].Type);

            var j = 0;
            while(j < fullMenuPromptLength)
            {
                Assert.AreEqual(fullMenuPrompt[j++], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i++].Type);
            }
        }

        [Test]
        public void CorrectlyDisplaysSpecialAttackSubMenu_WhenUserHasSpecialMoves()
        {
            var player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var playerTeam = new Team(_menuManager, player);

            player.AddSpecialMove((SpecialMove)TestMoveFactory.Get(TargetType.Field, "test", BattleMoveType.Special));

            Assert.AreEqual(1, player.SpecialMoves.Count);

            var fullMenuPrompt = new List<string>
            {
                "How would you like to fight?\n",
                "1. attack\n",
                "2. special move\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var fullMenuPromptLength = fullMenuPrompt.Count;

            _menu.Build(player, playerTeam, _enemyTeam, null);
            
            _menuInput.Push(new List<string> { "back" });

            var ret = _menu.GetInput();

            Assert.AreEqual("back", ret.Description);

            var outputs = _menuOutput.GetOutputs();

            Assert.AreEqual(fullMenuPromptLength, outputs.Length);

            var i = 0;

            while(i < fullMenuPromptLength)
            {
                Assert.AreEqual(fullMenuPrompt[i], outputs[i].Message);
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i++].Type);
            }
        }

        [Test]
        public void CorrectlyReturnsSpecialAttack_WhenUserHasSpecialMoves()
        {
            var player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var playerTeam = new Team(_menuManager, player);

            var specialMove = (SpecialMove) TestMoveFactory.Get(TargetType.Field, "test", BattleMoveType.Special);
            player.AddSpecialMove(specialMove);

            Assert.AreEqual(1, player.SpecialMoves.Count);

            _menu.Build(player, playerTeam, _enemyTeam, null);

            _menuInput.Push(new List<string> { "special", specialMove.Description, "1" });

            var ret = _menu.GetInput();

            Assert.AreEqual(specialMove, ret.Move);
        }
    }
}
