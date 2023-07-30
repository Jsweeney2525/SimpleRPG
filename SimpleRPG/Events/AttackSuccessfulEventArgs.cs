using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class AttackSuccessfulEventArgs : EventArgs
    {
        public int DamageDealt { get; private set; }

        public IFighter TargettedFoe { get; private set; }

        public AttackSuccessfulEventArgs(IFighter foe, int damageDealt)
        {
            TargettedFoe = foe;
            DamageDealt = damageDealt;
        }
    }
}
