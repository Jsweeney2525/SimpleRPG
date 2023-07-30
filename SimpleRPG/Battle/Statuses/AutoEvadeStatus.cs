namespace SimpleRPG.Battle.Statuses
{
    public class AutoEvadeStatus : Status
    {
        public bool ShouldCounterAttack { get; }

        public AutoEvadeStatus(int numberOfTurns, bool shouldCounterAttack)
            : base(numberOfTurns, false)
        {
            ShouldCounterAttack = shouldCounterAttack;
        }

        public override Status Copy()
        {
            return new AutoEvadeStatus(TurnCounter, ShouldCounterAttack);
        }

        public override bool AreEqual(Status status)
        {
            AutoEvadeStatus evadeStatus = status as AutoEvadeStatus;

            bool areEqual = evadeStatus != null && evadeStatus.ShouldCounterAttack == ShouldCounterAttack;

            return areEqual;
        }
    }
}
