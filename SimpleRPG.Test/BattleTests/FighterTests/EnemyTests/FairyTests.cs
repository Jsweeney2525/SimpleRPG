using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class FairyTests
    {
        //TODO: make it more likely the fairy will recover mana if it's mana is low. Guaranteed if no spells available
        private Fairy _fairy;
        private Team _fairyTeam;
        private Team _humanTeam;

        private MockChanceService _mockChanceService;

        [SetUp]
        public void Setup()
        {
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);

            _fairy = (Fairy)TestFighterFactory.GetFighter(TestFighterType.Fairy, 1);
            _fairyTeam = new Team(TestMenuManager.GetTestMenuManager(), _fairy);

            _humanTeam = new Team(new TestMenuManager(new MockInput(), new MockOutput()), (HumanFighter)TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, 1));
        }

        [Test]
        public void AvailableMoves_AppropriatelySetUp_Level1()
        {
            var moves = _fairy.AvailableMoves;

            Assert.AreEqual(3, moves.Count);

            Assert.NotNull(moves.FirstOrDefault(m => m.Description == "fairy wind"));
            Assert.NotNull(moves.FirstOrDefault(m => m.Description == "fairy wish"));
            Assert.NotNull(moves.FirstOrDefault(m => m.MoveType == BattleMoveType.Attack));
        }

        [Test]
        public void SelectMove_AppropriatelySelectsFairyWindWhenManaFull()
        {
            var selectedMove = _fairy.SetupMove(_fairyTeam, _humanTeam);

            Assert.AreEqual("fairy wind", selectedMove.Move.Description);
        }

        [Test]
        public void SelectMove_AppropriatelySelectsFairyWishWhenManaEmpty()
        {
            _fairy.DrainMana(_fairy.MaxMana);
            var selectedMove = _fairy.SetupMove(_fairyTeam, _humanTeam);

            Assert.AreEqual("fairy wish", selectedMove.Move.Description);
        }
    }
}
