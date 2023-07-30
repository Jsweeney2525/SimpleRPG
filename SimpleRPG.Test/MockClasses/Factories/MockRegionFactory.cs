using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public class MockRegionFactory : IRegionFactory
    {
        private Dictionary<WorldRegion, Region> RegionDictionary { get; } = new Dictionary<WorldRegion, Region>();

        private RegionFactory RealFactory { get; }

        public MockRegionFactory(IDecisionManager decisionManager)
        {
            RealFactory = new RegionFactory(decisionManager);
        }

        public void SetRegion(WorldRegion regionEnum, Region region)
        {
            RegionDictionary[regionEnum] = region;
        }

        public Region GetRegion(WorldRegion region)
        {
            return RegionDictionary.ContainsKey(region) ? RegionDictionary[region] : RealFactory.GetRegion(region);
        }

        public IEnumerable<Region> GetRegions(IEnumerable<WorldRegion> regionEnums)
        {
            return regionEnums.Select(GetRegion);
        }
    }
}
