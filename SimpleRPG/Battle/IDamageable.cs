using System;
using SimpleRPG.Events;

namespace SimpleRPG.Battle
{
    public interface IDamageable
    {
        EventHandler<PhysicalDamageTakenEventArgs> DamageTaken { get; set; }

        void OnDamageTaken(PhysicalDamageTakenEventArgs e);

        EventHandler<MagicalDamageTakenEventArgs> MagicalDamageTaken { get; set; }

        void OnMagicalDamageTaken(MagicalDamageTakenEventArgs e);
    }
}
