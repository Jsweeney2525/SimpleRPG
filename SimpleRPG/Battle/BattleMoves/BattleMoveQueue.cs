using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Battle.BattleMoves
{
    public class BattleMoveQueue : IBattleMoveQueue
    {
        private List<BattleMoveWithTarget> _battleMoves;

        public int Count => _battleMoves.Count;

        public BattleMoveQueue(List<BattleMoveWithTarget> battleMoves)
        {
            _battleMoves = new List<BattleMoveWithTarget>(battleMoves);
        }

        public void AddRange(List<BattleMoveWithTarget> moves)
        {
            _battleMoves.AddRange(moves);
        }

        public BattleMoveWithTarget Pop()
        {
            BattleMoveWithTarget ret = _battleMoves[0];

            _battleMoves.Remove(ret);

            return ret;
        }

        private static readonly Func<IFighter, int> DefaultCalculateEffectiveSpeedFunc = f => f.Speed;

        private static readonly Func<BattleMoveWithTarget, Func<IFighter, int>, int> SortBattleMovesFunc =
            (bm, speedFunc) => bm.Move.Priority*100000 + speedFunc(bm.Owner);

        public void Sort(Func<IFighter, int> calculateEffectiveSpeedFunc = null)
        {
            if (calculateEffectiveSpeedFunc == null)
            {
                calculateEffectiveSpeedFunc = DefaultCalculateEffectiveSpeedFunc;
            }

            _battleMoves = _battleMoves.OrderByDescending(bm => SortBattleMovesFunc(bm, calculateEffectiveSpeedFunc)).ToList();
        }

        public BattleMoveWithTarget SortAndPop(Func<IFighter, int> calculateEffectiveSpeedFunc = null)
        {
            Sort(calculateEffectiveSpeedFunc);
            return Pop();
        }
    }
}
