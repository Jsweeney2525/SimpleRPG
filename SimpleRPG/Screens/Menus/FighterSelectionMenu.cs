using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public class FighterSelectionMenu : Menu
    {
        private static List<MenuAction> GenerateMenuActions(IEnumerable<IFighter> fighters, MenuAction[] additionalMenuActions)
        {
            IEnumerable<MenuAction> menuActions = fighters.Select(f => new MenuAction(f.DisplayName, fighter: f));

            if (additionalMenuActions != null)
            {
                menuActions = menuActions.Concat(additionalMenuActions);
            }

            return menuActions.ToList();
        }

        public FighterSelectionMenu(string prompt, IInput input, IOutput output, MenuAction[] additionalMenuActions = null, params IFighter[] fighters) 
            : base(false, false, false, prompt, null, null, GenerateMenuActions(fighters, additionalMenuActions), input, output)
        {
            _hasBeenBuilt = true;
        }
    }
}
