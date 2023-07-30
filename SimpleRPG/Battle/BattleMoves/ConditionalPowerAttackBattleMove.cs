using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public interface IConditionalPowerExecutor
    {
        int GetAttackBonus();
    }

    public class ConditionalPowerAttackBattleMove : AttackBattleMove
    {
        protected int ConditionalPower;

        public override int AttackPower => base.AttackPower + ConditionalPower;

        public ConditionalPowerAttackBattleMove(string description, TargetType targetType, int accuracy, int critChance, int attackPower = 0, int priority = 0, string executionText = null, params BattleMoveEffect[] effects)
            : base(description, targetType, accuracy, critChance, attackPower, priority, BattleMoveType.ConditionalPowerAttack, executionText, effects)
        {
            ConditionalPower = 0;
        }

        public ConditionalPowerAttackBattleMove(BattleMove copy) : base(copy)
        {
            //right now, the only difference between an AttackBattleMove and a ConditionalPowerAttackBattleMove is the MoveType
            MoveType = BattleMoveType.ConditionalPowerAttack;
        }

        public void SetConditionalPower(int bonus)
        {
            ConditionalPower = bonus;
        }
    }
}
