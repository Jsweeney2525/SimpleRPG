using System.Collections.Generic;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class SpellSelectionMenu : Menu
    {
        private readonly IMenuFactory _menuFactory;

        public SpellSelectionMenu(bool allowBackOption, string helpText, IInput input, IOutput output, IMenuFactory menuFactory)
            : base(allowBackOption, "Which spell would you like to cast?", helpText, new List<MenuAction>(), input, output)
        {
            _menuFactory = menuFactory;
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            MenuActions = new List<MenuAction>();

            foreach (var spell in owner.Spells)
            {
                MenuActions.Add(new MenuAction($"{spell.Description} {spell.Cost}", spell.Description, isDisabled: owner.CurrentMana < spell.Cost, subMenu: _menuFactory.GetMenu(MenuType.ChooseTargetMenu, _input, _output), move: spell));
            }

            base.Build(owner, ownTeam, enemyTeam, terrainInteractables);
        }

        public override MenuSelection GetInput()
        {
            this.Build(Owner, _ownTeam, _enemyTeam, _terrainInteractables);
            
            return base.GetInput();
        }

        protected override void PrintPromptHeader(string promptText = null)
        {
            _output.WriteLine($"Which spell would you like to cast?\n{Owner.DisplayName} currently has {Owner.CurrentMana} / {Owner.MaxMana} Mana");
        }
    }
}
