using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class MagicMultiplierFieldEffect : FieldEffect
    {
        public double Percentage { get; }

        public MagicType MagicType { get; }

        public MagicMultiplierFieldEffect(
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
            return new MagicMultiplierFieldEffect(TargetType, MoveName, MagicType, Percentage, EffectDuration, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var multiplierEffect = effect as MagicMultiplierFieldEffect;

            return multiplierEffect != null 
                && multiplierEffect.MagicType == MagicType 
                && Math.Abs(multiplierEffect.Percentage - Percentage) <= .01;
        }
    }
}
