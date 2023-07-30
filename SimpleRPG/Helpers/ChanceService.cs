using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleRPG.Helpers
{
    /// <summary>
    /// Represents something that has a chance of happening. Chance is between 0 and 100 inclusive, and represents the percentage probability of the event happening.
    /// Value can be whatever it needs to be, but clear candidates are Enums and strings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChanceEvent<T>
    {
        public T Value { get; }

        public double Chance { get; }

        public ChanceEvent(T value, double chance)
        {
            Value = value;
            Chance = chance;
        }
    }

    public class ChanceService : IChanceService
    {
        private readonly Random _r;

        public ChanceService()
        {
            _r = new Random();
        }

        /// <summary>
        /// Determines whether a binary event occurs based on the probability of the event coming to pass
        /// </summary>
        /// <param name="chance">A decimal number representing the probability.</param>
        /// <returns></returns>
        public bool EventOccurs(double chance)
        {
            var val = _r.Next(0, 100);

            return val < (chance * 100);
        }

        public int WhichEventOccurs(IEnumerable<double> chances)
        {
            var val = _r.Next(0, 100);
            List<double> chanceList = chances.ToList();

            var ret = -1;
            var found = false;
            double total = 0;

            for (var i = 0; i < chanceList.Count && !found; ++i)
            {
                total += (chanceList[i] * 100);

                if (val < total)
                {
                    found = true;
                    ret = i;
                }
            }

            return ret;
        }

        /// <summary>
        /// Given N events that are all equally likely, returns the index of the event that happens
        /// e.g. if there are 4 items, then there's a 25% chance for 0, 1, 2, and 3
        /// </summary>
        /// <param name="numberEvents">The number of events that are possible</param>
        /// <returns>the 0-based index of the event that occured</returns>
        public int WhichEventOccurs(int numberEvents)
        {
            return _r.Next(0, numberEvents);
        }

        public T WhichEventOccurs<T>(IEnumerable<ChanceEvent<T>> chanceEvents)
        {
            List<ChanceEvent<T>> chanceEventsList = chanceEvents.ToList();
            IEnumerable<double> chances = chanceEventsList.Select(ce => ce.Chance);

            int whichEventOccurs = WhichEventOccurs(chances);

            return chanceEventsList[whichEventOccurs].Value;
        }

        public T WhichEventOccurs<T>(IEnumerable<T> events)
        {
            List<T> eventList = events.ToList();
            int numberEvents = eventList.Count;

            int whichEventOccurs = WhichEventOccurs(numberEvents);

            return eventList[whichEventOccurs];
        }

        public IEnumerable<T> Shuffle<T>(IEnumerable<T> items)
        {
            List<T> itemList = items.ToList();
            int max = itemList.Count;

            List<T> ret = new List<T>();

            int i = 0;
            
            for (;i < max - 1; ++i)
            {
                int selectedIndex = _r.Next(i, max);

                T selectedItem = itemList[selectedIndex];
                itemList[selectedIndex] = itemList[i];
                itemList[i] = selectedItem;

                ret.Add(selectedItem);
            }

            ret.Add(itemList[i]);

            return ret;
        }
    }
}
