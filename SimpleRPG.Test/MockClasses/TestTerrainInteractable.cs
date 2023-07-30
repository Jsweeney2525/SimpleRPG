using System.Collections.Generic;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Test.MockClasses
{
    public class TestTerrainInteractable : TerrainInteractable
    {
        public List<MenuAction> MenuActions;

        public TestTerrainInteractable(List<MenuAction> menuActions, string displayName = "test terrain interactable")
            : base(displayName)
        {
            MenuActions = menuActions ?? new List<MenuAction>();
        }

        public override List<MenuAction> GetInteractableMenuActions(IInput input = null, IOutput output = null)
        {
            return MenuActions;
        }
    }
}
