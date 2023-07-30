using System.Collections.Generic;
using SimpleRPG.Enums;
using SimpleRPG.Regions;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public class MockMapManager : IMapManager
    {
        private MapManager _realMapManager;

        public MockMapManager(GroupingKeys groupingKeys = null)
        {
            _realMapManager = new MapManager(groupingKeys ?? Globals.GroupingKeys);
            _subRegionMaps = new Dictionary<WorldRegion, AreaMap<SubRegion, WorldSubRegion>>();
        }

        private AreaMap<Region, WorldRegion> _regionalMap;

        public void SetRegionalMap(AreaMap<Region, WorldRegion> regionalMap)
        {
            _regionalMap = regionalMap;
        }

        public AreaMap<Region, WorldRegion> GetRegionalMap(params Region[] regions)
        {
            return _regionalMap ?? _realMapManager.GetRegionalMap(regions);
        }

        private readonly Dictionary<WorldRegion, AreaMap<SubRegion, WorldSubRegion>> _subRegionMaps;

        public void SetSubRegionalMap(WorldRegion regionEnum, AreaMap<SubRegion, WorldSubRegion> subRegionalMap)
        {
            _subRegionMaps[regionEnum] = subRegionalMap;
        }

        public AreaMap<SubRegion, WorldSubRegion> GetSubRegionalMap(WorldRegion regionEnum, List<SubRegion> subRegions)
        {
            return _subRegionMaps.ContainsKey(regionEnum)
                ? _subRegionMaps[regionEnum]
                : _realMapManager.GetSubRegionalMap(regionEnum, subRegions);
        }

        public MapGrouping<T, TAreaId> GetGrouping<T, TAreaId>(int groupingId) where T : Area<TAreaId>
        {
            return _realMapManager.GetGrouping<T, TAreaId>(groupingId);
        }

        public void UnlockRegion(int groupingId, WorldRegion regionEnum)
        {
            _realMapManager.UnlockRegion(groupingId, regionEnum);
        }

        public void UnlockSubRegion(int groupingId, WorldSubRegion subRegionEnum)
        {
            _realMapManager.UnlockSubRegion(groupingId, subRegionEnum);
        }
    }
}
