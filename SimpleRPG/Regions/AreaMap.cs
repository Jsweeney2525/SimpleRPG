using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Events;
using SimpleRPG.Helpers;

namespace SimpleRPG.Regions
{
    public class AreaMap<TArea, TAreaId> where TArea : Area<TAreaId>
    {
        public TArea CurrentArea { get; private set; }

        public List<MapPath<TArea, TAreaId>> MapPaths { get; }

        public AreaMap(TArea startingPlace, params MapPath<TArea, TAreaId>[] paths)
        {
            CurrentArea = startingPlace;
            MapPaths = new List<MapPath<TArea, TAreaId>>(paths);
        }

        public TArea Advance(IDecisionManager decisionManager, Team advancingTeam)
        {
            TArea newArea;

            if (typeof(TArea) == typeof(Region))
            {
                Region currentRegion = CurrentArea as Region;

                RegionCompletedEventArgs e = new RegionCompletedEventArgs(currentRegion);
                currentRegion?.OnRegionCompleted(e);
            }
            if (typeof(TArea) == typeof(SubRegion))
            {
                SubRegion currentSubRegion = CurrentArea as SubRegion;

                SubRegionCompletedEventArgs e = new SubRegionCompletedEventArgs(currentSubRegion);
                currentSubRegion?.OnSubRegionCompleted(e);
            }

            MapPath<TArea, TAreaId> currentPath = MapPaths.First(p => p.From.Equals(CurrentArea));

            List<TArea> availableAreas = currentPath.To.GetAvaialableAreas().ToList();

            if (availableAreas.Count == 0)
            {
                newArea = null;
            }
            else if (availableAreas.Count == 1)
            {
                newArea = availableAreas[0];
            }
            else
            {
                newArea = decisionManager.PickNextArea(currentPath.To, advancingTeam);
            }

            CurrentArea = newArea;
            return newArea;
        }
    }
}