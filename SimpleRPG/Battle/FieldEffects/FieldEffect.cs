using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public abstract class FieldEffect
    {
        public TargetType TargetType { get; }

        public string MoveName { get; protected set; }

        //note: for some FieldEffect types (e.g. StatusFieldEffect), this duration might not have much relevance
        public int? EffectDuration { get; protected set; }

        public bool IsImmediatelyExecuted { get; }

        protected FieldEffect(TargetType targetType, string moveName, int? effectDuration, bool immediatelyExecuted)
        {
            TargetType = targetType;
            MoveName = moveName;
            EffectDuration = immediatelyExecuted ? 0 : effectDuration;
            IsImmediatelyExecuted = immediatelyExecuted;
        }

        public void SetMoveName(string name)
        {
            MoveName = name;
        }

        public void SetEffectDuration(int effectDuration)
        {
            EffectDuration = effectDuration;
        }

        public abstract FieldEffect Copy();

        public abstract bool AreEqual(FieldEffect effect);
    }
}
