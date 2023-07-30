using System;

namespace SimpleRPG.Events
{
    public class PhysicalDamageTakenEventArgs : EventArgs
    {
        public int Damage { get; private set; }

        public PhysicalDamageTakenEventArgs(int damage)
        {
            Damage = damage;
        }
    }
}
