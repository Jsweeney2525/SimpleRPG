using System;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class DancerBoss: EnemyFighter
    {
        public DancerBoss(FighterClass fighterClass, int level, IChanceService chanceService) 
            : base("",
                  level,
                  LevelUpManager.GetHealthByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetManaByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetStrengthByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetDefenseByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetSpeedByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetEvadeByLevel<DancerBoss>(level, fighterClass),
                  LevelUpManager.GetLuckByLevel<DancerBoss>(level, fighterClass),
                  chanceService, 
                  SpellFactory.GetSpellsByLevel<DancerBoss>(level, fighterClass: fighterClass)
                  , MoveFactory.GetMovesByLevel<DancerBoss>(level, fighterClass: fighterClass))
        {
            if (fighterClass == FighterClass.BossDancerKiki)
            {
                BaseName = "Kiki";
            }
            else if (fighterClass == FighterClass.BossDancerRiki)
            {
                BaseName = "Riki";
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fighterClass), "DancerBoss class can only be initialized with Kiki or Riki fighter class!");
            }
        }
    }
}
