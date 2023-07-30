using SimpleRPG.Events;
using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Fighters
{
    public interface IFighter
    {
        int Level { get; }

        int CurrentHealth { get; }

        int MaxHealth { get; }

        int CurrentMana { get; }

        int MaxMana { get; }

        int Strength { get; }

        int Defense { get; }

        int Speed { get; }

        int Evade { get; }

        int Luck { get; }

        int MagicStrength { get; }

        MagicSet<int> MagicStrengthBonuses { get; }

        int MagicResistance { get; }

        MagicSet<int> MagicResistanceBonuses { get; }

        string BaseName { get; }

        string AppendText { get; }

        string DisplayName { get; }

        List<Spell> Spells { get; }

        List<BattleMove> SpecialMoves { get; }

        MagicSet<FighterMagicRelationshipType> MagicAffinities { get; }

        BattleShield BattleShield { get; }

        List<Status> Statuses { get; }

        Team Team { get; }

        #region events

        EventHandler<PhysicalDamageTakenEventArgs> DamageTaken { get; set; }

        void OnDamageTaken(PhysicalDamageTakenEventArgs e);

        EventHandler<FighterHealedEventArgs> Healed { get; set; }

        void OnHealed(FighterHealedEventArgs e);

        EventHandler<MagicalDamageTakenEventArgs> MagicalDamageTaken { get; set; }

        void OnMagicalDamageTaken(MagicalDamageTakenEventArgs e);

        EventHandler<AttackSuccessfulEventArgs> AttackSuccessful { get; set; }

        void OnAttackSuccessful(AttackSuccessfulEventArgs e);

        EventHandler<CriticalAttackEventArgs> CriticalAttack { get; set; }

        void OnCriticalAttack(CriticalAttackEventArgs e);

        EventHandler<AttackMissedEventArgs> AttackMissed { get; set; }

        void OnAttackMissed(AttackMissedEventArgs e);

        EventHandler<AutoEvadedEventArgs> AutoEvaded { get; set; }

        void OnAutoEvaded(AutoEvadedEventArgs e);

        EventHandler<EnemyAttackCounteredEventArgs> EnemyAttackCountered { get; set; }

        void OnEnemyAttackCountered(EnemyAttackCounteredEventArgs e);

        EventHandler<SpecialMoveExecutedEventArgs> SpecialMoveExecuted { get; set; }

        void OnSpecialMoveExecuted(SpecialMoveExecutedEventArgs e);

        EventHandler<SpecialMoveFailedEventArgs> SpecialMoveFailed { get; set; }

        void OnSpecialMoveFailed(SpecialMoveFailedEventArgs e);

        EventHandler<ManaLostEventArgs> ManaLost { get; set; }

        void OnManaLost(ManaLostEventArgs e);

        EventHandler<ManaRestoredEventArgs> ManaRestored { get; set; }

        void OnManaRestored(ManaRestoredEventArgs e);

        EventHandler<SpellSuccessfulEventArgs> SpellSuccessful { get; set; }

        void OnSpellSuccessful(SpellSuccessfulEventArgs e);

        EventHandler<TurnEndedEventArgs> TurnEnded { get; set; }

        void OnTurnEnded(TurnEndedEventArgs e);

        EventHandler<RoundEndedEventArgs> RoundEnded { get; set; }

        void OnRoundEnded(RoundEndedEventArgs e);

        EventHandler<KilledEventArgs> Killed { get; set; }

        void OnKilled(KilledEventArgs e);

        EventHandler<EnemyKilledEventArgs> EnemyKilled { get; set; }

        void OnEnemyKilled(EnemyKilledEventArgs e);

        EventHandler<StatusAddedEventArgs> StatusAdded { get; set; }

        void OnStatusAdded(StatusAddedEventArgs e);

        EventHandler<StatusRemovedEventArgs> StatusRemoved { get; set; }

        void OnStatusRemoved(StatusRemovedEventArgs e);

        EventHandler<ShieldAddedEventArgs> ShieldAdded { get; set; }

        void OnShieldAdded(ShieldAddedEventArgs e);

        EventHandler<StatRaisedEventArgs> StatRaised { get; set; }

        void OnStatRaised(StatRaisedEventArgs e);

        #endregion

        void SetTeam(Team team);

        int GetStatValue(StatType stat);

        int RaiseStat(StatType statType, int amount);

        /// <summary>
        /// Damages the user's current health through a physical attack
        /// </summary>
        /// <param name="amount">The maximum amount of damage to be done</param>
        /// <returns>The amount of Damage the fighter took (e.g. if amount if 5 but current health is 3, fighter only took 3 damage)</returns>
        int PhysicalDamage(int amount);

        /// <summary>
        /// Damages the user's current health through a magical attack
        /// </summary>
        /// <param name="amount">The maximum amount of damage to be done</param>
        /// <param name="magicType">The type of damage being dealt</param>
        /// <returns>The amount of Damage the fighter took (e.g. if amount if 5 but current health is 3, fighter only took 3 damage)</returns>
        int MagicalDamage(int amount, MagicType magicType);

        /// <summary>
        /// Raises the fighter's current health.
        /// However, it cannot be used on a fighter whose HP is below 0, use <see cref="Revive"/> if this is required.
        /// Also, the amount cannot exceet the Max Health. E.g. if the fighter has 9 out of 10 health and is healed for 100 they'll only regain 1 HP
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="isFullHeal"></param>
        /// <returns></returns>
        int Heal(int amount, bool isFullHeal = false);

        /// <summary>
        /// A method used to automatically raise <see cref="CurrentHealth"/> to <see cref="MaxHealth"/>,
        /// This also displays a different display message during battle
        /// </summary>
        /// <returns></returns>
        int FullyHeal();

        /// <summary>
        /// Raises the character's max HP. Might also raise the Current HP by the same amount the max was raised by, or they may even be fully healed
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="alsoRaiseCurrentHealth"></param>
        /// <param name="alsoFullyHeal"></param>
        void RaiseMaxHealth(int amount, bool alsoRaiseCurrentHealth = true, bool alsoFullyHeal = false);

        /// <summary>
        /// Restore's a fighter's health if they have 0 Health
        /// </summary>
        /// <param name="amount">The amount to set their health to</param>
        void Revive(int amount);

        /// <summary>
        /// Lowers the users <see cref="CurrentMana"/> by the specified <param name="amount"></param>
        /// </summary>
        /// <exception cref="ArgumentException"> if amount exceeds CurrentMana. Use <see cref="DrainMana"/> if this should simply be capped at 0</exception>
        /// <param name="amount">The amount by which to lower the mana</param>
        /// <returns>The amount of mana actually spent</returns>
        int SpendMana(int amount);

        /// <summary>
        /// The same as <see cref="SpendMana"/>, except no exception is thrown is <param name="amount"></param>
        /// exceeds <see cref="CurrentMana"/>
        /// </summary>
        /// <param name="amount">The amount by which to lower the mana</param>
        /// <returns>The amount of mana actually drained</returns>
        int DrainMana(int amount);

        /// <summary>
        /// Raises the amount of mana the user has, capping it off at the MaxMana
        /// </summary>
        /// <param name="amount">The maximum amount by which to raise the user's CurrentMana</param>
        /// <returns>The amount of mana that was restored</returns>
        int RestoreMana(int amount);

        /// <summary>
        /// The method used to reduce an opponent's health and to trigger appropriate onAttack events
        /// </summary>
        /// <param name="opponent">The opponent being damaged</param>
        /// <param name="attackMultiplier">The value that </param>
        /// <param name="isCrit">Whether the attack should do critical damage or not</param>
        /// <returns>The amount of damage dealt</returns>
        int Attack(IFighter opponent, double attackMultiplier, bool isCrit);

        /// <summary>
        /// Determines if a fighter has learned a particular spell
        /// </summary>
        /// <param name="spell">The spell to be checked</param>
        /// <returns>True is the Fighter has the spell</returns>
        bool HasSpell(Spell spell);

        void AddSpell(Spell spell);

        void RemoveSpell(Spell spell);

        //TODO: should fire a "Special Move learned" event
        void AddSpecialMove(SpecialMove specialMove);

        /// <summary>
        /// sets the <see cref="BattleShield"/> property, copying by value, not by reference
        /// </summary>
        /// <param name="battleShield"></param>
        void SetBattleShield(BattleShield battleShield);

        void RemoveBattleShield();

        void AddStatus(Status status);

        void RemoveStatuses(Func<Status, bool> removePredicate, bool suppressEvents);

        void SetAppendText(string text);

        bool IsAlive();
    }
}
