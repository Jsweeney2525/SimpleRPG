using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.BattleMoveEffects
{
    public class CannotBeEvadedBattleMoveEffect : BattleMoveEffect
    {
        public CannotBeEvadedBattleMoveEffect(BattleCondition battleCondition = null)
            : base(BattleMoveEffectActivationType.WhenCalculatingAttackStrength, battleCondition)
        {
        }
    }
}
