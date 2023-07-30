using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.BattleMoveEffects
{
    public class NeverMissBattleMoveEffect : BattleMoveEffect
    {
        public NeverMissBattleMoveEffect(BattleCondition battleCondition = null)
            : base(BattleMoveEffectActivationType.WhenDeterminingIfAttackShouldHit, battleCondition)
        {
        }
    }
}
