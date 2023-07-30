using System;

namespace SimpleRPG.Events
{
    public class ShieldFortifiedEventArgs : EventArgs
    {
        public int FortifyAmount { get; }

        public ShieldFortifiedEventArgs(int fortifyAmount)
        {
            FortifyAmount = fortifyAmount;
        }
    }
}
