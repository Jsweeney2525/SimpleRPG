using System;

namespace SimpleRPG.Events
{
    public class ManaLostEventArgs : EventArgs
    {
        public int ManaSpent { get; private set; }

        public ManaLostEventArgs(int manaSpent)
        {
            ManaSpent = manaSpent;
        }
    }
}
