using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Events;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Test.MockClasses
{
    public class TestTeam : Team
    {

        public TestTeam(IFighter fighter) :
            base(TestMenuManager.GetTestMenuManager(), fighter)
        {
        }

        public TestTeam(MenuManager menuManager, IFighter fighter) :
            base(menuManager, fighter)
        {
        }

        public TestTeam(params HumanFighter[] fighters)
            : base(TestMenuManager.GetTestMenuManager(), fighters.Cast<IFighter>().ToArray())
        {
        }

        public TestTeam(MenuManager menuManager, params HumanFighter[] fighters)
            : base(menuManager, fighters.Cast<IFighter>().ToArray())
        {
        }

        public TestTeam(List<HumanFighter> fighters)
            : base(TestMenuManager.GetTestMenuManager(), fighters.Cast<IFighter>().ToArray())
        {
        }

        public TestTeam(MenuManager menuManager, List<HumanFighter> fighters)
            : base(menuManager, fighters.Cast<IFighter>().ToArray())
        {
        }

        /// <summary>
        /// Call this method to make the fighter die the next time TurnEnd is fired
        /// </summary>
        public void SetDeathsOnRoundEndEvent()
        {
            this.RoundEnded += KilledOnRoundEnded;
        }

        private static void KilledOnRoundEnded(object sender, RoundEndedEventArgs e)
        {
            Team team = e.Team;

            if (team == null)
            {
                throw new ArgumentException("KilledOnRoundEnd somehow called on class that is not a Team!");
            }

            foreach (IFighter fighter in team.Fighters)
            {
                fighter.PhysicalDamage(fighter.MaxHealth);

                //TODO: should the basic "damage" method be called, and should it fire OnKilled if it brings health to 0?
                //Then, how do we ensure the proper event args are passed into the event? Or do we even care, since killed is empty?
                fighter.OnKilled(new KilledEventArgs());
            }
        }
    }
}
