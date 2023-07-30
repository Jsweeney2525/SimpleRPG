using SimpleRPG.Battle.Fighters;
using System;
using SimpleRPG.Battle.Magic;

namespace SimpleRPG.Events
{
    public class SpellSuccessfulEventArgs : EventArgs
    {
        public int DamageDealt { get; private set; }

        public IFighter TargettedFoe { get; private set; }

        public Spell Spell { get; private set; }

        public SpellSuccessfulEventArgs(IFighter foe, Spell spell, int damageDealt)
        {
            TargettedFoe = foe;
            Spell = spell;
            DamageDealt = damageDealt;
        }
    }
}
