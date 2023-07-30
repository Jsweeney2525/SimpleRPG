using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class ShieldBusterMove : SpecialMove
    {
        /// <summary>
        /// If a targetted Shield has greater ShieldBusterDefense, the shield will not be destroyed. Default shields and default SHieldBusters results in the shieldBuster winning
        /// </summary>
        public int ShieldBusterStrength { get; }

        public ShieldBusterMove(string description, TargetType targetType, string executionText, int shieldBusterStrength = 0) 
            : base(description, BattleMoveType.ShieldBuster, targetType, executionText)
        {
            ShieldBusterStrength = shieldBusterStrength;
        }

        public ShieldBusterMove(ShieldBusterMove copy) : base(copy)
        {
            ShieldBusterStrength = copy.ShieldBusterStrength;
        }
    }
}
