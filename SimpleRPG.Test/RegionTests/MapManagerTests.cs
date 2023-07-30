using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.RegionTests
{
    [TestFixture]
    public class MapManagerTests
    {
        private MockRegionFactory _regionFactory;
        private MapManager _mapManager;

        [SetUp]
        public void SetUp()
        {
            _regionFactory = new MockRegionFactory(Globals.DecisionManager);

            _mapManager = new MapManager(Globals.GroupingKeys);
        }

        [TearDown]
        public void TearDown()
        {
            _regionFactory = null;
            _mapManager = null;
        }

        [Test]
        public void UnlockSubRegionMethod_CorrectlyUnlocksArea([Range(0, 3)] int regionToUnlockIndex)
        {
            Region desertRegion = _regionFactory.GetRegion(WorldRegion.Desert);
            AreaMap<SubRegion, WorldSubRegion> subregionalMap = _mapManager.GetSubRegionalMap(WorldRegion.Desert, desertRegion.SubRegions);

            MapPath<SubRegion, WorldSubRegion> pathFromDesertIntro = subregionalMap.MapPaths.First(path => path.From.AreaId == WorldSubRegion.DesertIntro);
            MapGrouping<SubRegion, WorldSubRegion> destinationGrouping = pathFromDesertIntro.To;

            Assert.True(destinationGrouping.Values.TrueForAll(groupingItem => !groupingItem.IsLocked));

            MapGroupingItem<SubRegion, WorldSubRegion> regionToUnlockGroupingItem = destinationGrouping.Values[regionToUnlockIndex];
            WorldSubRegion unlockedRegionEnum = regionToUnlockGroupingItem.Item.AreaId;

            _mapManager.UnlockSubRegion(destinationGrouping.GroupingId, unlockedRegionEnum);

            Assert.False(regionToUnlockGroupingItem.IsLocked);

            List<MapGroupingItem<SubRegion, WorldSubRegion>> stillLockedRegions =
                destinationGrouping.Values.Where(dgi => dgi.Item.AreaId != unlockedRegionEnum).ToList();

            Assert.True(stillLockedRegions.TrueForAll(groupingItem => !groupingItem.IsLocked));
        }

        [Test]
        public void UnlockRegionMethod_CorrectlyUnlocksArea()
        {
            IEnumerable<WorldRegion> allRegionEnums = EnumHelperMethods.GetAllValuesForEnum<WorldRegion>();
            IEnumerable<Region> allRegions = _regionFactory.GetRegions(allRegionEnums);
            AreaMap<Region, WorldRegion> regionalMap = _mapManager.GetRegionalMap(allRegions.ToArray());

            MapGrouping<Region, WorldRegion> destinationGrouping =
                regionalMap.MapPaths.First(mp => mp.From.AreaId == WorldRegion.Fields).To;

            MapGroupingItem<Region, WorldRegion> darkCastleGroupingItem =
                destinationGrouping.Values.Single(gi => gi.Item.AreaId == WorldRegion.DarkCastle);

            Assert.True(darkCastleGroupingItem.IsLocked);

            _mapManager.UnlockRegion(destinationGrouping.GroupingId, WorldRegion.DarkCastle);

            Assert.False(darkCastleGroupingItem.IsLocked);
        }

        [Test]
        public void DarkCastleRegion_UnlockedAfterBeatingOtherAreas()
        {
            Region fieldsRegion = new Region(WorldRegion.Fields, new BattleMove[0], new SubRegion[0]);
            Region desertRegion = new Region(WorldRegion.Desert, new BattleMove[0], new SubRegion[0]);
            Region casinoRegion = new Region(WorldRegion.Casino, new BattleMove[0], new SubRegion[0]);
            Region crystalCavesRegion = new Region(WorldRegion.CrystalCaves, new BattleMove[0], new SubRegion[0]);
            Region darkCastleRegion = new Region(WorldRegion.DarkCastle, new BattleMove[0], new SubRegion[0]);

            AreaMap<Region, WorldRegion> regionalMap = _mapManager.GetRegionalMap(fieldsRegion, desertRegion, casinoRegion, crystalCavesRegion, darkCastleRegion);

            MapGroupingItem<Region, WorldRegion> darkCastleGroupItem = regionalMap.MapPaths.First(mp => mp.To.Values.FirstOrDefault(gi => gi.Item == darkCastleRegion) != null).To.Values.First(gi => gi.Item == darkCastleRegion);
            
            MockDecisionManager decisionManager = new MockDecisionManager(new GodRelationshipManager());

            int i = 0;
            for(; i < 3; ++i)
            {
                Assert.True(darkCastleGroupItem.IsLocked, "Dark Castle should be locked, not all requisite areas have been completed");
                decisionManager.SetGroupingChoice(i);
                regionalMap.Advance(decisionManager, new TestTeam());
            }

            decisionManager.SetGroupingChoice(i);
            Region finalRegion = null;
            Assert.DoesNotThrow(() => finalRegion = regionalMap.Advance(decisionManager, new TestTeam()));

            Assert.False(darkCastleGroupItem.IsLocked, "Dark Castle should be unlocked, all requisite areas have been completed");

            Assert.AreEqual(darkCastleRegion, finalRegion);
        }

        [Test]
        public void GetGroupingMethod_ReturnsSingleton()
        {
            //arrange
            List<Region> regions = _regionFactory.GetRegions(EnumHelperMethods.GetAllValuesForEnum<WorldRegion>()).ToList();
            _mapManager.GetRegionalMap(regions.ToArray());

            Region desertRegion = regions.First(r => r.AreaId == WorldRegion.Desert);
            _mapManager.GetSubRegionalMap(WorldRegion.Desert, desertRegion.SubRegions);

            int regionalGroupingId = Globals.GroupingKeys.MainRegionalMapGroupingId;
            int groupingId = Globals.GroupingKeys.FirstDesertGroupingId;

            MapGrouping<SubRegion, WorldSubRegion> originalSubRegionalGrouping = _mapManager.GetGrouping<SubRegion, WorldSubRegion>(groupingId);
            originalSubRegionalGrouping.Lock(sr => sr.AreaId != WorldSubRegion.Oasis);

            MapGrouping<Region, WorldRegion> originalRegionalGrouping = _mapManager.GetGrouping<Region, WorldRegion>(regionalGroupingId);
            originalRegionalGrouping.Lock(sr => sr.AreaId != WorldRegion.Casino);

            //act
            MapGrouping<SubRegion, WorldSubRegion> secondSubRegionalGrouping = _mapManager.GetGrouping<SubRegion, WorldSubRegion>(groupingId);
            MapGrouping<Region, WorldRegion> secondRegionalGrouping = _mapManager.GetGrouping<Region, WorldRegion>(regionalGroupingId);

            //Assert
            Assert.AreEqual(1, secondSubRegionalGrouping.GetAvaialableAreas().Count());
            Assert.AreEqual(1, secondRegionalGrouping.GetAvaialableAreas().Count());
        }
    }
}
