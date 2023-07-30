using System;
using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Battle.TerrainInteractables
{
    public class Bell : TerrainInteractable, IMoveExecutor
    {
        protected IMenuFactory MenuFactory { get; }

        protected IChanceService ChanceService { get; }

        public BellType BellType { get; }

        public Bell(string displayName, BellType bellType, IMenuFactory menuFactory, IChanceService chanceService)
            :base(displayName)
        {
            BellType = bellType;
            MenuFactory = menuFactory;
            ChanceService = chanceService;
        }

        public override List<MenuAction> GetInteractableMenuActions(IInput input = null, IOutput output = null)
        {
            IMenu offerBloodTargetMenu = MenuFactory.GetMenu(MenuType.ChooseTargetMenu, input, output);
            IMenu offerBloodSubMenu = MenuFactory.GetMenu(MenuType.KeysOffOwnerNumberInputMenu, input, output, prompt: "how much HP shall you offer?", subMenu: offerBloodTargetMenu);

            IMenu praySubMenu = MenuFactory.GetMenu(MenuType.ChooseTargetMenu, input, output);
            string bellType = BellType.ToString().ToLower();
            string bloodShortcut = $"blood {bellType}";
            string prayShortcut = $"pray {bellType}";

            return new List<MenuAction>
            {
                new MenuAction($"offer blood to {DisplayName} ('{bloodShortcut}')",
                altText: bloodShortcut,
                move: MoveFactory.Get(BellMoveType.ControlMove),
                moveExecutor: this,
                subMenu: offerBloodSubMenu),

                new MenuAction($"pray to {DisplayName} ('{prayShortcut}')", 
                altText: prayShortcut, 
                move: MoveFactory.Get(BellMoveType.SealMove),
                moveExecutor: this,
                subMenu: praySubMenu)
            };
        }

        //TODO: this should have return type void and print messages should queue off events, not a return value
        public bool ExecuteMove(BattleMoveWithTarget battleMoveWithTarget)
        {
            bool moveSucceeded = false;
            BellMove bellMove = battleMoveWithTarget.Move as BellMove;

            if (bellMove == null)
            {
                throw new InvalidOperationException($"Bell.ExecuteMove() was erroneously called for a move that was not a Bell move. Movetype: {battleMoveWithTarget.Move.MoveType}");
            }

            Shade targetAsShade = battleMoveWithTarget.Target as Shade;

            if (targetAsShade == null)
            {
                throw new InvalidOperationException($"Bell.ExecuteMove() was given a target that was not a shade! target: {battleMoveWithTarget.Target}");
            }

            int healthDiff = targetAsShade.MaxHealth - targetAsShade.CurrentHealth;
            double healthPercentage = ((double)targetAsShade.CurrentHealth) / targetAsShade.MaxHealth;

            double chance = 1.0/3;

            if (healthDiff > 0)
            {
                if (healthPercentage < 0.26)
                {
                    chance = 0.8;
                }
                else if (healthPercentage < 0.51)
                {
                    chance = 0.65;
                }
                else if (healthPercentage < 0.76)
                {
                    chance = 0.5;
                }
            }

            if (targetAsShade.ShadeExperience > 1)
            {
                int levelDiff = targetAsShade.ShadeExperience - 1;

                chance -= 0.1*levelDiff;

                //minimum chance is 10% that it works
                chance = Math.Max(.1, chance);
            }

            if (bellMove.BellMoveType == BellMoveType.ControlMove)
            {
                BattleMoveWithTargetAndNumberInput moveWithNumber = battleMoveWithTarget as BattleMoveWithTargetAndNumberInput;

                if (moveWithNumber == null)
                {
                    throw new InvalidOperationException("Bell.ExecuteMove() should be supplied a BattleMoveWIthTargetAndNUmberInput if the move was a control type Bell move");
                }

                int bloodAmount = moveWithNumber.NumberValue;

                chance += bloodAmount*0.05;
            }

            if ((BellType == BellType.Silver && bellMove.BellMoveType == BellMoveType.SealMove) ||
                (BellType == BellType.Copper && bellMove.BellMoveType == BellMoveType.ControlMove))
            {
                chance += .25;
            }

            if (ChanceService.EventOccurs(chance))
            {
                int healAmount = targetAsShade.CurrentHealth;
                targetAsShade.Seal();

                moveSucceeded = true;

                if (bellMove.BellMoveType == BellMoveType.SealMove)
                {
                    battleMoveWithTarget.Owner.Heal(healAmount);
                }
                else if (bellMove.BellMoveType == BellMoveType.ControlMove)
                {
                    battleMoveWithTarget.Owner.RaiseMaxHealth(targetAsShade.ShadeExperience);
                }
            }

            return moveSucceeded;
        }
    }
}
