using System;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class KikiAndRikiTests
    {
        private DancerBoss _kiki;
        private DancerBoss _riki;
        private TestHumanFighter _hero;
        private TestHumanFighter _sidekick;

        private Team _enemyTeam;
        private Team _humanTeam;

        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        [SetUp]
        public void Setup()
        {
            _output = new MockOutput();
            _input = new MockInput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();

            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            TestFighterFactory.SetChanceService(_chanceService);

            _kiki = (DancerBoss)FighterFactory.GetFighter(FighterType.DancerBoss, 1, fighterClass: FighterClass.BossDancerKiki);
            _riki = (DancerBoss)FighterFactory.GetFighter(FighterType.DancerBoss, 1, fighterClass: FighterClass.BossDancerRiki);
            _enemyTeam = new Team(_menuManager, _kiki, _riki);

            _hero = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Hero");
            _sidekick = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "SideKick");
            _humanTeam = new Team(new TestMenuManager(new MockInput(), new MockOutput()), _hero, _sidekick);
        }

        [Test]
        public void KikiCorrectlyInitializes_WithElementalDances()
        {
            var specials = _kiki.SpecialMoves;
            Tuple<string, DanceEffectType>[] expectedVals =
            {
                new Tuple<string, DanceEffectType>("fire dance", DanceEffectType.Fire),
                new Tuple<string, DanceEffectType>("water dance", DanceEffectType.Water),
                new Tuple<string, DanceEffectType>("earth dance", DanceEffectType.Earth),
                new Tuple<string, DanceEffectType>("wind dance", DanceEffectType.Wind)
            };

            Assert.AreEqual(4, specials.Count);

            foreach (var danceMove in specials.Select(special => special as DanceMove))
            {
                Assert.That(danceMove, Is.Not.Null);
            }

            foreach (var expected in expectedVals)
            {
                var dances = specials.Cast<DanceMove>();
                var find = dances.FirstOrDefault(d => d.DanceEffect == expected.Item2 && d.Description == expected.Item1);
                Assert.That(find, Is.Not.Null, $"Kiki should have a move called '{expected.Item1}' with DanceEffect '{expected.Item2}', but it was not found!");
            }
        }

        [Test]
        public void RikiCorrectlyInitializes_WithEssenceDances()
        {
            var specials = _riki.SpecialMoves;
            Tuple<string, DanceEffectType>[] expectedVals =
            {
                new Tuple<string, DanceEffectType>("heart dance", DanceEffectType.Heart),
                new Tuple<string, DanceEffectType>("soul dance", DanceEffectType.Soul),
                new Tuple<string, DanceEffectType>("mind dance", DanceEffectType.Mind),
                new Tuple<string, DanceEffectType>("danger dance", DanceEffectType.Danger)
            };

            Assert.AreEqual(4, specials.Count);

            foreach (var danceMove in specials.Select(special => special as DanceMove))
            {
                Assert.That(danceMove, Is.Not.Null);
            }

            foreach (var expected in expectedVals)
            {
                var dances = specials.Cast<DanceMove>();
                var find = dances.FirstOrDefault(d => d.DanceEffect == expected.Item2 && d.Description == expected.Item1);
                Assert.That(find, Is.Not.Null, $"riki should have a move called '{expected.Item1}' with DanceEffect '{expected.Item2}', but it was not found!");
            }
        }
    }
}
