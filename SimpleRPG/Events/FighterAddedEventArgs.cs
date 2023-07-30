using System;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Events
{
    public class FighterAddedEventArgs : EventArgs
    {
        public IFighter Fighter { get;  }

        public FighterAddedEventArgs(IFighter fighter)
        {
            Fighter = fighter;
        }
    }
}
