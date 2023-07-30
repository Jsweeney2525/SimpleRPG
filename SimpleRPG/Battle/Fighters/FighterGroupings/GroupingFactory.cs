using System.Collections.Generic;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.FighterGroupings
{
    public class GroupingFactory
    {
        private IChanceService _chanceService;

        private ITeamFactory _teamFactory;

        private FighterFactory _fighterFactory;

        public GroupingFactory(IChanceService chanceService, ITeamFactory teamFactory, FighterFactory fighterFactory)
        {
            _chanceService = chanceService;
            _teamFactory = teamFactory;
            _fighterFactory = fighterFactory;
        }

        public FighterGrouping GetGrouping(FighterGroupingConfiguration config)
        {
            FighterGrouping generatedGrouping = null;

            ShadeGroupingConfiguration shadeConfig = config as ShadeGroupingConfiguration;

            if (shadeConfig != null)
            {
                List<Shade> generatedFighters = new List<Shade>();
                int minLevel = shadeConfig.MinLevel;
                 
                for (int i = 0; i < shadeConfig.NumberOfShades; ++i)
                {
                    int level = shadeConfig.MaxLevel == null ? 
                        minLevel : 
                        _chanceService.WhichEventOccurs(shadeConfig.MaxLevel.Value - minLevel) + minLevel;

                    Shade generatedShade = FighterFactory.GetShade(level);
                    generatedFighters.Add(generatedShade);
                }

                generatedGrouping = new ShadeFighterGrouping(_chanceService, generatedFighters.ToArray()) ;
            }

            return generatedGrouping;
        }
    }
}
