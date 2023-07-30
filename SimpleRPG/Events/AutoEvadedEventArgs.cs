using SimpleRPG.Battle.Fighters;
using System;

namespace SimpleRPG.Events
{
    public class AutoEvadedEventArgs : EventArgs
    {
        public IFighter Evader { get; private set; }

        public bool AlsoCountered { get; private set; }

        public AutoEvadedEventArgs(IFighter evader, bool alsoCountered)
        {
            Evader = evader;
            AlsoCountered = alsoCountered;
        }
    }
}
