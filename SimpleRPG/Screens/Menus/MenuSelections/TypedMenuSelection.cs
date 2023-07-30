using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Screens.Menus.MenuSelections
{
    public class TypedMenuSelection<T> : MenuSelection
    {
        public T Item { get; }

        public TypedMenuSelection(T item, string description, BattleMove move, IFighter target, IMoveExecutor moveExecutor = null)
            :base(description, move, target, moveExecutor)
        {
            Item = item;
        }
    }
}
