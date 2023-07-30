using System.Collections.Generic;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Battle.TerrainInteractables
{
    //TODO: Status screen should show what terrain interactables are on the field
    public abstract class TerrainInteractable
    {
        //note: the DisplayName property must be initialzied already taking duplicates into accounts.
        //that is, any class consuming the Bell must manually add "A" or "B" or similar suffix
        //also, it's displayed as "pray to [DisplayName]"
        public string DisplayName { get; }

        protected TerrainInteractable(string displayName)
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Generates menu actions to be displayed under the "special actions" menu
        /// </summary>
        /// <param name="input">An optional parameter that will be passed into any subMenus of the menuActions</param>
        /// <param name="output">An optional parameter that will be passed into any subMenus of the menuActions</param>
        /// <returns></returns>
        public abstract List<MenuAction> GetInteractableMenuActions(IInput input = null, IOutput output = null);

        public virtual string GetFullDisplayString()
        {
            return $"There is a {DisplayName} on the field";
        }
    }
}
