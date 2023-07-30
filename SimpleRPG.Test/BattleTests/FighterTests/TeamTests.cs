using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    internal class TeamTests
    {
        private TestHumanFighter _humanPlayer1, _humanPlayer2;
        private Team _humanTeam;
        private TestEnemyFighter _enemyPlayer1, _enemyPlayer2;
        private Team _enemyTeam;
        
        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;

        private const string DefaultEnemyName = "Test";

        private BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();

            TestFighterFactory.SetChanceService(_chanceService);

            _humanPlayer1 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _humanPlayer2 = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jed");
            _enemyPlayer1 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
            _enemyPlayer2 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);

            _humanTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2);
            _enemyTeam = new Team(_menuManager, _enemyPlayer1, _enemyPlayer2);

            _humanTeam.InitializeForBattle(_enemyTeam, _input, _output);
        }

        [Test]
        public void EnemyTeam_TestGetInputs()
        {
            var count = _enemyTeam.Fighters.Count;

            for (var i = 0; i < count; ++i)
            {
                _chanceService.PushWhichEventOccurs(0);
            }

            var moves = _enemyTeam.GetInputs(_humanTeam);

            
            Assert.AreEqual(count, moves.Count);

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(BattleMoveType.Attack, moves[i].Move.MoveType);
                Assert.AreEqual(_humanTeam.Fighters[0], moves[i].Target);
                Assert.AreEqual(_enemyTeam.Fighters[i], moves[i].Owner);
            }
        }

        [Test]
        public void HumanTeam_TestGetInputs()
        {
            var count = _humanTeam.Fighters.Count;

            for (var i = 0; i < count; ++i)
            {
                _input.Push("fight");
                _input.Push("attack");
                _input.Push("1");
            }

            var moves = _humanTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(count, moves.Count);

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(BattleMoveType.Attack, moves[i].Move.MoveType);
                Assert.AreEqual(_enemyPlayer1, moves[i].Target);
            }
        }

        [Test]
        public void EnemyTeam_TestAdd()
        {
            var count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            var newFighter = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam.Add(newFighter);

            count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(3, count);
            Assert.IsTrue(_enemyTeam.Contains(newFighter));
        }

        [Test]
        public void EnemyTeam_TestAdd_ThenGetInputs()
        {
            var newFighter = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam.Add(newFighter);

            var newFighter2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam.Add(newFighter2);

            _chanceService.PushWhichEventOccurs(0);
            _chanceService.PushWhichEventOccurs(1);
            _chanceService.PushWhichEventOccurs(0);
            _chanceService.PushWhichEventOccurs(1);

            var count = _enemyTeam.Fighters.Count;
            var inputs = _enemyTeam.GetInputs(_humanTeam);

            Assert.AreEqual(count, inputs.Count);
        }

        [Test]
        public void AddMethod_CorrectlyUpdatesDisplayNames_UpdateDisplayNamesSetToTrue()
        {
            var count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            var newFighter =
                (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
            _enemyTeam.Add(newFighter);

            var fighters = _enemyTeam.Fighters;
            for (var i = 0; i < fighters.Count; ++i)
            {
                Assert.AreEqual($"{DefaultEnemyName} {(char)('A' + i)}", fighters[i].DisplayName);
            }
        }

        [Test]
        public void AddMethod_DoesNotUpdateDisplayNames_UpdateDisplayNamesSetToFalse()
        {
            var count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            var newFighter =
                (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
            _enemyTeam.Add(newFighter, false);

            var fighters = _enemyTeam.Fighters;
            for (var i = 0; i < 2; ++i)
            {
                Assert.AreEqual($"{DefaultEnemyName} {(char)('A' + i)}", fighters[i].DisplayName);
            }

            Assert.AreEqual($"{DefaultEnemyName}", fighters[2].DisplayName);
        }

        [Test]
        public void EnemyTeam_TestAdd_CorrectlyUpdatesDisplayNames_MoreThan26Fighters()
        {
            TestTeamFactory teamFactory = new TestTeamFactory(_chanceService);
            var team = teamFactory.GetTeam(27, DefaultEnemyName);

            Assert.AreEqual("Test AA", team.Fighters[26].DisplayName);
            
            var fighter = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
            
            for (var i = 0; i < 26; ++i)
            {
                fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
                team.Add(fighter);
            }

            Assert.AreEqual(53, team.Fighters.Count);

            for (short i = 26, j = 0; i < 52; ++i, ++j)
            {
                Assert.AreEqual($"Test A{(char)('A' + j)}", team.Fighters[i].DisplayName);
            }

            Assert.AreEqual("Test BA", fighter.DisplayName);
        }

        [Test]
        public void HumanTeam_TestAdd()
        {
            var count = _humanTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            var newFighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam.Add(newFighter);

            count = _humanTeam.Fighters.Count;
            Assert.AreEqual(3, count);
            Assert.IsTrue(_humanTeam.Contains(newFighter));
        }

        [Test]
        public void HumanTeam_TestAdd_ThenGetInputs()
        {
            var newFighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam.Add(newFighter);

            var newFighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam.Add(newFighter2);

            var newEnemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam.Add(newEnemy);

            var count = _humanTeam.Fighters.Count;
            
            for (var i = 0; i < count; ++i)
            {
                _input.Push("fight");
                _input.Push("attack");
                _input.Push("3");
            }

            var inputs = _humanTeam.GetInputs(_enemyTeam);
            Assert.AreEqual(count, inputs.Count);

            foreach (var input in inputs)
            {
                Assert.AreEqual(newEnemy, input.Target);
            }
        }

        [Test]
        public void EnemyTeam_TestRemove()
        {
            var count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            _enemyTeam.Remove(_enemyPlayer2);

            count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(1, count);
            Assert.IsFalse(_enemyTeam.Contains(_enemyPlayer2));
        }

        [Test]
        public void Team_TestRemove_CorrectlyUpdatesDisplayNames()
        {
            Goblin goblin1 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin2 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin3 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin4 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);

            Team testTeam = new Team(TestMenuManager.GetTestMenuManager(),
                goblin1,
                goblin2,
                goblin3,
                goblin4
                );

            Assert.AreEqual("D", goblin4.AppendText);

            testTeam.Remove(goblin2);
            testTeam.Remove(goblin3);

            Assert.AreEqual("B", goblin2.AppendText);
        }

        [Test]
        public void Team_TestAddRange_CorrectlyUpdatesDisplayNames()
        {
            Goblin goblin1 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin2 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin3 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin4 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);

            Team testTeam = new Team(TestMenuManager.GetTestMenuManager(),
                goblin1
                );

            List<Goblin> goblinsToAdd = new List<Goblin> { goblin2, goblin3, goblin4 };
            testTeam.AddRange(goblinsToAdd);

            for (var i = 0; i < 4; ++i)
            {
                EnemyFighter enemy = testTeam.Fighters[i] as EnemyFighter;
                char expectedAppendedChar = (char) ('A' + i);
                string expectedAppendStr = $"{expectedAppendedChar}";

                Assert.AreEqual(expectedAppendStr, enemy?.AppendText);
            }
        }

        [Test]
        public void EnemyTeam_TestRemove_ThenGetInputs()
        {
            var count = _enemyTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            _enemyTeam.Remove(_enemyPlayer1);

            _chanceService.PushWhichEventOccurs(0);
            var inputs = _enemyTeam.GetInputs(_humanTeam);

            Assert.AreEqual(1, inputs.Count);
        }

        [Test]
        public void HumanTeam_TestRemove()
        {
            var count = _humanTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            _humanTeam.Remove(_humanPlayer1);

            count = _humanTeam.Fighters.Count;
            Assert.AreEqual(1, count);
            Assert.IsFalse(_humanTeam.Contains(_humanPlayer1));
        }

        [Test]
        public void HumanTeam_TestRemove_ThenGetInputs()
        {
            var count = _humanTeam.Fighters.Count;
            Assert.AreEqual(2, count);

            _humanTeam.Remove(_humanPlayer2);
            _enemyTeam.Remove(_enemyPlayer2);

            for (var i = 0; i < count; ++i)
            {
                _input.Push("fight");
                _input.Push("attack");
                _input.Push("2");
                _input.Push("1");
            }

            var inputs = _humanTeam.GetInputs(_enemyTeam);
            Assert.AreEqual(1, inputs.Count);

            Assert.AreEqual(_enemyPlayer1, inputs[0].Target);
        }

        [Test]
        public void EnemyTeam_TestIsTeamDefeated()
        {
            Assert.IsFalse(_enemyTeam.IsTeamDefeated());

            foreach (var enemy in _enemyTeam.Fighters)
            {
                enemy.PhysicalDamage(enemy.MaxHealth);
            }

            Assert.IsTrue(_enemyTeam.IsTeamDefeated());
        }

        [Test]
        public void HumanTeam_TestIsTeamDefeated()
        {
            Assert.IsFalse(_humanTeam.IsTeamDefeated());

            foreach (var human in _humanTeam.Fighters)
            {
                human.PhysicalDamage(human.MaxHealth);
            }

            Assert.IsTrue(_humanTeam.IsTeamDefeated());
        }

        [Test]
        public void HumanTeam_GetInputs_MultiTurnMove()
        {
            var multiMove = (MultiTurnBattleMove)TestMoveFactory.Get(description: "testMultiTurn");

            _humanPlayer1.AddSpecialMove(multiMove);

            _humanTeam = new Team(_menuManager, _humanPlayer1);
            _humanTeam.InitializeForBattle(_enemyTeam, _input, _output);

            _input.Push(new List<string> { "fight", "special", multiMove.Description, "1"});

            foreach (var move in multiMove.Moves)
            {
                var battleMove = _humanTeam.GetInputs(_enemyTeam)[0];

                Assert.AreEqual(move, battleMove.Move);

                switch (move.TargetType)
                {
                    case TargetType.Self:
                        Assert.AreEqual(_humanPlayer1, battleMove.Target);
                        break;
                    case TargetType.SingleEnemy:
                        Assert.AreEqual(_enemyTeam.Fighters[0], battleMove.Target);
                        break;
                }
            }

        }

        [Test]
        public void HumanTeam_MultiTurnMove_MembersOfTeamIsDead()
        {
            var multiMove = (MultiTurnBattleMove) TestMoveFactory.Get(description: "testMultiTurn");
            var attackMove = TestMoveFactory.Get(TargetType.SingleEnemy, "", BattleMoveType.Attack);

            _humanPlayer1.AddSpecialMove(multiMove);

            _humanTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2);
            _humanTeam.InitializeForBattle(_enemyTeam, _input, _output);

            _input.Push(new List<string> {"fight", "special", multiMove.Description, "1"});
            _input.Push(new List<string> {"fight", "attack", "1"});

            var inputs = _humanTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(2, inputs.Count);

            Assert.AreEqual(multiMove.Moves[0], inputs[0].Move);
            Assert.AreEqual(attackMove, inputs[1].Move);

            _humanPlayer1.PhysicalDamage(_humanPlayer1.MaxHealth);

            _input.Push(new List<string> { "fight", "attack", "1" });
            inputs = _humanTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(1, inputs.Count);

            Assert.AreEqual(attackMove, inputs[0].Move);
        }

        [Test]
        public void TeamClass_CorrectlyGetsInputs_TeamComprisedOnlyOfHumanFighters()
        {
            Team testTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2);
            
            for (var i = 0; i < testTeam.Fighters.Count; ++i)
            {
                _input.Push(new List<string> {"fight", "attack", "1"});
            }
            
            List<BattleMoveWithTarget> selectedMoves = testTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(2, selectedMoves.Count);
        }

        [Test]
        public void GetInputs_TeamEntirelyHumanFighters_DoesNotGetInputFromDefeatedFighters()
        {
            TestHumanFighter fighter3 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            TestHumanFighter fighter4 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            Team testTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2, fighter3, fighter4);

            _humanPlayer2.PhysicalDamage(_humanPlayer2.MaxHealth);
            fighter3.PhysicalDamage(fighter3.MaxHealth);
            fighter4.PhysicalDamage(fighter4.MaxHealth);

            foreach (TestHumanFighter fighter in testTeam.Fighters.Cast<TestHumanFighter>())
            {
                fighter.SetMove(_basicAttackMove);
                fighter.SetMoveTarget(_enemyPlayer1);
            }

            List<BattleMoveWithTarget> returnedInputs = testTeam.GetInputs(testTeam);
            Assert.AreEqual(1, returnedInputs.Count);
            BattleMoveWithTarget moveWithTarget = returnedInputs[0];
            Assert.AreEqual(_humanPlayer1, moveWithTarget.Owner);

        }

        [Test]
        public void TeamClass_CorrectlyGetsInputs_TeamComprisedOnlyOfEnemyFighters()
        {
            Team testTeam = new Team(_menuManager, _enemyPlayer1, _enemyPlayer2);
            _chanceService.PushWhichEventsOccur(0, 0);
            
            List<BattleMoveWithTarget> selectedMoves = testTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(2, selectedMoves.Count);
        }

        [Test]
        public void GetInputs_TeamEntirelyEnemyFighters_DoesNotGetInputFromDefeatedFighters()
        {
            TestEnemyFighter fighter3 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            TestEnemyFighter fighter4 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team testTeam = new Team(_menuManager, _enemyPlayer1, _enemyPlayer2, fighter3, fighter4);

            _enemyPlayer1.PhysicalDamage(_enemyPlayer1.MaxHealth);
            fighter3.PhysicalDamage(fighter3.MaxHealth);
            fighter4.PhysicalDamage(fighter4.MaxHealth);

            foreach (TestEnemyFighter fighter in testTeam.Fighters.Cast<TestEnemyFighter>())
            {
                fighter.SetMove(_basicAttackMove);
                fighter.SetMoveTarget(_enemyPlayer1);
            }

            List<BattleMoveWithTarget> returnedInputs = testTeam.GetInputs(testTeam);
            Assert.AreEqual(1, returnedInputs.Count);
            BattleMoveWithTarget moveWithTarget = returnedInputs[0];
            Assert.AreEqual(_enemyPlayer2, moveWithTarget.Owner);

        }

        [Test]
        public void TeamClass_CorrectlyGetsInputs_TeamComprisedOfBothHumanAndEnemyFighters()
        {
            Team testTeam = new Team(_menuManager, _humanPlayer1, _enemyPlayer1);

            //setup human fighter Move
            _input.Push(new List<string> { "fight", "attack", "1" });
            //setup enemy fighter Move
            _chanceService.PushWhichEventsOccur(0);
            
            _enemyTeam = new Team(_menuManager, _enemyPlayer2);

            List<BattleMoveWithTarget> selectedMoves = testTeam.GetInputs(_enemyTeam);

            Assert.AreEqual(2, selectedMoves.Count);
        }

        [Test]
        public void GetInputs_TeamBothHumanAndEnemyFighters_DoesNotGetInputFromDefeatedFighters()
        {
            TestEnemyFighter fighter3 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            TestHumanFighter fighter4 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            Team testTeam = new Team(_menuManager, _enemyPlayer1, _humanPlayer1, fighter3, fighter4);

            _enemyPlayer1.PhysicalDamage(_enemyPlayer1.MaxHealth);
            _humanPlayer1.PhysicalDamage(_humanPlayer1.MaxHealth);
            fighter4.PhysicalDamage(fighter4.MaxHealth);

            foreach (ITestFighter fighter in testTeam.Fighters.Cast<ITestFighter>())
            {
                fighter.SetMove(_basicAttackMove);
                fighter.SetMoveTarget(_enemyPlayer1);
            }

            List<BattleMoveWithTarget> returnedInputs = testTeam.GetInputs(testTeam);
            Assert.AreEqual(1, returnedInputs.Count);
            BattleMoveWithTarget moveWithTarget = returnedInputs[0];
            Assert.AreEqual(fighter3, moveWithTarget.Owner);

        }

        [Test]
        public void Constructor_CorrectlySetsTeamPropertyOfFighters()
        {
            Team newTeam = new Team(_menuManager, _humanPlayer1, _humanPlayer2);

            Assert.AreEqual(newTeam, _humanPlayer1.Team);
            Assert.AreEqual(newTeam, _humanPlayer2.Team);
        }

        [Test]
        public void AddMethod_CorrectlySetsTeamPropertyOfFighters()
        {
            IFighter newFighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            Assert.IsNull(newFighter.Team);

            _humanTeam.Add(newFighter);

            Assert.AreEqual(_humanTeam, newFighter.Team);
        }

        [Test]
        public void AddRangeMethod_CorrectlySetsTeamPropertyOfFighters()
        {
            IFighter newFighter1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            IFighter newFighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);

            Assert.IsNull(newFighter1.Team);
            Assert.IsNull(newFighter2.Team);

            _humanTeam.AddRange(newFighter1, newFighter2);

            Assert.AreEqual(_humanTeam, newFighter1.Team);
            Assert.AreEqual(_humanTeam, newFighter2.Team);
        }

        [Test]
        public void RemoveMethod_CorrectlySetsTeamPropertyOfFighterToNull()
        {
            Assert.AreEqual(_humanTeam, _humanPlayer1.Team);

            _humanTeam.Remove(_humanPlayer1);

            Assert.IsNull(_humanPlayer1.Team);
        }

        [Test]
        public void Constructor_CorrectlySetsDisplayNames_TeamEntirelyComprisedOfHumanControlledEnemies()
        {
            HumanControlledEnemyFighter humanControlledEnemy1 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy1 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            humanControlledEnemy1.SetEnemy(enemy1);

            HumanControlledEnemyFighter humanControlledEnemy2 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy2 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            humanControlledEnemy2.SetEnemy(enemy2);

            HumanControlledEnemyFighter humanControlledEnemy3 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy3 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Fairy, 1);
            humanControlledEnemy3.SetEnemy(enemy3);

            HumanControlledEnemyFighter humanControlledEnemy4 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy4 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Fairy, 1);
            humanControlledEnemy4.SetEnemy(enemy4);

            List<IFighter> fighters = new List<IFighter>
            {
                humanControlledEnemy1,
                humanControlledEnemy2,
                humanControlledEnemy3,
                humanControlledEnemy4,
            };

            Team team = new Team(_menuManager, fighters);

            for (var i = 0; i < 4; ++i)
            {
                IFighter fighter = team.Fighters[i];

                char expectedChar = (char)('A' + (i % 2));
                Assert.AreEqual($"{expectedChar}", fighter.AppendText);
            }
        }

        [Test]
        public void Constructor_CorrectlySetsDisplayNames_TeamEntirelyComprisedOfNormalEnemies()
        {
            IFighter enemy1 = FighterFactory.GetFighter(FighterType.Goblin, 1);
            IFighter enemy2 = FighterFactory.GetFighter(FighterType.Goblin, 1);
            IFighter enemy3 = FighterFactory.GetFighter(FighterType.Fairy, 1);
            IFighter enemy4 = FighterFactory.GetFighter(FighterType.Fairy, 1);

            List<IFighter> fighters = new List<IFighter>
            {
                enemy1,
                enemy2,
                enemy3,
                enemy4,
            };

            Team team = new Team(_menuManager, fighters);

            for (var i = 0; i < 4; ++i)
            {
                IFighter fighter = team.Fighters[i];

                char expectedChar = (char)('A' + (i % 2));
                Assert.AreEqual($"{expectedChar}", fighter.AppendText);
            }
        }

        [Test]
        public void Constructor_CorrectlySetsDisplayNames_DoesNotGroupEggsOfDifferentTypes()
        {
            IFighter enemy1 = FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Fire);
            IFighter enemy2 = FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Ice);
            IFighter enemy3 = FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Lightning);

            List<IFighter> fighters = new List<IFighter>
            {
                enemy1,
                enemy2,
                enemy3
            };

            Team team = new Team(_menuManager, fighters);

            for (var i = 0; i < 3; ++i)
            {
                IFighter fighter = team.Fighters[i];

                Assert.AreEqual("", fighter.AppendText);
            }
        }

        [Test]
        public void Constructor_CorrectlySetsDisplayNames_TeamComprisedOfMixOfNormalEnemiesAndHumanControlledEnemies()
        {
            HumanControlledEnemyFighter humanControlledEnemy1 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy1 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            humanControlledEnemy1.SetEnemy(enemy1);

            HumanControlledEnemyFighter humanControlledEnemy2 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy2 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            humanControlledEnemy2.SetEnemy(enemy2);

            HumanControlledEnemyFighter humanControlledEnemy3 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy3 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Fairy, 1);
            humanControlledEnemy3.SetEnemy(enemy3);

            HumanControlledEnemyFighter humanControlledEnemy4 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy4 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Fairy, 1);
            humanControlledEnemy4.SetEnemy(enemy4);

            HumanControlledEnemyFighter humanControlledEnemy5 = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1);
            EnemyFighter enemy9 = (EnemyFighter)FighterFactory.GetFighter(FighterType.Warrior, 1);
            humanControlledEnemy5.SetEnemy(enemy9);

            IFighter enemy5 = FighterFactory.GetFighter(FighterType.Goblin, 1);
            IFighter enemy6 = FighterFactory.GetFighter(FighterType.Goblin, 1);
            IFighter enemy7 = FighterFactory.GetFighter(FighterType.Fairy, 1);
            IFighter enemy8 = FighterFactory.GetFighter(FighterType.Fairy, 1);
            IFighter enemy10 = FighterFactory.GetFighter(FighterType.Golem, 1);

            List<IFighter> fighters = new List<IFighter>
            {
                humanControlledEnemy1,
                humanControlledEnemy2,
                enemy5,
                enemy6,
                humanControlledEnemy3,
                humanControlledEnemy4,
                enemy7,
                enemy8,
                humanControlledEnemy5,
                enemy10
            };

            Team team = new Team(_menuManager, fighters);
            IFighter fighter;

            for (var i = 0; i < 8; ++i)
            {
                fighter = team.Fighters[i];

                char expectedChar = (char)('A' + (i % 4));
                Assert.AreEqual($"{expectedChar}", fighter.AppendText, $"i: {i}");
            }

            fighter = team.Fighters[8];
            Assert.AreEqual("", fighter.AppendText);

            fighter = team.Fighters[9];
            Assert.AreEqual("", fighter.AppendText);
        }

        [Test]
        public void Constructor_CorrectlyAllowsGroupingInput_AndSetsDisplayNames()
        {
            Goblin goblin1 = (Goblin) FighterFactory.GetFighter(FighterType.Goblin, 1);
            Goblin goblin2 = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            FighterGrouping grouping = new FighterGrouping(goblin1, goblin2);

            Team team = new Team(TestMenuManager.GetTestMenuManager(), grouping);

            Assert.AreEqual(2, team.Fighters.Count);

            Assert.True(team.Fighters.Contains(goblin1));
            Assert.True(team.Fighters.Contains(goblin2));
        }
    }
}