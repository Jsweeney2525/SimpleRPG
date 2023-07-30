using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class AttackMissedEventArgs : EventArgs
    {
        public IFighter TargettedFoe { get; private set; }

        public AttackMissedEventArgs(IFighter foe)
        {
            TargettedFoe = foe;
        }
    }
}
