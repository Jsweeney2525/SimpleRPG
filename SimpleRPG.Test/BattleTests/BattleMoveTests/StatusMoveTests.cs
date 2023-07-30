using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.BattleMoveTests
{
    [TestFixture]
    public class StatusMoveTests
    {
        private MockOutput _output;
        private MockInput _input;
        private MockChanceService _chanceService;

        private TestEnemyFighter _team1Fighter;
        private TestEnemyFighter _team2Fighter;

        private Team _team1;
        private Team _team2;

        private TestBattleManager _battleManager;

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void SetUp()
        {
            _chanceService = new MockChanceService();
            _output = new MockOutput();
            _input = new MockInput();

            _team1Fighter = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _team2Fighter = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);

            _team1 = new Team(TestMenuManager.GetTestMenuManager(), _team1Fighter);
            _team2 = new Team(TestMenuManager.GetTestMenuManager(), _team2Fighter);

            _battleManager = new TestBattleManager(_chanceService, _input, _output);
        }

        [Test]
        public void StatusMove_AppropriatelyDisplaysExecutionText()
        {
            const string executionText = "performs the foo ritual";
            Status status = new MagicMultiplierStatus(2, MagicType.Water, 2);
            StatusMove move = new StatusMove("foo", TargetType.Self, status, executionText);
            
            _team1Fighter.SetMove(move, 1);
            _team1Fighter.SetMove(_runawayMove);

            _team2Fighter.SetMove(_doNothingMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowExpAndLevelUpMessages = false
            };

            //status move hits
            _chanceService.PushEventOccurs(true);

            _battleManager.Battle(_team1, _team2, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Length);

            string expectedStatusOutput = $"{_team1Fighter.DisplayName} {executionText}!\n";

            Assert.AreEqual(expectedStatusOutput, outputs[0].Message);
        }

        [Test]
        public void StatusMove_AppropriatelyChecksAccuracy([Values(10, 25, 50, 90)] int accuracy)
        {
            //Arrange
            Status status = new BlindStatus(2);
            StatusMove move = new StatusMove("foo", TargetType.SingleEnemy, status, accuracy: accuracy);

            _team1Fighter.SetMove(move, 1);
            _team1Fighter.SetMoveTarget(_team2Fighter);
            _team1Fighter.SetMove(_runawayMove);

            _team2Fighter.SetMove(_doNothingMove);

            _chanceService.PushEventOccurs(false);

            BattleManagerBattleConfiguration config = new SilentBattleConfiguration();

            //Act
            _battleManager.Battle(_team1, _team2, config: config);

            //Assert
            Assert.Null(_team2Fighter.Statuses.FirstOrDefault(s => s.AreEqual(status)));

            double expectedChance = accuracy/100.0;

            Assert.AreEqual(expectedChance, _chanceService.LastChanceVals[0]);
        }
    }
}
