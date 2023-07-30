using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Statuses
{
    public class RestorePercentageStatus : Status
    {
        public double Percentage { get; }

        public RestorationType RestorationType { get; }

        public RestorePercentageStatus(
            int numberOfTurns,
            RestorationType restorationType,
            double percentage)
            : base(numberOfTurns, true)
        {
            RestorationType = restorationType;
            Percentage = percentage;
        }

        public override Status Copy()
        {
            return new RestorePercentageStatus(TurnCounter, RestorationType, Percentage);
        }

        public override bool AreEqual(Status status)
        {
            bool areEqual = false;

            RestorePercentageStatus restoreStatus = status as RestorePercentageStatus;

            if (restoreStatus != null)
            {
                areEqual = Math.Abs(restoreStatus.Percentage - Percentage) < .01;
            }

            return areEqual;
        }
    }
}
