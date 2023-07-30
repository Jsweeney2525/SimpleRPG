using System;

namespace SimpleRPG.Events
{
    public class ShieldHealedEventArgs : EventArgs
    {
        public int HealedAmount { get; }

        public ShieldHealedEventArgs(int healedAmount)
        {
            HealedAmount = healedAmount;
        }
    }
}
