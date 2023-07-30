using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Events
{
    public class StatRaisedEventArgs : EventArgs
    {
        public StatType RaisedStat { get; }

        public int BoostAmount { get; }

        public StatRaisedEventArgs(StatType raisedStat, int boostAmount)
        {
            RaisedStat = raisedStat;
            BoostAmount = boostAmount;
        }
    }
}
