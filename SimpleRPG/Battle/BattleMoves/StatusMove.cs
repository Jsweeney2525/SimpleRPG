using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    /// <summary>
    /// A battle move that assigns a status to the target
    /// </summary>
    public class StatusMove: SpecialMove
    {
        public Status Status { get; protected set; }

        public int Accuracy { get; }

        public StatusMove(string description, TargetType targetType, Status status, string executionText = null, int priority = 0, int accuracy = 100)
            : base(description, BattleMoveType.Status, targetType, executionText, priority)
        {
            Status = status.Copy();
            Accuracy = accuracy;
        }
    }
}
