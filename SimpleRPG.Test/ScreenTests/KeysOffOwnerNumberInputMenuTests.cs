using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuSelections;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    public class KeysOffOwnerNumberInputMenuTests
    {
        private MockInput _input;
        private MockOutput _output;
        private NumberInputMenu _menu;

        private TestHumanFighter _owner;
        private TestTeam _ownerTeam;
        private TestTeam _enemyTeam;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();

            _menu = new KeysOffOwnerNumberInputMenu("foo", _input, _output);

            _owner = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _ownerTeam = new TestTeam(_owner);
            _enemyTeam = new TestTeam((TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1));
        }

        [Test]
        public void CorrectlyKeysOffOwnersHealth([Values(10, 50)] int maxHealth)
        {
            _owner.SetHealth(maxHealth);

            _menu.Build(_owner, _ownerTeam, _enemyTeam, null);

            _input.Push($"{maxHealth}", $"{maxHealth - 1}");

            NumberInputMenuSelection selection = _menu.GetInput() as NumberInputMenuSelection;

            Assert.AreEqual(maxHealth - 1, selection?.Number);
        }

        [Test]
        public void CorrectlyKeysOffCurrentHealth_NotMaxHealth()
        {
            _owner.SetHealth(100, 10);

            _menu.Build(_owner, _ownerTeam, _enemyTeam, null);

            _input.Push("99", "50", "10", "9");

            NumberInputMenuSelection selection = _menu.GetInput() as NumberInputMenuSelection;

            Assert.AreEqual(9, selection?.Number);
        }

        [Test]
        public void GetInput_ReturnsCorrectMenuSelection_SubMenuRequiresBattleMoveInput()
        {
            //arrange
            MockMenu mockTargetMenu = new MockMenu(requiresBattleMoveInput: true, input: _input, output: _output);
            mockTargetMenu.SetEchoMode();
            KeysOffOwnerNumberInputMenu menu = new KeysOffOwnerNumberInputMenu("foo", _input, _output, mockTargetMenu);

            int expectedReturnedNumber = 1;
            _owner.SetHealth(expectedReturnedNumber+1);
            menu.Build(_owner, _ownerTeam, _enemyTeam, null);

            _input.Push(expectedReturnedNumber.ToString());

            BattleMove eatPotatoMove = new DoNothingMove("eats a potato");

            //act
            NumberInputMenuSelection returnedSelection = menu.GetInput(eatPotatoMove, null) as NumberInputMenuSelection;

            //assert
            Assert.NotNull(returnedSelection);
            Assert.AreEqual(expectedReturnedNumber, returnedSelection.Number);
            Assert.AreEqual(eatPotatoMove, returnedSelection.Move);
        }
    }
}
