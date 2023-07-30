using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class SpellSelectionMenuTests : ScreenTestBase
    {
        SpellSelectionMenu _menu;
        MockInput _menuInput;
        MockOutput _menuOutput;
        private TestMenuManager _menuManager;
        TestHumanFighter _player;
        Team _playerTeam;
        Team _enemyTeam;

        private List<string> _fullSpellMenuPrompt;

        [SetUp]
        public void SetUp()
        {
            _menuInput = new MockInput();
            _menuOutput = new MockOutput();
            _menuManager = new TestMenuManager(_menuInput, _menuOutput);

            _enemyTeam = new Team(_menuManager, FighterFactory.GetFighter(FighterType.Goblin, 1));
            _player = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _playerTeam = new Team(_menuManager, _player);

            _menu = (SpellSelectionMenu)Globals.MenuFactory.GetMenu(MenuType.ChooseSpellMenu, _menuInput, _menuOutput);
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
        public void HappyPath_SelectValidSpell([Values(MagicType.Fire, MagicType.Earth, MagicType.Water, MagicType.Wind)] MagicType spellType)
        {
            var spell = SpellFactory.GetSpell(spellType, 1);
            var spellName = spell.Description;
            _player.AddSpell(spell);
            _player.SetMana(spell.Cost);

            _menuInput.Push(new List<string> {spellName, "1"});

            _fullSpellMenuPrompt = new List<string>
            {
                $"Which spell would you like to cast?\n{_player.DisplayName} currently has {_player.CurrentMana} / {_player.MaxMana} Mana\n",
                "1. " + spellName + " " + spell.Cost + "\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var count = _fullSpellMenuPrompt.Count;

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(MockOutputMessageType.Normal, outputs[i].Type);
                Assert.AreEqual(_fullSpellMenuPrompt[i], outputs[i].Message);
            }

            Assert.AreEqual(spellName, ret.Move.Description);

            _player.RemoveSpell(spell);
        }

        [Test]
        public void SpellsCannotBeSelectedIfPlayerDoesNotHaveSufficientMana()
        {
            var spell = SpellFactory.GetSpell(MagicType.Fire, 1);
            _player.AddSpell(spell);
            _player.SetMana(spell.Cost - 1);

            _menuInput.Push(new List<string> { "fireball", "back", "y" });

            _fullSpellMenuPrompt = new List<string>
            {
                $"Which spell would you like to cast?\n{_player.DisplayName} currently has {_player.CurrentMana} / {_player.MaxMana} Mana\n",
                "1. " + spell.Description + " " + spell.Cost + "\n",
                StatusPrompt,
                BackPrompt,
                HelpPrompt
            };
            var count = _fullSpellMenuPrompt.Count;

            var ret = _menu.GetInput();

            var outputs = _menuOutput.GetOutputs();

            for (var i = 0; i < count; ++i)
            {
                var output = outputs[i];

                Assert.AreEqual(MockOutputMessageType.Normal, output.Type);
                Assert.AreEqual(_fullSpellMenuPrompt[i], output.Message);

                if (i == 1)
                {
                    Assert.AreEqual(Globals.DisabledColor, output.Color);
                }
            }

            Assert.AreEqual(MockOutputMessageType.Error, outputs[count].Type);
            Assert.AreEqual(ErrorMessage, outputs[count].Message);

            Assert.AreEqual("back", ret.Description);

            _player.RemoveSpell(spell);
        }
    }
}
