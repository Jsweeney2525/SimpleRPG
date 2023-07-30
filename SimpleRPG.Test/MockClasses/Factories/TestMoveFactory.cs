using System;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses.BattleMoves;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public static class TestMoveFactory
    {
        private static readonly BattleMove FakeSelfTargetMove = new BattleMove("foo", BattleMoveType.Special, TargetType.Self);
        private static readonly SpecialMove FakeSpecialMove = new SpecialMove("foo", BattleMoveType.Special, TargetType.Field, null);

        private static readonly MultiTurnBattleMove FakeMultiTurnMove = new MultiTurnBattleMove("foo", TargetType.SingleEnemy,
            new DoNothingMove("Foo!"),
            new DoNothingMove("Bar!"),
            new AttackBattleMove("Baz", TargetType.SingleEnemy, 50, 100));

        /// <summary>
        /// Fake moves include targetType = self/description = "test" for a test self-target
        /// moveType = "Special" and description = "test" for a test special move (target type field)
        /// and description = "testMultiTurn" to get a fake multi-turn move (2 turns do nothing, 3rd turn attack)
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="description"></param>
        /// <param name="moveType"></param>
        /// <returns></returns>
        public static BattleMove Get(TargetType targetType = TargetType.Self, string description = "test", BattleMoveType? moveType = null)
        {
            BattleMove ret;

            if (description == "test")
            {
                if (targetType == TargetType.Self && moveType == null)
                {
                    ret = FakeSelfTargetMove;
                }
                else if (targetType == TargetType.SingleAlly || targetType == TargetType.SingleAllyOrSelf)
                {
                    ret = new BattleMove(description, BattleMoveType.DoNothing, targetType);
                }
                else if (!moveType.HasValue)
                {
                    throw new ArgumentException(
                        "Either TargetType self must be specified or a valid moveType must be given");
                }
                else
                {
                    switch (moveType.Value)
                    {
                        case BattleMoveType.Special:
                            ret = FakeSpecialMove;
                            break;
                        case BattleMoveType.DoNothing:
                            ret = new TestDoNothingMove();
                            break;
                        case BattleMoveType.Field:
                            ret = new TestFieldEffectMove("foo", TargetType.OwnTeam, 2);
                            break;
                        case BattleMoveType.Dance:
                            ret = new TestDanceMove("foo dance", TargetType.Field, 0);
                            break;
                        default:
                            throw new ArgumentException(
                                $"No test move found for the given inputs. TargetType: '{targetType}', moveType: '{moveType}'");
                    }
                }
            }
            else if (description == "testMultiTurn")
            {
                ret = FakeMultiTurnMove;
            }
            else if (moveType == null)
            {
                throw new ArgumentException(
                    "TestMoveFactory.Get() must either specify a moveType, or indicate it wants a test move!");
            }
            else
            {
                ret = MoveFactory.Get(moveType.Value);
            }

            return ret;
        }
    }
}
