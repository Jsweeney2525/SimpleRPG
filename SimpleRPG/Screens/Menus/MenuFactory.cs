using System.Collections.Generic;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public class MenuFactory : IMenuFactory
    {
        public IMenu GetMenu(MenuType type, IInput menuInput = null, IOutput menuOutput = null, bool allowBack = false,
            bool allowHelp = true, bool allowStatus = true, string prompt = "", string errorText = "",
            string helpText = "", 
            List<MenuAction> menuActions = null, List<MenuAction> specialMenuActions = null, 
            bool shuffleOptions = false, IChanceService chanceService = null,
            IMenu subMenu = null)
        {
            if (menuInput == null)
            {
                menuInput = new ConsoleInput();
            }
            if (menuOutput == null)
            {
                menuOutput = new ConsoleOutput();
            }
            if (errorText == null)
            {
                errorText = Globals.GenericErrorMessage;
            }
            if (menuActions == null)
            {
                menuActions = new List<MenuAction>();
            }
            if (chanceService == null)
            {
                chanceService = Globals.ChanceService;
            }

            IMenu ret;

            switch (type)
            {
                case MenuType.NonSpecificMenu:
                    ret = _CreateBasicMenu(menuInput, menuOutput, allowBack, allowHelp, allowStatus, prompt, errorText, helpText, menuActions, shuffleOptions, chanceService);
                    break;
                case MenuType.ConfirmationMenu:
                    ret = _CreateConfirmationMenu(prompt, allowBack, menuInput, menuOutput);
                    break;
                case MenuType.TutorialMenu:
                    ret = _CreateConfirmationMenu("Would you like a quick tutorial on how to play SimpleRPG? (y/n also accepted)", false, menuInput, menuOutput);
                    break;
                case MenuType.BattleMenu:
                    ret = _CreateBattleMenu(menuInput, menuOutput, allowBack, specialMenuActions);
                    break;
                case MenuType.ChooseTargetMenu:
                    ret = _CreateSelectTargetMenu(menuInput, menuOutput);
                    break;
                case MenuType.ChooseAttackTypeMenu:
                    ret = _CreateChooseAttackTypeMenu(menuInput, menuOutput);
                    break;
                case MenuType.ChooseSpellMenu:
                    ret = _CreateChooseSpellMenu(menuInput, menuOutput);
                    break;
                case MenuType.ChooseSpecialAttackMenu:
                    ret = _CreateChooseSpecialMoveMenu(menuInput, menuOutput);
                    break;
                case MenuType.ChooseSpecialOptionMenu:
                    ret = _CreateChooseSpecialOptionMenu(menuInput, menuOutput, menuActions);
                    break;
                case MenuType.KeysOffOwnerNumberInputMenu:
                    ret = _CreateKeysOffOwnerNumberInputMenu(prompt, menuInput, menuOutput, subMenu);
                    break;
                default:
                    ret = _CreateBattleMenu(menuInput, menuOutput, allowBack, specialMenuActions);
                    break;
            }

            return ret;
        }

        private static Menu _CreateBasicMenu(IInput menuInput, IOutput menuOutput, bool allowBack,
            bool allowHelp, bool allowStatus, string prompt, string errorText,
            string helpText, List<MenuAction> menuActions, bool shuffleOptions, IChanceService chanceService)
        {
            return new Menu(allowBack, allowHelp, allowStatus, prompt, errorText, helpText, menuActions, menuInput, menuOutput, chanceService, shuffleOptions);
        }

        private static IMenu _CreateConfirmationMenu(string prompt, bool allowBack, IInput menuInput, IOutput menuOutput)
        {
            return new ConfirmationMenu(allowBack, prompt, menuInput, menuOutput);
        }

        private IMenu _CreateBattleMenu(IInput menuInput, IOutput menuOutput, bool allowBack, List<MenuAction> specialMenuActions)
        {
            const string helpText = "This is your basic battle menu. You are facing a monster and can either choose to do damage with the \"attack\" command,\n" +
                                    "use an item for various effect\nOr you could run, you coward.";
            return new BattleMenu(allowBack, helpText, menuInput, menuOutput, this, specialMenuActions);
        }

        private IMenu _CreateChooseAttackTypeMenu(IInput menuInput, IOutput menuOutput)
        {
            string helpText = "An attack is just using yoru fists or whatever, while magic uses some MP to cause various effects.\n";
            helpText += "Try different strategies on different monsters!";

            return new AttackTypeSelectionMenu(helpText, menuInput, menuOutput, this);
        }

        private IMenu _CreateChooseSpellMenu(IInput menuInput, IOutput menuOutput)
        {
            string helpText = "A spell costs Mana and is usually associated with an element.\n";
            helpText += "Wind is strong against earthen enemies,\n";
            helpText += "Earth is strong against aquatic enemies,\n";
            helpText += "Water is strong against fiery enemies,\n";
            helpText += "Fire is strong against airborn enemies";

            return new SpellSelectionMenu(true, helpText, menuInput, menuOutput, this);
        }

        private IMenu _CreateChooseSpecialMoveMenu(IInput menuInput, IOutput menuOutput)
        {
            string helpText = "A special move does not require mana as a spell does.\n";
            helpText += "However, they have different effects, compared to an attack that just deals basic damage";

            return new SpecialMoveSelectionMenu(helpText, menuInput, menuOutput, this);
        }

        private IMenu _CreateChooseSpecialOptionMenu(IInput menuInput, IOutput menuOutput, List<MenuAction> menuActions)
        {
            string helpText = "Special options are situational, they may not carry from one battle to the next";
            return new Menu(true, true, true, "Select what action to take:", null, helpText, menuActions, menuInput, menuOutput);
        }

        private static IMenu _CreateSelectTargetMenu(IInput menuInput, IOutput menuOutput)
        {
            string menuPrompt = "Who will you target for this action?";
            string helpText = "A spell costs MP and is usually associated with an element.\n";
            helpText += "Wind is strong against earthen enemies,\n";
            helpText += "Earth is strong against aquatic enemies,\n";
            helpText += "Water is strong against fiery enemies,\n";
            helpText += "Fire is strong against airborn enemies";

            return new TargetMenu(menuPrompt, helpText, menuInput, menuOutput);
        }

        private static IMenu _CreateKeysOffOwnerNumberInputMenu(string prompt, IInput input, IOutput output, IMenu subMenu)
        {
            return new KeysOffOwnerNumberInputMenu(prompt, input, output, subMenu);
        }
    }
}
