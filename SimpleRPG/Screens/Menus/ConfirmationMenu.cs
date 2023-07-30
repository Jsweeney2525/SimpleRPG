using System.Collections.Generic;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class ConfirmationMenu : Menu
    {
        public ConfirmationMenu(bool allowBackOption, string prompt, IInput input, IOutput output) :
            base(allowBackOption, false, false, prompt, null, "", GenerateMenuActions(), input, output)
        {
            //The confirmationMenu shouldn't need an owner, or teams
            _hasBeenBuilt = true;
        }

        private static List<MenuAction> GenerateMenuActions()
        {
            return new List<MenuAction>
            {
                new MenuAction("yes", altText: "y"),
                new MenuAction("no", altText: "n")
            };
        }

        public MenuSelection GetInput(MenuAction action)
        {
            var originalRet = base.GetInput();

            MenuSelection newRet;

            switch (originalRet.Description)
            {
                case "yes":
                    newRet = action.ConvertToMenuSelection();
                    break;
                default:
                    newRet = new MenuSelection("back", null, null);
                    break;
            }

            return newRet;
        }
    }
}
