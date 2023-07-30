using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class AttackBattleMove : BattleMove
    {
        public int Accuracy { get; protected set; }

        public int CritChance { get; protected set; }

        public virtual int AttackPower { get; protected set; }

        protected AttackBattleMove(string description, TargetType targetType, int accuracy, int critChance,
            int attackPower = 0, int priority = 0, BattleMoveType moveType = BattleMoveType.Attack,
            string executionText = null, params BattleMoveEffect[] effects)
            : base(description, moveType, targetType, priority, executionText, effects: effects)
        {
            Accuracy = accuracy;
            CritChance = critChance;
            AttackPower = attackPower;
        }

        public AttackBattleMove(string description, TargetType targetType, int accuracy, int critChance, int attackPower = 0, int priority = 0, string executionText = null, params BattleMoveEffect[] effects)
            : this(description, targetType, accuracy, critChance, attackPower, priority, BattleMoveType.Attack, executionText, effects)
        {
        }

        public AttackBattleMove(BattleMove copy) : base(copy)
        {
            AttackBattleMove copyAsAttack = copy as AttackBattleMove;

            if (copyAsAttack != null)
            {
                Accuracy = copyAsAttack.Accuracy;
                CritChance = copyAsAttack.CritChance;
            }
        }
    }
}
