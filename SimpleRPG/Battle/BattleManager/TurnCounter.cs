namespace SimpleRPG.Battle.BattleManager
{
    public class TurnCounter
    {
        public int TurnsLeft { get; protected set; }

        public TurnCounter(int turns)
        {
            TurnsLeft = turns;
        }

        public void DecrementTurnsLeft()
        {
            --TurnsLeft;
        }

        public void SetTurnsLeft(int turnsLeft)
        {
            TurnsLeft = turnsLeft;
        }
    }
}
