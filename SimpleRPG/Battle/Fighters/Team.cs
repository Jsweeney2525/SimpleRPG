using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Battle.Fighters
{
    public class Team : ITeam<IFighter>
    {
        public List<IFighter> Fighters { get; }

        protected MenuManager MenuManager;

        #region events

        public EventHandler<TeamDefeatedEventArgs> TeamDefeated { get; set; }

        public void OnTeamDefeated(TeamDefeatedEventArgs e)
        {
            TeamDefeated?.Invoke(this, e);
        }

        public EventHandler<RoundEndedEventArgs> RoundEnded { get; set; }

        public void OnRoundEnded(RoundEndedEventArgs e)
        {
            RoundEnded?.Invoke(this, e);
        }

        public EventHandler<RanEventArgs> Ran { get; set; }

        public void OnRun(RanEventArgs e)
        {
            Ran?.Invoke(this, e);
        }

        public EventHandler<FighterAddedEventArgs> FighterAdded { get; set; }

        public void OnFighterAdded(FighterAddedEventArgs e)
        {
            FighterAdded?.Invoke(this, e);
        }


        protected void _PropagateRunEvent(object sender, RanEventArgs e)
        {
            OnRun(e);
        }

        private void _onFighterKilled(object sender, KilledEventArgs e)
        {
            if (IsTeamDefeated())
            {
                OnTeamDefeated(new TeamDefeatedEventArgs(this));
            }
        }

        private void _onFighterSealed(object sender, FighterSealedEventArgs e)
        {
            if (IsTeamDefeated())
            {
                OnTeamDefeated(new TeamDefeatedEventArgs(this));
            }
        }

        private void _onRoundEnded(object sender, RoundEndedEventArgs e)
        {
            foreach (IFighter fighter in Fighters)
            {
                fighter.OnRoundEnded(new RoundEndedEventArgs(this, fighter));
            }
        }

        #endregion

        protected Team(MenuManager menuManager)
        {
            RoundEnded += _onRoundEnded;
            MenuManager = menuManager;
            MenuManager.Ran += _PropagateRunEvent;
            Fighters = new List<IFighter>();
        }

        public Team(MenuManager menuManager, List<IFighter> fighters) : this(menuManager, fighters.ToArray())
        {
        }

        public Team(MenuManager menuManager, params IFighter[] fighters) : this(menuManager)
        {
            AddRange(fighters);
        }

        public Team(MenuManager menuManager, FighterGrouping grouping, params IFighter[] fighters) : this(menuManager, fighters.Concat(grouping.GetFighters()).ToArray())
        {
        }

        /// <summary>
        /// A helper method to be called to ensure all Enemy 
        /// </summary>
        public void SetupDisplayNames()
        {
            //TODO: have some way to check if an enemy has the "default" name. If there's a goblin named Eurdar the Wise, we don't want them to become Eurdar the Wise A
            List <EnemyFighter> enemies = GetEnemyFighters().ToList();

            //List<HumanControlledEnemyFighter> humanControlledEnemies = enemies.OfType<HumanControlledEnemyFighter>().ToList();
            //
            //IEnumerable<IGrouping<Type, EnemyFighter>> groupedEnemies = humanControlledEnemies.GroupBy(e => e.Fighter.GetType());
            //
            //enemies = enemies.Except(humanControlledEnemies).ToList();
            //
            ////names only need to be set for enemies for which there are multiple types
            //groupedEnemies = groupedEnemies.Concat(enemies.GroupBy(e => e.GetType()));
            //
            //groupedEnemies = groupedEnemies.Where(g => g.Count() > 1);

            //IEnumerable<IGrouping<Type, EnemyFighter>> groupedEnemies = enemies.GroupBy(e =>
            IEnumerable<IGrouping<Tuple<Type, MagicType>, EnemyFighter>> groupedEnemies = enemies.GroupBy(e =>
            {
                Type type;
                MagicType magicType;
                HumanControlledEnemyFighter humanControlled = e as HumanControlledEnemyFighter;

                if (humanControlled != null)
                {
                    EnemyFighter fighter = humanControlled.Fighter;
                    magicType = GetFighterMagicType(fighter);
                    type = fighter.GetType();
                }
                else
                {
                    type = e.GetType();
                    magicType = GetFighterMagicType(e);
                }

                return new Tuple<Type, MagicType>(type, magicType);
            }).Where(g => g.Count() > 1);

            foreach (IGrouping<Tuple<Type, MagicType>, EnemyFighter> group in groupedEnemies)
            {
                int i = 0;

                foreach (EnemyFighter enemy in group)
                {
                    string appendText;

                    if (i < 26)
                    {
                        appendText = $"{(char)('A' + i)}";
                    }
                    else
                    {
                        int firstIndex = (i/26) - 1;
                        int secondIndex = i % 26;

                        char firstChar = (char) ('A' + firstIndex);
                        char secondChar = (char) ('A' + secondIndex);
                        appendText = $"{firstChar}{secondChar}";
                    }

                    enemy.SetAppendText(appendText);

                    ++i;
                }
            }
        }

        public bool Contains(IFighter fighter)
        {
            return Fighters.Contains(fighter);
        }

        public bool IsTeamDefeated()
        {
            return Fighters.TrueForAll(f => !f.IsAlive());
        }

        public void Add(IFighter newFighter, bool setupDisplayNamesAfterAdd = true)
        {
            newFighter.Killed += _onFighterKilled;
            newFighter.SetTeam(this);

            Fighters.Add(newFighter);

            if (setupDisplayNamesAfterAdd)
            {
                SetupDisplayNames();
            }

            Shade fighterAsShade = newFighter as Shade;

            if (fighterAsShade != null)
            {
                fighterAsShade.FighterSealed += _onFighterSealed;
            }

            FighterAddedEventArgs e = new FighterAddedEventArgs(newFighter);
            OnFighterAdded(e);
        }

        public void AddRange(params IFighter[] newFighters)
        {
            AddRange(new List<IFighter>(newFighters));
        }

        public void AddRange(IEnumerable<IFighter> newFighters)
        {
            foreach (IFighter newFighter in newFighters)
            {
                Add(newFighter, false);
            }

            SetupDisplayNames();
        }

        public void Remove(IFighter fighter)
        {
            Fighters.Remove(fighter);

            fighter.SetTeam(null);
            
            SetupDisplayNames();
        }

        public void AddStatus(Status status)
        {
            IEnumerable<IFighter> aliveFighters = Fighters.Where(f => f.IsAlive());
            foreach (IFighter fighter in aliveFighters)
            {
                fighter.AddStatus(status);
            }
        }

        public List<BattleMoveWithTarget> GetInputs(Team enemyTeam, List<MenuAction> specialMenuActions = null)
        {
            List<BattleMoveWithTarget> allInputs = new List<BattleMoveWithTarget>();

            IEnumerable<HumanFighter> humanFighters = GetHumanFighters();

            humanFighters = humanFighters.Where(f => f.IsAlive());

            List<BattleMoveWithTarget> humanInputs = MenuManager.GetInputs(humanFighters.ToList(), specialMenuActions);

            allInputs.AddRange(humanInputs);

            //TODO: should we rename enemyFighters, since we're now programming assuming they can be on either team
            IEnumerable<EnemyFighter> enemyFighters = GetEnemyFighters();

            enemyFighters = enemyFighters.Where(e => e.IsAlive());

            List<BattleMoveWithTarget> enemyInputs = enemyFighters.Select(e => e.SetupMove(this, enemyTeam)).ToList();

            allInputs.AddRange(enemyInputs);

            return allInputs;
        }

        public IEnumerable<HumanFighter> GetHumanFighters()
        {
            IEnumerable<HumanFighter> myHumanFighters = Fighters.OfType<HumanFighter>();

            return myHumanFighters;
        }

        public IEnumerable<EnemyFighter> GetEnemyFighters()
        {
            IEnumerable<EnemyFighter> myEnemyFighters = Fighters.OfType<EnemyFighter>();

            return myEnemyFighters;
        }

        public virtual void InitializeForBattle(Team enemyTeam, IInput input, IOutput output)
        {
            MenuManager.InitializeForBattle(this, enemyTeam);
        }

        private static MagicType GetFighterMagicType(EnemyFighter fighter)
        {
            MagicType ret = MagicType.None;
            
            IMagicEnemy fighterAsMagicEnemy = fighter as IMagicEnemy;

            if (fighterAsMagicEnemy != null)
            {
                ret = fighterAsMagicEnemy.MagicType;
            }

            return ret;
        }
    }
}
