using System.Collections.Generic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public interface IMenuFactory
    {
        IMenu GetMenu(MenuType type, 
            IInput menuInput = null, 
            IOutput menuOutput = null, 
            bool allowBack = false,
            bool allowHelp = true, 
            bool allowStatus = true, 
            string prompt = "", 
            string errorText = "",
            string helpText = "", 
            List<MenuAction> menuActions = null,
            List<MenuAction> specialMenuActions = null,
            bool shuffleOptions = false, 
            IChanceService chanceService = null
            , IMenu subMenu = null);
       
    }
}
