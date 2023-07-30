using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;

namespace SimpleRPG.Screens.Menus.MenuSelections
{
    public class MenuSelection
    {
        public IFighter Target { get; }

        public string Description { get; }

        public BattleMove Move { get; }

        public IMoveExecutor MoveExecutor { get; }

        public MenuSelection(string description, BattleMove move, IFighter target, IMoveExecutor moveExecutor = null)
        {
            Description = description;
            Move = move;
            Target = target;
            MoveExecutor = moveExecutor;
        }

        public virtual BattleMoveWithTarget Convert(IFighter owner)
        {
            return new BattleMoveWithTarget(Move, Target, owner, MoveExecutor);
        }
    }
}
