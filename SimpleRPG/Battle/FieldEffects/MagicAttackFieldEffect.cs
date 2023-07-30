using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class MagicAttackFieldEffect : FieldEffect, IHasOwnerFieldEffect
    {
        public MagicType MagicType { get; }

        public int Power { get; }

        public IFighter Owner { get; protected set; }

        public MagicAttackFieldEffect(
            TargetType targetType, 
            string moveName, 
            MagicType magicType, 
            int power, 
            int? effectDuration = null,
            bool immediatelyExecuted = false) 
            : base(targetType, moveName, effectDuration, immediatelyExecuted)
        {
            MagicType = magicType;
            Power = power;
        }

        public void SetOwner(IFighter owner)
        {
            Owner = owner;
        }

        public override FieldEffect Copy()
        {
            return new MagicAttackFieldEffect(TargetType, MoveName, MagicType, Power, EffectDuration, IsImmediatelyExecuted);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var attackEffect = effect as MagicAttackFieldEffect;

            var ret = (attackEffect != null 
                && attackEffect.MagicType == MagicType 
                && attackEffect.Power == Power 
                && attackEffect.Owner == Owner);

            return ret;
        }
    }
}
