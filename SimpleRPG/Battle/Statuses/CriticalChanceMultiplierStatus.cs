namespace SimpleRPG.Battle.Statuses
{
    public class CriticalChanceMultiplierStatus : MultiplierStatus
    {
        public CriticalChanceMultiplierStatus(
            int numberOfTurns,
            double multiplier)
            : base(numberOfTurns, multiplier)
        {
        }

        public override Status Copy()
        {
            return new CriticalChanceMultiplierStatus(TurnCounter, Multiplier);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            CriticalChanceMultiplierStatus criticalChanceMultiplierStatus = status as CriticalChanceMultiplierStatus;

            if (criticalChanceMultiplierStatus != null)
            {
                areEqual = base.AreEqual(criticalChanceMultiplierStatus);
            }

            return areEqual;
        }
    }
}
