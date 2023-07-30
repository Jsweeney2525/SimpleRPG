namespace SimpleRPG.Battle.Statuses
{
    public abstract class Status
    {
        public int TurnCounter { get; protected set; }

        public bool IsTurnEndStatus { get; protected set; }

        protected Status(int numberOfTurns, bool isTurnEndStatus)
        {
            TurnCounter = numberOfTurns;
            IsTurnEndStatus = isTurnEndStatus;
        }

        public abstract bool AreEqual(Status status);

        public abstract Status Copy();

        public void DecrementTurnCounter()
        {
            --TurnCounter;
        }
    }
}
