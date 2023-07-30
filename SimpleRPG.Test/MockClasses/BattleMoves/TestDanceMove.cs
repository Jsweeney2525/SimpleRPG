using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses.BattleMoves
{
    public class TestDanceMove: DanceMove
    {
        public TestDanceMove(string description, TargetType targetType, int effectDuration)
            : base(description, targetType, effectDuration, DanceEffectType.None, new List<FieldEffect>())
        {
        }

        public void AddEffect(FieldEffect effect)
        {
            if (!FieldEffects.Contains(effect))
            {
                FieldEffects.Add(effect);
            }
        }

        public void ClearEffects()
        {
            FieldEffects.RemoveAll(e => true);
        }

        public void AddMove(BattleMove move)
        {
            Moves.Add(move);
        }

        public void SetDuration(int duration)
        {
            EffectDuration = duration;
        }

        public void SetDanceEffect(DanceEffectType effect)
        {
            DanceEffect = effect;
        }
    }
}
