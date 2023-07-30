using System;

namespace SimpleRPG.Events
{
    public class FighterHealedEventArgs : EventArgs
    {
        public int HealAmount { get; }

        public bool IsFullHeal { get; }

        public FighterHealedEventArgs(int healAmount, bool isFullHeal)
        {
            HealAmount = healAmount;
            IsFullHeal = isFullHeal;
        }
    }
}
