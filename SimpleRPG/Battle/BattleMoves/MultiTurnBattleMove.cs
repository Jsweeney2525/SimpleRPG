using System.Collections.Generic;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    //TODO: need to test when a user is using a multi turn move, then killed, the multi turn move is cancelled.
    public class MultiTurnBattleMove: SpecialMove
    {
        public readonly List<BattleMove> Moves;

        public MultiTurnBattleMove(string description, TargetType targetType, params BattleMove[] moves)
            : base(description, BattleMoveType.MultiTurn, targetType, null)
        {
            Moves = new List<BattleMove>();
            Moves.AddRange(moves);
        }

        public MultiTurnBattleMove(BattleMove copy) : base(copy)
        {
            
        }
    }
}
