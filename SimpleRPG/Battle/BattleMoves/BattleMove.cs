using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public abstract class SpecialTargettingRule
    {
        public abstract IEnumerable<IFighter> ApplyRule(IEnumerable<IFighter> unfilteredFighters);
    }

    public class RestrictedFighterTypesSpecialTargettingRule<T> : SpecialTargettingRule where T : IFighter
    {
        public override IEnumerable<IFighter> ApplyRule(IEnumerable<IFighter> unfilteredFighters)
        {
            return unfilteredFighters.Where(f => f is T || (f as HumanControlledEnemyFighter)?.Fighter is T);
        }
    }

    public class SpecialTargettingRuleCollection
    {
        private readonly List<SpecialTargettingRule> _targettingRules;

        public SpecialTargettingRuleCollection(params SpecialTargettingRule[] targettingRules)
        {
            _targettingRules = new List<SpecialTargettingRule>(targettingRules);
        }

        public IEnumerable<IFighter> ApplyRules(IEnumerable<IFighter> fighters)
        {
            IEnumerable<IFighter> filteredFighters = fighters;

            foreach (SpecialTargettingRule rule in _targettingRules)
            {
                filteredFighters = rule.ApplyRule(filteredFighters);
            }

            return filteredFighters;
        }
    }

    public class BattleMove
    {
        public string Description { get; protected set; }

        public BattleMoveType MoveType { get; protected set; }

        public TargetType TargetType { get; protected set; }

        protected SpecialTargettingRuleCollection TargettingRuleCollection;

        public int Priority { get; protected set; }

        /// <summary>
        /// A list of special effects that are implemented when the move is executed- some are when the attack hits, some are only activated if a particalar condition is met
        /// </summary>
        public List<BattleMoveEffect> BattleMoveEffects { get; protected set; }

        private readonly string _executionText;

        /// <summary>
        /// A string to be concactenated to the executor's display name (e.g. "{DisplayName} {ExecutionText}")
        /// Can be null
        /// </summary>
        public string ExecutionText => string.Copy(_executionText ?? "");

        public BattleMove(string description, 
            BattleMoveType moveType, 
            TargetType targetType, 
            int priority = 0, 
            string executionText = null, 
            SpecialTargettingRuleCollection targettingRuleCollection = null,
            params BattleMoveEffect[] effects)
        {
            Description = description;
            MoveType = moveType;
            TargetType = targetType;
            BattleMoveEffects = new List<BattleMoveEffect>(effects);
            Priority = priority;
            _executionText = executionText;
            TargettingRuleCollection = targettingRuleCollection ?? new SpecialTargettingRuleCollection();
        }

        public BattleMove(BattleMove copy)
        {
            Description = copy.Description;
            MoveType = copy.MoveType;
            TargetType = copy.TargetType;
            Priority = copy.Priority;
            BattleMoveEffects = new List<BattleMoveEffect>(copy.BattleMoveEffects);
            TargettingRuleCollection = copy.TargettingRuleCollection;
        }

        public IEnumerable<IFighter> GetAvailableTargets(IEnumerable<IFighter> unfilteredFighters)
        {
            return TargettingRuleCollection.ApplyRules(unfilteredFighters);
        }
    }
}
