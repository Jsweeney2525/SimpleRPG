using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class StatusFieldEffect : FieldEffect
    {
        public Status Status { get; }

        public StatusFieldEffect(TargetType targetType, string moveName, Status status)
            :base(targetType, moveName, null, false)
        {
            Status = status;
        }

        public override FieldEffect Copy()
        {
            return new StatusFieldEffect(TargetType, MoveName, Status.Copy());
        }

        public override bool AreEqual(FieldEffect effect)
        {
            bool areEqual = false;

            StatusFieldEffect statusFieldEffect = effect as StatusFieldEffect;

            if (statusFieldEffect != null)
            {
                areEqual = Status.AreEqual(statusFieldEffect.Status);
            }

            return areEqual;
        }
    }
}
