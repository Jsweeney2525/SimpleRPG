using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Fairy: EnemyFighter
    {
        public Fairy(int level, IChanceService chanceService) 
            : base("Fairy", 
                  level, 
                  LevelUpManager.GetHealthByLevel<Fairy>(level), 
                  LevelUpManager.GetManaByLevel<Fairy>(level),
                  LevelUpManager.GetStrengthByLevel<Fairy>(level), 
                  LevelUpManager.GetDefenseByLevel<Fairy>(level),
                  LevelUpManager.GetSpeedByLevel<Fairy>(level),
                  LevelUpManager.GetEvadeByLevel<Fairy>(level),
                  LevelUpManager.GetLuckByLevel<Fairy>(level),
                  chanceService, 
                  SpellFactory.GetSpellsByLevel<Fairy>(level)
                  ,MoveFactory.GetMovesByLevel<Fairy>(level))
        {

        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            var allSpells = _availableMoves.Where(m => m is Spell).Cast<Spell>();
            var availableAttackSpells = allSpells.Where(s => s.TargetType == TargetType.SingleEnemy && s.Cost <= CurrentMana).ToList();
            BattleMove move;

            switch (availableAttackSpells.Count)
            {
                case 0:
                    move = SelectManaRestoreMove();
                    break;
                case 1:
                    move = availableAttackSpells[0];
                    
                    break;
                default:
                    //TODO: this logic will need to be updated someday
                    move = availableAttackSpells[0];
                    break;
            }

            return move;
        }

        private BattleMove SelectManaRestoreMove()
        {
            var allSpells = _availableMoves.Where(m => m is Spell).Cast<Spell>();
            var healingSpells = allSpells.Where(s => s.SpellType == SpellType.RestoreMana).ToList();

            //TODO: this logic will need to be updated someday
            return healingSpells[0];
        }
    }
}