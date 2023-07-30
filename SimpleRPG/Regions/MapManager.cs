using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Regions
{
    public class MapManager : IMapManager
    {
        //private List<MapGrouping<Region, WorldRegion>> _allRegionGroupings;
        //private List<MapGrouping<SubRegion, WorldSubRegion>> _allSubRegionGroupings;

        private readonly GroupingKeys _groupingKeys;

        private static Dictionary<int, MapGrouping<Region, WorldRegion>> _regionGroupingsDictionary;
        private static Dictionary<int, MapGrouping<SubRegion, WorldSubRegion>> _subRegionGroupingsDictionary;

        public MapManager(GroupingKeys groupingKeys)
        {
            _regionGroupingsDictionary = new Dictionary<int, MapGrouping<Region, WorldRegion>>();
            _subRegionGroupingsDictionary = new Dictionary<int, MapGrouping<SubRegion, WorldSubRegion>>();
            _groupingKeys = groupingKeys;
        }

        public AreaMap<Region, WorldRegion> GetRegionalMap(params Region[] regions)
        {
            List<Region> regionList = regions.ToList();
            int mainRegionalGroupingId = _groupingKeys.MainRegionalMapGroupingId;

            Region fieldsRegion = regionList.First(r => r.AreaId == WorldRegion.Fields);
            Region desertRegion = regionList.First(r => r.AreaId == WorldRegion.Desert);
            Region crystalRegion = regionList.First(r => r.AreaId == WorldRegion.CrystalCaves);
            Region casinoRegion = regionList.First(r => r.AreaId == WorldRegion.Casino);
            Region castleRegion = regionList.First(r => r.AreaId == WorldRegion.DarkCastle);

            //note: if there may ever come a time to lock these regions or a reason GetRegionalMap() is called more than once, 
            //it could mess with the logic, perhaps
            desertRegion.RegionCompleted += LogRegionCompleted;
            crystalRegion.RegionCompleted += LogRegionCompleted;
            casinoRegion.RegionCompleted += LogRegionCompleted;

            MapGrouping<Region, WorldRegion> mainGrouping = new MapGrouping<Region, WorldRegion>(mainRegionalGroupingId,
                desertRegion,
                crystalRegion,
                casinoRegion,
                castleRegion);

            mainGrouping.Lock(m => m.AreaId == WorldRegion.DarkCastle);

            if (!_regionGroupingsDictionary.ContainsKey(mainRegionalGroupingId))
            {
                _regionGroupingsDictionary.Add(mainRegionalGroupingId, mainGrouping);
            }

            List<MapPath<Region, WorldRegion>> mapPaths = new List<MapPath<Region, WorldRegion>>();

            foreach (Region region in regions)
            {
                MapPath<Region, WorldRegion> pathToAdd;
                if (region.AreaId == WorldRegion.DarkCastle)
                {
                    pathToAdd = new MapPath<Region, WorldRegion>(region);
                }
                else
                {
                    pathToAdd = new MapPath<Region, WorldRegion>(region, mainGrouping);
                }

                mapPaths.Add(pathToAdd);
            }

            AreaMap<Region, WorldRegion> ret = new AreaMap<Region, WorldRegion>(fieldsRegion,
                mapPaths.ToArray()
            );

            mainGrouping.SetParent(ret);

            return ret;
        }

        public AreaMap<SubRegion, WorldSubRegion> GetSubRegionalMap(WorldRegion regionEnum, List<SubRegion> subRegions)
        {
            AreaMap<SubRegion, WorldSubRegion> ret = null;

            switch (regionEnum)
            {
                case WorldRegion.Fields:
                    SubRegion fieldIntroArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.Fields);
                    ret = new AreaMap<SubRegion, WorldSubRegion>(fieldIntroArea, new MapPath<SubRegion, WorldSubRegion>(fieldIntroArea));
                    break;
         
                case WorldRegion.Desert:
                    ret = CreateDesertSubRegionMap(subRegions);
                    break;
                case WorldRegion.Casino:
                    ret = CreateCasinoSubRegionMap(subRegions);
                    break;
                case WorldRegion.CrystalCaves:
                    ret = CreateCrystalCavesSubRegionMap(subRegions);
                    break;
                case WorldRegion.DarkCastle:
                    ret = CreateDarkCastleSubRegionMap(subRegions);
                    break;
            }

            return ret;
        }

        public MapGrouping<T, TAreaId> GetGrouping<T, TAreaId>(int groupingId) where T : Area<TAreaId>
        {
            MapGrouping<T, TAreaId> ret = null;
            Type tType = typeof(T);

            if (tType == typeof(Region))
            {
                ret = _regionGroupingsDictionary[groupingId] as MapGrouping<T, TAreaId>;
            }
            else if (tType == typeof(SubRegion))
            {
                ret = _subRegionGroupingsDictionary[groupingId] as MapGrouping<T, TAreaId>;
            }

            return ret;
        }

        public void UnlockRegion(int groupingId, WorldRegion regionalEnum)
        {
            MapGrouping<Region, WorldRegion> grouping = _regionGroupingsDictionary[groupingId];

            MapGroupingItem<Region, WorldRegion> regionGroupingItem = grouping.Values.Single(sr => sr.Item.AreaId == regionalEnum);

            regionGroupingItem.Unlock();
        }

        public void UnlockSubRegion(int groupingId, WorldSubRegion subRegionEnum)
        {
            MapGrouping<SubRegion, WorldSubRegion> grouping = _subRegionGroupingsDictionary[groupingId];

            MapGroupingItem<SubRegion, WorldSubRegion> subRegionGroupingItem = grouping.Values.Single(sr => sr.Item.AreaId == subRegionEnum);

            subRegionGroupingItem.Unlock();
        }

        private AreaMap<SubRegion, WorldSubRegion> CreateDesertSubRegionMap(List<SubRegion> subRegions)
        {
            SubRegion desertIntroArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.DesertIntro);
            SubRegion coliseumArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.Coliseum);

            int firstDesertGroupingId = _groupingKeys.FirstDesertGroupingId;
            MapGrouping<SubRegion, WorldSubRegion> firstGrouping = new MapGrouping<SubRegion, WorldSubRegion>(firstDesertGroupingId,
                subRegions.First(sr => sr.AreaId == WorldSubRegion.DesertCrypt),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.TavernOfHeroes),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.AncientLibrary),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.Oasis));

            if (!_subRegionGroupingsDictionary.ContainsKey(firstDesertGroupingId))
            {
                _subRegionGroupingsDictionary.Add(firstDesertGroupingId, firstGrouping);
            }

            int secondDesertGroupingId = _groupingKeys.SecondDesertGroupingId;
            MapGrouping<SubRegion, WorldSubRegion> secondGrouping = new MapGrouping<SubRegion, WorldSubRegion>(secondDesertGroupingId,
                subRegions.First(sr => sr.AreaId == WorldSubRegion.VillageCenter),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.CliffsOfAThousandPushups),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.TempleOfDarkness),
                subRegions.First(sr => sr.AreaId == WorldSubRegion.BeastTemple));

            if (!_subRegionGroupingsDictionary.ContainsKey(secondDesertGroupingId))
            {
                _subRegionGroupingsDictionary.Add(secondDesertGroupingId, secondGrouping);
            }

            int coliseumDesertGroupingId = _groupingKeys.ColiseumDesertGroupingId;
            MapGrouping<SubRegion, WorldSubRegion> coliseumGrouping = new MapGrouping<SubRegion, WorldSubRegion>(coliseumDesertGroupingId, coliseumArea);

            if (!_subRegionGroupingsDictionary.ContainsKey(coliseumDesertGroupingId))
            {
                _subRegionGroupingsDictionary.Add(coliseumDesertGroupingId, coliseumGrouping);
            }

            List<MapPath<SubRegion, WorldSubRegion>> mapPaths = new List<MapPath<SubRegion, WorldSubRegion>>
            {
                new MapPath<SubRegion, WorldSubRegion>(desertIntroArea, firstGrouping),
                new MapPath<SubRegion, WorldSubRegion>(coliseumArea)
            };

            //each subregion in the first grouping should have a path that leads to the second grouping
            mapPaths.AddRange(firstGrouping.Values.Select(sr => new MapPath<SubRegion, WorldSubRegion>(sr.Item, secondGrouping)));
            //each subregion in the second grouping should have a path that leads to the coliseum
            mapPaths.AddRange(secondGrouping.Values.Select(sr => new MapPath<SubRegion, WorldSubRegion>(sr.Item, coliseumGrouping)));

            AreaMap<SubRegion, WorldSubRegion> ret = new AreaMap<SubRegion, WorldSubRegion>(desertIntroArea,
                mapPaths.ToArray());

            firstGrouping.SetParent(ret);
            secondGrouping.SetParent(ret);
            coliseumGrouping.SetParent(ret);

            return ret;
        }

        private AreaMap<SubRegion, WorldSubRegion> CreateCasinoSubRegionMap(List<SubRegion> subRegions)
        {
            SubRegion introArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.CasinoIntro);
            AreaMap<SubRegion, WorldSubRegion> ret = new AreaMap<SubRegion, WorldSubRegion>(introArea, new MapPath<SubRegion, WorldSubRegion>(introArea));

            return ret;
        }

        private AreaMap<SubRegion, WorldSubRegion> CreateCrystalCavesSubRegionMap(List<SubRegion> subRegions)
        {
            SubRegion introArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.DarkCastleIntro);
            AreaMap<SubRegion, WorldSubRegion> ret = new AreaMap<SubRegion, WorldSubRegion>(introArea, new MapPath<SubRegion, WorldSubRegion>(introArea));

            return ret;
        }

        private AreaMap<SubRegion, WorldSubRegion> CreateDarkCastleSubRegionMap(List<SubRegion> subRegions)
        {
            SubRegion introArea = subRegions.First(sr => sr.AreaId == WorldSubRegion.DarkCastleIntro);
            AreaMap<SubRegion, WorldSubRegion> ret = new AreaMap<SubRegion, WorldSubRegion>(introArea, new MapPath<SubRegion, WorldSubRegion>(introArea));

            return ret;
        }

        private bool _desertRegionCompleted;
        private bool _crystalRegionCompleted;
        private bool _casionRegionCompleted;

        private void LogRegionCompleted(object sender, RegionCompletedEventArgs e)
        {
            switch (e.CompletedRegion.AreaId)
            {
                case WorldRegion.Casino:
                    _desertRegionCompleted = true;
                    break;
                case WorldRegion.CrystalCaves:
                    _crystalRegionCompleted = true;
                    break;
                case WorldRegion.Desert:
                    _casionRegionCompleted = true;
                    break;
            }

            if (_desertRegionCompleted && _crystalRegionCompleted && _casionRegionCompleted)
            {
                UnlockRegion(_groupingKeys.MainRegionalMapGroupingId, WorldRegion.DarkCastle);
            }
        }
    }
}
