using System;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.BattleShields
{
    public abstract class BattleShield : IBattleShield, IDamageable
    {
        public int CurrentHealth { get; protected set; }

        public int MaxHealth { get; protected set; }

        public int Defense { get; protected set; }

        public int MagicResistance { get; protected set; }

        /// <summary>
        /// If a ShieldBuster move has equal or greater ShieldBusterPower, the shield will be destroyed. Otherwise, it will survive
        /// </summary>
        public int ShieldBusterDefense { get; }

        public IFighter Owner { get; protected set; }

        public string DisplayName { get; protected set; }

        /// <summary>
        /// A property that specifies if the display text for this object should be "a" or "an".
        /// (e.g. "an iron shield" vs. "a fire shield")
        /// </summary>
        public string DisplayArticle { get; protected set; }

        #region events

        public EventHandler<PhysicalDamageTakenEventArgs> DamageTaken { get; set; }

        public void OnDamageTaken(PhysicalDamageTakenEventArgs e)
        {
            DamageTaken?.Invoke(this, e);
        }

        public EventHandler<MagicalDamageTakenEventArgs> MagicalDamageTaken { get; set; }

        public void OnMagicalDamageTaken(MagicalDamageTakenEventArgs e)
        {
            MagicalDamageTaken?.Invoke(this, e);
        }

        public EventHandler<ShieldHealedEventArgs> ShieldHealed { get; set; }

        public void OnShieldHealed(ShieldHealedEventArgs e)
        {
            ShieldHealed?.Invoke(this, e);
        }

        public EventHandler<ShieldFortifiedEventArgs> ShieldFortified { get; set; }

        public void OnShieldFortified(ShieldFortifiedEventArgs e)
        {
            ShieldFortified?.Invoke(this, e);
        }

        public EventHandler<ShieldDestroyedEventArgs> ShieldDestroyed { get; set; }

        public void OnShieldDestroyed(ShieldDestroyedEventArgs e)
        {
            ShieldDestroyed?.Invoke(this, e);
        }

        #endregion

        protected BattleShield(int health, int defense, int magicResistance, int shieldBusterDefense = 0, string displayArticle = null, string displayName = null)
        {
            CurrentHealth = health;
            MaxHealth = health;
            Defense = defense;
            MagicResistance = magicResistance;
            ShieldBusterDefense = shieldBusterDefense;

            DisplayArticle = displayArticle ?? "a";
            DisplayName = displayName ?? "battle shield";
        }

        protected BattleShield(BattleShield copy)
        {
            CurrentHealth = copy.CurrentHealth;
            MaxHealth = copy.MaxHealth;
            Defense = copy.Defense;
            MagicResistance = copy.MagicResistance;
            ShieldBusterDefense = copy.ShieldBusterDefense;

            DisplayArticle = copy.DisplayArticle;
            DisplayName = copy.DisplayName;
        }

        public void SetOwner(IFighter fighter)
        {
            Owner = fighter;
        }

        public abstract IBattleShield Copy();

        public virtual int DecrementHealth(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentException("DecrementHealth cannot be given a negative amount of damage!", nameof(damage));
            }
            var previousHealth = CurrentHealth;

            CurrentHealth -= damage;

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            PhysicalDamageTakenEventArgs e = new PhysicalDamageTakenEventArgs(damage);
            OnDamageTaken(e);

            if (CurrentHealth == 0)
            {
                OnShieldDestroyed(new ShieldDestroyedEventArgs());
            }

            return (previousHealth - CurrentHealth);
        }

        /// <summary>
        /// A method that fires the <see cref="OnMagicalDamageTaken" /> event and potentially the <seealso cref="OnShieldDestroyed"/> event
        /// Taking into acocunt elemental weaknesses. THe base version does not actually use the <paramref name="attackingType"/> to somehow inform the damage taken
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="attackingType"></param>
        /// <returns></returns>
        public virtual int DecrementHealth(int damage, MagicType attackingType)
        {
            if (damage < 0)
            {
                throw new ArgumentException("DecrementHealth cannot be given a negative amount of damage!", nameof(damage));
            }

            int previousHealth = CurrentHealth;

            CurrentHealth -= damage;

            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }

            MagicalDamageTakenEventArgs e = new MagicalDamageTakenEventArgs(damage, attackingType);
            OnMagicalDamageTaken(e);

            if (CurrentHealth == 0)
            {
                OnShieldDestroyed(new ShieldDestroyedEventArgs());
            }

            return (previousHealth - CurrentHealth);
        }


        public int IncrementHealth(int healAmount)
        {
            if (CurrentHealth <= 0)
            {
                throw new InvalidOperationException("IncrementHealth cannot be called on a shield with 0 health!");
            }
            if (healAmount < 0)
            {
                throw new ArgumentException("IncrementHealth cannot be given a negative amount of damage!", nameof(healAmount));
            }
            int previousHealth = CurrentHealth;

            CurrentHealth += healAmount;

            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }

            int actualHealAmount = CurrentHealth - previousHealth;

            OnShieldHealed(new ShieldHealedEventArgs(actualHealAmount));

            return actualHealAmount;
        }

        public virtual bool AreEqual(IBattleShield shield)
        {
            return MaxHealth == shield.MaxHealth 
                && Defense == shield.Defense 
                && MagicResistance == shield.MagicResistance
                && ShieldBusterDefense == shield.ShieldBusterDefense;
        }

        public void FortifyDefense(int amount)
        {
            Defense += amount;

            ShieldFortifiedEventArgs e = new ShieldFortifiedEventArgs(amount);
            OnShieldFortified(e);
        }

        public string GetDisplayText(bool withArticle = true)
        {
            string displayText = withArticle ? $"{DisplayArticle} {DisplayName}" : $"{DisplayName}";

            return displayText;
        }
    }
}
