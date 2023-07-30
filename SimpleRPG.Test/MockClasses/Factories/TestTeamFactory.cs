using System.Collections.Generic;
using SimpleRPG.Battle;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Helpers;
using SimpleRPG.Regions;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses.Enemies;

namespace SimpleRPG.Test.MockClasses.Factories
{ 
    public class TestTeamFactory : ITeamFactory
    {
        private readonly IChanceService _chanceService;
        private readonly TeamFactory _realTeamFactory;
        private readonly Queue<Team> _teams;

        public TestTeamFactory(IChanceService chanceService, IMenuFactory menuFactory = null)
        {
            _chanceService = chanceService;
            _realTeamFactory = new TeamFactory(chanceService, menuFactory ?? Globals.MenuFactory);
            _teams = new Queue<Team>();
        }

        /// <summary>
        /// Used to generate a team consisting of X testEnemyFighters, each with the given name, if supplied.
        /// This is specific to TestTeamFactory and won't be called by any classes in the main project
        /// </summary>
        /// <param name="numEnemies"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Team GetTeam(int numEnemies, string name = null)
        {
            List<IFighter> enemyList = new List<IFighter>();
            TestFighterFactory.SetChanceService(_chanceService);

            for (var i = 0; i < numEnemies; ++i)
            {
                enemyList.Add((TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, name));
            }

            return new Team(TestMenuManager.GetTestMenuManager(), enemyList);
        }

        public void PushTeams(params Team[] teams)
        {
            foreach (Team team in teams)
            {
                _teams.Enqueue(team);
            }
        }

        public Team GetTeam(SubRegion region)
        {
            return _teams.Count > 0 ? _teams.Dequeue() : _realTeamFactory.GetTeam(region);
        }

        public Team GetTeam(TeamConfiguration configuration)
        {
            return _teams.Count > 0 ? _teams.Dequeue() : _realTeamFactory.GetTeam(configuration);
        }

        public Team GetTeam(FighterGrouping grouping)
        {
            return _teams.Count > 0 ? _teams.Dequeue() : _realTeamFactory.GetTeam(grouping);
        }
    }
}