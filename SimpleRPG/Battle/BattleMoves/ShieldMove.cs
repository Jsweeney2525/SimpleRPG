using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class ShieldMove : SpecialMove
    {
        public IBattleShield Shield { get; }


        public ShieldMove(string description, TargetType targetType, string executionText, IBattleShield shield) 
            : base(description, BattleMoveType.Shield, targetType, executionText)
        {
            Shield = shield;
        }

        public ShieldMove(BattleMove copy) : base(copy)
        {
            ShieldMove shieldMove = copy as ShieldMove;

            if (shieldMove != null)
            {
                Shield = shieldMove.Shield.Copy();
            }
        }
    }
}
