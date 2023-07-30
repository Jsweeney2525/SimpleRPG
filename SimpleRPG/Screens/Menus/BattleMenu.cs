using System.Collections.Generic;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public class BattleMenu : Menu
    {
        public BattleMenu(bool allowBackOption, string helpText, IInput input, IOutput output, IMenuFactory menuFactory, List<MenuAction> specialMenuActions = null )
            : base(allowBackOption, true, true, "", null, helpText, GetMenuActions(input, output, menuFactory, specialMenuActions), input, output)
        {
        }

        protected override void PrintPromptHeader(string promptText = null)
        {
            _output.WriteLine($"What action will {Owner.DisplayName} take?");
        }

        private static List<MenuAction> GetMenuActions(IInput input, IOutput output, IMenuFactory menuFactory, List<MenuAction> specialMenuActions = null)
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("fight", subMenu: menuFactory.GetMenu(MenuType.ChooseAttackTypeMenu, input, output))
            };

            if (specialMenuActions != null && specialMenuActions.Count > 0)
            {
                menuActions.Add(new MenuAction("special actions", altText: "special", subMenu: menuFactory.GetMenu(MenuType.ChooseSpecialOptionMenu, input, output, menuActions: specialMenuActions)));
            }
            
            menuActions.Add(new MenuAction("item", isDisabled: true));
            menuActions.Add(new MenuAction("run", subMenu: menuFactory.GetMenu(MenuType.ConfirmationMenu, input, output, true, prompt: "Are you sure you want to run?")));

            return menuActions;
        }
    }
}
