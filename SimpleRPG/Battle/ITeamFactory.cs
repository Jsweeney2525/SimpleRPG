using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Regions;

namespace SimpleRPG.Battle
{
    public interface ITeamFactory
    {
        Team GetTeam(SubRegion region);

        Team GetTeam(TeamConfiguration configuration);

        Team GetTeam(FighterGrouping grouping);
    }
}
