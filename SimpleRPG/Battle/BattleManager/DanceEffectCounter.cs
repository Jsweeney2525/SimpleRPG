using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleManager
{
    public class DanceEffectCounter : TurnCounter
    {
        public DanceEffectType Effect { get; }

        public IFighter Owner { get; }

        public DanceEffectCounter(IFighter owner, int turns, DanceEffectType effect)
            : base(turns)
        {
            Owner = owner;
            Effect = effect;
        }
    }
}
