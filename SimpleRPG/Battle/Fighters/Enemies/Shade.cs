using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Shade: EnemyFighter, IConditionalPowerExecutor
    {
        #region events

        public EventHandler<FighterSealedEventArgs> FighterSealed { get; set; }

        public void OnFighterSealed(FighterSealedEventArgs e)
        {
            FighterSealed?.Invoke(this, e);
        }

        public EventHandler<ShadeAbsorbedEventArgs> ShadeAbsorbed { get; set; }

        public void OnShadeAbsorbed(ShadeAbsorbedEventArgs e)
        {
            ShadeAbsorbed?.Invoke(this, e);
        }

        public EventHandler<FighterTransformedEventArgs> ShadeTransformed { get; set; }

        public void OnShadeTransformed(FighterTransformedEventArgs e)
        {
            ShadeTransformed?.Invoke(this, e);
        }

        #endregion
        
        public static readonly List<StatType> AbsorptionBonuses = new List<StatType> { StatType.Speed, StatType.Defense, StatType.Evade };

        public static readonly List<int> ExperienceForLevel = new List<int> {0, 1, 2, 5};

        public const int MaxShadeLevel = 3;

        public const int MaxMalevolenceLevel = 3;

        private int _malevolenceCounter;

        private readonly BattleMove _chargeMove;

        private readonly BattleMove _malevolenceAttackMove;

        /// <summary>
        /// Raises the <see cref="_malevolenceCounter"/> by the given amount, capping at <seealso cref="MaxMalevolenceLevel"/>
        /// </summary>
        /// <param name="malevolenceAmount"></param>
        private void IncrementMalevolenceCounter(int malevolenceAmount)
        {
            _malevolenceCounter = Math.Min(_malevolenceCounter + malevolenceAmount, MaxMalevolenceLevel);
        }

        private int _shadeExperience;

        public int ShadeExperience
        {
            get { return _shadeExperience; }
            protected set
            {
                _shadeExperience = value;
                CheckExperienceForLevelUp();
            }
        }

        private int _shadeLevel;

        public int ShadeLevel
        {
            get {return _shadeLevel; }
            protected set
            {
                int oldLevel = _shadeLevel;
                _shadeLevel = value;

                if (oldLevel != _shadeLevel)
                {
                    LevelUp();
                }
            }
        }

        private void CheckExperienceForLevelUp()
        {
            if (ShadeLevel >= MaxShadeLevel)
            {
                return;
            }

            for (int i = MaxShadeLevel; i > 0; --i)
            {
                if (ShadeExperience >= ExperienceForLevel[i])
                {
                    ShadeLevel = i;
                    break;
                }
            }
        }

        private void LevelUp()
        {
            string baseNameBeforeLevelUp = DisplayName;
            CalcualteBaseName();

            FighterTransformedEventArgs e = new FighterTransformedEventArgs(baseNameBeforeLevelUp, $"a {BaseName}");
            OnShadeTransformed(e);
        }

        private void CalcualteBaseName()
        {
            string baseName;

            switch (ShadeLevel)
            {
                default:
                    baseName = "Shade";
                    break;
                case 2:
                    baseName = "Strong Shade";
                    break;
                case 3:
                    baseName = "Powerful Shade";
                    break;
            }

            BaseName = baseName;
        }

        public Shade(int level, IChanceService chanceService, int shadeExperience)
            : base("Shade",
                level,
                LevelUpManager.GetHealthByLevel<Shade>(level),
                LevelUpManager.GetManaByLevel<Shade>(level),
                LevelUpManager.GetStrengthByLevel<Shade>(level),
                LevelUpManager.GetDefenseByLevel<Shade>(level),
                LevelUpManager.GetSpeedByLevel<Shade>(level),
                LevelUpManager.GetEvadeByLevel<Shade>(level),
                LevelUpManager.GetLuckByLevel<Shade>(level),
                chanceService,
                SpellFactory.GetSpellsByLevel<Shade>(level)
                ,MoveFactory.GetMovesByLevel<Shade>(level))
        {
            ShadeExperience = shadeExperience;
            _malevolenceCounter = 0;

            _chargeMove = AvailableMoves.FirstOrDefault(bm => bm.MoveType == BattleMoveType.Special);
            _malevolenceAttackMove = AvailableMoves.FirstOrDefault(bm => bm.MoveType == BattleMoveType.ConditionalPowerAttack);
        }

        public override void ExecuteMove(BattleManager.BattleManager battleManager, BattleMoveWithTarget move, Team ownTeam, Team enemyTeam, IOutput output)
        {
            if (move.Move is ShadeAbsorbingMove)
            {
                Shade target = move.Target as Shade;
                AbsorbShade(target);
            }
            else if (move.Move.MoveType == BattleMoveType.Special && move.Move.Description == "dark energy gather")
            {
                if (_malevolenceCounter < MaxMalevolenceLevel)
                {
                    _malevolenceCounter++;
                }
            }
        }

        protected override IEnumerable<IFighter> _getViableTargets(BattleMove move, Team enemyTeam)
        {
            IEnumerable<IFighter> viableTargets;

            StatusMove moveAsStatusMove = move as StatusMove;
            ShadeAbsorbingMove moveAsAbsorbingMove = move as ShadeAbsorbingMove;

            if (moveAsStatusMove != null && move.TargetType == TargetType.SingleEnemy)
            {
                viableTargets =
                    enemyTeam.Fighters.Where(f => !f.Statuses.Exists(s => s.AreEqual(moveAsStatusMove.Status)) && f.IsAlive());
            }
            else if (moveAsAbsorbingMove != null)
            {
                viableTargets = Team.Fighters.OfType<Shade>().Where(s => s.IsAlive() && s != this);
            }
            else
            {
                viableTargets = base._getViableTargets(move, enemyTeam);
            }

            return viableTargets;
        }

        public override List<BattleMove> GetExecutableMoves(Team enemyTeam)
        {
            List<BattleMove> executableMoves = AvailableMoves.Where(bm => bm.MoveType != BattleMoveType.Attack).ToList();

            executableMoves.RemoveAll(m => _getViableTargets(m, enemyTeam).ToList().Count == 0);

            if (_malevolenceCounter >= MaxMalevolenceLevel)
            {
                executableMoves.RemoveAll(m => m.MoveType == BattleMoveType.Special);
            }

            return executableMoves.ToList();
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            var index = 0;

            List<BattleMove> executableMoves = GetExecutableMoves(enemyTeam);
            int executableMovesCount = executableMoves.Count;

            if (executableMovesCount > 1)
            {
                if (executableMoves.Contains(_chargeMove))
                {
                    double[] chances = new double[executableMovesCount];

                    for (var i = 0; i < executableMovesCount; ++i)
                    {
                        chances[i] = 1.0/executableMovesCount;
                    }

                    double combinedChance = chances[0]*2;

                    double attackChance = (combinedChance * _malevolenceCounter)/MaxMalevolenceLevel;
                    double chargeChance = combinedChance - attackChance;

                    int chargeIndex = executableMoves.IndexOf(_chargeMove);
                    int attackIndex = executableMoves.IndexOf(_malevolenceAttackMove);

                    chances[chargeIndex] = chargeChance;
                    chances[attackIndex] = attackChance;

                    index = ChanceService.WhichEventOccurs(chances);
                }
                else
                {
                    index = ChanceService.WhichEventOccurs(executableMovesCount);
                }
            }

            var move = executableMoves[index];

            return move;
        }

        protected override IFighter _selectTarget(BattleMove move, Team ownTeam, Team enemyTeam)
        {
            List<IFighter> viableTargets =  _getViableTargets(move, enemyTeam).ToList();

            if (move is ShadeAbsorbingMove && viableTargets.Any())
            {
                int minCurrentHealth = viableTargets.OrderBy(f => f.CurrentHealth).First().CurrentHealth;

                viableTargets = viableTargets.Where(f => f.CurrentHealth == minCurrentHealth).ToList();
            }

            var selectedTarget = _selectTarget(viableTargets.ToList());

            return selectedTarget;
        }

        public void AbsorbShade(Shade shade)
        {
            ShadeAbsorbedEventArgs e = new ShadeAbsorbedEventArgs(shade);
            OnShadeAbsorbed(e);
            FullyHeal();

            IncrementMalevolenceCounter(shade._malevolenceCounter);

            StatType statBonus = ChanceService.WhichEventOccurs(AbsorptionBonuses);

            //TODO: move into a method
            int increaseAmount;
            switch (statBonus)
            {
                case StatType.Speed:
                case StatType.Defense:
                    increaseAmount = shade.ShadeExperience;
                    break;
                case StatType.Evade:
                    increaseAmount = shade.ShadeExperience*5;
                    break;
                default:
                    throw new Exception($"Shade.AbsorbShade() does not know how to handle a boost to the '{statBonus}' stat");
            }

            RaiseStat(statBonus, increaseAmount);
            ShadeExperience += shade.ShadeExperience;
            shade.CurrentHealth = 0;
        }

        public void Seal()
        {
            CurrentHealth = 0;

            FighterSealedEventArgs e = new FighterSealedEventArgs(this);
            OnFighterSealed(e);
        }
        
        public int GetAttackBonus()
        {
            int attackBonus = _malevolenceCounter;
            _malevolenceCounter = 0;
            return attackBonus;
        }
    }
}
