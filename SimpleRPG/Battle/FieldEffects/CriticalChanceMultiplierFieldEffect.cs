using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class CriticalChanceMultiplierFieldEffect : FieldEffect
    {
        public double Percentage { get; }

        public CriticalChanceMultiplierFieldEffect(
            TargetType targetType, 
            string moveName, 
            double percentage, 
            int? effectDuration = null, 
            bool immediatelyExecuted = false) 
            : base(targetType, moveName, effectDuration, immediatelyExecuted)
        {
            Percentage = percentage;
        }

        public override FieldEffect Copy()
        {
            return new CriticalChanceMultiplierFieldEffect(TargetType, MoveName, Percentage, EffectDuration, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var critEffect = effect as CriticalChanceMultiplierFieldEffect;

            var ret = (critEffect != null 
                && Math.Abs(critEffect.Percentage - Percentage) <= .01);

            return ret;
        }
    }
}
