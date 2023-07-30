using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class ShieldFortifyingMove : SpecialMove
    {
        public ShieldFortifyingType FortifyingType { get; }

        public int FortifyingAmount { get; }

        public ShieldFortifyingMove(string description, TargetType targetType, string executionText, ShieldFortifyingType fortifyingType, int fortifyingAmount) 
            : base(description, BattleMoveType.ShieldFortifier, targetType, executionText)
        {
            FortifyingType = fortifyingType;
            FortifyingAmount = fortifyingAmount;
        }

        public ShieldFortifyingMove(BattleMove copy) : base(copy)
        {
            ShieldFortifyingMove shieldFortifyingMove = copy as ShieldFortifyingMove;

            if (shieldFortifyingMove != null)
            {
                FortifyingType = shieldFortifyingMove.FortifyingType;
                FortifyingAmount = shieldFortifyingMove.FortifyingAmount;
            }
        }
    }
}
