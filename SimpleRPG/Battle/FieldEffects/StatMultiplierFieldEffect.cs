using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class StatMultiplierFieldEffect : FieldEffect
    {
        public double Percentage { get; }

        public StatType Stat { get; }

        public StatMultiplierFieldEffect(
            TargetType targetType, 
            string moveName, 
            StatType stat, 
            double percentage, 
            int? effectDuration = null) 
            : base(targetType, moveName, effectDuration, false)
        {
            Percentage = percentage;
            Stat= stat;
        }

        public override FieldEffect Copy()
        {
            return new StatMultiplierFieldEffect(TargetType, MoveName, Stat, Percentage, EffectDuration);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var statEffect = effect as StatMultiplierFieldEffect;

            return statEffect != null
                   && statEffect.Stat == Stat
                   && Math.Abs(statEffect.Percentage - Percentage) <= .01;
        }
    }
}
