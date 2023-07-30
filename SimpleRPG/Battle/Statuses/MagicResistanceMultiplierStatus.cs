using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Statuses
{
    public class MagicResistanceMultiplierStatus : MultiplierStatus
    {
        public MagicType MagicType { get; }

        public MagicResistanceMultiplierStatus(
            int numberOfTurns,
            MagicType magicType, 
            double multiplier)
            : base(numberOfTurns, multiplier)
        {
            MagicType = magicType;
        }

        public override Status Copy()
        {
            return new MagicResistanceMultiplierStatus(TurnCounter, MagicType, Multiplier);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            MagicResistanceMultiplierStatus resistanceMultiplierStatus = status as MagicResistanceMultiplierStatus;

            if (resistanceMultiplierStatus != null)
            {
                areEqual = MagicType == resistanceMultiplierStatus.MagicType && base.AreEqual(resistanceMultiplierStatus);
            }

            return areEqual;
        }
    }
}
