namespace SimpleRPG.Battle.Statuses
{
    public class SpellCostMultiplierStatus : MultiplierStatus
    {
        public SpellCostMultiplierStatus(int numberOfTurns, double multiplier)
            :base(numberOfTurns, multiplier)
        {
        }

        public override Status Copy()
        {
            return new SpellCostMultiplierStatus(TurnCounter, Multiplier);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            SpellCostMultiplierStatus costMultiplierStatus = status as SpellCostMultiplierStatus;

            if (costMultiplierStatus != null)
            {
                areEqual = base.AreEqual(costMultiplierStatus);
            }

            return areEqual;
        }
    }
}
