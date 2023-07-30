using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class EnemyKilledEventArgs : EventArgs
    {
        public IFighter Enemy { get; private set; }

        public EnemyKilledEventArgs(IFighter enemy)
        {
            Enemy = enemy;
        }
    }
}
