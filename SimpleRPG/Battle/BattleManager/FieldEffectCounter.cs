using System;
using System.Collections.Generic;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Battle.BattleManager
{
    public class FieldEffectCounter : TurnCounter
    {
        private readonly List<IFighter> _owners;

        public FieldEffect Effect { get; }

        public bool IsHumanEffect { get; }

        public List<IFighter> Owners
        {
            get
            {
                var ret = new List<IFighter>();
                ret.AddRange(_owners);
                return ret;
            }
        }

        public FieldEffectCounter(List<IFighter> owners, int turns, FieldEffect effect, bool isHumanEffect)
            : base(turns)
        {
            if (owners == null || owners.Count == 0)
            {
                throw new ArgumentException("A dance effect counter must be initialized with a non-empty list of owners!");
            }

            _owners = owners;
            Effect = effect;
            IsHumanEffect = isHumanEffect;
        }
    }
}
