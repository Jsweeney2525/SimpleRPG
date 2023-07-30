using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Screens;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public interface IBossEnemy
    {
        void PreBattleSetup(Team ownTeam, Team enemyTeam, IOutput output, BattleConfigurationSpecialFlag specialFlag);
        
        BattleMoveWithTarget GetZeroTurnMove(Team ownTeam, Team enemyTeam);
    }
}
