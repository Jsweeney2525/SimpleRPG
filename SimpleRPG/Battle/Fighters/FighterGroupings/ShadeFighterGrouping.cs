using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Events;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.FighterGroupings
{
    public class ShadeFighterGrouping : FighterGrouping
    {
        private IChanceService _chanceService;
        private List<Shade> _shades;

        public ShadeFighterGrouping(IChanceService chanceService, params Shade[] fighters) : base(fighters.Cast<IFighter>().ToArray())
        {
            _chanceService = chanceService;
            _shades = new List<Shade>(fighters);

            foreach (Shade shade in fighters)
            {
                shade.Killed += OnShadeDeath;
            }
        }

        public List<Shade> GetShades()
        {
            return new List<Shade>(Fighters.OfType<Shade>());
        }

        private void OnShadeDeath(object sender, KilledEventArgs e)
        {
            Shade senderAsShade = sender as Shade;

            if (senderAsShade == null)
            {
                throw new InvalidOperationException("ShadeFighterGrouping.OnShadeDeath should only subscribe to a Shade fighters' Killed event!");
            }

            List<Shade> stillAliveShades = _shades.FindAll(s => s.IsAlive());
            int numberOfShadesStillAlive = stillAliveShades.Count;

            if (numberOfShadesStillAlive > 0)
            {
                int chosenShadeIndex = _chanceService.WhichEventOccurs(numberOfShadesStillAlive);
                stillAliveShades[chosenShadeIndex].AbsorbShade(senderAsShade);
            }
        }
    }
}
