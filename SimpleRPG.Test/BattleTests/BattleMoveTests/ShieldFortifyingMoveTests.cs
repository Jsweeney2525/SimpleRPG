using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    public class ShieldFortifyingMoveTests
    {
        private EventLogger _logger;
        private TestHumanFighter _humanFighter;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemy;
        private Team _enemyTeam;

        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;

        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private DoNothingMove _doNothing;
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            _humanFighter = new TestHumanFighter("foo", 1);
            _humanTeam = new TestTeam(_humanFighter);

            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _enemyTeam = new Team(_menuManager, _enemy);

            _doNothing = new DoNothingMove();
        }

        [Test]
        public void CopyMethod_ReturnsCorrectResult()
        {
            const int shieldFortifyingAmount = 3;
            ShieldFortifyingMove move = new ShieldFortifyingMove("foo", TargetType.SingleAlly, null, ShieldFortifyingType.Defense, shieldFortifyingAmount);

            ShieldFortifyingMove copy = new ShieldFortifyingMove(move);

            Assert.AreEqual(ShieldFortifyingType.Defense, copy.FortifyingType);
            Assert.AreEqual(shieldFortifyingAmount, copy.FortifyingAmount);
        }

        [Test]
        public void BattleManager_CorrectlyExecutes_ShieldFortifyingMove([Values(ShieldFortifyingType.Defense, ShieldFortifyingType.Health)] ShieldFortifyingType shieldFortifyingType)
        {
            ElementalBattleShield shield = new ElementalBattleShield(10, 5, 0, MagicType.Fire);
            shield.DecrementHealth(5);
            _humanFighter.SetBattleShield(shield);
            IBattleShield copiedShield = _humanFighter.BattleShield;

            ShieldFortifyingMove shieldFortifyingMove = new ShieldFortifyingMove("foo", TargetType.Self, null, shieldFortifyingType, 5);

            _humanFighter.SetMove(shieldFortifyingMove, 1);
            _humanFighter.SetMove(_runawayMove);
            _humanFighter.SetMoveTarget(_humanFighter);

            _enemy.SetMove(_doNothing);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.NotNull(copiedShield);

            int actualValue = shieldFortifyingType == ShieldFortifyingType.Defense ? copiedShield.Defense : copiedShield.CurrentHealth;
            Assert.AreEqual(10, actualValue);
        }

        [Test]
        public void BattleManager_CorrectlyPrintsErrorMessage_TargetHasNoShieldEquipped()
        {
            ShieldFortifyingMove shieldFortifyingMove = new ShieldFortifyingMove("foo", TargetType.Self, null, ShieldFortifyingType.Defense, 5);

            _humanFighter.SetMove(shieldFortifyingMove, 1);
            _humanFighter.SetMove(_runawayMove);
            _humanFighter.SetMoveTarget(_humanFighter);

            _enemy.SetMove(_doNothing);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);
            Assert.AreEqual($"But it failed because {_humanFighter.DisplayName} did not have a battleShield equipped!\n", outputs[0].Message);
        }
    }
}
