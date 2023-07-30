using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class SpellCostMultiplierFieldEffect : FieldEffect
    {
        public double Multiplier { get; }

        public SpellCostMultiplierFieldEffect(
            TargetType targetType, 
            string moveName, 
            double multiplier, 
            int? effectDuration = null,
            bool immediatelyExecuted = false) 
            : base(targetType, moveName, effectDuration, immediatelyExecuted)
        {
            Multiplier = multiplier;
        }

        public override FieldEffect Copy()
        {
            return new SpellCostMultiplierFieldEffect(TargetType, MoveName, Multiplier, EffectDuration, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var costEffect = effect as SpellCostMultiplierFieldEffect;

            return costEffect != null
                   && Math.Abs(costEffect.Multiplier - Multiplier) <= .01;
        }
    }
}
