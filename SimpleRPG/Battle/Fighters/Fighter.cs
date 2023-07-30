using SimpleRPG.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Fighters
{
    public abstract class Fighter: IFighter, IStatusable, IDamageable
    {
        public int Level { get; protected set; }

        public int CurrentHealth { get; protected set; }

        public int MaxHealth { get; protected set; }

        public int CurrentMana { get; protected set; }

        public int MaxMana { get; protected set; }

        public string BaseName { get; protected set; }

        public string AppendText { get; protected set; }
        
        public virtual string DisplayName => $"{BaseName}" + (string.IsNullOrEmpty(AppendText) ? "" : $" {AppendText}");

        public int Strength { get; protected set; }

        public int Defense { get; protected set; }

        public int Speed { get; protected set; }

        public int MagicStrength { get; protected set; }

        public MagicSet<int> MagicStrengthBonuses { get; protected set; }

        public int MagicResistance { get; protected set; }

        public MagicSet<int> MagicResistanceBonuses { get; protected set; }

        public int Evade { get; protected set; }

        public int Luck { get; protected set; }

        public List<Spell> Spells { get; protected set; }

        public List<BattleMove> SpecialMoves { get; protected set; }
        
        public MagicSet<FighterMagicRelationshipType> MagicAffinities { get; }

        public BattleShield BattleShield { get; protected set; }

        public List<Status> Statuses => StatusManager.Statuses;

        public Team Team { get; protected set; }

        protected StatusManager StatusManager { get; set; }

        #region events

        public EventHandler<PhysicalDamageTakenEventArgs> DamageTaken { get; set; }

        public void OnDamageTaken(PhysicalDamageTakenEventArgs e)
        {
            DamageTaken?.Invoke(this, e);
        }

        public EventHandler<FighterHealedEventArgs> Healed { get; set; }

        public void OnHealed(FighterHealedEventArgs e)
        {
            Healed?.Invoke(this, e);
        }

        public EventHandler<MagicalDamageTakenEventArgs> MagicalDamageTaken { get; set; }

        public void OnMagicalDamageTaken(MagicalDamageTakenEventArgs e)
        {
            MagicalDamageTaken?.Invoke(this, e);
        }

        public EventHandler<AttackSuccessfulEventArgs> AttackSuccessful { get; set; }

        public void OnAttackSuccessful(AttackSuccessfulEventArgs e)
        {
            AttackSuccessful?.Invoke(this, e);
        }

        public EventHandler<CriticalAttackEventArgs> CriticalAttack { get; set; }

        public void OnCriticalAttack(CriticalAttackEventArgs e)
        {
            CriticalAttack?.Invoke(this, e);
        }

        public EventHandler<SpecialMoveExecutedEventArgs> SpecialMoveExecuted { get; set; }

        public void OnSpecialMoveExecuted(SpecialMoveExecutedEventArgs e)
        {
            SpecialMoveExecuted?.Invoke(this, e);
        }

        public EventHandler<SpecialMoveFailedEventArgs> SpecialMoveFailed { get; set; }

        public void OnSpecialMoveFailed(SpecialMoveFailedEventArgs e)
        {
            SpecialMoveFailed?.Invoke(this, e);
        }

        public EventHandler<KilledEventArgs> Killed { get; set; }

        public void OnKilled(KilledEventArgs e)
        {
            Killed?.Invoke(this, e);
        }

        public EventHandler<EnemyKilledEventArgs> EnemyKilled { get; set; }

        public void OnEnemyKilled(EnemyKilledEventArgs e)
        {
            EnemyKilled?.Invoke(this, e);
        }

        public EventHandler<ManaLostEventArgs> ManaLost { get; set; }

        public void OnManaLost(ManaLostEventArgs e)
        {
            ManaLost?.Invoke(this, e);
        }

        public EventHandler<ManaRestoredEventArgs> ManaRestored { get; set; }

        public void OnManaRestored(ManaRestoredEventArgs e)
        {
            ManaRestored?.Invoke(this, e);
        }

        public EventHandler<SpellSuccessfulEventArgs> SpellSuccessful { get; set; }

        public void OnSpellSuccessful(SpellSuccessfulEventArgs e)
        {
            SpellSuccessful?.Invoke(this, e);
        }

        public EventHandler<TurnEndedEventArgs> TurnEnded { get; set; }

        public void OnTurnEnded(TurnEndedEventArgs e)
        {
            TurnEnded?.Invoke(this, e);
        }

        public EventHandler<RoundEndedEventArgs> RoundEnded { get; set; }

        public void OnRoundEnded(RoundEndedEventArgs e)
        {
            RoundEnded?.Invoke(this, e);
        }

        public EventHandler<AttackMissedEventArgs> AttackMissed { get; set; }

        public void OnAttackMissed(AttackMissedEventArgs e)
        {
            AttackMissed?.Invoke(this, e);
        }

        public EventHandler<AutoEvadedEventArgs> AutoEvaded { get; set; }

        public void OnAutoEvaded(AutoEvadedEventArgs e)
        {
            AutoEvaded?.Invoke(this, e);
        }

        public EventHandler<EnemyAttackCounteredEventArgs> EnemyAttackCountered { get; set; }

        public void OnEnemyAttackCountered(EnemyAttackCounteredEventArgs e)
        {
            EnemyAttackCountered?.Invoke(this, e);
        }

        public EventHandler<StatusAddedEventArgs> StatusAdded { get; set; }

        public void OnStatusAdded(StatusAddedEventArgs e)
        {
            StatusAdded?.Invoke(this, e);
        }

        public EventHandler<StatusRemovedEventArgs> StatusRemoved { get; set; }

        public void OnStatusRemoved(StatusRemovedEventArgs e)
        {
            StatusRemoved?.Invoke(this, e);
        }

        public EventHandler<ShieldAddedEventArgs> ShieldAdded { get; set; }

        public void OnShieldAdded(ShieldAddedEventArgs e)
        {
            ShieldAdded?.Invoke(this, e);
        }

        public EventHandler<StatRaisedEventArgs> StatRaised { get; set; }

        public void OnStatRaised(StatRaisedEventArgs e)
        {
            StatRaised?.Invoke(this, e);
        }

        #endregion

        protected Fighter(string name
            ,int level
            ,int health
            ,int mana
            ,int strength
            ,int defense
            ,int speed
            ,int evade
            ,int luck
            ,List<Spell> spells = null
            ,List<BattleMove> specialMoves = null)
        {
            BaseName = name;
            AppendText = "";
            Level = level;
            MaxHealth = health;
            CurrentHealth = health;
            MaxMana = mana;
            CurrentMana = mana;
            Strength = strength;
            Defense = defense;
            Speed = speed;
            Evade = evade;
            Luck = luck;

            MagicStrength = 1;

            Spells = spells ?? new List<Spell>();
            SpecialMoves = specialMoves ?? new List<BattleMove>();

            MagicAffinities = new MagicSet<FighterMagicRelationshipType>();

            MagicStrengthBonuses = new MagicSet<int>();
            MagicResistanceBonuses = new MagicSet<int>();

            StatusManager = new StatusManager();
            TurnEnded += StatusManager.TurnEnded;
            RoundEnded += StatusManager.RoundEnded;
        }

        public void SetTeam(Team team)
        {
            Team = team;
        }

        public int GetStatValue(StatType stat)
        {
            int ret;

            switch (stat)
            {
                case StatType.Strength:
                    ret = Strength;
                    break;
                case StatType.Defense:
                    ret = Defense;
                    break;
                case StatType.Speed:
                    ret = Speed;
                    break;
                case StatType.Evade:
                    ret = Evade;
                    break;
                case StatType.Luck:
                    ret = Luck;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stat), stat, null);
            }

            return ret;
        }

        public int RaiseStat(StatType statType, int amount)
        {
            switch (statType)
            {
                case StatType.Strength:
                    Strength += amount;
                    break;
                case StatType.Defense:
                    Defense += amount;
                    break;
                case StatType.Speed:
                    Speed += amount;
                    break;
                case StatType.Evade:
                    Evade += amount;
                    break;
                case StatType.Luck:
                    Luck += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, "Unrecognized statType value given to Fighter.RaiseStat()");
            }

            StatRaisedEventArgs e = new StatRaisedEventArgs(statType, amount);
            OnStatRaised(e);

            return GetStatValue(statType);
        }

        /// <summary>
        /// Reduces the user's current health by at most the amount specified by the input.
        /// If the fighter's <see cref="BattleShield"/> is not null, it will take the damage instead of the fighter
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>The amount of Damage the fighter took (e.g. if amount if 5 but current health is 3, fighter only took 3 damage)</returns>
        public virtual int PhysicalDamage(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentException("PhysicalDamage cannot be given a negative amount of damage!", nameof(amount));
            }

            int ret;

            if (BattleShield != null)
            {
                ret = BattleShield.DecrementHealth(amount);

                //TODO: move into an event handler?
                if (BattleShield.CurrentHealth == 0)
                {
                    RemoveBattleShield();
                }
            }
            else
            {
                var prevHealth = CurrentHealth;

                CurrentHealth -= amount;
                OnDamageTaken(new PhysicalDamageTakenEventArgs(amount));

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 0;
                }

                if (CurrentHealth == 0)
                {
                    OnKilled(new KilledEventArgs());
                }
                ret = (prevHealth - CurrentHealth);
            }

            return ret; 
        }

        public void RaiseMaxHealth(int amount, bool alsoRaiseCurrentHealth = true, bool alsoFullyHeal = false)
        {
            MaxHealth += amount;

            if (alsoFullyHeal)
            {
                CurrentHealth = MaxHealth;
            }
            else if (alsoRaiseCurrentHealth)
            {
                Heal(amount);
            }
        }

        private int CalculateMagicalDamageAfterMultiplier(int amount, MagicType magicType)
        {
            var ret = amount;

            //normal magic damage 
            if (magicType != MagicType.None)
            {
                var affinity = MagicAffinities[MagicType.All];

                if (affinity == FighterMagicRelationshipType.None)
                {
                    affinity = MagicAffinities[magicType];
                }

                switch (affinity)
                {
                    case FighterMagicRelationshipType.Weak:
                        ret *= 2;
                        break;
                    case FighterMagicRelationshipType.Resistant:
                        ret /= 2;
                        break;
                    case FighterMagicRelationshipType.Immune:
                        ret = 0;
                        break;
                    case FighterMagicRelationshipType.None:
                    default:
                        break;
                }

            }

            return ret;
        }

        /// <summary>
        /// Reduces the user's current health by at most the amount specified by the input.
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="magicType"></param>
        /// <returns>The amount of Damage the fighter took (e.g. if amount if 5 but current health is 3, fighter only took 3 damage)</returns>
        public virtual int MagicalDamage(int amount, MagicType magicType)
        {
            if (amount < 0)
            {
                throw new ArgumentException("MagicalDamage cannot be given a negative amount of damage!", nameof(amount));
            }

            int ret;

            if (BattleShield != null)
            {
                ret = BattleShield.DecrementHealth(amount, magicType);

                if (BattleShield.CurrentHealth == 0)
                {
                    RemoveBattleShield();
                }
            }
            else
            {
                var prevHealth = CurrentHealth;

                var damage = CalculateMagicalDamageAfterMultiplier(amount, magicType);

                CurrentHealth -= damage;

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 0;
                }

                ret = (prevHealth - CurrentHealth);
                OnMagicalDamageTaken(new MagicalDamageTakenEventArgs(ret, magicType));

                if (CurrentHealth == 0)
                {
                    OnKilled(new KilledEventArgs());
                }
            }
            

            return ret;
        }

        public int Heal(int amount, bool isFullHeal = false)
        {
            var prevHealth = CurrentHealth;
            int healAmount = 0;

            if (CurrentHealth > 0)
            {
                CurrentHealth += amount;

                if (CurrentHealth > MaxHealth)
                {
                    CurrentHealth = MaxHealth;
                }

                healAmount = CurrentHealth - prevHealth;

                FighterHealedEventArgs e = new FighterHealedEventArgs(healAmount, isFullHeal);
                OnHealed(e);
            }

            return healAmount;
        }

        public int FullyHeal()
        {
            return Heal(MaxHealth, true);
        }

        public void Revive(int amount)
        {
            if (CurrentHealth > 0)
            {
                throw new InvalidOperationException(
                    "Fighter.Revive() cannot be called on a fighter that has more than 0 health!");
            }
            else if (amount > MaxHealth)
            {
                throw new ArgumentException(
                   "Fighter.Revive() cannot restore a user past full health!", nameof(amount));
            }

            CurrentHealth = amount;
        }

        /// <summary>
        /// Lowers the users <see cref="CurrentMana"/> by the specified <param name="amount"></param>
        /// </summary>
        /// <exception cref="ArgumentException"> if amount exceeds CurrentMana. Use <see cref="DrainMana"/> if this should simply be capped at 0</exception>
        /// <param name="amount">The amount by which to lower the mana</param>
        /// <returns>The amount of mana actually spent</returns>
        public int SpendMana(int amount)
        {
            if (amount > CurrentMana)
            {
                throw new ArgumentException($"Fighter {DisplayName} tried to spend {amount} mana, but only had {CurrentMana} to spend!");
            }

            return DrainMana(amount);
        }

        /// <summary>
        /// The same as <see cref="SpendMana"/>, except no exception is thrown is <param name="amount"></param>
        /// exceeds <see cref="CurrentMana"/>
        /// </summary>
        /// <param name="amount">The amount by which to lower the mana</param>
        /// <returns>The amount of mana actually drained</returns>
        public int DrainMana(int amount)
        {
            var prevMana = CurrentMana;

            CurrentMana -= amount;

            if (CurrentMana < 0)
            {
                CurrentMana = 0;
            }

            var manaLost = prevMana - CurrentMana;
            OnManaLost(new ManaLostEventArgs(manaLost));
            return manaLost;

        }

        /// <summary>
        /// Raises the amount of mana the user has, capping it off at the MaxMana
        /// </summary>
        /// <param name="amount">The maximum amount by which to raise the user's CurrentMana</param>
        /// <returns>The amount of mana that was restored</returns>
        public int RestoreMana(int amount)
        {
            var prevMana = CurrentMana;

            CurrentMana += amount;

            if (CurrentMana > MaxMana)
            {
                CurrentMana = MaxMana;
            }

            var restoreAmount = CurrentMana - prevMana;
            OnManaRestored(new ManaRestoredEventArgs(restoreAmount));
            return restoreAmount;
        }

        //TODO: Delete this method. Should be handled by the BattleManager
        public int Attack(IFighter opponent, double attackMultiplier = 1.0, bool isCrit = false)
        {
            var damage = Strength - opponent.Defense;

            if (isCrit)
            {
                OnCriticalAttack(new CriticalAttackEventArgs());
            }

            damage = (int)(damage * attackMultiplier);

            if (damage < 0)
            {
                damage = 0;
            }

            var damageDealt = opponent.PhysicalDamage(damage);

            OnAttackSuccessful(new AttackSuccessfulEventArgs(opponent, damageDealt));

            if (!opponent.IsAlive())
            {
                OnEnemyKilled(new EnemyKilledEventArgs(opponent));
            }

            return damageDealt;
        }

        /// <summary>
        /// Determines if a fighter has learned a particular spell
        /// </summary>
        /// <param name="spell">The spell to be checked</param>
        /// <returns>True is the Fighter has the spell</returns>
        public bool HasSpell(Spell spell)
        {
            return Spells.FirstOrDefault(s => s.Description == spell.Description) != null;
        }

        public virtual void AddSpell(Spell spell)
        {
            Spells.Add(spell);
        }

        public virtual void AddSpells(List<Spell> spells)
        {
            Spells.AddRange(spells);
        }

        public void RemoveSpell(Spell spell)
        {
            Spells.Remove(spell);
        }

        public void AddSpecialMove(SpecialMove specialMove)
        {
            SpecialMoves.Add(specialMove);
        }

        public void SetBattleShield(BattleShield battleShield)
        {
            BattleShield = battleShield.Copy() as BattleShield;
            BattleShield.SetOwner(this);

            ShieldAddedEventArgs e = new ShieldAddedEventArgs(BattleShield);
            OnShieldAdded(e);
        }

        public void RemoveBattleShield()
        {
            BattleShield = null;
        }

        public void AddStatus(Status status)
        {
            StatusManager.AddOrRefreshStatus(status);

            OnStatusAdded(new StatusAddedEventArgs(status));
        }

        public void RemoveStatuses(Func<Status, bool> removePredicate, bool suppressEvents = false)
        {
            IEnumerable<Status> removedStatuses = StatusManager.RemoveStatuses(removePredicate);

            if (!suppressEvents)
            {
                foreach (Status removedStatus in removedStatuses)
                {
                    OnStatusRemoved(new StatusRemovedEventArgs(removedStatus));
                }
            }
        }

        public void SetAppendText(string text)
        {
            AppendText = text;
        }

        public bool IsAlive()
        {
            return CurrentHealth > 0;
        }
    }
}
