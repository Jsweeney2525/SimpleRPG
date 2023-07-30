using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class SelectEnemyTeamMenuManager
    {
        protected readonly IInput Input;

        protected readonly IOutput Output;

        protected List<SelectEnemyFighterMenu> Menus;

        public SelectEnemyTeamMenuManager(IInput input, IOutput output)
        {
            Input = input;
            Output = output;

            Menus = new List<SelectEnemyFighterMenu>(5);

            for (var i = 0; i < 5; ++i)
            {
                Menus.Add(new SelectEnemyFighterMenu(input, output, i > 0));
            }
        }

        public Team GetTeam(MenuManager menuManager, IInput input, IOutput output, out BattleConfigurationSpecialFlag battleFlag)
        {
            List<IFighter> fighters = new List<IFighter>
            {
                null,
                null,
                null,
                null,
                null
            };
            bool continuer = true;
            battleFlag = BattleConfigurationSpecialFlag.None;

            ConfirmationMenu humanControlledTeamConfirmationMenu = new ConfirmationMenu(false, "Do you want this team to be human controlled?", input, output);
            MenuSelection confirmationInput = humanControlledTeamConfirmationMenu.GetInput();

            bool isHumanControlledTeam = confirmationInput.Description == "yes";

            for (var i = 0; i < 5 && continuer;)
            {
                SelectEnemyFighterMenu menu = Menus[i];

                MenuSelection menuSelection = menu.GetInput();

                if (i > 0 && menuSelection.Description == "back")
                {
                    --i;
                }
                else
                {
                    SelectEnemyFighterMenuSelection selectEnemyMenuSelection =
                        menuSelection as SelectEnemyFighterMenuSelection;

                    if (selectEnemyMenuSelection == null)
                    {
                        throw new InvalidOperationException(
                            "SelectEnemyFighterMenu.GetInput() didn't return a SelectEnemyFighterMenuSelection");
                    }

                    FighterFactory.SetInput(input);
                    FighterFactory.SetOutput(output);

                    IFighter returnedEnemy = FighterFactory.GetFighter(selectEnemyMenuSelection.FighterType, selectEnemyMenuSelection.FighterLevel);

                    if (!(returnedEnemy is EnemyFighter))
                    {
                        throw new InvalidOperationException(
                            $"The selected FighterType {selectEnemyMenuSelection.FighterType} cannot be cast to an EnemyFighter");
                    }
                    EnemyFighter enemy = (EnemyFighter) returnedEnemy;
                    

                    IFighter fighterToAdd;
                    if (isHumanControlledTeam)
                    {
                        HumanControlledEnemyFighter humanControlledEnemy = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
                        humanControlledEnemy.SetEnemy(enemy);
                        fighterToAdd = humanControlledEnemy;
                    }
                    else
                    {
                        fighterToAdd = enemy;
                    }

                    fighters[i] = fighterToAdd;

                    if (selectEnemyMenuSelection.SpecialFlag != BattleConfigurationSpecialFlag.None &&
                        battleFlag == BattleConfigurationSpecialFlag.None)
                    {
                        battleFlag = selectEnemyMenuSelection.SpecialFlag;
                    }

                    if (i < 4)
                    {
                        ConfirmationMenu addAnotherFighrerConfirmationMenu = new ConfirmationMenu(false, "Do you want to add another fighter?", input, output);

                        confirmationInput = addAnotherFighrerConfirmationMenu.GetInput();

                        if (confirmationInput.Description == "no")
                        {
                            continuer = false;
                        }
                    }

                    ++i;
                }
            }

            fighters = fighters.Where(f => f != null).ToList();

            Team ret = new Team(menuManager, fighters);

            return ret;
        }
    }
}
