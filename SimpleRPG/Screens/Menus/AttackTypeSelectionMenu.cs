using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class AttackTypeSelectionMenu : Menu
    {
        private readonly IMenuFactory _menuFactory;

        public AttackTypeSelectionMenu(string helpText, IInput input, IOutput output, IMenuFactory menuFactory)
            : base(true, true, true, "How would you like to fight?", null, helpText, new List<MenuAction>(), input, output)
        {
            _menuFactory = menuFactory;
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            MenuActions = new List<MenuAction>
            {
                new MenuAction("attack", subMenu: _menuFactory.GetMenu(MenuType.ChooseTargetMenu, _input, _output), move: MoveFactory.Get(BattleMoveType.Attack))
            };

            if (owner.Spells.Any())
            {
                MenuActions.Add(new MenuAction("magic", subMenu: _menuFactory.GetMenu(MenuType.ChooseSpellMenu, _input, _output)));
            }

            if (owner.SpecialMoves.Any())
            {
                //MenuActions.Add(new MenuAction("special move", altText: "special"));
                MenuActions.Add(new MenuAction("special move", altText: "special", subMenu: _menuFactory.GetMenu(MenuType.ChooseSpecialAttackMenu, _input, _output)));
            }

            base.Build(owner, ownTeam, enemyTeam, terrainInteractables);
        }

        public override MenuSelection GetInput()
        {
            Build(Owner, _ownTeam, _enemyTeam, _terrainInteractables);
            
            return base.GetInput();
        }
    }
}
