using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class SpecialMoveSelectionMenu : Menu
    {
        private readonly IMenuFactory _menuFactory;

        public SpecialMoveSelectionMenu(string helpText, IInput input, IOutput output, IMenuFactory menuFactory)
            : base(true, "Which special move would you like to use?", helpText, new List<MenuAction>(), input, output)
        {
            _menuFactory = menuFactory;
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            MenuActions = new List<MenuAction>();

            HumanFighter ownerAsHuman = owner as HumanFighter;

            IEnumerable<BattleMove> moves;

            if (ownerAsHuman != null)
            {
                moves = ownerAsHuman.AllSpecialMoves;
            }
            else
            {
                moves = owner.SpecialMoves;
            }

            foreach (var move in moves)
            {
                MenuActions.Add(new MenuAction(move.Description, subMenu: _menuFactory.GetMenu(MenuType.ChooseTargetMenu, _input, _output), move: move));
            }

            base.Build(owner, ownTeam, enemyTeam, terrainInteractables);
        }

        public override MenuSelection GetInput()
        {
            this.Build(Owner, _ownTeam, _enemyTeam, _terrainInteractables);
            
            return base.GetInput();
        }
    }
}
