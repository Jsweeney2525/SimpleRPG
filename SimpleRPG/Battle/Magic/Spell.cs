using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Magic
{
    public class Spell : BattleMove
    {
        public int Cost { get; }

        public int Power { get; }

        public MagicType ElementalType { get; }

        public SpellType SpellType { get; protected set; }

        public Spell(string description, MagicType elementalType, SpellType spellType, TargetType targetType, int cost, int power) 
            : base(description, BattleMoveType.Spell, targetType)
        {
            ElementalType = elementalType;
            SpellType = spellType;
            Cost = cost;
            Power = power;
        }
    }
}
