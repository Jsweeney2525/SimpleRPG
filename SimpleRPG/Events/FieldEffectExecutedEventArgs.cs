using System;
using System.Collections.Generic;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Events
{
    public class FieldEffectExecutedEventArgs : EventArgs
    {
        public FieldEffect Effect { get; private set; }

        public List<IFighter> EffectOwners { get; private set; }

        public FieldEffectExecutedEventArgs(FieldEffect effect, params IFighter[] owners)
        {
            Effect = effect;
            EffectOwners = new List<IFighter>(owners);
        }
    }
}
