using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Statuses
{
    public class StatMultiplierStatus : MultiplierStatus
    {
        public StatType StatType { get; }

        public StatMultiplierStatus(
            int numberOfTurns,
            StatType statType, 
            double multiplier)
            :base(numberOfTurns, multiplier)
        {
            StatType = statType;
        }

        public override Status Copy()
        {
            return new StatMultiplierStatus(TurnCounter, StatType, Multiplier);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            StatMultiplierStatus statMultiplierStatus = status as StatMultiplierStatus;

            if (statMultiplierStatus != null)
            {
                areEqual = StatType == statMultiplierStatus.StatType &&
                           base.AreEqual(statMultiplierStatus);
            }

            return areEqual;
        }
    }
}
