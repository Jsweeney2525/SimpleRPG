using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class ShieldGuy : EnemyFighter
    {
        private readonly BattleMove _basicAttack;

        private readonly ShieldMove _ironShieldMove;

        private readonly ShieldFortifyingMove _healShield;

        private readonly ShieldFortifyingMove _fortifyShield;

        public ShieldGuy(int level, IChanceService chanceService, string name = null) :
            base(name ?? "Shield Guy"
                , level
                , LevelUpManager.GetHealthByLevel<ShieldGuy>(level)
                , LevelUpManager.GetManaByLevel<ShieldGuy>(level)
                , LevelUpManager.GetStrengthByLevel<ShieldGuy>(level)
                , LevelUpManager.GetDefenseByLevel<ShieldGuy>(level)
                , LevelUpManager.GetSpeedByLevel<ShieldGuy>(level)
                , LevelUpManager.GetEvadeByLevel<ShieldGuy>(level)
                , LevelUpManager.GetLuckByLevel<ShieldGuy>(level)
                , chanceService
                , SpellFactory.GetSpellsByLevel<ShieldGuy>(level)
                , MoveFactory.GetMovesByLevel<ShieldGuy>(level))
        {
            _basicAttack = AvailableMoves.FirstOrDefault(bm => bm.MoveType == BattleMoveType.Attack);
            _ironShieldMove = AvailableMoves.FirstOrDefault(bm => bm is ShieldMove) as ShieldMove;
            _healShield = AvailableMoves.FirstOrDefault(bm => bm is ShieldFortifyingMove && ((ShieldFortifyingMove)bm).FortifyingType == ShieldFortifyingType.Health) as ShieldFortifyingMove;
            _fortifyShield = AvailableMoves.FirstOrDefault(bm => bm is ShieldFortifyingMove && ((ShieldFortifyingMove)bm).FortifyingType == ShieldFortifyingType.Defense) as ShieldFortifyingMove;
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            List<IFighter> targetableAllies = ownTeam.Fighters.Where(f => f.IsAlive()).ToList();

            int totalNumberOfAllies = targetableAllies.Count;
            int numberOfAlliesWithShields = CountAlliesWithShields(targetableAllies);
            int numberOfAlliesWithShieldsMissingHealth = CountAlliesWithShieldsMissingHealth(targetableAllies);

            List<BattleMove> movesToSelect = new List<BattleMove> { _basicAttack };

            List<double> chances = new List<double> { 0.2 };

            if (numberOfAlliesWithShields < totalNumberOfAllies)
            {
                movesToSelect.Add(_ironShieldMove);
            }

            if (numberOfAlliesWithShields > 0)
            {
                movesToSelect.Add(_fortifyShield);
            }

            if (numberOfAlliesWithShieldsMissingHealth > 0)
            {
                movesToSelect.Add(_healShield);
            }

            switch (movesToSelect.Count)
            {
                case 4: //all four moves
                    chances.Add(.4); //Iron Shield
                    chances.Add(.2); //fortify
                    chances.Add(.2); //heal
                    break;
                case 3:
                    if (movesToSelect.Contains(_ironShieldMove))
                    {
                        chances.Add(.6); //iron shield
                        chances.Add(.2); //whatever the other move is
                    }
                    else
                    {
                        chances.Add(.4);
                        chances.Add(.4);
                    }
                    break;
                case 2:
                    chances.Add(.8);
                    break;
            }

            int selectedIndex = ChanceService.WhichEventOccurs(chances.ToArray());

            return movesToSelect[selectedIndex];
        }

        protected override IFighter _selectTarget(BattleMove move, Team ownTeam, Team enemyTeam)
        {
            IEnumerable<IFighter> viableTargets;
            IFighter ret;

            if (move == _ironShieldMove)
            {
                viableTargets = GetAlliesWithoutShield(ownTeam);
                ret = _selectTarget(viableTargets.ToList());
            }
            else if (move == _fortifyShield)
            {
                viableTargets = GetAlliesWithShield(ownTeam);
                ret = _selectTarget(viableTargets.ToList());
            }
            else if (move == _healShield)
            {
                viableTargets = GetAlliesWithDamagedShield(ownTeam);
                ret = _selectTarget(viableTargets.ToList());
            }
            else
            {
                ret = base._selectTarget(move, ownTeam, enemyTeam);
            }

            return ret;
        }

        private IEnumerable<IFighter> GetAlliesWithoutShield(Team ownTeam)
        {
            return ownTeam.Fighters.Where(f => f.BattleShield == null);
        }

        private IEnumerable<IFighter> GetAlliesWithShield(Team ownTeam)
        {
            return ownTeam.Fighters.Where(f => f.BattleShield != null);
        }

        private IEnumerable<IFighter> GetAlliesWithDamagedShield(Team ownTeam)
        {
            return ownTeam.Fighters.Where(f => f.BattleShield != null && f.BattleShield.CurrentHealth < f.BattleShield.MaxHealth);
        }

        private int CountAlliesWithShields(List<IFighter> allies)
        {
            return allies.Count(f => f.BattleShield != null);
        }

        private int CountAlliesWithShieldsMissingHealth(List<IFighter> allies)
        {
            return allies.Count(f => f.BattleShield != null && f.BattleShield.CurrentHealth < f.BattleShield.MaxHealth);
        }
    }
}
