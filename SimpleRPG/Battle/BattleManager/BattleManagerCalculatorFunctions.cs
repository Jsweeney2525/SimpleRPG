using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleManager
{
    public partial class BattleManager
    {
        protected int CalculateAttackPower(IFighter attacker, IFighter defender, AttackBattleMove attackMove, bool isCrit)
        {
            double attackMultiplier = CalculateStatMultiplier(attacker, StatType.Strength);

            if (isCrit)
            {
                attackMultiplier *= 2;
            }

            int attackerStrength = attacker.Strength;

            List<AttackBoostBattleMoveEffect> attackBoostEffects = attackMove.BattleMoveEffects.OfType<AttackBoostBattleMoveEffect>().Where(e => IsEffectConditionMet(e, attacker, defender)).ToList();

            if (attackBoostEffects.Any())
            {
                double totalAttackBoostMultiplier = attackBoostEffects.Sum(e => e.Multiplier);
                attackerStrength = (int)(attackerStrength* totalAttackBoostMultiplier);
            }

            int initialAttackPower = attackerStrength + attackMove.AttackPower;
            int calculatedAttackPower = (int) (initialAttackPower*attackMultiplier);

            return calculatedAttackPower;
        }

        /// <summary>
        /// Returns either the defender's shield's defense power (if the defender has a shield), or the calculated defense power of the defender,
        /// taking statuses and field effects into account
        /// </summary>
        /// <param name="defender"></param>
        /// <returns></returns>
        protected int CalculateDefensePower(IFighter defender)
        {
            int defensePower;

            if (defender.BattleShield != null)
            {
                defensePower = defender.BattleShield.Defense;
            }
            else
            {
                double defenseMultiplier = CalculateStatMultiplier(defender, StatType.Defense);

                defensePower = (int)(defender.Defense * defenseMultiplier);
            }

            return defensePower;
        }

        /// <summary>
        /// Calculates a multiplier for a particular stat based off of any statuses applied to fighter or their team
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="stat"></param>
        /// <returns></returns>
        protected double CalculateStatMultiplier(IFighter fighter, StatType stat)
        {
            var ret = 1.0;

            bool humanEffect = _humanTeam.Contains(fighter);

            Func<StatMultiplierStatus, bool> whereFunc = s => s.StatType == stat;

            IEnumerable<StatMultiplierStatus> statuses =
                fighter.Statuses.OfType<StatMultiplierStatus>().Where(whereFunc);

            IEnumerable<StatMultiplierFieldEffect> fieldEffects =
                FindEffectsByType<StatMultiplierFieldEffect>(humanEffect);

            fieldEffects = fieldEffects.Where(effect => effect.Stat == stat);

            ret = statuses.Aggregate(ret, (current, multiplierStatus) => current * multiplierStatus.Multiplier);

            ret = fieldEffects.Aggregate(ret, (current, fieldEffect) => current*fieldEffect.Percentage);

            return ret;
        }

        protected int CalculateEffectiveSpeed(IFighter fighter)
        {
            double speedMultiplier = CalculateStatMultiplier(fighter, StatType.Speed);

            return (int)(fighter.Speed * speedMultiplier);
        }

        protected double CalculateCritMultiplier(IFighter fighter)
        {
            var ret = 1.0;

            IEnumerable<CriticalChanceMultiplierStatus> statuses =
                fighter.Statuses.OfType<CriticalChanceMultiplierStatus>();

            ret = statuses.Aggregate(ret, (current, multiplierStatus) => current * multiplierStatus.Multiplier);

            return ret;
        }
        
        protected int CalculateEvadeBonus(IFighter fighter)
        {
            var ret = fighter.Evade;

            var humanEffect = _humanTeam.Contains(fighter);

            var effects =
                Config.FieldEffectCounters.Where(fec =>
                {
                    var multiplierEffect = fec.Effect as StatMultiplierFieldEffect;

                    return fec.IsHumanEffect == humanEffect
                           && multiplierEffect != null
                           && multiplierEffect.Stat == StatType.Evade;
                })
                    .Select(fec => fec.Effect);

            var critMultipliers = effects.Cast<StatMultiplierFieldEffect>();

            ret = critMultipliers.Aggregate(ret, (current, multiplier) => (int) (current * multiplier.Percentage));

            return ret;
        }

        protected int CalculateMagicalStrength(IFighter caster, Spell spell)
        {
            return CalculateMagicalStrength(caster, spell.ElementalType, spell.Power);
        }

        /// <summary>
        /// Returns a value representing the attack power of a particular fighter wielding a particular magic type
        /// with a base spell power
        /// </summary>
        /// <param name="caster">The <see cref="IFighter"/> casting the magic attack</param>
        /// <param name="magicType">The type of the spell, which may affecting the bonuses available to the caster</param>
        /// <param name="spellPower">The base power of the spell</param>
        /// <returns></returns>
        protected int CalculateMagicalStrength(IFighter caster, MagicType magicType, int spellPower)
        {
            int ret = spellPower + caster.MagicStrength + caster.MagicStrengthBonuses[magicType];

            ret = (int)(ret * CalculateMagicMultiplier(caster, magicType));

            return ret;
        }

        /// <summary>
        /// Returns a value representing the defensive power of a particular fighter being attacked by a particular magic type
        /// This calculation ignores the magic Affinity, such as being weak or resistant to an element
        /// </summary>
        /// <param name="caster">The <see cref="IFighter"/> casting the magic attack</param>
        /// <param name="magicType">The type of the spell, which may affecting the bonuses available to the caster</param>
        /// <returns></returns>
        protected int CalculateEffectiveResistance(IFighter caster, MagicType magicType)
        {
            int ret;

            if (caster.BattleShield != null)
            {
                ret = caster.BattleShield.MagicResistance;
            }
            else
            {
                ret = caster.MagicResistance + caster.MagicResistanceBonuses[magicType];

                ret = (int)(ret * CalculateResistanceMultiplier(caster, magicType));
            }

            return ret;
        }

        protected int CalculateMagicDamageDealt(IFighter target, MagicType magicType, int magicPower)
        {
            var totalResist = target.MagicResistance + target.MagicResistanceBonuses[magicType];
            //damage done before affinities or resistance taken into account
            var ret = magicPower - totalResist;

            return ret;
        }

        protected double CalculateMagicMultiplier(IFighter caster, MagicType magicType)
        {
            var ret = 1.0;

            IEnumerable<MagicMultiplierStatus> statuses = caster.Statuses.OfType<MagicMultiplierStatus>();
            statuses = statuses.Where(s => s.MagicType == magicType || s.MagicType == MagicType.All);
            ret = statuses.Aggregate(ret, (current, multiplier) => current * multiplier.Multiplier);

            return ret;
        }

        protected double CalculateResistanceMultiplier(IFighter target, MagicType magicType)
        {
            double ret = 1.0;

            IEnumerable<MagicResistanceMultiplierStatus> statuses = target.Statuses.OfType<MagicResistanceMultiplierStatus>();

            statuses = statuses.Where(s => s.MagicType == magicType || s.MagicType == MagicType.All);
            ret = statuses.Aggregate(ret, (current, multiplier) => current*multiplier.Multiplier);

            return ret;
        }

        /// <summary>
        /// Given a particular caster trying to launch a particular spell,
        /// calculates the total cost of that spell, given statuses, field effects, etc.
        /// </summary>
        /// <param name="caster">The one casting the spell</param>
        /// <param name="spell">The spell being cast</param>
        /// <returns></returns>
        protected int CalculateSpellCost(IFighter caster, Spell spell)
        {
            double multiplier = CalculateSpellCostMultiplier(caster);

            int ret = (int)(spell.Cost * multiplier);

            return ret;
        }

        protected double CalculateSpellCostMultiplier(IFighter caster)
        {
            var ret = 1.0;

            IEnumerable<SpellCostMultiplierStatus> multiplierStatuses = caster.Statuses.OfType<SpellCostMultiplierStatus>();
            ret = multiplierStatuses.Aggregate(ret, (current, multiplier) => current*multiplier.Multiplier);

            return ret;
        }
    }
}
