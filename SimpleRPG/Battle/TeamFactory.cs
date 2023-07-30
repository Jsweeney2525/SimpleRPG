using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using System.Collections.Generic;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Regions;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Battle
{
    public class TeamFactory : ITeamFactory
    {
        private readonly IChanceService _chanceService;

        private readonly IMenuFactory _menuFactory;

        public TeamFactory(IChanceService chanceService, IMenuFactory menuFactory)
        {
            _chanceService = chanceService;
            _menuFactory = menuFactory;
        }

        public Team GetTeam(SubRegion region)
        {
            var enemyList = new List<IFighter>();

            int numEnemies = _chanceService.WhichEventOccurs(region.NumberEnemyFighterChances);

            for (var i = 0; i < numEnemies; ++i)
            {
                enemyList.Add(GetEnemy(region));
            }

            //TODO: MenuManager should probably be supplied, perhaps?
            return new Team(new MenuManager(new ConsoleInput(), new ConsoleOutput(), _menuFactory), enemyList);
        }

        private EnemyFighter GetEnemy(SubRegion region)
        {
            //FighterType[] types = regionFighterTypesDictionary[region.AreaId];
            //var length = types.Length;
            //var odds = new double[length];

            //for (var i = 0; i < length; ++i)
            //{
            //    odds[i] = (1.0/length);
            //}

            FighterType selectedFighterType = _chanceService.WhichEventOccurs(region.EnemyTypes);

            return (EnemyFighter)FighterFactory.GetFighter(selectedFighterType, 1);
        }

        public Team GetTeam(TeamConfiguration configuration)
        {
            List<IFighter> fighters = new List<IFighter>();
            foreach (EnemyConfiguration enemyConfiguration in configuration.Enemies)
            {
                fighters.Add(GetEnemy(enemyConfiguration));
            }

            return new Team(new MenuManager(new ConsoleInput(), new ConsoleOutput(), _menuFactory), fighters);
        }

        private static EnemyFighter GetEnemy(EnemyConfiguration configuration)
        {
            return (EnemyFighter)FighterFactory.GetFighter(configuration.EnemyType, configuration.Level, magicType: configuration.MagicType);
        }

        public Team GetTeam(FighterGrouping grouping)
        {
            List<IFighter> fighters = grouping.GetFighters();

            return new Team(new MenuManager(new ConsoleInput(), new ConsoleOutput(), _menuFactory), fighters);
        }
    }
}
