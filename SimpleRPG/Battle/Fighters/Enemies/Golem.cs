using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Golem: EnemyFighter
    {
        public Golem(int level, IChanceService chanceService)
            : base("Golem",
                  level,
                  LevelUpManager.GetHealthByLevel<Golem>(level),
                  LevelUpManager.GetManaByLevel<Golem>(level),
                  LevelUpManager.GetStrengthByLevel<Golem>(level),
                  LevelUpManager.GetDefenseByLevel<Golem>(level),
                  LevelUpManager.GetSpeedByLevel<Golem>(level),
                  LevelUpManager.GetEvadeByLevel<Golem>(level),
                  LevelUpManager.GetLuckByLevel<Golem>(level),
                  chanceService,
                  SpellFactory.GetSpellsByLevel<Golem>(level)
                  ,MoveFactory.GetMovesByLevel<Golem>(level))
        {
        }
    }
}
