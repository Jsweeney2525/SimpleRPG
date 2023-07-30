using System;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.BattleMoveEffects
{
    //TODO: potentially rename and allow the effect to do negative damage. Or should that be a separate effect?
    public class RestorationBattleMoveEffect: BattleMoveEffect
    {
        public RestorationType RestorationType { get; }

        public int Percentage { get; }

        public RestorationBattleMoveEffect(RestorationType restorationType, int percentage, BattleMoveEffectActivationType effectActivationType, BattleCondition battleCondition = null)
            : base(effectActivationType, battleCondition)
        {
            if (percentage < 0)
            {
                throw new ArgumentException("RestoreHealthEffect cannot be initialized with a negative percentage!", nameof(percentage));
            }
            if (percentage == 0)
            {
                throw new ArgumentException("RestoreHealthEffect cannot be initialized with a percentage of 0!", nameof(percentage));
            }
            if (percentage > 100)
            {
                throw new ArgumentException("RestoreHealthEffect cannot be initialized with a percentage that exceeds 100!", nameof(percentage));
            }

            RestorationType = restorationType;
            Percentage = percentage;
        }
    }
}
