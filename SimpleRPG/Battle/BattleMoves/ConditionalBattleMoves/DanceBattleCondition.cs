using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves
{
    public class DanceBattleCondition : BattleCondition
    {
        public DanceEffectType EffectType { get; set; }

        public DanceBattleCondition(DanceEffectType danceEffectType)
        {
            EffectType = danceEffectType;
        }
    }
}
