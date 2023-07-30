using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses.BattleMoves
{
    public class TestMultiTurnMove: MultiTurnBattleMove
    {
        public TestMultiTurnMove(string description, TargetType targetType, params BattleMove[] moves)
            : base(description, targetType, moves)
        {
        }

        public void AddMove(BattleMove move)
        {
            Moves.Add(move);
        }
    }
}
