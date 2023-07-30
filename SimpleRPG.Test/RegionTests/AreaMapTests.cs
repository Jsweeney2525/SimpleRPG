using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.RegionTests
{
    [TestFixture]
    public class AreaMapTests
    {
        private EventLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new EventLogger();
        }

        [TearDown]
        public void TearDown()
        {
            _logger = null;
        }

        [Test]
        public void RegionalMap_CorrectlyFiresOnRegionCompletedEvent()
        {
            SubRegion fakeSubRegion = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0],
                new FighterType[0], new BattlefieldConfiguration(new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, MagicType.Fire))));

            Region fakeRegion = new Region(WorldRegion.Fields, new BattleMove[0], new List<SubRegion> {fakeSubRegion});

            _logger.Subscribe(fakeRegion, EventType.RegionCompleted);

            AreaMap<Region, WorldRegion> fakeRegionalMap =
                new AreaMap<Region, WorldRegion>(fakeRegion, new MapPath<Region, WorldRegion>(fakeRegion));

            fakeRegionalMap.Advance(new MockDecisionManager(new GodRelationshipManager()), new TestTeam());

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.RegionCompleted, log.Type);

            RegionCompletedEventArgs e = log.E as RegionCompletedEventArgs;

            Assert.AreEqual(fakeRegion, e?.CompletedRegion);
        }

        [Test]
        public void SubRegionalMap_CorrectlyFiresOnRegionCompletedEvent()
        {
            SubRegion fakeSubRegion = new SubRegion(WorldSubRegion.Fields, 0, new ChanceEvent<int>[0],
                new FighterType[0], new BattlefieldConfiguration(new TeamConfiguration(new EnemyConfiguration(FighterType.Egg, 1, MagicType.Fire))));

            _logger.Subscribe(fakeSubRegion, EventType.SubRegionCompleted);

            AreaMap<SubRegion, WorldSubRegion> fakeRegionalMap =
                new AreaMap<SubRegion, WorldSubRegion>(fakeSubRegion, new MapPath<SubRegion, WorldSubRegion>(fakeSubRegion));

            fakeRegionalMap.Advance(new MockDecisionManager(new GodRelationshipManager()), new TestTeam());

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.SubRegionCompleted, log.Type);

            SubRegionCompletedEventArgs e = log.E as SubRegionCompletedEventArgs;

            Assert.AreEqual(fakeSubRegion, e?.CompletedSubRegion);
        }

        [Test]
        public void ResetRegionalGroupings_CorrectlyResetsDesertSubRegions()
        {
            
        }
    }
}