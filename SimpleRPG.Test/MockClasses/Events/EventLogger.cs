using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Events;
using SimpleRPG.Regions;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Test.MockClasses.Events
{
    public enum EventType
    {
        DamageTaken,
        MagicalDamageTaken,
        Killed,
        AttackSuccessful,
        CriticalAttack,
        AttackMissed,
        Healed,
        ManaLost,
        ManaRestored,
        SpellSuccessful,
        EnemyKilled,
        ExpGained,
        LeveledUp
        ,StatRaised
        ,StatBonusApplied
        ,MagicBonusApplied
        ,SpellLearned
        ,MoveLearned
        ,TurnEnded
        ,RoundEnded
        ,TeamDefeated
        ,Ran
        ,ShieldAdded
        ,ShieldHealed
        ,ShieldFortified
        ,ShieldDestroyed
        ,StatusAdded
        ,StatusRemoved
        ,FieldEffectExecuted
        ,AutoEvaded
        ,EnemyAttackCountered
        ,SpecialMoveExecuted
        ,SpecialMoveFailed
        ,RegionCompleted
        ,SubRegionCompleted
        ,FighterAdded
        ,FighterSealed
        ,ShadeAbsorbed
        ,FighterTransformed
    }

    public class EventLog
    {
        public EventType Type { get; private set; }

        public object Sender { get; private set; }

        public EventArgs E { get; private set; }

        public EventLog(EventType type, object sender, EventArgs e)
        {
            Type = type;
            Sender = sender;
            E = e;
        }
    }

    public class EventLogger
    {
        public List<EventLog> Logs { get; }

        public EventLogger()
        {
            Logs = new List<EventLog>();
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }

        public void SubscribeAll(IFighter fighter)
        {
            fighter.DamageTaken += _logDamageTaken;
            fighter.Healed += _logHealed;
            fighter.MagicalDamageTaken += _logMagicalDamageTaken;
            fighter.AttackSuccessful += _logAttackSuccessful;
            fighter.CriticalAttack += _logCriticalAttack;
            fighter.AttackMissed += _logAttackMissed;
            fighter.ManaLost += _logManaLost;
            fighter.ManaRestored += _logManaRestored;
            fighter.SpellSuccessful += _logSpellSuccessful;
            fighter.Killed += _logKilled;
            fighter.EnemyKilled += _logEnemyKilled;
            fighter.TurnEnded += _logTurnEnded;
            fighter.StatusAdded += _logStatusAdded;
            fighter.StatusRemoved += _logStatusRemoved;
            fighter.AutoEvaded += _logAutoEvaded;
            fighter.EnemyAttackCountered += _logEnemyAttackCountered;
            fighter.SpecialMoveExecuted += _logSpecialMoveExecuted;
            fighter.SpecialMoveFailed += _logSpecialMoveFailed;
            fighter.ShieldAdded += _logShieldAdded;
            fighter.StatRaised += _logStatRaised;
        }

        public void SubscribeAll(HumanFighter fighter)
        {
            fighter.ExpGained += _logExpGained;
            fighter.LeveledUp += _logLeveledUp;
            fighter.SpellsLearned += _logSpellLearned;
            fighter.MoveLearned += _logMoveLearned;
            fighter.StatBonusApplied += _logStatBonusApplied;
            fighter.MagicBonusApplied += _logMagicBonusApplied;
            SubscribeAll(fighter as IFighter);
        }

        public void SubscribeAll(Shade shade)
        {
            shade.FighterSealed += _logFighterSealed;
            shade.ShadeAbsorbed += _logShadeAbsorbed;
            shade.ShadeTransformed += _logFighterTransformed;
            SubscribeAll(shade as IFighter);
        }

        public void SubscribeAll(Team team)
        {
            team.TeamDefeated += _logTeamDefeated;
            team.RoundEnded += _logRoundEnded;
            team.Ran += _logRan;
            team.FighterAdded += _logFighterAdded;
        }

        public void SubscribeAll(IBattleShield shield)
        {
            shield.ShieldHealed += _logShieldHealed;
            shield.ShieldDestroyed += _logShieldDestroyed;
            shield.ShieldFortified += _logShieldFortified;

            BattleShield battleShield = shield as BattleShield;
            if (battleShield != null)
            {
                battleShield.DamageTaken += _logDamageTaken;
                battleShield.MagicalDamageTaken += _logMagicalDamageTaken;
            }
        }

        public void Subscribe(EventType type, IFighter fighter)
        {
            HumanFighter humanFighter = fighter as HumanFighter;
            Shade fighterAsShade = fighter as Shade;
            switch (type)
            {
                case EventType.DamageTaken:
                    fighter.DamageTaken += _logDamageTaken;
                    break;
                case EventType.Healed:
                    fighter.Healed += _logHealed;
                    break;
                case EventType.MagicalDamageTaken:
                    fighter.MagicalDamageTaken += _logMagicalDamageTaken;
                    break;
                case EventType.AttackSuccessful:
                    fighter.AttackSuccessful += _logAttackSuccessful;
                    break;
                case EventType.CriticalAttack:
                    fighter.CriticalAttack += _logCriticalAttack;
                    break;
                case EventType.AttackMissed:
                    fighter.AttackMissed += _logAttackMissed;
                    break;
                case EventType.ManaLost:
                    fighter.ManaLost += _logManaLost;
                    break;
                case EventType.ManaRestored:
                    fighter.ManaRestored += _logManaRestored;
                    break;
                case EventType.SpellSuccessful:
                    fighter.SpellSuccessful += _logSpellSuccessful;
                    break;
                case EventType.Killed:
                    fighter.Killed += _logKilled;
                    break;
                case EventType.EnemyKilled:
                    fighter.EnemyKilled += _logEnemyKilled;
                    break;
                case EventType.ExpGained:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Exp Gained event!");
                    }
                    humanFighter.ExpGained += _logExpGained;
                    break;
                case EventType.LeveledUp:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Leveled Up event!");
                    }
                    humanFighter.LeveledUp += _logLeveledUp;
                    break;
                case EventType.StatRaised:
                    fighter.StatRaised += _logStatRaised;
                    break;
                case EventType.StatBonusApplied:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Stat Bonus Applied event!");
                    }
                    humanFighter.StatBonusApplied += _logStatBonusApplied;
                    break;
                case EventType.MagicBonusApplied:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Magic Bonus Applied event!");
                    }
                    humanFighter.MagicBonusApplied += _logMagicBonusApplied;
                    break;
                case EventType.SpellLearned:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Spell Learned event!");
                    }
                    humanFighter.SpellsLearned += _logSpellLearned;
                    break;
                case EventType.MoveLearned:
                    if (humanFighter == null)
                    {
                        throw new ArgumentException("Only Human Fighters can raise the Move Learned event!");
                    }
                    humanFighter.MoveLearned += _logMoveLearned;
                    break;
                case EventType.TurnEnded:
                    fighter.TurnEnded += _logTurnEnded;
                    break;
                case EventType.StatusAdded:
                    fighter.StatusAdded += _logStatusAdded;
                    break;
                case EventType.StatusRemoved:
                    fighter.StatusRemoved += _logStatusRemoved;
                    break;
                case EventType.AutoEvaded:
                    fighter.AutoEvaded += _logAutoEvaded;
                    break;
                case EventType.EnemyAttackCountered:
                    fighter.EnemyAttackCountered += _logEnemyAttackCountered;
                    break;
                case EventType.SpecialMoveExecuted:
                    fighter.SpecialMoveExecuted += _logSpecialMoveExecuted;
                    break;
                case EventType.SpecialMoveFailed:
                    fighter.SpecialMoveFailed += _logSpecialMoveFailed;
                    break;
                case EventType.ShieldAdded:
                    fighter.ShieldAdded += _logShieldAdded;
                    break;
                case EventType.FighterSealed:
                    if (fighterAsShade == null)
                    {
                        throw new InvalidOperationException("EventLogger.SubScribe() cannot subscribe to event type 'fighter sealed' if the subscribee is not a Shade enemy type");
                    }
                    fighterAsShade.FighterSealed += _logFighterSealed;
                    break;
                case EventType.ShadeAbsorbed:
                    if (fighterAsShade == null)
                    {
                        throw new InvalidOperationException("EventLogger.SubScribe() cannot subscribe to event type 'fighter sealed' if the subscribee is not a Shade enemy type");
                    }
                    fighterAsShade.ShadeAbsorbed += _logShadeAbsorbed;
                    break;
                case EventType.FighterTransformed:
                    if (fighterAsShade == null)
                    {
                        throw new InvalidOperationException("EventLogger.Subscribe() cannot subscribe to event type 'fighter transformed' if the subscribee is not a Shade enemy type");
                    }
                    fighterAsShade.ShadeTransformed += _logFighterTransformed;
                    break;
                case EventType.TeamDefeated:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with IFighters's implemented events");
                case EventType.Ran:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with IFighters's implemented events");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with IFighters's implemented events");
            }
        }

        public void Subscribe(IFighter fighter, params EventType[] eventTypes)
        {
            foreach (EventType eventType in eventTypes)
            {
                Subscribe(eventType, fighter);
            }
        }

        public void Subscribe(EventType type, Team team)
        {
            switch (type)
            {
                case EventType.TeamDefeated:
                    team.TeamDefeated += _logTeamDefeated;
                    break;
                case EventType.RoundEnded:
                    team.RoundEnded += _logRoundEnded;
                    break;
                case EventType.FighterAdded:
                    team.FighterAdded += _logFighterAdded;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with Team's implemented events");
            }
        }

        public void Subscribe(EventType type, MenuManager manager)
        {
            switch (type)
            {
                case EventType.Ran:
                    manager.Ran += _logRan;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with MenuManager's implemented events");
            }
        }

        public void Subscribe(EventType type, BattleManager manager)
        {
            switch (type)
            {
                case EventType.FieldEffectExecuted:
                    manager.FieldEffectExecuted += _logFieldEffectExecuted;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with BattleManager's implemented events");
            }
        }

        public void Subscribe(EventType type, IBattleShield shield)
        {
            switch (type)
            {
                case EventType.ShieldDestroyed:
                    shield.ShieldDestroyed += _logShieldDestroyed;
                    break;
                case EventType.ShieldHealed:
                    shield.ShieldHealed += _logShieldHealed;
                    break;
                case EventType.ShieldFortified:
                    shield.ShieldFortified += _logShieldFortified;
                    break;
                case EventType.DamageTaken:
                    BattleShield battleShield = shield as BattleShield;
                    if (battleShield != null)
                    {
                        battleShield.DamageTaken += _logDamageTaken;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "a valid EventType must be specified compatible with IBattleShield's implemented events");
            }
        }

        public void Subscribe(Region region, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.RegionCompleted:
                    region.RegionCompleted += _logRegionCompleted;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "a valid EventType must be specified compatible with Region's implemented events");
            }
        }

        public void Subscribe(SubRegion subRegion, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.SubRegionCompleted:
                    subRegion.SubRegionCompleted += _logSubRegionCompleted;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "a valid EventType must be specified compatible with SubRegion's implemented events");
            }
        }

        private void _logDamageTaken(object sender, PhysicalDamageTakenEventArgs e)
        {
            Logs.Add(new EventLog(EventType.DamageTaken, sender, e));
        }

        private void _logHealed(object sender, FighterHealedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.Healed, sender, e));
        }

        private void _logMagicalDamageTaken(object sender, MagicalDamageTakenEventArgs e)
        {
            Logs.Add(new EventLog(EventType.MagicalDamageTaken, sender, e));
        }

        private void _logAttackSuccessful(object sender, AttackSuccessfulEventArgs e)
        {
            Logs.Add(new EventLog(EventType.AttackSuccessful, sender, e));
        }

        private void _logCriticalAttack(object sender, CriticalAttackEventArgs e)
        {
            Logs.Add(new EventLog(EventType.CriticalAttack, sender, e));
        }

        private void _logAttackMissed(object sender, AttackMissedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.AttackMissed, sender, e));
        }

        private void _logManaLost(object sender, ManaLostEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ManaLost, sender, e));
        }

        private void _logManaRestored(object sender, ManaRestoredEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ManaRestored, sender, e));
        }

        private void _logSpellSuccessful(object sender, SpellSuccessfulEventArgs e)
        {
            Logs.Add(new EventLog(EventType.SpellSuccessful, sender, e));
        }

        private void _logKilled(object sender, KilledEventArgs e)
        {
            Logs.Add(new EventLog(EventType.Killed, sender, e));
        }

        private void _logEnemyKilled(object sender, EnemyKilledEventArgs e)
        {
            Logs.Add(new EventLog(EventType.EnemyKilled, sender, e));
        }
        private void _logExpGained(object sender, ExpGainedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ExpGained, sender, e));
        }

        private void _logLeveledUp(object sender, LeveledUpEventArgs e)
        {
            Logs.Add(new EventLog(EventType.LeveledUp, sender, e));
        }

        private void _logStatRaised(object sender, StatRaisedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.StatRaised, sender, e));
        }

        private void _logStatBonusApplied(object sender, StatBonusAppliedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.StatBonusApplied, sender, e));
        }

        private void _logMagicBonusApplied(object sender, MagicBonusAppliedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.MagicBonusApplied, sender, e));
        }

        private void _logSpellLearned(object sender, SpellsLearnedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.SpellLearned, sender, e));
        }

        private void _logMoveLearned(object sender, MoveLearnedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.MoveLearned, sender, e));
        }

        private void _logTurnEnded(object sender, TurnEndedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.TurnEnded, sender, e));
        }

        private void _logRoundEnded(object sender, RoundEndedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.RoundEnded, sender, e));
        }

        private void _logTeamDefeated(object sender, TeamDefeatedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.TeamDefeated, sender, e));
        }

        private void _logRan(object sender, EventArgs e)
        {
            Logs.Add(new EventLog(EventType.TeamDefeated, sender, e));
        }

        private void _logShieldHealed(object sender, ShieldHealedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ShieldHealed, sender, e));
        }

        private void _logShieldFortified(object sender, ShieldFortifiedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ShieldFortified, sender, e));
        }

        private void _logShieldAdded(object sender, ShieldAddedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ShieldAdded, sender, e));
        }

        private void _logShieldDestroyed(object sender, ShieldDestroyedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ShieldDestroyed, sender, e));
        }

        private void _logStatusAdded(object sender, StatusAddedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.StatusAdded, sender, e));
        }

        private void _logStatusRemoved(object sender, StatusRemovedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.StatusRemoved, sender, e));
        }

        private void _logAutoEvaded(object sender, AutoEvadedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.AutoEvaded, sender, e));
        }

        private void _logEnemyAttackCountered(object sender, EnemyAttackCounteredEventArgs e)
        {
            Logs.Add(new EventLog(EventType.EnemyAttackCountered, sender, e));
        }

        private void _logSpecialMoveExecuted(object sender, SpecialMoveExecutedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.SpecialMoveExecuted, sender, e));
        }

        private void _logSpecialMoveFailed(object sender, SpecialMoveFailedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.SpecialMoveFailed, sender, e));
        }

        private void _logFieldEffectExecuted(object sender, FieldEffectExecutedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.FieldEffectExecuted, sender, e));
        }

        private void _logRegionCompleted(object sender, RegionCompletedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.RegionCompleted, sender, e));
        }

        private void _logSubRegionCompleted(object sender, SubRegionCompletedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.SubRegionCompleted, sender, e));
        }

        private void _logFighterAdded(object sender, FighterAddedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.FighterAdded, sender, e));
        }

        private void _logFighterSealed(object sender, FighterSealedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.FighterSealed, sender, e));
        }

        private void _logShadeAbsorbed(object sender, ShadeAbsorbedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.ShadeAbsorbed, sender, e));
        }

        private void _logFighterTransformed(object sender, FighterTransformedEventArgs e)
        {
            Logs.Add(new EventLog(EventType.FighterTransformed, sender, e));
        }
    }
}
