using System;
using SimpleRPG.Regions;

namespace SimpleRPG.Events
{
    public class SubRegionCompletedEventArgs : EventArgs
    {
        public SubRegion CompletedSubRegion { get; }

        public SubRegionCompletedEventArgs(SubRegion subRegion)
        {
            CompletedSubRegion = subRegion;
        }
    }
}
