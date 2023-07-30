using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.FieldEffects
{
    //TODO: consider what happens if Dante already has a fire shield and then a special move is supposed to equip him with a lightning shield.
    public class ShieldFieldEffect : FieldEffect
    {
        public IBattleShield BattleShield { get; protected set; }

        //Shield effect is always immediately executed. 
        //If I want to ensure the shields are destroyed after X turns, that is a property of the shield, not the effect
        public ShieldFieldEffect(
            TargetType targetType, 
            string moveName, 
            IBattleShield battleShield) 
            : base(targetType, moveName, 0, true)
        {
            BattleShield = battleShield;
        }

        public override FieldEffect Copy()
        {
            return new ShieldFieldEffect(TargetType, MoveName, BattleShield.Copy());
        }

        public override bool AreEqual(FieldEffect effect)
        {
            var shieldEffect = effect as ShieldFieldEffect;

            return shieldEffect != null
                   && shieldEffect.BattleShield.AreEqual(BattleShield);
        }
    }
}
