using System;

namespace SimpleRPG.Battle.Statuses
{
    public abstract class MultiplierStatus : Status
    {
        public double Multiplier { get; }

        protected MultiplierStatus(
            int numberOfTurns, 
            double multiplier)
            :base(numberOfTurns, false)
        {
            Multiplier = multiplier;
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            MultiplierStatus multiplierStatus = status as MultiplierStatus;

            if (multiplierStatus != null)
            {
                areEqual = AreEqual(multiplierStatus);
            }

            return areEqual;
        }

        public bool AreEqual(MultiplierStatus status)
        {
            bool areEqual = Math.Abs(Multiplier - status.Multiplier) < .01;

            return areEqual;
        }
    }
}
