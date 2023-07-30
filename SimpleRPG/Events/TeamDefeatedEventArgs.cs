using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class TeamDefeatedEventArgs : EventArgs
    {
        public Team Team { get; private set; }

        public TeamDefeatedEventArgs(Team team)
        {
            Team = team;
        }
    }
}
