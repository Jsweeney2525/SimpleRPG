using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    public class EggTest
    {
        private MockChanceService _chanceService;

        private Egg _egg1;
        private Egg _egg2;
        private Egg _egg3;

        private Team _enemyTeam;

        [SetUp]
        public void Setup()
        {
            _chanceService = new MockChanceService();

            _egg1 = new Egg(MagicType.Fire);
            _egg2 = new Egg(MagicType.Fire);
            _egg3 = new Egg(MagicType.Fire);

            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _egg1, _egg2, _egg3);
        }

        [Test]
        public void InitializesWithONlyDoNothingMove()
        {
            List<BattleMove> availableMoves = _egg1.AvailableMoves;

            Assert.AreEqual(1, availableMoves.Count);

            Assert.IsTrue(availableMoves[0] is DoNothingMove);
        }

        [Test]
        public void Egg_RemovesSelfFromTeamOnDeath()
        {
            Assert.AreEqual(3, _enemyTeam.Fighters.Count);
            Assert.IsTrue(_enemyTeam.Contains(_egg1));

            _egg1.PhysicalDamage(_egg1.MaxHealth);

            Assert.AreEqual(2, _enemyTeam.Fighters.Count);
            Assert.IsFalse(_enemyTeam.Contains(_egg1));
        }

        [Test]
        public void FighterFactory_CorrectlyRandomizesEggType_IfNoneSpecified()
        {
            TestFighterFactory.SetChanceService(_chanceService);
            _chanceService.PushWhichEventOccurs(0);

            Egg egg = (Egg)FighterFactory.GetFighter(FighterType.Egg, 1);

            Assert.AreEqual(Globals.EggMagicTypes[0], egg.MagicType);
        }
    }
}
