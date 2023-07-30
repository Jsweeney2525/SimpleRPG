using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Battle.BattleMoves
{
    public class BattleMoveWithTargetAndNumberInput : BattleMoveWithTarget
    {
        public int NumberValue { get; }

        public BattleMoveWithTargetAndNumberInput(BattleMove move, IFighter target, IFighter owner, int numberValue, IMoveExecutor moveExecutor = null)
            :base(move, target, owner, moveExecutor)
        {
            NumberValue = numberValue;
        }

        public BattleMoveWithTargetAndNumberInput(MenuSelection selection, IFighter owner, int numberValue)
            :base(selection, owner)
        {
            NumberValue = numberValue;
        }
    }
}
