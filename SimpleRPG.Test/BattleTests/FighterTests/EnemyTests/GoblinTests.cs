using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class GoblinTests
    {
        private Goblin _goblin;
        private Team _goblinTeam;
        private Team _humanTeam;

        private MockChanceService _mockChanceService;

        [SetUp]
        public void Setup()
        {
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);

            _goblin = (Goblin)TestFighterFactory.GetFighter(TestFighterType.Goblin, 1);
            _goblinTeam = new Team(TestMenuManager.GetTestMenuManager(), _goblin);

            _humanTeam = new Team(new TestMenuManager(new MockInput(), new MockOutput()), (HumanFighter)TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, 1));
        }

        [Test]
        public void AvailableMoves_AppropriatelySetUp_Level1()
        {
            var moves = _goblin.AvailableMoves;

            Assert.AreEqual(2, moves.Count);

            var goblinPunch = moves.FirstOrDefault(m => m.Description == "goblin punch");
            Assert.NotNull(goblinPunch);
            Assert.IsInstanceOf<MultiTurnBattleMove>(goblinPunch);

            var multiTurnGoblinPunch = goblinPunch as MultiTurnBattleMove;
            if (multiTurnGoblinPunch != null)
            {
                Assert.AreEqual(2, multiTurnGoblinPunch.Moves.Count);
            }

            Assert.NotNull(moves.FirstOrDefault(m => m.Description == "attack"));
        }

        [Test]
        public void SelectMove_AppropriatelySelectsGoblinPunch()
        {
            _mockChanceService.PushEventOccurs(true);
            var goblinPunch = _goblin.AvailableMoves.Single(am => am.Description == "goblin punch") as MultiTurnBattleMove;

            if (goblinPunch == null)
            {
                throw new AssertionException("goblin punch should not be null");
            }

            BattleMoveWithTarget selectedMove;

            for (var i = 0; i < goblinPunch.Moves.Count; ++i)
            {
                selectedMove = _goblin.SetupMove(_goblinTeam, _humanTeam);

                var expectedMove = (i == 0) ? goblinPunch : goblinPunch.Moves[i];

                Assert.AreEqual(expectedMove, selectedMove.Move);
            }

            _mockChanceService.PushEventOccurs(false);

            selectedMove = _goblin.SetupMove(_goblinTeam, _humanTeam);

            Assert.AreEqual("attack", selectedMove.Move.Description);
            Assert.AreEqual(0.75, _mockChanceService.LastChanceVals[1]);
        }
    }
}
