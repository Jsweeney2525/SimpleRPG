namespace SimpleRPG.Battle.Statuses
{
    public class UndoDebuffsStatus : Status
    {
        public UndoDebuffsStatus(int numberOfTurns)
            : base(numberOfTurns, false)
        {
        }

        public override bool AreEqual(Status status)
        {
            return status is UndoDebuffsStatus;
        }

        public override Status Copy()
        {
            return new UndoDebuffsStatus(TurnCounter);
        }
    }
}
