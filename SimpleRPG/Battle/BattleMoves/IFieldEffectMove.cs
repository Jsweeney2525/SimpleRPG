using System.Collections.Generic;
using SimpleRPG.Battle.FieldEffects;

namespace SimpleRPG.Battle.BattleMoves
{
    public interface IFieldEffectMove
    {
        int EffectDuration { get; }

        List<FieldEffect> FieldEffects { get; }
    }
}
