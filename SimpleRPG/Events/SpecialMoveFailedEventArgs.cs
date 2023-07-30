using SimpleRPG.Battle.Fighters;
using System;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Events
{
    public class SpecialMoveFailedEventArgs : EventArgs
    {
        public SpecialMove Move { get; }

        public IFighter Executor { get; }

        public IFighter MoveTarget { get; }

        public SpecialMoveFailedReasonType Reason { get; }

        public SpecialMoveFailedEventArgs(IFighter executor, IFighter target, SpecialMove move, SpecialMoveFailedReasonType reason)
        {
            Executor = executor;
            MoveTarget = target;
            Move = move;
            Reason = reason;
        }
    }
}
