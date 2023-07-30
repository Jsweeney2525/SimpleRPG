using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Screens.Menus.MenuSelections
{
    /// <summary>
    /// A class to represent something that can be selected in a menu. The current use cases are:
    /// SubMenu is null- this represents a "leaf" in a menu tree. Fighter should be non-null
    /// </summary>
    public class NumberInputMenuSelection : MenuSelection
    {
        public int Number { get; }

        public NumberInputMenuSelection(int number, string description = "", BattleMove move = null, IFighter target = null, IMoveExecutor moveExecutor = null)
            :base(description, move, target, moveExecutor)
        {
            Number = number;
        }

        public override BattleMoveWithTarget Convert(IFighter owner)
        {
            return new BattleMoveWithTargetAndNumberInput(Move, Target, owner, Number, MoveExecutor);
        }
    }
}
