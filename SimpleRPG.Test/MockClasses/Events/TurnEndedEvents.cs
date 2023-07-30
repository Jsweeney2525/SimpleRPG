using SimpleRPG.Events;

namespace SimpleRPG.Test.MockClasses.Events
{
    public class TurnEndedEvents
    {
        private static int _foo = 0;
        public static void RestoreManaOnTurnEnd(object sender, TurnEndedEventArgs e)
        {
            e.Fighter.RestoreMana(e.Fighter.MaxMana);
            _foo++;
        }
    }
}
