using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class SelectEnemyFighterMenu : Menu
    {
        public SelectEnemyFighterMenu(IInput input, IOutput output, bool allowBack) : base(allowBack, false, false, "Select an enemy type", null, null, null, input, output)
        {
            IEnumerable<FighterType> fighterTypes = EnumHelperMethods.GetAllValuesForEnum<FighterType>();

            fighterTypes = fighterTypes.Where(ft => ft != FighterType.HumanControlledPlayer && ft != FighterType.HumanControlledEnemy && ft != FighterType.DancerBoss);
            MenuActions = new List<MenuAction>();

            NumberInputMenu numberSubMenu = new NumberInputMenu("select a level for this fighter (between 1 and 5)", input, output, 1, 5);

            foreach (FighterType fighterType in fighterTypes)
            {
                string fighterTypeString = fighterType.ToString();
                if (fighterType == FighterType.Barbarian)
                {
                    MenuAction menuAction = new TypedMenuAction<BattleConfigurationSpecialFlag>(BattleConfigurationSpecialFlag.FirstBarbarianBattle, fighterType + " (first battle)", fighterTypeString, subMenu: numberSubMenu);
                    MenuActions.Add(menuAction);
                }
                else
                {
                    MenuAction menuAction = new TypedMenuAction<BattleConfigurationSpecialFlag>(BattleConfigurationSpecialFlag.None, fighterTypeString, fighterTypeString, subMenu: numberSubMenu);
                    MenuActions.Add(menuAction);
                }
            }

            _hasBeenBuilt = true;
        }

        protected override MenuSelection GetSubMenuInput(IMenu subMenu, MenuAction selectedAction)
        {
            NumberInputMenuSelection numberInputSelection = subMenu.GetInput() as NumberInputMenuSelection;

            if (numberInputSelection == null)
            {
                throw new InvalidOperationException("SelectEnemyFighterMenu's action's subMenu did not return a MenuAction of type NumberInputMenuSelection");
            }

            TypedMenuAction<BattleConfigurationSpecialFlag> typedSelectedAction =
                selectedAction as TypedMenuAction<BattleConfigurationSpecialFlag>;

            if (typedSelectedAction == null)
            {
                throw new InvalidOperationException("SelectEnemyFighterMenu should have initialized its menu actions as TypedMenuAction<BattleConfigurationSpecialFlags>");
            }

            FighterType selectedType = (FighterType) Enum.Parse(typeof(FighterType), selectedAction.CommandText);
            
            var menuSelection = new SelectEnemyFighterMenuSelection(selectedType, numberInputSelection.Number, typedSelectedAction.Item);

            return menuSelection;
        }
    }
}
