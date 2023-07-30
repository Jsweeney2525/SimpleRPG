using System;
using SimpleRPG.Regions;

namespace SimpleRPG.Events
{
    public class RegionCompletedEventArgs : EventArgs
    {
        public Region CompletedRegion { get; }

        public RegionCompletedEventArgs(Region region)
        {
            CompletedRegion = region;
        }
    }
}
