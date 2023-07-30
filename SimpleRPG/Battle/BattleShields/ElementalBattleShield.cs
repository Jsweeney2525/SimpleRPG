using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleShields
{
    public class ElementalBattleShield : BattleShield
    {
        public MagicType ElementalType { get; }

        public ElementalBattleShield(int health, int defense, int magicResistance, MagicType elementalType, int shieldBusterDefense = 0, string displayArticle = null, string displayName = null) 
            : base(health, defense, magicResistance, shieldBusterDefense,
                  displayArticle ?? (elementalType == MagicType.Ice || elementalType == MagicType.Earth ? "an" : "a"),
                  displayName ?? $"{elementalType.ToString().ToLower()} elemental battle shield")
        {
            ElementalType = elementalType;
        }

        public ElementalBattleShield(ElementalBattleShield shield)
            : base(shield)
        {
            ElementalType = shield.ElementalType;
        }

        public override IBattleShield Copy()
        {
            return new ElementalBattleShield(this);
        }
        
        public override int DecrementHealth(int damage, MagicType attackingType)
        {
            var relationship = MagicRelationshipCalculator.GetRelationship(attackingType, ElementalType);

            switch (relationship)
            {
                case MagicRelationshipType.Strong:
                    damage *= 2;
                    break;
                case MagicRelationshipType.Weak:
                    damage /= 2;
                    break;
            }

            return base.DecrementHealth(damage, attackingType);
        }

        public override bool AreEqual(IBattleShield shield)
        {
            var elementalShield = shield as ElementalBattleShield;

            return elementalShield != null
                   && base.AreEqual(shield)
                   && elementalShield.ElementalType == ElementalType;
        }
    }
}
