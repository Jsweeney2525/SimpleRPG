using System.Collections.Generic;

namespace SimpleRPG.Enums
{
    public enum WorldSubRegion
    {
        //**** field Subregions ****//
        Fields

        //**** Desert Subregions ****//
        , DesertIntro
        ,DesertCrypt
        ,TavernOfHeroes
        ,AncientLibrary
        ,Oasis
        ,CliffsOfAThousandPushups
        ,TempleOfDarkness
        ,VillageCenter
        ,BeastTemple
        ,Coliseum

        //**** Crystal caves ****//
        ,CavesIntro

        //**** Casino ****//
        , CasinoIntro

        //**** DarkCastle ****//
        ,DarkCastleIntro
    }

    public class WorldSubRegions
    {
        public static IEnumerable<WorldSubRegion> GetSubRegionsForRegions(WorldRegion region)
        {
            WorldSubRegion[] ret = null;

            switch (region)
            {
                case WorldRegion.Desert:
                    ret = new[] {WorldSubRegion.DesertIntro, WorldSubRegion.DesertCrypt, WorldSubRegion.TavernOfHeroes
                        , WorldSubRegion.AncientLibrary, WorldSubRegion.Oasis, WorldSubRegion.CliffsOfAThousandPushups
                        , WorldSubRegion.TempleOfDarkness, WorldSubRegion.VillageCenter, WorldSubRegion.BeastTemple, WorldSubRegion.Coliseum };
                    break;
                case WorldRegion.Fields:
                    ret = new[] { WorldSubRegion.Fields };
                    break;
                case WorldRegion.CrystalCaves:
                    ret = new[] { WorldSubRegion.CavesIntro };
                    break;
                case WorldRegion.Casino:
                    ret = new[] { WorldSubRegion.CasinoIntro };
                    break;
                case WorldRegion.DarkCastle:
                    ret = new[] { WorldSubRegion.DarkCastleIntro };
                    break;
            }

            return ret;
        }

        public static IEnumerable<WorldSubRegion> GetSubRegionsByGroupingId(int groupingId, GroupingKeys groupingKeys = null)
        {
            WorldSubRegion[] ret = null;

            if (groupingKeys == null)
            {
                groupingKeys = Globals.GroupingKeys;
            }

            if (groupingId == groupingKeys.FirstDesertGroupingId)
            {
                ret = new[]
                {
                    WorldSubRegion.DesertCrypt,
                    WorldSubRegion.TavernOfHeroes,
                    WorldSubRegion.AncientLibrary,
                    WorldSubRegion.Oasis
                };
            }
            else if (groupingId == groupingKeys.SecondDesertGroupingId)
            {
                ret = new[]
                {
                    WorldSubRegion.VillageCenter,
                    WorldSubRegion.CliffsOfAThousandPushups,
                    WorldSubRegion.TempleOfDarkness,
                    WorldSubRegion.BeastTemple
                };
            }

            return ret;
        }

        public static GodEnum GetGodEnumBySubRegion(WorldSubRegion subRegion)
        {
            GodEnum ret = GodEnum.None;
            
            switch (subRegion)
            {
                case WorldSubRegion.AncientLibrary:
                case WorldSubRegion.TempleOfDarkness:
                    ret = GodEnum.IntellectGod;
                    break;
                case WorldSubRegion.TavernOfHeroes:
                case WorldSubRegion.CliffsOfAThousandPushups:
                    ret = GodEnum.StrengthGod;
                    break;
                case WorldSubRegion.DesertCrypt:
                case WorldSubRegion.VillageCenter:
                    ret = GodEnum.TricksterGod;
                    break;
                case WorldSubRegion.Oasis:
                    ret = GodEnum.MercyGod;
                    break;
                case WorldSubRegion.BeastTemple:
                    ret = GodEnum.BeastGod;
                    break;
            }

            return ret;
        }
    }
}