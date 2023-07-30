using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class RestoreHealthPercentageFieldEffect : FieldEffect
    {
        public int Percentage { get; }

        public RestoreHealthPercentageFieldEffect(
            TargetType targetType, 
            string moveName, 
            int percentage, 
            int? effectDuration = null,
            bool immediatelyExecuted = false) 
            : base(targetType, moveName, effectDuration, immediatelyExecuted)
        {
            if (percentage == 0)
            {
                throw new ArgumentException(
                    "percentage-based Restore Health Field Effect initialized without a percentage specified");
            }
            else if (percentage < 0 || percentage > 100)
            {
                throw new ArgumentException(
                    $"percentage value is out of range for Restore Health Field Effect! Value: {percentage}. Must be between 1 and 100 inclusive");
            }

            Percentage = percentage;
        }

        public override FieldEffect Copy()
        {
            return new RestoreHealthPercentageFieldEffect(TargetType, MoveName, Percentage, Percentage, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var healthEffect = effect as RestoreHealthPercentageFieldEffect;

            return healthEffect != null
                   && healthEffect.Percentage == Percentage;
        }
    }
}
