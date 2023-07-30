using System;
using SimpleRPG.Battle.Fighters.Enemies;

namespace SimpleRPG.Events
{
    public class ShadeAbsorbedEventArgs : EventArgs
    {
        public Shade AbsorbedShade { get; }

        public ShadeAbsorbedEventArgs(Shade absorbedShade)
        {
            AbsorbedShade = absorbedShade;
        }
    }
}
