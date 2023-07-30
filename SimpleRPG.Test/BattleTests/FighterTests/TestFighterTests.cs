using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class TestFighterTests
    {
        private TestHumanFighter _fighter;
        private TestEnemyFighter _enemy;

        private EventLogger _logger;

        [SetUp]
        public void Setup()
        {
            _fighter = new TestHumanFighter("Hero", 1);
            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "enemy");

            _logger = new EventLogger();
        }

        #region TestEnemyFighter methods

        [Test]
        public void TestEnemyFighter_CorrectlyDies_SetDeathOnTurnEndEventCalled()
        {
            _enemy.SetHealth(100);
            _enemy.SetDeathOnTurnEndEvent();

            Assert.AreEqual(100, _enemy.CurrentHealth);

            _enemy.OnTurnEnded(new TurnEndedEventArgs(_enemy));

            Assert.AreEqual(0, _enemy.CurrentHealth);
        }

        [Test]
        public void TestEnemyFighter_CorrectlyFiresOnDeathEvent_SetDeathOnTurnEndEventCalled()
        {
            _logger.Subscribe(EventType.Killed, _enemy);

            _enemy.SetDeathOnTurnEndEvent();

            _enemy.OnTurnEnded(new TurnEndedEventArgs(_enemy));

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            var log = logs[0];
            Assert.IsInstanceOf<KilledEventArgs>(log.E);
        }

        [Test]
        public void TestEnemyFighter_GetMove_CorrectlyGetsIndefiniteMove()
        {
            AttackBattleMove move = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);

            _enemy.SetMove(move);

            for (var i = 0; i < 10; ++i)
            {
                BattleMove returnedMove = _enemy.GetMove();

                Assert.AreEqual(move, returnedMove);
            }
        }

        [Test]
        public void TestEnemyFighter_GetMove_CorrectlyGetsMovesBasedOnNumberOfCalls()
        {
            BattleMove threeTurnMove = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);
            BattleMove indefiniteMove = new SpecialMove("bar", BattleMoveType.Special, TargetType.Self, null);

            _enemy.SetMove(threeTurnMove, 3);
            _enemy.SetMove(indefiniteMove);

            for (var i = 0; i < 10; ++i)
            {
                BattleMove returnedMove = _enemy.GetMove();

                BattleMove expectedReturn = (i < 3) ? threeTurnMove : indefiniteMove;

                Assert.AreEqual(expectedReturn, returnedMove);
            }
        }

        [Test]
        public void TestEnemyFighter_GetMove_ThrowsException_NoMoveInQueue()
        {
            AttackBattleMove threeTurnMove = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);
            BattleMove sixTurnMove = new SpecialMove("bar", BattleMoveType.Special, TargetType.Self, null);

            _enemy.SetMove(threeTurnMove, 3);
            _enemy.SetMove(sixTurnMove, 6);

            for (var i = 0; i < 9; ++i)
            {
                _enemy.GetMove();
            }

            Assert.Throws<IndexOutOfRangeException>(() => _enemy.GetMove());
        }

        #endregion

        #region TesHumanFighter methods

        [Test]
        public void TestHumanFighter_CorrectlyDies_SetDeathOnTurnEndEventCalled()
        {
            _fighter.SetHealth(100);
            _fighter.SetDeathOnTurnEndEvent();

            Assert.AreEqual(100, _fighter.CurrentHealth);

            _fighter.OnTurnEnded(new TurnEndedEventArgs(_fighter));

            Assert.AreEqual(0, _fighter.CurrentHealth);
        }

        [Test]
        public void TestHumanFighter_CorrectlyFiresOnDeathEvent_SetDeathOnTurnEndEventCalled()
        {
            _logger.Subscribe(EventType.Killed, _fighter);

            _fighter.SetDeathOnTurnEndEvent();

            _fighter.OnTurnEnded(new TurnEndedEventArgs(_fighter));

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            var log = logs[0];
            Assert.IsInstanceOf<KilledEventArgs>(log.E);
        }

        [Test]
        public void TestHumanFighter_GetMove_CorrectlyGetsIndefiniteMove()
        {
            BattleMove move = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);

            _fighter.SetMove(move);

            for (var i = 0; i < 10; ++i)
            {
                BattleMove returnedMove = _fighter.GetMove();

                Assert.AreEqual(move, returnedMove);
            }
        }

        [Test]
        public void TestHumanFighter_GetMove_CorrectlyGetsMovesBasedOnNumberOfCalls()
        {
            BattleMove threeTurnMove = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);
            BattleMove indefiniteMove = new SpecialMove("bar", BattleMoveType.Special, TargetType.Self, null);

            _fighter.SetMove(threeTurnMove, 3);
            _fighter.SetMove(indefiniteMove);

            for (var i = 0; i < 10; ++i)
            {
                BattleMove returnedMove = _fighter.GetMove();

                BattleMove expectedReturn = (i < 3) ? threeTurnMove : indefiniteMove;

                Assert.AreEqual(expectedReturn, returnedMove);
            }
        }

        [Test]
        public void TestHumanFighter_GetMove_ThrowsException_NoMoveInQueue()
        {
            BattleMove threeTurnMove = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);
            BattleMove sixTurnMove = new SpecialMove("bar", BattleMoveType.Special, TargetType.Self, null);

            _fighter.SetMove(threeTurnMove, 3);
            _fighter.SetMove(sixTurnMove, 6);

            for (var i = 0; i < 9; ++i)
            {
                _fighter.GetMove();
            }

            Assert.Throws<IndexOutOfRangeException>(() => _fighter.GetMove());
        }

        #endregion
    }
}
