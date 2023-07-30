using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class ShadeAbsorbingMove : SpecialMove
    {
        public ShadeAbsorbingMove(string description, string executionText) 
            : base(description, BattleMoveType.AbsorbShade, TargetType.SingleAlly, executionText, 
                  targettingRuleCollection: new SpecialTargettingRuleCollection(new RestrictedFighterTypesSpecialTargettingRule<Shade>()))
        {
        }

        public ShadeAbsorbingMove(BattleMove copy) : base(copy)
        {
        }
    }
}
