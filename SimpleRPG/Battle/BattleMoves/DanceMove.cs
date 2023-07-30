using System.Collections.Generic;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class DanceMove: MultiTurnBattleMove, IFieldEffectMove
    {
        public int EffectDuration { get; protected set; }

        public List<FieldEffect> FieldEffects { get;  }

        public DanceEffectType DanceEffect { get; protected set; }

        public DanceMove(string description, TargetType targetType, int effectDuration, DanceEffectType? danceEffect, List<FieldEffect> fieldEffects, params BattleMove[] moves)
            : base(description, targetType, moves)
        {
            EffectDuration = effectDuration;
            FieldEffects = fieldEffects;
            MoveType = BattleMoveType.Dance;
            DanceEffect = danceEffect ?? DanceEffectType.None;
        }

        public DanceMove(DanceMove copy) : base(copy)
        {
            FieldEffects = copy.FieldEffects;
            EffectDuration = copy.EffectDuration;
        }
    }
}
