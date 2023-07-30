using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class ZombieTests
    {
        private Zombie _zombie;
        private Team _zombieTeam;
        private Team _humanTeam;

        private MockChanceService _mockChanceService;

        [SetUp]
        public void Setup()
        {
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);

            _zombie = (Zombie)TestFighterFactory.GetFighter(TestFighterType.Zombie, 1);
            _zombieTeam = new Team(TestMenuManager.GetTestMenuManager(), _zombie);

            _humanTeam = new Team(new TestMenuManager(new MockInput(), new MockOutput()), (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1));
        }

        [TearDown]
        public void TearDown()
        {
            _zombie = null;
            _zombieTeam = null;
            _humanTeam = null;
            _mockChanceService = null;
        }

        [Test]
        public void ZombieRevivesAfterDying()
        {
            //Will Revive after 1 round, excluding the round it was killed
            _mockChanceService.PushWhichEventOccurs(0);

            _zombie.PhysicalDamage(_zombie.MaxHealth);

            //zombies will not revive the same turn they had died
            //TODO: change this to OnRoundEnd
            _zombie.OnTurnEnded(new TurnEndedEventArgs(_zombie));
            Assert.IsFalse(_zombie.IsAlive());

            _zombie.OnTurnEnded(new TurnEndedEventArgs(_zombie));
            Assert.IsTrue(_zombie.IsAlive());
        }

        [Test]
        public void ZombieVulnerableToLightMagic()
        {

        }
    }
}
