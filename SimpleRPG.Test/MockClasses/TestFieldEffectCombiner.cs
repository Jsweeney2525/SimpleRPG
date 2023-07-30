using System;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses
{
    public class TestFieldEffectCombiner : FieldEffectCombiner
    {
        //TODO: lazy! Only allows one test combo!
        private DanceEffectType? _firstEffectType;

        private DanceEffectType? _secondEffectType;

        private CombinedFieldEffect _effect;

        public TestFieldEffectCombiner()
        {
            _effect = null;
        }

        public void AddFakeCombination(DanceEffectType effect1, DanceEffectType effect2, CombinedFieldEffect combinedEffect)
        {
            DanceEffectType first = (DanceEffectType)Math.Min((int)effect1, (int)effect2);
            DanceEffectType second = (DanceEffectType)Math.Max((int)effect1, (int)effect2);

            _firstEffectType = first;
            _secondEffectType = second;
            _effect = combinedEffect;
        }

        public override CombinedFieldEffect Combine(DanceEffectType effect1, DanceEffectType effect2)
        {
            CombinedFieldEffect ret;

            DanceEffectType first = (DanceEffectType) Math.Min((int) effect1, (int) effect2);
            DanceEffectType second = (DanceEffectType) Math.Max((int) effect1, (int) effect2);

            if (_firstEffectType.HasValue && _secondEffectType.HasValue && _firstEffectType.Value == first && _secondEffectType == second && _effect != null)
            {
                ret = _effect;
            }
            else
            {
                ret = base.Combine(effect1, effect2);
            }

            return ret;
        }
    }
}
