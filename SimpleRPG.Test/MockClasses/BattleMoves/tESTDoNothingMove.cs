using SimpleRPG.Battle.BattleMoves;

namespace SimpleRPG.Test.MockClasses.BattleMoves
{
    public class TestDoNothingMove: DoNothingMove
    {
        public TestDoNothingMove(string message = "")
            : base(message)
        {
            Message = message;
        }

        public TestDoNothingMove(DoNothingMove copy)
            : base(copy)
        {
        }

        public void SetMessage(string message)
        {
            Message = message;
        }
    }
}
