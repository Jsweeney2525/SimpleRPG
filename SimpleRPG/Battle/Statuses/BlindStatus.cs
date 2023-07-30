namespace SimpleRPG.Battle.Statuses
{
    public class BlindStatus : Status
    {
        public BlindStatus(int numberOfTurns) : base(numberOfTurns, false)
        {
        }

        public override bool AreEqual(Status status)
        {
            return status is BlindStatus;
        }

        public override Status Copy()
        {
            return new BlindStatus(TurnCounter);
        }
    }
}
