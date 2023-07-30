using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class ReflectFieldEffect : FieldEffect
    {
        public MagicType MagicType { get; }
        
        //TODO: set up way to have reflect status stay indefinitely
        public ReflectFieldEffect(
            TargetType targetType, 
            string moveName, 
            MagicType magicType, 
            int? effectDuration = null) 
            : base(targetType, moveName, effectDuration, false)
        {
            MagicType = magicType;
        }

        public override FieldEffect Copy()
        {
            return new ReflectFieldEffect(TargetType, MoveName, MagicType, EffectDuration);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var reflectEffect = effect as ReflectFieldEffect;

            return reflectEffect != null
                   && reflectEffect.MagicType == MagicType;
        }
    }
}
