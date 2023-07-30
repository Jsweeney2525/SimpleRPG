using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.Fighters
{
    public class HumanFighter: Fighter
    {
        public int CurrentExp { get; protected set; }

        private int _expToNextLevel;
        
        protected List<BattleMove> _extraSpecialMoves { get; set; }

        public List<BattleMove> AllSpecialMoves => SpecialMoves.Concat(_extraSpecialMoves).ToList();

        public List<PersonalityFlag> PersonalityFlags { get; } = new List<PersonalityFlag>();

        #region Events

        public EventHandler<ExpGainedEventArgs> ExpGained { get; set; }

        public void OnExpGained(ExpGainedEventArgs e)
        {
            ExpGained?.Invoke(this, e);
        }

        public EventHandler<LeveledUpEventArgs> LeveledUp { get; set; }

        public void OnLeveledUp(LeveledUpEventArgs e)
        {
            LeveledUp?.Invoke(this, e);
        }

        public EventHandler<StatBonusAppliedEventArgs> StatBonusApplied { get; set; }

        public void OnStatBonusApplied(StatBonusAppliedEventArgs e)
        {
            StatBonusApplied?.Invoke(this, e);
        }

        public EventHandler<MagicBonusAppliedEventArgs> MagicBonusApplied { get; set; }

        public void OnMagicBonusApplied(MagicBonusAppliedEventArgs e)
        {
            MagicBonusApplied?.Invoke(this, e);
        }

        public EventHandler<SpellsLearnedEventArgs> SpellsLearned { get; set; }

        public void OnSpellsLearned(SpellsLearnedEventArgs e)
        {
            SpellsLearned?.Invoke(this, e);
        }

        public EventHandler<MoveLearnedEventArgs> MoveLearned { get; set; }

        public void OnMoveLearned(MoveLearnedEventArgs e)
        {
            MoveLearned?.Invoke(this, e);
        }

        #endregion

        public HumanFighter(string name, int level, List<Spell> spells = null, List<SpecialMove> specialMoves = null)
            : base(name 
                  ,level
                  ,LevelUpManager.GetHealthByLevel(level)
                  ,LevelUpManager.GetManaByLevel(level)
                  ,LevelUpManager.GetStrengthByLevel(level)
                  ,LevelUpManager.GetDefenseByLevel(level)
                  ,LevelUpManager.GetSpeedByLevel(level)
                  ,LevelUpManager.GetEvadeByLevel(level)
                  ,LevelUpManager.GetLuckByLevel(level)
                  ,SpellFactory.GetSpellsByLevel<HumanFighter>(level)
                  ,MoveFactory.GetMovesByLevel<HumanFighter>(level)
                  )
        {
            if (spells != null)
            {
                Spells.AddRange(spells);
            }

            if (specialMoves != null)
            {
                SpecialMoves.AddRange(specialMoves);
            }

            _extraSpecialMoves = new List<BattleMove>();
            CurrentExp = LevelUpManager.GetExpForLevel(level);
            _expToNextLevel = LevelUpManager.GetExpForLevel(level + 1);
        }

        protected HumanFighter(string name, int level, int health, int mana, int strength, int defense, int speed, int evade, int luck, List<Spell> spells = null) 
            : base(name, level, health, mana, strength, defense, speed, evade, luck, spells)
        {
        }

        public override void AddSpell(Spell spell)
        {
            base.AddSpell(spell);
            SpellsLearnedEventArgs e = new SpellsLearnedEventArgs(spell);
            OnSpellsLearned(e);
        }

        public override void AddSpells(List<Spell> spells)
        {
            base.AddSpells(spells);
            if (spells.Count > 0)
            {
                SpellsLearnedEventArgs e = new SpellsLearnedEventArgs(spells);
                OnSpellsLearned(e);
            }
        }

        public void AddMove(BattleMove move)
        {
            SpecialMove specialMove = move as SpecialMove;

            if (specialMove != null)
            {
                AddSpecialMove(specialMove);
            }
            else
            {
                _extraSpecialMoves.Add(move);
            }

            MoveLearnedEventArgs e = new MoveLearnedEventArgs(move);
            OnMoveLearned(e);
        }

        /// <summary>
        /// Grants the user a particular amount of Experience
        /// </summary>
        /// <param name="amount">the amount by which to raise <see cref="CurrentExp"/></param>
        public void GainExp(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException($"A player cannot gain negative experience! GainExp: called with {amount}");
            }

            CurrentExp += amount;
            OnExpGained(new ExpGainedEventArgs(amount));

            if (CurrentExp >= _expToNextLevel && Level < LevelUpManager.MAX_LEVEL)
            {
                LevelUp();
            }

        }

        /// <summary>
        /// Assumes CurrentExp has already been updated
        /// </summary>
        private void LevelUp()
        {
            var continuer = true;

            for (var i = Level + 1; i <= LevelUpManager.MAX_LEVEL && continuer; ++i)
            {
                if (CurrentExp >= LevelUpManager.GetExpForLevel(i))
                {
                    ++Level;

                    CurrentHealth = LevelUpManager.GetHealthByLevel(Level);
                    MaxHealth = LevelUpManager.GetHealthByLevel(Level);
                    CurrentMana = LevelUpManager.GetManaByLevel(Level);
                    MaxMana = LevelUpManager.GetManaByLevel(Level);
                    Strength = LevelUpManager.GetStrengthByLevel(Level);
                    Defense = LevelUpManager.GetDefenseByLevel(Level);
                    Speed = LevelUpManager.GetSpeedByLevel(Level);
                    Evade = LevelUpManager.GetEvadeByLevel(Level);
                    Luck = LevelUpManager.GetLuckByLevel(Level);

                    OnLeveledUp(new LeveledUpEventArgs(
                        Level
                        , LevelUpManager.HealthBoostByLevel(Level)
                        , LevelUpManager.ManaBoostByLevel(Level)
                        , LevelUpManager.StrengthBoostByLevel(Level)
                        , LevelUpManager.DefenseBoostByLevel(Level)
                        , LevelUpManager.SpeedBoostByLevel(Level)));

                    var spells = SpellFactory.GetSpellsByLevel<HumanFighter>(Level, Level);

                    if (spells.Count > 0)
                    {
                        AddSpells(spells);
                    }
                }
                else
                {
                    continuer = false;
                }
            }
            
            _expToNextLevel = LevelUpManager.GetExpForLevel(Level + 1);
        }

        public void AddStatBonus(StatType stat, int bonus, bool isSecretBonus = false)
        {
            switch (stat)
            {
                case StatType.Strength:
                    Strength += bonus;
                    break;
                case StatType.Defense:
                    Defense += bonus;
                    break;
                case StatType.Speed:
                    Speed += bonus;
                    break;
                case StatType.Luck:
                    Luck += bonus;
                    break;
            }

            StatBonusAppliedEventArgs e = new StatBonusAppliedEventArgs(stat, bonus, isSecretBonus);
            OnStatBonusApplied(e);
        }

        /// <summary>
        /// Used to add bonuses specific to a fighter (e.g. influenced by a decision during gameplay).
        /// This will increase either one of MagicStrengthBonus's fields or MagicResistanceBonus's fields (if magic type is a simple element), 
        /// or the basic MagicStrength/MagicResistance value if magicType "none" or "all" is specified
        /// </summary>
        /// <param name="magicStatType">Determines whther it's magic attack or magic resistance being boosted</param>
        /// <param name="magicType"></param>
        /// <param name="bonus"></param>
        /// <param name="isSecretStatBoost"></param>
        public void AddMagicBonus(MagicStatType magicStatType, MagicType magicType, int bonus, bool isSecretStatBoost = false)
        {
            if (magicType == MagicType.All || magicType == MagicType.None)
            {
                if (magicStatType == MagicStatType.Power)
                {
                    MagicStrength += bonus;
                }
                else
                {
                    MagicResistance += bonus;
                }
            }
            else
            {
                MagicSet<int> bonusesSet = magicStatType == MagicStatType.Power
                    ? MagicStrengthBonuses
                    : MagicResistanceBonuses;

                bonusesSet[magicType] += bonus;
            }

            MagicBonusAppliedEventArgs e = new MagicBonusAppliedEventArgs(magicStatType, magicType, bonus, isSecretStatBoost);
            OnMagicBonusApplied(e);
        }

        public void AddPersonalityFlag(PersonalityFlag flag)
        {
            if (!PersonalityFlags.Contains(flag))
            {
                PersonalityFlags.Add(flag);
            }
        }
    }
}
