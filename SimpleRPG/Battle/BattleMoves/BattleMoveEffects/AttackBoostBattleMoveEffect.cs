using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.BattleMoveEffects
{
    public class AttackBoostBattleMoveEffect : BattleMoveEffect
    {
        public double Multiplier { get; }

        public AttackBoostBattleMoveEffect(double multiplier, BattleCondition battleCondition = null)
            : base(BattleMoveEffectActivationType.WhenCalculatingAttackStrength, battleCondition)
        {
            Multiplier = multiplier;
        }
    }
}
