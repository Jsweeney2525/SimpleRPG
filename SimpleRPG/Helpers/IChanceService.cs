using System.Collections.Generic;

namespace SimpleRPG.Helpers
{
    public interface IChanceService
    {
        /// <summary>
        /// Determines whether a binary event occurs based on the probability of the event coming to pass
        /// </summary>
        /// <param name="chance">A decimal number representing the probability.</param>
        /// <returns></returns>
        bool EventOccurs(double chance);

        /// <summary>
        /// Given an array of events that could occur, determines whicch will occur (or perhaps none of them, depending on the chance values)
        /// The amounts arecumulative, so if the first chance is 1.0, then it will always be selected.
        /// </summary>
        /// <param name="chances"></param>
        /// <returns>The index of the selected event, e.g. 0 or 2</returns>
        int WhichEventOccurs(IEnumerable<double> chances);

        /// <summary>
        /// Given N events that are all equally likely, returns the index of the event that happens
        /// e.g. if there are 4 items, then there's a 25% chance for 0, 1, 2, and 3
        /// </summary>
        /// <param name="numberEvents">The number of events that are possible</param>
        /// <returns>the 0-based index of the event that occured</returns>
        int WhichEventOccurs(int numberEvents);

        /// <summary>
        /// Given several items, each paired with a probability, returns which item occurs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="chanceEvents"></param>
        /// <returns></returns>
        T WhichEventOccurs<T>(IEnumerable<ChanceEvent<T>> chanceEvents);

        /// <summary>
        /// Given a list of events, each assumed to be equally likely, determines which event will occur
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="events"></param>
        /// <returns></returns>
        T WhichEventOccurs<T>(IEnumerable<T> events);

        IEnumerable<T> Shuffle<T>(IEnumerable<T> items);
    }
}
