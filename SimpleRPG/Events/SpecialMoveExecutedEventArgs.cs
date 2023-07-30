using SimpleRPG.Battle.Fighters;
using System;
using SimpleRPG.Battle.BattleMoves;

namespace SimpleRPG.Events
{
    public class SpecialMoveExecutedEventArgs : EventArgs
    {
        public SpecialMove Move { get; private set; }

        public IFighter Executor { get; private set; }

        public IFighter MoveTarget { get; private set; }

        public SpecialMoveExecutedEventArgs(IFighter executor, IFighter target, SpecialMove move)
        {
            Executor = executor;
            MoveTarget = target;
            Move = move;
        }
    }
}
