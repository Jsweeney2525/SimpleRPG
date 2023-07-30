namespace SimpleRPG.Battle.BattleShields
{
    public class IronBattleShield : BattleShield
    {
        public IronBattleShield(int health, int defense, int magicResistance, int shieldBusterDefense = 0, string displayArticle = null, string displayName = null) 
            : base(health, defense, magicResistance, shieldBusterDefense, displayArticle ?? "an", displayName ?? "iron battle shield")
        {
        }

        public IronBattleShield(BattleShield copy) : base(copy)
        {
        }

        public override IBattleShield Copy()
        {
            return new IronBattleShield(this);
        }
    }
}
