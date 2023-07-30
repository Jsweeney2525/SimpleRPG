namespace SimpleRPG.Battle.Statuses
{
    //note: may want to add some kind of "attack multiplier" in case some counter attacks should be stronger than their normal attacks
    public class CounterAttackStatus : Status
    {
        public CounterAttackStatus(int numberOfTurns)
            : base(numberOfTurns, false)
        {
        }

        public override Status Copy()
        {
            return new CounterAttackStatus(TurnCounter);
        }

        public override bool AreEqual(Status status)
        {
            CounterAttackStatus evadeStatus = status as CounterAttackStatus;

            var areEqual = evadeStatus != null;

            return areEqual;
        }
    }
}
