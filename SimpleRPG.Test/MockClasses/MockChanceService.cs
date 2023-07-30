using System;
using SimpleRPG.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SimpleRPG.Test.MockClasses
{
    public class MockChanceService : IChanceService
    {
        private readonly List<bool> _eventOccursReturnValues;
        private readonly List<int> _whichEventOccursReturnValues;

        private int _eventOccursIndex;
        private int _whichEventOccursIndex;

        private ChanceService _realChanceService;

        /// <summary>
        /// Stores each chance value passed into <see cref="EventOccurs"/> in order.
        /// e.g. it was called with 80% chance of a move hitting and a 30% chance of crit, LastChanceVals[0] = 0.8 and LastChanceVals[1] = 0.3
        /// </summary>
        public List<double> LastChanceVals { get; }

        /// <summary>
        /// Returns the array of chances that was last passed into the <see cref="WhichEventOccurs"/> method
        /// </summary>
        public double[] LastEventOccursArgs { get; private set; }

        public MockChanceService()
        {
            _realChanceService = new ChanceService();

            _eventOccursReturnValues = new List<bool>();
            _whichEventOccursReturnValues = new List<int>();

            _eventOccursIndex = 0;
            _whichEventOccursIndex = 0;

            LastChanceVals = new List<double>();
            
            _shuffleIndices =  new List<int>();
        }

        public void PushEventOccurs(bool eventOccurs)
        {
            _eventOccursReturnValues.Add(eventOccurs);
        }

        public void PushEventsOccur(params bool[] eventsOccur)
        {
            _eventOccursReturnValues.AddRange(eventsOccur);
        }

        /// <summary>
        /// Pushes the appropriate "EventOccurs" return values for an attack to hit and not crit for X attacks, X is specified by the input
        /// </summary>
        /// <param name="numberAttacks"></param>
        public void PushAttackHitsNotCrit(int numberAttacks = 1)
        {
            if (numberAttacks < 1)
            {
                throw new ArgumentException("Must specify at least one attack!");
            }

            for (var i = 0; i < numberAttacks; ++i)
            {
                PushEventsOccur(true, false);
            }
        }

        public bool EventOccurs(double chance)
        {
            LastChanceVals.Add(chance);
            return _eventOccursReturnValues[_eventOccursIndex++];
        }

        public void PushWhichEventOccurs(int whichEventOccurs)
        {
            _whichEventOccursReturnValues.Add(whichEventOccurs);
        }

        public void PushWhichEventsOccur(params int[] whichEventsOccur)
        {
            _whichEventOccursReturnValues.AddRange(whichEventsOccur);
        }

        public int WhichEventOccurs(IEnumerable<double> chances)
        {
            LastEventOccursArgs = chances.ToArray();
            return _whichEventOccursReturnValues[_whichEventOccursIndex++];
        }

        public int WhichEventOccurs(int numberEvents)
        {
            LastEventOccursArgs = new double[numberEvents];
            for (var i = 0; i < numberEvents; ++i)
            {
                LastEventOccursArgs[i] = 1.0/numberEvents;
            }
            return _whichEventOccursReturnValues[_whichEventOccursIndex++];
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
            List<T> eventsList = events.ToList();
            
            int whichEventOccurs = WhichEventOccurs(eventsList.Count);

            return eventsList[whichEventOccurs];
        }

        private List<int> _shuffleIndices;

        public void SetShuffleIndices(IEnumerable<int> items)
        {
            _shuffleIndices = items.ToList();
        }

        public IEnumerable<T> Shuffle<T>(IEnumerable<T> items)
        {
            IEnumerable<T> ret;

            int shuffleIndicesCount = _shuffleIndices.Count;

            if (shuffleIndicesCount == 0)
            {
                ret = _realChanceService.Shuffle(items);
            }
            else
            {
                List<T> retList = new List<T>();
                List<T> itemList = items.ToList();
                for (var i = 0; i < shuffleIndicesCount; ++i)
                {
                    int selectedIndex = _shuffleIndices[i];
                    retList.Add(itemList[selectedIndex]);
                }

                ret = retList;
            }

            return ret;
        }
    }
}
