using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Test.MockClasses;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class TeamFactoryTests
    {
        private Team _enemyTeam;
        private MockChanceService _chanceService;

        private TeamFactory _teamFactory;

        [SetUp]
        public void Setup()
        {
            _chanceService = new MockChanceService();
            _teamFactory = new TeamFactory(_chanceService, Globals.MenuFactory);
        }

        [Test]
        public void GetTeam_AssignsAppropriateNumberOfEnemiesToTeam([Values(1, 2, 3, 4)] int numEnemies)
        {
            //push logic for determining how many fighters will be in the enemy team, driven by SubRegion configuration
            _chanceService.PushWhichEventOccurs(0);

            Type[] types = {typeof (Goblin), typeof (Fairy), typeof (Golem), typeof (Ogre)};
            for (var i = 0; i < numEnemies; ++i)
            {
                _chanceService.PushWhichEventOccurs(i);
            }

            SubRegion subRegion = new SubRegion(WorldSubRegion.Fields, 1, new []{ new ChanceEvent<int>(numEnemies, 1) }, 
                new[] {FighterType.Goblin, FighterType.Fairy, FighterType.Golem, FighterType.Ogre}, null);
            _enemyTeam = _teamFactory.GetTeam(subRegion);

            Assert.AreEqual(numEnemies, _enemyTeam.Fighters.Count);

            for (var i = 0; i < numEnemies; ++i)
            {
                var enemy = _enemyTeam.Fighters[i];
                Assert.AreEqual(types[i], enemy.GetType());
            }
        }

        [Test, Pairwise]
        public void GetTeamByConfiguration_ReturnsAppropriateTeam([Values(FighterType.Fairy, FighterType.Goblin, FighterType.Warrior)] FighterType firstFighterType,
            [Range(1, 3)] int numberFirstFighters,
            [Values(1, 2)] int enemyLevel,
            [Values(FighterType.Ogre, FighterType.Golem)] FighterType secondFighterType)
        {
            List<EnemyConfiguration> enemyConfigurations = new List<EnemyConfiguration>();

            int i;
            for (i = 0; i < numberFirstFighters; ++i)
            {
                enemyConfigurations.Add(new EnemyConfiguration(firstFighterType, enemyLevel));
            }
            enemyConfigurations.Add(new EnemyConfiguration(secondFighterType, enemyLevel));

            TeamConfiguration configuration = new TeamConfiguration(enemyConfigurations.ToArray());

            Team returnedTeam = _teamFactory.GetTeam(configuration);

            Assert.AreEqual(numberFirstFighters + 1, returnedTeam.Fighters.Count);

            Assert.IsTrue(returnedTeam.Fighters.TrueForAll(f => f.Level == enemyLevel));
            
            for (i = 0; i < numberFirstFighters; ++i)
            {
                Assert.IsTrue(firstFighterType.IsCorrectType(returnedTeam.Fighters[i]));
            }
            Assert.IsTrue(secondFighterType.IsCorrectType(returnedTeam.Fighters[i]));
        }
    }
}
