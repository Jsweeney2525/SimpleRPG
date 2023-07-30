using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Events;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Zombie: EnemyFighter
    {
        protected int ReviveCounter;

        public Zombie(int level, IChanceService chanceService) 
            : base("Zombie", 
                  level, 
                  LevelUpManager.GetHealthByLevel<Zombie>(level), 
                  LevelUpManager.GetManaByLevel<Zombie>(level),
                  LevelUpManager.GetStrengthByLevel<Zombie>(level), 
                  LevelUpManager.GetDefenseByLevel<Zombie>(level),
                  LevelUpManager.GetSpeedByLevel<Zombie>(level),
                  LevelUpManager.GetEvadeByLevel<Zombie>(level),
                  LevelUpManager.GetLuckByLevel<Zombie>(level),
                  chanceService, 
                  SpellFactory.GetSpellsByLevel<Zombie>(level)
                  ,MoveFactory.GetMovesByLevel<Zombie>(level))
        {
            ReviveCounter = 0;
            Killed = _setReviveCounter;
            TurnEnded += _turnEnded;
        }

        private void _turnEnded(object sender, TurnEndedEventArgs e)
        {
            if (ReviveCounter > 0)
            {
                --ReviveCounter;

                if (ReviveCounter == 0)
                {
                    Revive(MaxHealth / 2);
                }
            }
        }

        private void _setReviveCounter(object sender, KilledEventArgs e)
        {
            ReviveCounter = ChanceService.WhichEventOccurs(3) + 2;
        }
    }
}