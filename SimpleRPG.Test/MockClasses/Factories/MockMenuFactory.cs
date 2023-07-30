using System.Collections.Generic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public class MockMenuFactory : IMenuFactory
    {
        private readonly MenuFactory _menuFactory;

        public MockMenuFactory()
        {
            _menuFactory = new MenuFactory();

            _returnMenus = new Dictionary<MenuType, Queue<IMenu>>();
        }

        private readonly Dictionary<MenuType, Queue<IMenu>> _returnMenus;

        public IMenu GetMenu(MenuType type, IInput menuInput = null, IOutput menuOutput = null, bool allowBack = false,
            bool allowHelp = true, bool allowStatus = true, string prompt = "", string errorText = "",
            string helpText = "", List<MenuAction> menuActions = null, List<MenuAction> specialMenuActions = null,
            bool shuffleOptions = false, IChanceService chanceService = null, IMenu subMenu = null)
        {
            IMenu ret;
            IMenu realFactoryMenu = _menuFactory.GetMenu(type, menuInput, menuOutput, allowBack, allowHelp, allowStatus, prompt,
                errorText, helpText, menuActions, specialMenuActions, shuffleOptions, chanceService, subMenu);

            if (!_returnMenus.ContainsKey(type) || _returnMenus[type].Count == 0)
            {
                ret = realFactoryMenu;
            }
            else
            {
                ret = _returnMenus[type].Dequeue();

                MockMenu retAsMockMenu = ret as MockMenu;

                if (retAsMockMenu != null)
                {
                    retAsMockMenu.SetMenuActions(menuActions);
                    retAsMockMenu.SetOutput(menuOutput);
                    retAsMockMenu.SetPrompt(prompt);

                    retAsMockMenu.SetInnerMenu(realFactoryMenu);
                }
            }

            return ret;
        }

        /// <summary>
        /// pushes the menus into the <see cref="_returnMenus"/> dictionary under the appropriate key.
        /// The next time GetMenu is called with that particular menu type, the next item in the list will be returned
        /// </summary>
        /// <param name="menuType"></param>
        /// <param name="menus"></param>
        public void SetMenu(MenuType menuType, params IMenu[] menus)
        {
            Queue<IMenu> queue = _returnMenus.ContainsKey(menuType) ? _returnMenus[menuType] : new Queue<IMenu>();

            foreach (IMenu menu in menus)
            {
                queue.Enqueue(menu);
            }

            _returnMenus[menuType] = queue;
        }
    }
}
