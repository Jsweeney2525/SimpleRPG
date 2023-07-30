using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    public class GroupingFactoryTests
    {
        private MockChanceService _chanceService;
        private TestTeamFactory _teamFactory;
        private MockMenuFactory _menuFactory;
        private GroupingFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _menuFactory = new MockMenuFactory();
            _teamFactory = new TestTeamFactory(_chanceService, _menuFactory);
            _factory = new GroupingFactory(_chanceService, _teamFactory, new FighterFactory());
        }

        [TearDown]
        public void TearDown()
        {
            _factory = null;
        }

        [Test, Pairwise]
        public void GetGroupingCorrectlyReturnsShadeGrouping_NoMaxLevel([Range(1, 4)] int numberEnemies, [Range(1,3)] int fighterLevel)
        {
            ShadeGroupingConfiguration config = new ShadeGroupingConfiguration(numberEnemies, fighterLevel);
            ShadeFighterGrouping grouping = _factory.GetGrouping(config) as ShadeFighterGrouping;

            Assert.NotNull(grouping);
            List<Shade> shades = grouping.GetShades();

            Assert.AreEqual(numberEnemies, shades.Count, $"There should be {numberEnemies} shades in the returned grouping");
            Assert.True(shades.TrueForAll(s => s.Level == fighterLevel));
        }

        [Test, Pairwise]
        public void GetGroupingCorrectlyReturnsShadeGrouping_LevelRange([Range(1, 3)] int minLevel, [Range(1,3)] int levelDiff)
        {
            //Arrange
            int maxLevel = minLevel + levelDiff;
            int numberFighters = levelDiff + 1;
            ShadeGroupingConfiguration config = new ShadeGroupingConfiguration(numberFighters, minLevel, maxLevel);

            for (int i = 0; i < numberFighters; ++i)
            {
                _chanceService.PushWhichEventOccurs(i);
            }

            //Act
            ShadeFighterGrouping grouping = _factory.GetGrouping(config) as ShadeFighterGrouping;

            //Assert
            Assert.NotNull(grouping);
            List<Shade> shades = grouping.GetShades();

            for (int i = 0; i < numberFighters; ++i)
            {
                Assert.AreEqual(minLevel + i, shades[i].Level);
            }
        }
    }
}
