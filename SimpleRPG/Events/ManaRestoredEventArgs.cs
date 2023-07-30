using System;

namespace SimpleRPG.Events
{
    public class ManaRestoredEventArgs : EventArgs
    {
        public int ManaRestored { get; private set; }

        public ManaRestoredEventArgs(int manaRestored)
        {
            ManaRestored = manaRestored;
        }
    }
}
