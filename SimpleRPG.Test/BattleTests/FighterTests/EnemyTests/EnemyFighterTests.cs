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
    public class EnemyFighterTests
    {
        private TestEnemyFighter _fighter;
        private HumanFighter _enemy1;
        private HumanFighter _enemy2;

        private Team _ownTeam;
        private Team _singleEnemyTeam;
        private Team _doubleEnemyTeam;

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

            _fighter = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "hero");
            _enemy1 = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "enemy");
            _enemy2  = (HumanFighter)FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1, "enemy");

            _ownTeam = new Team(_menuManager, _fighter);
            _singleEnemyTeam = new Team(_menuManager, _enemy1);
            _doubleEnemyTeam = new Team(_menuManager, _enemy1, _enemy2);
        }

        [Test]
        public void SelectMove_ShouldReturnAttack_OnlyOneEnemy()
        {
            var selectedMove = _fighter.SetupMove(_ownTeam, _singleEnemyTeam);

            Assert.IsNotNull(selectedMove);
            Assert.AreEqual(BattleMoveType.Attack, selectedMove.Move.MoveType);
            Assert.AreEqual("attack", selectedMove.Move.Description);

            Assert.AreEqual(_enemy1, selectedMove.Target);
        }

        [Test]
        public void SelectMove_ShouldReturnAttack_TwoEnemies()
        {
            _mockChanceService.PushWhichEventOccurs(0);
            var selectedMove = _fighter.SetupMove(_ownTeam, _doubleEnemyTeam);
            
            Assert.AreEqual(BattleMoveType.Attack, selectedMove.Move.MoveType);
            Assert.AreEqual("attack", selectedMove.Move.Description);

            Assert.AreEqual(_enemy1, selectedMove.Target);

            _mockChanceService.PushWhichEventOccurs(1);
            selectedMove = _fighter.SetupMove(_ownTeam, _doubleEnemyTeam);
            
            Assert.AreEqual(BattleMoveType.Attack, selectedMove.Move.MoveType);
            Assert.AreEqual("attack", selectedMove.Move.Description);
            Assert.AreEqual(_enemy2, selectedMove.Target);
        }

        [Test]
        public void SelectMove_CorrectlySelectsSelfAsTarget_IfTargetTypeIsSelf()
        {
            var selfTargetMove = TestMoveFactory.Get();
            _fighter.SetAvailableMove(selfTargetMove);

            var selectedMove = _fighter.SetupMove(_ownTeam, _singleEnemyTeam);

            Assert.That(selectedMove, Is.Not.Null);
            Assert.AreEqual(selfTargetMove, selectedMove.Move);
            Assert.AreEqual(_fighter, selectedMove.Target);
        }

        [Test]
        public void SelectTarget_CorrectlySelectsAlly_TargetTypeSingleAlly()
        {
            BattleMove targetAllyMove = new BattleMove("foo", BattleMoveType.Spell, TargetType.SingleAlly);
            _fighter.SetAvailableMove(targetAllyMove);

            _ownTeam = new Team(TestMenuManager.GetTestMenuManager(), _fighter, FighterFactory.GetFighter(FighterType.Fairy, 2), FighterFactory.GetFighter(FighterType.Goblin, 2));
            _mockChanceService.PushWhichEventOccurs(0);

            BattleMoveWithTarget selectedMove = _fighter.SetupMove(_ownTeam, _singleEnemyTeam);

            double[] lastChanceArr = _mockChanceService.LastEventOccursArgs;
            Assert.AreEqual(2, lastChanceArr.Length); //will be 1 if it targets the human team, 3 if the fighter included itself

            IFighter selectedTarget = selectedMove.Target;

            Assert.AreEqual(_ownTeam.Fighters[1], selectedTarget);
        }

        [Test]
        public void SelectTarget_CorrectlySelectsAlly_TargetTypeSingleAllyOrSelf()
        {
            BattleMove targetAllyMove = new BattleMove("foo", BattleMoveType.Spell, TargetType.SingleAllyOrSelf);
            _fighter.SetAvailableMove(targetAllyMove);

            _ownTeam = new Team(TestMenuManager.GetTestMenuManager(), _fighter, FighterFactory.GetFighter(FighterType.Fairy, 2), FighterFactory.GetFighter(FighterType.Goblin, 2));
            _mockChanceService.PushWhichEventOccurs(0);

            BattleMoveWithTarget selectedMove = _fighter.SetupMove(_ownTeam, _singleEnemyTeam);

            double[] lastChanceArr = _mockChanceService.LastEventOccursArgs;
            Assert.AreEqual(3, lastChanceArr.Length);

            IFighter selectedTarget = selectedMove.Target;

            Assert.AreEqual(_fighter, selectedTarget);
        }

        [Test]
        public void SelectMove_DoesNotTargetDefeatedEnemy_OnlyOneAliveEnemy()
        {
            _enemy1.PhysicalDamage(_enemy1.MaxHealth);

            var selectedMove = _fighter.SetupMove(_ownTeam, _doubleEnemyTeam);

            Assert.AreEqual(BattleMoveType.Attack, selectedMove.Move.MoveType);
            Assert.AreEqual("attack", selectedMove.Move.Description);
            Assert.AreEqual(_enemy2, selectedMove.Target);
        }

        [Test]
        public void SelectMove_DoesNotTargetDefeatedEnemy_MultipleAliveEnemies()
        {
            var enemies = _doubleEnemyTeam.Fighters;

            var newEnemy1 = (HumanFighter) FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            enemies.Add(newEnemy1);
            var newEnemy2 = (HumanFighter) FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            enemies.Add(newEnemy2);

            var enemyTeam = new Team(_menuManager, enemies.ToArray());

            _enemy1.PhysicalDamage(_enemy1.MaxHealth);

            //should select the first new enemy
            _mockChanceService.PushWhichEventOccurs(1);
            var selectedMove = _fighter.SetupMove(_ownTeam, enemyTeam);

            Assert.AreEqual(BattleMoveType.Attack, selectedMove.Move.MoveType);
            Assert.AreEqual("attack", selectedMove.Move.Description);
            Assert.AreEqual(newEnemy1, selectedMove.Target);
        }

        [Test]
        public void SelectMove_MultiTurnMoveSelected()
        {
            var enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            var enemyTeam = new Team(_menuManager, enemy);

            var human = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            var humanTeam = new Team(_menuManager, human);

            var fooMove = new DoNothingMove("Foo!");
            var barMove = new DoNothingMove("Bar!");
            var bazMove = new AttackBattleMove("Baz", TargetType.SingleEnemy, 50, 100);

            var multiTurnMove = new MultiTurnBattleMove("multi turn move", TargetType.SingleEnemy,
                fooMove,
                barMove,
                bazMove);

            enemy.SetAvailableMove(multiTurnMove);

            var selectedMove = enemy.SetupMove(enemyTeam, humanTeam);
            Assert.AreEqual(multiTurnMove, selectedMove.Move);

            var attack = MoveFactory.Get(BattleMoveType.Attack);
            enemy.SetAvailableMove(attack);

            selectedMove = enemy.SetupMove(enemyTeam, humanTeam);
            Assert.AreEqual(barMove, selectedMove.Move);

            selectedMove = enemy.SetupMove(enemyTeam, humanTeam);
            Assert.AreEqual(bazMove, selectedMove.Move);

            selectedMove = enemy.SetupMove(enemyTeam, humanTeam);
            Assert.AreEqual(attack, selectedMove.Move);
        }

        [Test]
        public void ConstructorInitializesExpGiven([Values(FighterType.Fairy, FighterType.Goblin, FighterType.Golem, FighterType.Ogre)] FighterType type,
            [Values(1, 2, 3)] int level)
        {
            var fighter = (EnemyFighter) FighterFactory.GetFighter(type, level);

            Assert.AreEqual(level * 5, fighter.ExpGivenOnDefeat);
        }

        [Test]
        public void EggEnemy_ConstructorInitializesExpGiven([Values(1, 2, 3)] int level)
        {
            var fighter = (EnemyFighter)FighterFactory.GetFighter(FighterType.Egg, level, null, MagicType.Fire);

            Assert.AreEqual(0, fighter.ExpGivenOnDefeat);
        }
    }
}
