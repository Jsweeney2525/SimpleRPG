using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class TargetMenu : Menu
    {
        public TargetMenu(string prompt, string helpText, IInput input, IOutput output)
            //a target menu is always a subMenu, and therefor should always allow the back option
            : base(true, prompt, helpText, new List<MenuAction>(), input, output, true)
        {
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            base.Build(owner, ownTeam, enemyTeam, terrainInteractables);

            BuildMenuActions();
        }

        public override MenuSelection GetInput(BattleMove move, IMoveExecutor moveExecutor)
        {
            BuildMenuActions(move);

            var originalRet = base.GetInput();

            return (originalRet.Description == "back") ? new MenuSelection("back", null, null) : new MenuSelection("", move, originalRet.Target, moveExecutor);
        }

        protected void BuildMenuActions(BattleMove move = null)
        {
            if (move == null || move.TargetType == TargetType.SingleEnemy)
            {
                MenuActions = _enemyTeam.Fighters.Where(enemy => enemy.IsAlive()).Select(enemy => new MenuAction(enemy.DisplayName, fighter: enemy)).ToList();
            }
            else {
                switch (move.TargetType)
                {
                    case TargetType.SingleAlly:
                    case TargetType.SingleAllyOrSelf:
                        IEnumerable<IFighter> fighters = _ownTeam.Fighters.Where(fighter => fighter.IsAlive());

                        if (move.TargetType == TargetType.SingleAlly)
                        {
                            fighters = fighters.Where(f => f != Owner);
                        }

                        MenuActions = fighters.Select(fighter => new MenuAction(fighter.DisplayName, fighter: fighter)).ToList();

                        break;
                    case TargetType.Self:
                        MenuActions = new List<MenuAction>
                        {
                            new MenuAction(Owner.DisplayName, fighter: Owner)
                        };
                        break;
                }
            }
        }
    }
}
