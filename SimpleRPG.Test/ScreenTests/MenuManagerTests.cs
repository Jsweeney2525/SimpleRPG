using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Events;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.BattleMoves;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.ScreenTests
{
    [TestFixture]
    class MenuManagerTests
    {
        private MockInput _input;
        private MockOutput _output;
        private IMenuFactory _menuFactory;
        private EventLogger _logger;
        private TestHumanFighter _playerOne, _playerTwo, _playerThree;
        private Team _playerTeam;
        private Team _enemyTeam;
        private MenuManager _manager;

        [SetUp]
        public void SetUp()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuFactory = new MenuFactory();
            _manager = new MenuManager(_input, _output, _menuFactory);
            _logger = new EventLogger();
            _playerOne = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "swordsman");
            Spell fireSpell = SpellFactory.GetSpell(MagicType.Fire, 1);
            _playerOne.AddSpell(fireSpell);
            _playerOne.SetMana(fireSpell.Cost);

            _playerTwo = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "mage");
            _playerThree = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "alchemist");
            _playerTeam = new Team(_manager, _playerOne, _playerTwo, _playerThree);

            _enemyTeam = new Team(_manager, new List<IFighter>
            {
                FighterFactory.GetFighter(FighterType.Goblin, 1),
                FighterFactory.GetFighter(FighterType.Goblin, 1),
                FighterFactory.GetFighter(FighterType.Goblin, 1)
            });

            _manager.InitializeForBattle(_playerTeam, _enemyTeam);
        }

        [TearDown]
        public void TearDown()
        {
            _input = null;
            _output = null;
            _playerOne = null;
            _playerTwo = null;
            _playerThree = null;
            _playerTeam = null;
            _enemyTeam = null;
            _manager = null;
        }

        [Test]
        public void HappyPath_AllFightersAttack()
        {
            for (var i = 1; i <= 3; ++ i)
            {
                _input.Push("fight");
                _input.Push("attack");
                _input.Push(i.ToString());
            }

            var selections = _manager.GetInputs();

            Assert.AreEqual(3, selections.Count);

            for (var i = 0; i < 3; ++i)
            {
                var selection = selections[i];
                var enemy = _enemyTeam.Fighters[i];

                Assert.AreEqual(BattleMoveType.Attack, selection.Move.MoveType);
                Assert.AreEqual(enemy, selection.Target);
                Assert.AreEqual(_playerTeam.Fighters[i], selection.Owner);
            }
        }

        [Test]
        public void FightersInputSkipped_IfFighterIsDead()
        {
            _playerTwo.PhysicalDamage(_playerTwo.MaxHealth);
            _playerThree.PhysicalDamage(_playerThree.MaxHealth);

            _input.Push("fight");
            _input.Push("attack");
            _input.Push("1");

            var selections = _manager.GetInputs();

            Assert.AreEqual(1, selections.Count);

            var selection = selections[0];
            var enemy = _enemyTeam.Fighters[0];


            Assert.AreEqual(BattleMoveType.Attack, selection.Move.MoveType);
            Assert.AreEqual(enemy, selection.Target);
            Assert.AreEqual(_playerOne, selection.Owner);
        }

        [Test]
        public void BackOption_CorrectlyHandled()
        {
            for (var i = 0; i < 3; ++i)
            {
                if (i == 1)
                {
                    _input.Push("back");
                    _input.Push("fight");
                    _input.Push("magic");
                    _input.Push("fireball");
                    _input.Push("1");
                }
                _input.Push("fight");
                _input.Push("attack");
                _input.Push("1");
            }

            var selections = _manager.GetInputs();

            Assert.AreEqual(3, selections.Count);

            var enemy = _enemyTeam.Fighters[0];
            for (var i = 0; i < 3; ++i)
            {
                var selection = selections[i];

                if (i == 0)
                {
                    Assert.AreEqual(BattleMoveType.Spell, selection.Move.MoveType);
                    Assert.AreEqual("fireball", selection.Move.Description);
                }
                else
                {
                    Assert.AreEqual(BattleMoveType.Attack, selection.Move.MoveType);
                }
                Assert.AreEqual(enemy, selection.Target);
                Assert.AreEqual(_playerTeam.Fighters[i], selection.Owner);
            }
        }

        [Test]
        public void OnRunMethod_AppropriatelyRaisesRanEvent()
        {
            _logger.Subscribe(EventType.Ran, _manager);

            _manager.OnRun(new RanEventArgs());

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
        }

        [Test]
        public void FighterRunning_RaisesRunEvent_FirstFighterRuns([Values(0, 1, 2)] int whoRuns)
        {
            for (var i = 0; i < 3; ++i)
            {
                if (i == whoRuns)
                {
                    _input.Push(new List<string> { "run", "yes" });
                }
                else
                {
                    _input.Push(new List<string> { "fight", "attack", "1" });
                }
            }

            _logger.Subscribe(EventType.Ran, _manager);

            var inputs = _manager.GetInputs();

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(3, inputs.Count);

            for (var i = 0; i < 3; ++i)
            {
                Assert.IsNull(inputs[i]);
            }
        }

        [Test]
        public void RunOptionRequiresConfirmation([Values(0, 1, 2)] int whoRuns)
        {
            for (var i = 0; i < 3; ++i)
            {
                if (i == whoRuns)
                {
                    _input.Push(new List<string> {"run", "no"});
                }

                _input.Push(new List<string> {"fight", "attack", "1"});
            }

            _logger.Subscribe(EventType.Ran, _manager);

            var inputs = _manager.GetInputs();

            var logs = _logger.Logs;

            Assert.AreEqual(3, inputs.Count);
            Assert.AreEqual(0, logs.Count);

            var enemy = _enemyTeam.Fighters[0];

            for (var i = 0; i < 3; ++i)
            {
                var selection = inputs[i];
                
                Assert.AreEqual(BattleMoveType.Attack, selection.Move.MoveType);
                Assert.AreEqual(enemy, selection.Target);
                Assert.AreEqual(_playerTeam.Fighters[i], selection.Owner);
            }
        }

        [Test]
        public void CorrectlyHandles_MultiTurnMoves()
        {
            DoNothingMove firstMove = new DoNothingMove("foo");
            DoNothingMove secondMove = new DoNothingMove("bar");
            DoNothingMove thirdMove = new DoNothingMove("baz");

            TestMultiTurnMove multiTurnMove = new TestMultiTurnMove("foo", TargetType.Self, firstMove, secondMove, thirdMove);

            TestEnemyFighter enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            enemy.SetMove(new DoNothingMove("fwop"));
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), enemy);

            TestHumanFighter human = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            human.AddSpecialMove(multiTurnMove);
            
            _playerTeam = new Team(_manager, human);

            _manager.InitializeForBattle(_playerTeam, _enemyTeam);

            _input.Push(new List<string> { "fight", "special move", "foo", "1" });

            List<HumanFighter> inputList = new List<HumanFighter> { human };
            List<BattleMoveWithTarget> returnedList = _manager.GetInputs(inputList);

            Assert.AreEqual(firstMove, returnedList[0].Move);

            returnedList = _manager.GetInputs(inputList);

            Assert.AreEqual(secondMove, returnedList[0].Move);

            returnedList = _manager.GetInputs(inputList);

            Assert.AreEqual(thirdMove, returnedList[0].Move);
        }
    }
}
