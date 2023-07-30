using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses.BattleMoves
{
    public class TestFieldEffectMove: FieldEffectMove
    {
        public TestFieldEffectMove(string description, TargetType targetType, int duration, string executionText = null)
            : base(description, targetType, duration, executionText)
        {
        }

        public void AddEffect(FieldEffect effect)
        {
            if (!FieldEffects.Contains(effect))
            {
                effect.SetMoveName(Description);
                if (effect.EffectDuration == null)
                {
                    effect.SetEffectDuration(EffectDuration);
                }
                FieldEffects.Add(effect);
            }
        }

        public void ClearEffects()
        {
            FieldEffects.Clear();
        }

        public void SetDuration(int duration)
        {
            EffectDuration = duration;
        }

        public void SetDescription(string description)
        {
            Description = description;
        }
    }
}
