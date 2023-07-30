using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    public class UndoDebuffsFieldEffect : FieldEffect
    {
        //debuffs are removed only once. If an effect needs to prevent stat changes for X turns, make a new field effect for it
        public UndoDebuffsFieldEffect(
            TargetType targetType, 
            string moveName) 
            : base(targetType, moveName, 0, true)
        {
        }

        public override FieldEffect Copy()
        {
            return new UndoDebuffsFieldEffect(TargetType, MoveName);
        }

        public override bool AreEqual(FieldEffect effect)
        {
            return effect is UndoDebuffsFieldEffect;
        }
    }
}
