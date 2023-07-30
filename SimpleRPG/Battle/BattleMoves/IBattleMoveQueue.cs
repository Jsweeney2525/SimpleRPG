using System;
using System.Collections.Generic;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Battle.BattleMoves
{
    public interface IBattleMoveQueue
    {
        int Count { get; }

        void AddRange(List<BattleMoveWithTarget> moves);

        BattleMoveWithTarget Pop();

        void Sort(Func<IFighter, int> calculateEffectiveSpeedFunc = null);

        BattleMoveWithTarget SortAndPop(Func<IFighter, int> calculateEffectiveSpeedFunc = null);
    }
}
