using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.BattleMoveEffects
{
    public abstract class BattleMoveEffect
    {
        /// <summary>
        /// A nullable property that indicates this effect should only be activated if certain dance effects are active
        /// </summary>
        public BattleCondition BattleCondition { get; }

        public BattleMoveEffectActivationType EffectActivationType { get; }

        protected BattleMoveEffect(BattleMoveEffectActivationType effectActivationType, BattleCondition battleCondition = null)
        {
            BattleCondition = battleCondition;
            EffectActivationType = effectActivationType;
        }
    }
}
