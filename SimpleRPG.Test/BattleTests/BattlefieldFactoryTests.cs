using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests
{
    [TestFixture]
    public class BattlefieldFactoryTests
    {
        private MockChanceService _chanceService;
        private MockMenuFactory _menuFactory;
        private TestTeamFactory _teamFactory;
        private GroupingFactory _groupingFactory;

        private BattlefieldFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _menuFactory = new MockMenuFactory();
            _teamFactory = new TestTeamFactory(_chanceService, _menuFactory);
            _groupingFactory = new GroupingFactory(_chanceService, _teamFactory, new FighterFactory());
            _factory = new BattlefieldFactory(_teamFactory, _groupingFactory, _menuFactory, _chanceService);
        }

        [Test]
        public void CorrectlyGeneratesEmptyListOfTerrainInteractables_FromNullInput()
        {
            TeamConfiguration teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.Goblin, 1), new EnemyConfiguration(FighterType.Fairy, 1));
            BattlefieldConfiguration battlefieldConfig = new BattlefieldConfiguration(teamInfo);

            BattleFieldInfo returnedBattleFieldInfo = _factory.GetBattleFieldSetUp(battlefieldConfig);

            Assert.NotNull(returnedBattleFieldInfo.TerrainInteractables);
            Assert.AreEqual(0, returnedBattleFieldInfo.TerrainInteractables.ToList().Count);
        }

        [Test]
        public void CorrectlyGeneratesBattlefield_TeamConfiguration_NoTerrainConfiguration()
        {
            TeamConfiguration teamInfo = new TeamConfiguration(new EnemyConfiguration(FighterType.Goblin, 1), new EnemyConfiguration(FighterType.Fairy, 1));
            BattlefieldConfiguration battlefieldConfig = new BattlefieldConfiguration(teamInfo);

            BattleFieldInfo returnedBattleFieldInfo = _factory.GetBattleFieldSetUp(battlefieldConfig);

            List<IFighter> fighters = returnedBattleFieldInfo.EnemyTeam.Fighters;

            Assert.AreEqual(2, fighters.Count);

            IFighter firstFighter = fighters[0];
            Assert.True(firstFighter is Goblin, $"first fighter should be a Goblin but was {firstFighter.GetType()}");
            Assert.True(firstFighter.Level == 1, "first fighter should be level 1");

            IFighter secondFighter = fighters[1];
            Assert.True(secondFighter is Fairy, $"second fighter should be a fairy but was {secondFighter.GetType()}");
            Assert.True(secondFighter.Level == 1, "second fighter should be level 1");
        }

        [Test]
        public void CorrectlyGeneratesBattlefield_ShadeGroupingAndBellTerrainConfiguration()
        {
            const int fighterLevel = 1;
            FighterGroupingConfiguration groupingConfig = new ShadeGroupingConfiguration(3, fighterLevel);
            BellType[] bellTypes = {BellType.Copper, BellType.Silver};
            TerrainInteractablesConfiguration terrainConfiguration = new BellTerrainConfiguration(bellTypes);

            BattlefieldConfiguration config = new BattlefieldConfiguration(groupingConfig, terrainConfiguration);

            BattleFieldInfo returnedBattleFieldInfo = _factory.GetBattleFieldSetUp(config);

            List<IFighter> fighters = returnedBattleFieldInfo.EnemyTeam.Fighters;

            Assert.AreEqual(3, fighters.Count);
            Assert.True(fighters.TrueForAll(f => f is Shade), "the returned fighters should be Shades!");
            Assert.True(fighters.TrueForAll(f => f.Level == fighterLevel), $"the returned fighters should all be level {fighterLevel}!");

            List<TerrainInteractable> terrainInteractables = returnedBattleFieldInfo.TerrainInteractables.ToList();

            Assert.AreEqual(2, terrainInteractables.Count);
            Assert.True(terrainInteractables.TrueForAll(b => b is Bell));

            List<Bell> bells = terrainInteractables.OfType<Bell>().ToList();

            for(var i = 0; i < bellTypes.Length; ++i)
            {
                Assert.AreEqual(bellTypes[i], bells[i].BellType);
            }
        }
    }
}
