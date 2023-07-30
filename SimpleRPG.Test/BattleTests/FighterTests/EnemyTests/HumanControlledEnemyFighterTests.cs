using System;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    public class HumanControlledEnemyFighterTests
    {
        private HumanControlledEnemyFighter _fighter;
        private TestEnemyFighter _enemy;

        private Team _ownTeam;
        private Team _enemyTeam;

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _mockChanceService;

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);
            FighterFactory.SetInput(_input);
            FighterFactory.SetOutput(_output);

            _fighter = (HumanControlledEnemyFighter)FighterFactory.GetFighter(FighterType.HumanControlledEnemy, 1, "hero");
            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "enemy");
        }

        [Test]
        public void SetEnemy_ProperlySetsHumanControlledEnemyStats()
        {
            TestEnemyFighter enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Grumbles");

            enemy.SetHealth(10);
            enemy.SetMana(10);
            enemy.SetMagicStrength(2);
            enemy.SetMagicResistance(2);
            enemy.SetStrength(3);
            enemy.SetDefense(2);
            enemy.SetSpeed(5);
            enemy.SetEvade(5);
            enemy.SetLuck(20);

            Assert.AreNotEqual(enemy.MaxHealth, _fighter.MaxHealth, "maxHealth");
            Assert.AreNotEqual(enemy.MaxMana, _fighter.MaxMana, "maxMana");
            Assert.AreNotEqual(enemy.MagicStrength, _fighter.MagicStrength, "magicStrength");
            Assert.AreNotEqual(enemy.MagicResistance, _fighter.MagicResistance, "magicResistance");
            Assert.AreNotEqual(enemy.Strength, _fighter.Strength, "strength");
            Assert.AreNotEqual(enemy.Defense, _fighter.Defense, "defense");
            Assert.AreNotEqual(enemy.Speed, _fighter.Speed, "speed");
            Assert.AreNotEqual(enemy.Evade, _fighter.Evade, "evade");
            Assert.AreNotEqual(enemy.Luck, _fighter.Luck, "luck");
            Assert.AreNotEqual(enemy.DisplayName, _fighter.DisplayName, "displayName");

            _fighter.SetEnemy(enemy);

            Assert.AreEqual(enemy.MaxHealth, _fighter.MaxHealth, "maxHealth");
            Assert.AreEqual(enemy.MaxMana, _fighter.MaxMana, "maxMana");
            Assert.AreEqual(enemy.MagicStrength, _fighter.MagicStrength, "magicStrength");
            Assert.AreEqual(enemy.MagicResistance, _fighter.MagicResistance, "magicResistance");
            Assert.AreEqual(enemy.Strength, _fighter.Strength, "strength");
            Assert.AreEqual(enemy.Defense, _fighter.Defense, "defense");
            Assert.AreEqual(enemy.Speed, _fighter.Speed, "speed");
            Assert.AreEqual(enemy.Evade, _fighter.Evade, "evade");
            Assert.AreEqual(enemy.Luck, _fighter.Luck, "luck");
            Assert.AreEqual(enemy.DisplayName, _fighter.DisplayName, "displayName");
        }

        [Test]
        public void SetupMove_PrintsCorrectPrompts()
        {
            Fairy fairy = (Fairy) FighterFactory.GetFighter(FighterType.Fairy, 1);

            _fighter.SetEnemy(fairy);

            _input.Push("1", "1");

            _fighter.SetupMove(_ownTeam, _enemyTeam);

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedOutputLength = 5;
            //menu prompt for both menus, plus "back," "help," and "status" option from target menu
            expectedOutputLength += fairy.AvailableMoves.Count + _enemyTeam.Fighters.Count;
            Assert.AreEqual(expectedOutputLength, outputs.Length);

            int i = 0;

            MockOutputMessage output = outputs[i++];
            Assert.AreEqual($"You are currently selecting a move for {fairy.DisplayName}. What move will you use?\n", output.Message);
            Assert.AreEqual(ConsoleColor.Cyan, output.Color);

            for (int j = 0; j < fairy.AvailableMoves.Count; ++j)
            {
                BattleMove move = fairy.AvailableMoves[j];

                output = outputs[i++];
                Assert.AreEqual($"{j + 1}. {move.Description}\n", output.Message);
            }
        }

        [Test]
        public void SetupMove_CorrectlyHandlesMultiTurnMoves()
        {
            Goblin goblin = (Goblin) FighterFactory.GetFighter(FighterType.Goblin, 1);
            _enemyTeam = new Team(_menuManager, goblin);

            MultiTurnBattleMove multiTurnMove =
                goblin.AvailableMoves.OfType<MultiTurnBattleMove>().First(am => am.MoveType == BattleMoveType.MultiTurn);

            _fighter.SetEnemy(goblin);

            _ownTeam = new Team(_menuManager, _fighter);

            _input.Push("2", "1");

            BattleMoveWithTarget returnedMove = _fighter.SetupMove(_ownTeam, _enemyTeam);

            Assert.AreEqual(multiTurnMove, returnedMove.Move);

            returnedMove = _fighter.SetupMove(_ownTeam, _enemyTeam);

            Assert.AreEqual(multiTurnMove.Moves[1], returnedMove.Move);
        }

        [Test]
        public void SelfTargetMove_TargetsHumanControlledEnemy_NotInnerFighter()
        {
            Goblin goblin = (Goblin)FighterFactory.GetFighter(FighterType.Goblin, 1);
            _enemyTeam = new Team(_menuManager, goblin);

            _fighter.SetEnemy(_enemy);
            _ownTeam = new Team(_menuManager, _fighter);

            _enemy.SetAvailableMove(new BattleMove("foo", BattleMoveType.Dance, TargetType.Self));

            _input.Push("1", "1");

            BattleMoveWithTarget ret = _fighter.SetupMove(_ownTeam, _enemyTeam);

            Assert.AreEqual(_fighter, ret.Target);
            //call me paranoid. I'd rather check
            Assert.AreNotEqual(_enemy, ret.Target);
        }
    }
}
