using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class EnemyAttackCounteredEventArgs : EventArgs
    {
        public IFighter Counterer { get; private set; }

        public IFighter Enemy { get; private set; }

        public EnemyAttackCounteredEventArgs(IFighter counterer, IFighter enemy)
        {
            Counterer = counterer;
            Enemy = enemy;
        }
    }
}
