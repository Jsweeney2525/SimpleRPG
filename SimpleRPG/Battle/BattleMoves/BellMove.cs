using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class BellMove : BattleMove
    {
        public BellMoveType BellMoveType { get; }

        public BellMove(string description,
            BellMoveType bellMoveType,
            TargetType targetType,
            int priority = 0,
            string executionText = null,
            params BattleMoveEffect[] effects)
            : base(description, BattleMoveType.BellMove, targetType, priority, executionText, effects: effects)
        {
            BellMoveType = bellMoveType;
        }
    }
}
