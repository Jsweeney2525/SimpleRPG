using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public class HumanControlledEnemyMenu : Menu
    {
        private readonly IMenuFactory _menuFactory;

        public HumanControlledEnemyMenu(IInput input, IOutput output, IMenuFactory menuFactory) : base(false, false, false, "", null, null, null, input, output)
        {
            _menuFactory = menuFactory;
        }

        public void Build(HumanControlledEnemyFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            Owner = owner;
            _ownTeam = ownTeam;
            _enemyTeam = enemyTeam;

            _hasBeenBuilt = true;

            EnemyFighter ownerAsEnemyFighter = owner.Fighter;

            if (ownerAsEnemyFighter == null)
            {
                throw new ArgumentException("HumanControlledEnemyMenu should only be built with an instance of Enemy Fighter", nameof(owner));
            }

            MenuActions = ownerAsEnemyFighter.AvailableMoves.Select(
                move => new MenuAction(move.Description, subMenu: _menuFactory.GetMenu(MenuType.ChooseTargetMenu, _input, _output), move: move))
                    .ToList();

            foreach (var menuAction in MenuActions)
            {
                menuAction.SubMenu?.Build(owner, ownTeam, enemyTeam, terrainInteractables);
            }
        }

        protected override void PrintPromptHeader(string promptText = null)
        {
            _output.WriteLine($"You are currently selecting a move for {Owner.DisplayName}. What move will you use?", ConsoleColor.Cyan);
        }
    }
}
