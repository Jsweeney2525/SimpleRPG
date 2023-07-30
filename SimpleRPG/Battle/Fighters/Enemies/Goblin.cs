using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Goblin: EnemyFighter
    {
        public Goblin(int level, IChanceService chanceService) 
            : base("Goblin", 
                  level, 
                  LevelUpManager.GetHealthByLevel<Goblin>(level), 
                  LevelUpManager.GetManaByLevel<Goblin>(level),
                  LevelUpManager.GetStrengthByLevel<Goblin>(level),
                  LevelUpManager.GetDefenseByLevel<Goblin>(level),
                  LevelUpManager.GetSpeedByLevel<Goblin>(level),
                  LevelUpManager.GetEvadeByLevel<Goblin>(level),
                  LevelUpManager.GetLuckByLevel<Goblin>(level),
                  chanceService, 
                  SpellFactory.GetSpellsByLevel<Goblin>(level)
                  , MoveFactory.GetMovesByLevel<Goblin>(level))
        {
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            BattleMove ret;

            if (ChanceService.EventOccurs(0.75))
            {
                ret = _availableMoves.Single(m => m.Description == "goblin punch");
            }
            else
            {
                ret = _availableMoves.Single(m => m.Description == "attack");
            }

            return ret;
        }
    }
}
