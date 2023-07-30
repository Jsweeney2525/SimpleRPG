using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class SpecialMove: BattleMove
    {
        public SpecialMove(string description, BattleMoveType moveType, TargetType targetType, string executionText, int priority = 0, SpecialTargettingRuleCollection targettingRuleCollection = null)
            : base(description, moveType, targetType, priority, executionText, targettingRuleCollection)
        {
        }

        public SpecialMove(BattleMove copy) : base(copy)
        {
            
        }
    }
}
