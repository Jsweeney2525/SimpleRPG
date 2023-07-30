using System.Collections.Generic;
using SimpleRPG.Enums;

namespace SimpleRPG.Regions
{
    public interface IRegionFactory
    {
        Region GetRegion(WorldRegion region);

        IEnumerable<Region> GetRegions(IEnumerable<WorldRegion> regions);
    }
}
