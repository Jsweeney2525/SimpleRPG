using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public class DoNothingMove: BattleMove
    {
        public string Message { get; protected set; }

        public DoNothingMove(string message = "", int priority = 0)
            : base("do nothing", BattleMoveType.DoNothing, TargetType.Self, priority)
        {
            Message = message;
        }

        public DoNothingMove(DoNothingMove copy)
            : base(copy)
        {
            Message = copy.Message;
        }
    }
}
