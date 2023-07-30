using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Ogre: EnemyFighter
    {
        public Ogre(int level, IChanceService chanceService)
            : base("Ogre",
                level,
                LevelUpManager.GetHealthByLevel<Ogre>(level),
                LevelUpManager.GetManaByLevel<Ogre>(level),
                LevelUpManager.GetStrengthByLevel<Ogre>(level),
                LevelUpManager.GetDefenseByLevel<Ogre>(level),
                LevelUpManager.GetSpeedByLevel<Ogre>(level),
                LevelUpManager.GetEvadeByLevel<Ogre>(level),
                LevelUpManager.GetLuckByLevel<Ogre>(level),
                chanceService,
                SpellFactory.GetSpellsByLevel<Ogre>(level)
                ,MoveFactory.GetMovesByLevel<Ogre>(level))
        {

        }
    }
}
