using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class MagicResistanceMultiplierFieldEffect : FieldEffect
    {
        public double Percentage { get; }

        public MagicType MagicType { get; }

        public MagicResistanceMultiplierFieldEffect(
            TargetType targetType, 
            string moveName, 
            MagicType magicType, 
            double percentage, 
            int? effectDuration = null,
            bool immediatelyExecuted = false) 
            : base(targetType, moveName, effectDuration, immediatelyExecuted)
        {
            Percentage = percentage;
            MagicType = magicType;
        }

        public override FieldEffect Copy()
        {
            return new MagicResistanceMultiplierFieldEffect(TargetType, MoveName, MagicType, Percentage, EffectDuration, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var multiplierEffect = effect as MagicResistanceMultiplierFieldEffect;

            return multiplierEffect != null 
                && multiplierEffect.MagicType == MagicType 
                && Math.Abs(multiplierEffect.Percentage - Percentage) <= .01;
        }
    }
}
