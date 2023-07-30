using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class BarbarianTests
    {
        private Barbarian _barbarian;
        private TestHumanFighter _hero;
        private TestHumanFighter _sidekick;

        private Team _enemyTeam;
        private TestTeam _humanTeam;

        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)TestMoveFactory.Get(TargetType.Self, moveType: BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            
            _chanceService = new MockChanceService();

            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            TestFighterFactory.SetChanceService(_chanceService);

            _barbarian = (Barbarian)FighterFactory.GetFighter(FighterType.Barbarian, 1);
            _enemyTeam = new Team(_menuManager, _barbarian);

            _hero = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Hero");
            _sidekick = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "SideKick");
            _humanTeam = new TestTeam(new List<HumanFighter> { _hero, _sidekick});
        }

        [Test]
        public void GetZeroTurnMove_FirstBattle_IsUnbustableShieldMove()
        {
            //Arrange
            _barbarian.PreBattleSetup(_enemyTeam, _humanTeam, _output, BattleConfigurationSpecialFlag.FirstBarbarianBattle);
            //Act
            BattleMoveWithTarget moveWithTarget = _barbarian.GetZeroTurnMove(_enemyTeam, _humanTeam);
            //Assert
            ShieldMove move = moveWithTarget.Move as ShieldMove;

            Assert.NotNull(move);

            IBattleShield shield = move.Shield;

            Assert.AreEqual(1, shield.ShieldBusterDefense);
        }

        [Test]
        public void FirstBattle_BarbarianDoesNothingWhileEquippedWithShield()
        {
            _barbarian.PreBattleSetup(_enemyTeam, _humanTeam, _output, BattleConfigurationSpecialFlag.FirstBarbarianBattle);
            BattleMoveWithTarget moveWithTarget = _barbarian.GetZeroTurnMove(_enemyTeam, _humanTeam);
            ShieldMove move = moveWithTarget.Move as ShieldMove;

            _barbarian.SetBattleShield(move?.Shield as BattleShield);

            BattleMove battleMove = _barbarian.SelectMove(_enemyTeam, _humanTeam);

            Assert.IsAssignableFrom<DoNothingMove>(battleMove);
        }

        [Test]
        public void FirstBattle_BarbarianRunsAway_3TurnsAfterShieldDestroyed()
        {
            _barbarian.PreBattleSetup(_enemyTeam, _humanTeam, _output, BattleConfigurationSpecialFlag.FirstBarbarianBattle);

            BattleMoveWithTarget moveWithTarget = _barbarian.SetupMove(_enemyTeam, _humanTeam);
            BattleMove battleMove = moveWithTarget.Move;
            MultiTurnBattleMove multiTurnMove = battleMove as MultiTurnBattleMove;
            
            Assert.NotNull(multiTurnMove);
            Assert.AreEqual(3, multiTurnMove.Moves.Count);

            for (int i = 0; i < 3; ++i)
            {
                battleMove = multiTurnMove.Moves[i];

                if (i < 2)
                {
                    Assert.IsAssignableFrom<DoNothingMove>(battleMove);
                }
                else
                {
                    Assert.AreEqual(BattleMoveType.Runaway, battleMove.MoveType);
                }
            }
        }
    }
}