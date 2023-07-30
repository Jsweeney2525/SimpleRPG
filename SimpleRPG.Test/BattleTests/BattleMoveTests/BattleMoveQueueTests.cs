using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    public class BattleMoveQueueTests
    {
        //TODO: need tests to ensure moves are also sorted by Priority
        private readonly BattleMove _testMove = new BattleMove("foo", BattleMoveType.DoNothing, TargetType.Self);
        [Test]
        public void BattleMoveQueue_AcceptsListInInitializer([Values(1, 3)] int numberOfMovesInInitializerList)
        {
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_testMove, null, null);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>();

            for (var i = 0; i < numberOfMovesInInitializerList; ++i)
            {
                initializerList.Add(moveWithTarget);
            }

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            Assert.AreEqual(numberOfMovesInInitializerList, queue.Count);
        }

        [Test]
        public void BattleMoveQueue_AddOperation_CorrectlyIncreasesCount([Values(1, 3)] int numberOfMovesToAdd)
        {
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_testMove, null, null);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                moveWithTarget,
                moveWithTarget
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            List<BattleMoveWithTarget> movesToAdd = new List<BattleMoveWithTarget>();

            for (var i = 0; i < numberOfMovesToAdd; ++i)
            {
                movesToAdd.Add(moveWithTarget);
            }

            queue.AddRange(movesToAdd);

            int expectedCount = initializerList.Count + numberOfMovesToAdd;
            Assert.AreEqual(expectedCount, queue.Count);
        }

        [Test]
        public void BattleMoveQueue_PopOperation_CorrectlyDecreasesCount([Values(1, 3)] int numberOfMovesToPop)
        {
            BattleMoveWithTarget moveWithTarget = new BattleMoveWithTarget(_testMove, null, null);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                moveWithTarget,
                moveWithTarget,
                moveWithTarget,
                moveWithTarget,
                moveWithTarget,
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            for (var i = 0; i < numberOfMovesToPop; ++i)
            {
                queue.Pop();
            }

            int expectedCount = initializerList.Count - numberOfMovesToPop;
            Assert.AreEqual(expectedCount, queue.Count);
        }

        [Test]
        public void BattleMoveQueue_PopOperation_CorrectlyReturnsNextMove([Values(1, 3)] int numberOfMovesToPop)
        {
            IFighter fighter1 = TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jeff");
            IFighter fighter2 = TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Jebb");

            BattleMoveWithTarget moveWithTarget1 = new BattleMoveWithTarget(_testMove, fighter1, fighter1);
            BattleMoveWithTarget moveWithTarget2 = new BattleMoveWithTarget(_testMove, fighter2, fighter2);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                moveWithTarget1,
                moveWithTarget2
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            Assert.AreEqual(moveWithTarget1, queue.Pop());
            Assert.AreEqual(moveWithTarget2, queue.Pop());
        }

        [Test]
        public void BattleMoveQueue_SortOperation_DefaultsBySpeed()
        {
            TestHumanFighter fighter1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            //sorted order is 2, 3, 1
            fighter1.SetSpeed(0);
            fighter2.SetSpeed(2);
            fighter3.SetSpeed(1);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                new BattleMoveWithTarget(_testMove, fighter1, fighter1),
                new BattleMoveWithTarget(_testMove, fighter2, fighter2),
                new BattleMoveWithTarget(_testMove, fighter3, fighter3)
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            queue.Sort();

            IFighter owner;

            //sorted order is 2, 3, 1
            BattleMoveWithTarget firstSortedMove = queue.Pop();
            owner = firstSortedMove.Owner;
            Assert.AreEqual(fighter2, owner);

            BattleMoveWithTarget secondSortedMove = queue.Pop();
            owner = secondSortedMove.Owner;
            Assert.AreEqual(fighter3, owner);

            BattleMoveWithTarget thirdSortedMove = queue.Pop();
            owner = thirdSortedMove.Owner;
            Assert.AreEqual(fighter1, owner);
        }

        [Test]
        public void BattleMoveQueue_SortOperation_DefaultsBySpeedAndPriority()
        {
            TestHumanFighter fighter1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter4 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter5 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter6 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter7 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter8 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter9 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            TestHumanFighter[] expectedOrderedFighters =
            {
                fighter9, fighter6, fighter3, //the high priority
                fighter8, fighter5, fighter2, //the med priority
                fighter7, fighter4, fighter1  //the low priority
            };
            
            fighter1.SetSpeed(0);
            fighter2.SetSpeed(0);
            fighter3.SetSpeed(0);
            fighter4.SetSpeed(1);
            fighter5.SetSpeed(1);
            fighter6.SetSpeed(1);
            fighter7.SetSpeed(2);
            fighter8.SetSpeed(2);
            fighter9.SetSpeed(2);

            BattleMove lowPriorityMove = new BattleMove("foo", BattleMoveType.DoNothing, TargetType.Self, -1);
            BattleMove medPriorityMove = new BattleMove("bar", BattleMoveType.DoNothing, TargetType.Self);
            BattleMove highPriorityMove = new BattleMove("baz", BattleMoveType.DoNothing, TargetType.Self, 1);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                new BattleMoveWithTarget(lowPriorityMove, fighter1, fighter1),
                new BattleMoveWithTarget(lowPriorityMove, fighter4, fighter4),
                new BattleMoveWithTarget(lowPriorityMove, fighter7, fighter7),
                new BattleMoveWithTarget(medPriorityMove, fighter2, fighter2),
                new BattleMoveWithTarget(medPriorityMove, fighter5, fighter5),
                new BattleMoveWithTarget(medPriorityMove, fighter8, fighter8),
                new BattleMoveWithTarget(highPriorityMove, fighter3, fighter3),
                new BattleMoveWithTarget(highPriorityMove, fighter6, fighter6),
                new BattleMoveWithTarget(highPriorityMove, fighter9, fighter9)
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            queue.Sort();

            for (int i = 0; i < 9; ++i)
            {
                BattleMoveWithTarget move = queue.Pop();
                IFighter owner = move.Owner;
                Assert.AreEqual(expectedOrderedFighters[i], owner, $"i: {i}");
            }
        }

        [Test]
        public void BattleMoveQueue_SortOperation_CorrectlyAcceptsMethodToCalculateEffectiveSpeed([Values(1, 3)] int numberOfMovesToPop)
        {
            TestHumanFighter fighter1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jeff");
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Bill");
            TestHumanFighter fighter3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Mike");

            //sorted order is Mike (3), Jeff (1), then Bill (2),
            //which is different than the initial order and different from speed order
            fighter1.SetSpeed(0);
            fighter2.SetSpeed(2);
            fighter3.SetSpeed(1);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                new BattleMoveWithTarget(_testMove, fighter1, fighter1),
                new BattleMoveWithTarget(_testMove, fighter2, fighter2),
                new BattleMoveWithTarget(_testMove, fighter3, fighter3)
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            Func<IFighter, int> sortHelperFunc = f =>
            {
                int ret = 0;

                switch (f.BaseName)
                {
                    case "Mike":
                        ret = 100;
                        break;
                    case "Jeff":
                        ret = 50;
                        break;
                    case "Bill":
                        ret = 10;
                        break;
                }

                return ret;
            };

            queue.Sort(sortHelperFunc);

            //sorted order is Mike (3), Jeff (1), then Bill (2)
            BattleMoveWithTarget firstSortedMove = queue.Pop();
            IFighter owner = firstSortedMove.Owner;
            Assert.AreEqual(fighter3, owner);

            BattleMoveWithTarget secondSortedMove = queue.Pop();
            owner = secondSortedMove.Owner;
            Assert.AreEqual(fighter1, owner);

            BattleMoveWithTarget thirdSortedMove = queue.Pop();
            owner = thirdSortedMove.Owner;
            Assert.AreEqual(fighter2, owner);
        }

        [Test]
        public void BattleMoveQueue_SortAndPopOperation_ReturnsCorrectMove()
        {
            TestHumanFighter fighter1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            //sorted order is 2, 3, 1
            fighter1.SetSpeed(0);
            fighter2.SetSpeed(2);
            fighter3.SetSpeed(1);

            List<BattleMoveWithTarget> initializerList = new List<BattleMoveWithTarget>
            {
                new BattleMoveWithTarget(_testMove, fighter1, fighter1),
                new BattleMoveWithTarget(_testMove, fighter2, fighter2),
                new BattleMoveWithTarget(_testMove, fighter3, fighter3)
            };

            BattleMoveQueue queue = new BattleMoveQueue(initializerList);

            BattleMoveWithTarget firstSortedMove = queue.SortAndPop();
            IFighter owner = firstSortedMove.Owner;
            Assert.AreEqual(fighter2, owner);

            Assert.AreEqual(2, queue.Count);
        }
    }
}
