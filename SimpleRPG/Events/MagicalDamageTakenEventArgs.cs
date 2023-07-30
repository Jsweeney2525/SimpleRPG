using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Events
{
    public class MagicalDamageTakenEventArgs : EventArgs
    {
        public int Damage { get; private set; }

        public MagicType MagicType { get; private set; }

        public MagicalDamageTakenEventArgs(int damage, MagicType magicType)
        {
            Damage = damage;
            MagicType = magicType;
        }
    }
}
