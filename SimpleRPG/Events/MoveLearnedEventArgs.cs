using System;
using SimpleRPG.Battle.BattleMoves;

namespace SimpleRPG.Events
{
    public class MoveLearnedEventArgs : EventArgs
    {
        public BattleMove Move { get; private set; }

        public MoveLearnedEventArgs(BattleMove move)
        {
            Move = move;
        }
    }
}
