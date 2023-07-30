using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Statuses
{
    public class MagicMultiplierStatus : MultiplierStatus
    {
        public MagicType MagicType { get; }

        public MagicMultiplierStatus(
            int numberOfTurns,
            MagicType magicType, 
            double multiplier)
            : base(numberOfTurns, multiplier)
        {
            MagicType = magicType;
        }

        public override Status Copy()
        {
            return new MagicMultiplierStatus(TurnCounter, MagicType, Multiplier);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            MagicMultiplierStatus magicMultiplierStatus = status as MagicMultiplierStatus;

            if (magicMultiplierStatus != null)
            {
                areEqual = MagicType == magicMultiplierStatus.MagicType && base.AreEqual(magicMultiplierStatus);
            }

            return areEqual;
        }
    }
}
