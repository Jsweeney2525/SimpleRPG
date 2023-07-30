using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Screens;

namespace SimpleRPG.Regions
{
    public class Region : Area<WorldRegion>
    {
        public IEnumerable<BattleMove> MovesUnlockedUponEnteringRegion { get; }

        public List<SubRegion> SubRegions { get; }

        #region events

        public EventHandler<RegionCompletedEventArgs> RegionCompleted { get; set; }

        public void OnRegionCompleted(RegionCompletedEventArgs e)
        {
            RegionCompleted?.Invoke(this, e);
        }

        #endregion

        public Region(WorldRegion areaId, IEnumerable<BattleMove> regionSpecificMoves, IEnumerable<SubRegion> subRegions, ColorString regionIntro = null) :
            base(areaId, regionIntro)
        {
            MovesUnlockedUponEnteringRegion = regionSpecificMoves;
            SubRegions = new List<SubRegion>(subRegions);
        }
    }
}
