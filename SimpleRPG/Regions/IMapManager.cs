using System.Collections.Generic;
using SimpleRPG.Enums;

namespace SimpleRPG.Regions
{
    public interface IMapManager
    {
        AreaMap<Region, WorldRegion> GetRegionalMap(params Region[] regions);

        AreaMap<SubRegion, WorldSubRegion> GetSubRegionalMap(WorldRegion regionEnum, List<SubRegion> subRegions);

        MapGrouping<T, TAreaId> GetGrouping<T, TAreaId>(int groupingId) where T: Area<TAreaId>;

        void UnlockRegion(int groupingId, WorldRegion subRegionEnum);

        void UnlockSubRegion(int groupingId, WorldSubRegion subRegionEnum);
    }
}
