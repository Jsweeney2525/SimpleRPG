using System.Collections.Generic;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class KeysOffOwnerNumberInputMenu : NumberInputMenu
    {
        public IMenu SubMenu { get; }

        public KeysOffOwnerNumberInputMenu(string prompt, IInput input, IOutput output, IMenu subMenu = null) 
            : base(prompt, input, output, 1, 0)
        {
            SubMenu = subMenu;
            if (subMenu != null && subMenu.RequiresBattleMoveInput)
            {
                RequiresBattleMoveInput = true;
            }
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            //TODO: What to do if owner has 1 current HP? 
            //note: currently assumed this menu is driven by the player's HP, and that they must offer less than current HP
            MaxValue = owner.CurrentHealth - 1;

            SubMenu?.Build(owner, ownTeam, enemyTeam, terrainInteractables);
        }

        public override MenuSelection GetInput(BattleMove move, IMoveExecutor moveExecutor)
        {
            MenuSelection ret;

            NumberInputMenuSelection baseInputSelection = base.GetInput() as NumberInputMenuSelection;

            if (SubMenu == null)
            {
                ret = baseInputSelection;
            }
            else
            {
                MenuSelection subMenuSelection = SubMenu.RequiresBattleMoveInput ? SubMenu.GetInput(move, moveExecutor) : SubMenu.GetInput();

                ret = new NumberInputMenuSelection(baseInputSelection?.Number ?? 0, 
                    subMenuSelection.Description, subMenuSelection.Move, subMenuSelection.Target, moveExecutor);
            }

            return ret;
        }
    }
}
