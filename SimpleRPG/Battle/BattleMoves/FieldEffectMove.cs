using System.Collections.Generic;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class FieldEffectMove: SpecialMove, IFieldEffectMove
    {
        public int EffectDuration { get; protected set; }

        public List<FieldEffect> FieldEffects { get; protected set; }

        public FieldEffectMove(string description, TargetType targetType, int duration, string executionText, params FieldEffect[] effects)
            : base(description, BattleMoveType.Field, targetType, executionText)
        {
            EffectDuration = duration;

            FieldEffects = new List<FieldEffect>();

            foreach (FieldEffect effect in effects)
            {
                effect.SetEffectDuration(duration);
                FieldEffects.Add(effect.Copy());
            }
        }
    }
}
