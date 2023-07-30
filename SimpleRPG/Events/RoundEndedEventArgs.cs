using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class RoundEndedEventArgs : EventArgs
    {
        public Team Team { get; }

        public IFighter Fighter { get; }

        public RoundEndedEventArgs(Team team, IFighter fighter = null)
        {
            Team = team;
            Fighter = fighter;
        }
    }
}
