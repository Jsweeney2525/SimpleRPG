using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Statuses
{
    public class ReflectStatus : Status
    {
        public MagicType MagicType { get; }

        /// <summary>
        /// optionally alters the power of the spell being reflected by this factor
        /// </summary>
        public double MultiplierBonus { get; }

        public ReflectStatus(int numberOfTurns, MagicType magicType, double multiplierBonus = 1.0)
            : base(numberOfTurns, false)
        {
            MagicType = magicType;
            MultiplierBonus = multiplierBonus;
        }

        public override Status Copy()
        {
            return new ReflectStatus(TurnCounter, MagicType, MultiplierBonus);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            ReflectStatus reflectStatus = status as ReflectStatus;

            if (reflectStatus != null)
            {
                areEqual = MagicType == reflectStatus.MagicType &&
                           Math.Abs(MultiplierBonus - reflectStatus.MultiplierBonus) < .01;
            }

            return areEqual;
        }
    }
}
