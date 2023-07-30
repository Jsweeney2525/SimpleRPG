using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class TurnEndedEventArgs : EventArgs
    {
        public IFighter Fighter { get; }

        public TurnEndedEventArgs(IFighter fighter)
        {
            Fighter = fighter;
        }
    }
}
