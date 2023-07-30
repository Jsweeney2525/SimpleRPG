using System;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;

namespace SimpleRPG.Events
{
    public class FighterSealedEventArgs : EventArgs
    {
        public Shade SealedFighter { get;  }

        public FighterSealedEventArgs(Shade shade)
        {
            SealedFighter = shade;
        }
    }
}
