using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus.MenuActions
{
    /// <summary>
    /// A class to represent something that can be selected in a menu. The current use cases are:
    /// SubMenu is null- this represents a "leaf" in a menu tree. Fighter should be non-null
    /// </summary>
    public class TypedMenuAction<T> : MenuAction
    {
        public T Item { get; }

        public TypedMenuAction(T item, string displayText, string commandText = null, string altText = null, bool isDisabled = false, bool isHidden = false, IMenu subMenu = null, BattleMove move = null, IFighter fighter = null)
            :base(displayText, commandText, altText, isDisabled, isHidden, subMenu, move, fighter)
        {
            Item = item;
        }

        public override MenuSelection ConvertToMenuSelection()
        {
            return new TypedMenuSelection<T>(Item, DisplayText, BattleMove, Fighter, MoveExecutor);
        }
    }
}
