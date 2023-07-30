using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public abstract class EnemyFighter : Fighter
    {
        protected readonly IChanceService ChanceService;

        public int ExpGivenOnDefeat { get; protected set; }

        protected List<BattleMove> _availableMoves;

        public List<BattleMove> AvailableMoves => _availableMoves;

        protected EnemyFighter(string name
            ,int level
            ,int health
            ,int mana
            ,int strength
            ,int defense
            ,int speed
            ,int evade
            ,int luck
            ,IChanceService chanceService
            ,List<Spell> spells = null
            ,List<BattleMove> specialMoves = null) 
            : base(name, level, health, mana, strength, defense, speed, evade, luck, spells, specialMoves)
        {
            ChanceService = chanceService;
            ExpGivenOnDefeat = 5 * level;

            _availableMoves = new List<BattleMove>
            {
                MoveFactory.Get(BattleMoveType.Attack)
            };

            if (spells != null)
            {
                _availableMoves.AddRange(spells);
            }

            if (specialMoves != null)
            {
                _availableMoves.AddRange(specialMoves);
            }
        }

        protected MultiTurnBattleMove _multiMove;
        protected int _multiMoveIndex;

        protected BattleMove BeforeSelectMove()
        {
            BattleMove ret = null;

            if (_multiMove != null)
            {
                ret = _multiMove.Moves[_multiMoveIndex++];

                if (_multiMoveIndex >= _multiMove.Moves.Count)
                {
                    _multiMove = null;
                    _multiMoveIndex = 0;
                }
            }

            return ret;
        }

        protected BattleMove AfterSelectMove(BattleMove move)
        {
            var multiMove = move as MultiTurnBattleMove;
            if (multiMove != null)
            {
                _multiMove = multiMove;
                _multiMoveIndex = 1;
            }
            
            return move;
        }

        /// <summary>
        /// Custom logic goes in here to determine which moves are usable. 
        /// For example, if the fighter does not have enough MP for a spell, it will not be included in the returned list
        /// </summary>
        /// <param name="enemyTeam">Sometimes used to determine if a move is executable (e.g. AI only uses status move if there are enemies that don't have it)</param>
        /// <returns></returns>
        public virtual List<BattleMove> GetExecutableMoves(Team enemyTeam)
        {
            return AvailableMoves;
        }

        /// <summary>
        /// Determines which move will be used for the current turn
        /// </summary>
        /// <param name="ownTeam"></param>
        /// <param name="enemyTeam"></param>
        /// <returns></returns>
        public virtual BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            var index = 0;

            List<BattleMove> executableMoves = GetExecutableMoves(enemyTeam);

            if (executableMoves.Count > 1)
            {
                index = ChanceService.WhichEventOccurs(executableMoves.Count);
            }

            var move = executableMoves[index];

            return move;
        }

        /// <summary>
        /// Determines which move will be used and who is the target
        /// </summary>
        /// <param name="ownTeam"></param>
        /// <param name="enemyTeam"></param>
        /// <returns></returns>
        public virtual BattleMoveWithTarget SetupMove(Team ownTeam, Team enemyTeam)
        {
            var move = BeforeSelectMove() ?? SelectMove(ownTeam, enemyTeam);

            move = AfterSelectMove(move);

            var target = _selectTarget(move, ownTeam, enemyTeam);

            return new BattleMoveWithTarget(move, target, this);
        }

        public virtual void ExecuteMove(BattleManager.BattleManager battleManager, BattleMoveWithTarget move, Team ownTeam, Team enemyTeam, IOutput output)
        {
        }

        protected virtual IFighter _selectTarget(BattleMove move, Team ownTeam, Team enemyTeam)
        {
            IFighter ret;
            TargetType targetType = move.TargetType;

            if (targetType == TargetType.Self || targetType == TargetType.Field)
            {
                ret = this;
            }
            else
            {
                List<IFighter> fighters = new List<IFighter>();

                switch (targetType)
                {
                    case TargetType.SingleEnemy:
                        fighters = enemyTeam.Fighters.Where(f => f.IsAlive()).ToList();
                        break;
                    case TargetType.SingleAlly:
                        fighters = Team.Fighters.Where(f => f.IsAlive() && f != this).ToList();
                        break;
                    case TargetType.SingleAllyOrSelf:
                        fighters = Team.Fighters.Where(f => f.IsAlive()).ToList();
                        break;
                }

                fighters = move.GetAvailableTargets(fighters).ToList();
                ret = _selectTarget(fighters);
            }

            return ret;
        }

        /// <summary>
        /// If a derived class uses custom logic to determine the pool of targets, call this method
        /// </summary>
        /// <param name="viableTargets"></param>
        /// <returns></returns>
        protected virtual IFighter _selectTarget(List<IFighter> viableTargets)
        {
            IFighter ret;

            if (viableTargets.Count == 0)
            {
                //TODO: should throw exception
            }
            if (viableTargets.Count == 1)
            {
                ret = viableTargets[0];
            }
            else
            {
                var index = ChanceService.WhichEventOccurs(viableTargets.Count);

                ret = viableTargets[index];
            }

            return ret;
        }

        protected virtual IEnumerable<IFighter> _getViableTargets(BattleMove move, Team enemyTeam)
        {
            IEnumerable<IFighter> viableTargets;

            switch (move.TargetType)
            {
                case TargetType.EnemyTeam:
                case TargetType.SingleEnemy:
                    viableTargets = enemyTeam.Fighters.Where(f => f.IsAlive()).ToList();
                    break;
                case TargetType.SingleAlly:
                    viableTargets = Team.Fighters.Where(f => f.IsAlive() && f != this).ToList();
                    break;
                case TargetType.SingleAllyOrSelf:
                case TargetType.OwnTeam:
                    viableTargets = Team.Fighters.Where(f => f.IsAlive()).ToList();
                    break;
                case TargetType.Self:
                    viableTargets = Team.Fighters.Where(f => f == this && f.IsAlive());
                    break;
                case TargetType.Field:
                    viableTargets =
                        Team.Fighters.Where(f => f.IsAlive()).Concat(enemyTeam.Fighters.Where(f => f.IsAlive()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return viableTargets;
        }
    }
}
