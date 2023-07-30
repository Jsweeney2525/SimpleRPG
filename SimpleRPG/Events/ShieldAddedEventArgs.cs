using System;
using SimpleRPG.Battle.BattleShields;

namespace SimpleRPG.Events
{
    public class ShieldAddedEventArgs : EventArgs
    {
        public BattleShield BattleShield { get; }

        public ShieldAddedEventArgs(BattleShield shield)
        {
            BattleShield = shield;
        }
    }
}
