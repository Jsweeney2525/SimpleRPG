using System.Collections.Generic;

namespace SimpleRPG.Battle.Fighters.FighterGroupings
{
    public class FighterGrouping
    {
        protected readonly List<IFighter> Fighters;

        public FighterGrouping(params IFighter[] fighters)
        {
            Fighters = new List<IFighter>(fighters);
        }

        public List<IFighter> GetFighters()
        {
            return new List<IFighter>(Fighters);
        }
    }
}
