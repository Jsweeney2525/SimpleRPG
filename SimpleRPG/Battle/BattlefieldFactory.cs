using System.Collections.Generic;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Battle
{
    public abstract class FighterGroupingConfiguration
    {
        public int MinLevel { get; }

        /// <summary>
        /// If supplied, indicates the range of values for level.
        /// If excluded, all enemies will be generated at level <see cref="MinLevel"/>
        /// </summary>
        public int? MaxLevel { get; }

        protected FighterGroupingConfiguration(int minLevel, int? maxLevel = null)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
        }
    }

    public class ShadeGroupingConfiguration : FighterGroupingConfiguration
    {
        public int NumberOfShades { get; }

        public ShadeGroupingConfiguration(int numberOfShades, int minLevel, int? maxLevel = null)
            : base(minLevel, maxLevel)
        {
            NumberOfShades = numberOfShades;
        }
    }

    public class TerrainInteractablesConfiguration
    {
    }

    public class BellTerrainConfiguration : TerrainInteractablesConfiguration
    {
        public List<BellType> BellTypes { get; }

        public BellTerrainConfiguration(params BellType[] bellTypes)
        {
            BellTypes = new List<BellType>();

            if (bellTypes != null)
            {
                BellTypes.AddRange(bellTypes);
            }
        }
    }

    public class BattlefieldConfiguration
    {
        public FighterGroupingConfiguration GroupingConfiguration { get; }

        public TerrainInteractablesConfiguration TerrainConfiguration { get; }

        public TeamConfiguration TeamConfiguration { get; }

        public BattlefieldConfiguration(TeamConfiguration teamConfiguration, TerrainInteractablesConfiguration terrainConfiguration = null)
        {
            TeamConfiguration = teamConfiguration;
            TerrainConfiguration = terrainConfiguration;
        }

        public BattlefieldConfiguration(FighterGroupingConfiguration groupingConfiguration, TerrainInteractablesConfiguration terrainConfiguration)
        {
            GroupingConfiguration = groupingConfiguration;
            TerrainConfiguration = terrainConfiguration;
        }
    }

    public class BattleFieldInfo
    {
        public Team EnemyTeam { get; }

        public IEnumerable<TerrainInteractable> TerrainInteractables { get; }

        public BattleFieldInfo(Team team, IEnumerable<TerrainInteractable> terrainInteractables)
        {
            EnemyTeam = team;
            TerrainInteractables = terrainInteractables;
        }
    }

    public class BattlefieldFactory
    {
        private ITeamFactory _teamFactory;
        private IMenuFactory _menuFactory;
        private IChanceService _chanceService;
        private GroupingFactory _groupingFactory;

        public BattlefieldFactory(ITeamFactory teamFactory, GroupingFactory groupingFactory, IMenuFactory menuFactory, IChanceService chanceService)
        {
            _teamFactory = teamFactory;
            _groupingFactory = groupingFactory;
            _menuFactory = menuFactory;
            _chanceService = chanceService;
        }

        public BattleFieldInfo GetBattleFieldSetUp(BattlefieldConfiguration config)
        {
            Team generatedTeam = null;

            if (config.GroupingConfiguration != null)
            {
                FighterGrouping generatedGrouping = _groupingFactory.GetGrouping(config.GroupingConfiguration);
                generatedTeam = _teamFactory.GetTeam(generatedGrouping);

            }

            if (config.TeamConfiguration != null)
            {
                generatedTeam = _teamFactory.GetTeam(config.TeamConfiguration);
            }

            List<TerrainInteractable> terrainInteractables = new List<TerrainInteractable>();

            BellTerrainConfiguration bellConfiguration = config.TerrainConfiguration as BellTerrainConfiguration;

            if (bellConfiguration != null)
            {
                foreach (BellType bellType in bellConfiguration.BellTypes)
                {
                    terrainInteractables.Add(new Bell($"{bellType.ToString().ToLower()} bell", bellType, _menuFactory, _chanceService));
                }
            }

            BattleFieldInfo generatedBattlefieldInfo = new BattleFieldInfo(generatedTeam, terrainInteractables);

            return generatedBattlefieldInfo;
        }
    }
}
