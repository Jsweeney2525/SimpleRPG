namespace SimpleRPG.Battle.Statuses
{
    //TODO: make sure the MenuManager appropriately communicates the player cannot cast magic if they have a sealed status
    public class MagicSealedStatus : Status
    {
        public MagicSealedStatus(int numberOfTurns) : base(numberOfTurns, false)
        {
        }

        public override Status Copy()
        {
            return new MagicSealedStatus(TurnCounter);
        }

        public override bool AreEqual(Status status)
        {
            return status is MagicSealedStatus;
        }
    }
}
