using System;

namespace SimpleRPG.Events
{
    public class ExpGainedEventArgs : EventArgs
    {
        public int AmountGained { get; private set; }

        public ExpGainedEventArgs(int amountGained)
        {
            AmountGained = amountGained;
        }
    }
}
