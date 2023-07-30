using SimpleRPG.Battle.BattleMoves;

namespace SimpleRPG.Battle
{
    public interface IMoveExecutor
    {
        //TODO: should return void
        bool ExecuteMove(BattleMoveWithTarget battleMoveWithTarget);
    }
}
