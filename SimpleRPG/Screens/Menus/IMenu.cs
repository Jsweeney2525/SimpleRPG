using System.Collections.Generic;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public interface IMenu
    {
        IFighter Owner { get; }

        bool RequiresBattleMoveInput { get; }

        void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables);

        MenuSelection GetInput();

        MenuSelection GetInput(BattleMove move, IMoveExecutor moveExecutor);
    }
}
