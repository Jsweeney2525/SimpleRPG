using System;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.BattleShields
{
    public interface IBattleShield
    {
        int CurrentHealth { get; }

        int MaxHealth { get; }

        int Defense { get; }

        int MagicResistance { get; }

        int ShieldBusterDefense { get; }

        IFighter Owner { get; }

        string DisplayName { get; }

        /// <summary>
        /// A property that specifies if the display text for this object should be "a" or "an".
        /// (e.g. "an iron shield" vs. "a fire shield")
        /// </summary>
        string DisplayArticle { get; }

        #region events

        EventHandler<ShieldHealedEventArgs> ShieldHealed { get; set; }

        void OnShieldHealed(ShieldHealedEventArgs e);

        EventHandler<ShieldFortifiedEventArgs> ShieldFortified { get; set; }

        void OnShieldFortified(ShieldFortifiedEventArgs e);

        EventHandler<ShieldDestroyedEventArgs> ShieldDestroyed { get; set; }

        void OnShieldDestroyed(ShieldDestroyedEventArgs e);

        #endregion

        IBattleShield Copy();

        void SetOwner(IFighter fighter);

        int DecrementHealth(int damage);

        int DecrementHealth(int damage, MagicType attackingType);

        int IncrementHealth(int healAmount);

        bool AreEqual(IBattleShield shield);

        void FortifyDefense(int amount);

        /// <summary>
        /// Returns a string that displays the specific qualities of the shield (e.g. "an iron shield" or "a fire elemental shield")
        /// </summary>
        /// <param name="withArticle">If true, display text will be soemthing like "an iron shield" versus "iron shield" if set to false</param>
        /// <returns></returns>
        string GetDisplayText(bool withArticle = true);
    }
}
