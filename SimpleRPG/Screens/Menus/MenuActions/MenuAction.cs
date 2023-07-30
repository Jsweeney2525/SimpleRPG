using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus.MenuActions
{
    /// <summary>
    /// A class to represent something that can be selected in a menu. The current use cases are:
    /// SubMenu is null- this represents a "leaf" in a menu tree. Fighter should be non-null
    /// </summary>
    public class MenuAction
    {
        /// <summary>
        /// How the Action will be displayed to the user
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// The text to be matched to this menu action. For instance, the display text may be "fireball 3", 
        /// but the commandText would be "fireball"
        /// </summary>
        public string CommandText { get; }

        /// <summary>
        /// The alternative text that can also be accepted as the command text
        /// </summary>
        public string AltText { get; }

        public bool IsDisabled { get; }

        public bool IsHidden { get; }

        /// <summary>
        /// What SubMenu to call up if the menu option is selected
        /// </summary>
        public IMenu SubMenu { get; }

        /// <summary>
        /// The fighter represented by an action- i.e. the 
        /// </summary>
        public IFighter Fighter { get; protected set; }

        /// <summary>
        /// The move that represents this action's selection (e.g. "attack" or "fireball")
        /// </summary>
        public BattleMove BattleMove { get; protected set; }

        /// <summary>
        /// Some actions/BattleMoves may need to be given to a special executor. If this value is not null, that's what will be executing the move's logic
        /// </summary>
        public IMoveExecutor MoveExecutor { get; }

        public MenuAction(string displayText, 
            string commandText = null, 
            string altText = null, 
            bool isDisabled = false, 
            bool isHidden = false, 
            IMenu subMenu = null, 
            BattleMove move = null, 
            IFighter fighter = null,
            IMoveExecutor moveExecutor = null)
        {
            DisplayText = displayText;
            CommandText = commandText ?? displayText;
            IsDisabled = isDisabled;
            IsHidden = isHidden;
            AltText = altText;
            SubMenu = subMenu;
            Fighter = fighter;
            BattleMove = move;
            MoveExecutor = moveExecutor;
        }

        public virtual MenuSelection ConvertToMenuSelection()
        {
            return new MenuSelection(DisplayText, BattleMove, Fighter, MoveExecutor);
        }
    }
}
