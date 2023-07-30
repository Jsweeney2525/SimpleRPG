using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    public class ShieldGuyTests
    {
        private TestHumanFighter _humanFighter;
        private ShieldGuy _level1ShieldGuy;
        private TestEnemyFighter _ally1;
        private TestEnemyFighter _ally2;

        private Team _humanTeam;
        private Team _shieldGuyTeam;

        private MockInput _input;
        private MockOutput _output;
        private MockChanceService _chanceService;
        private TestMenuManager _menuManager;
        private TestBattleManager _battleManager;

        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _chanceService = new MockChanceService();
            _menuManager = new TestMenuManager(_input, _output);
            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            TestFighterFactory.SetChanceService(_chanceService);

            _humanFighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam = new Team(_menuManager, _humanFighter);

            _level1ShieldGuy = (ShieldGuy)FighterFactory.GetFighter(FighterType.ShieldGuy, 1);
            _ally1 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _ally2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _shieldGuyTeam = new Team(_menuManager, _level1ShieldGuy, _ally1, _ally2);
        }

        [Test]
        public void Level1ShieldGuy_InitializesWith4Moves()
        {
            List<BattleMove> availableMoves = _level1ShieldGuy.AvailableMoves;
            Assert.AreEqual(4, availableMoves.Count);

            Assert.IsTrue(availableMoves.Exists(bm => bm.MoveType == BattleMoveType.Attack));
            Assert.IsTrue(availableMoves.Exists(bm => bm is ShieldMove));

            Assert.IsTrue(availableMoves.Exists(bm => bm is ShieldFortifyingMove && ((ShieldFortifyingMove)bm).FortifyingType == ShieldFortifyingType.Health));
            Assert.IsTrue(availableMoves.Exists(bm => bm is ShieldFortifyingMove && ((ShieldFortifyingMove)bm).FortifyingType == ShieldFortifyingType.Defense));
        }

        #region move selection

        /// <summary>
        /// Shield Guy should only select between 2 moves if there are no shields on their team-
        /// Attack and create shield
        /// </summary>
        [Test]
        public void Level1ShieldGuy_DoesNotTryToSelectShieldFortifyingMoves_NoAlliesWithShields()
        {
            _chanceService.PushWhichEventsOccur(0); //have to set this up to prevent error
            _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(2, lastEventOccursArgs.Length);
            Assert.AreEqual(0.2, lastEventOccursArgs[0]);
            Assert.AreEqual(0.8, lastEventOccursArgs[1]);

            _chanceService.PushWhichEventsOccur(0); //have to set this up to prevent error
            BattleMove returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            Assert.AreEqual(BattleMoveType.Attack, returnedMove.MoveType);

            _chanceService.PushWhichEventsOccur(1); //have to set this up to prevent error
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            Assert.IsTrue(returnedMove is ShieldMove);
        }

        /// <summary>
        /// Shield Guy should only select between 2 moves (attack and fortify shield) if every ally has a fully healed shield
        /// </summary>
        [Test]
        public void Level1ShieldGuy_CorrectlyDeterminesViableMoves_AllAlliesWithFullyHealedShields()
        {
            IronBattleShield shield = new IronBattleShield(1, 1, 0);

            _level1ShieldGuy.SetBattleShield(shield);
            _ally1.SetBattleShield(shield);
            _ally2.SetBattleShield(shield);

            _chanceService.PushWhichEventsOccur(0);
            BattleMove returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(2, lastEventOccursArgs.Length);
            Assert.AreEqual(0.2, lastEventOccursArgs[0]);
            Assert.AreEqual(0.8, lastEventOccursArgs[1]);

            Assert.AreEqual(BattleMoveType.Attack, returnedMove.MoveType);

            _chanceService.PushWhichEventsOccur(1);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            ShieldFortifyingMove shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Defense, shieldFortifyingMove.FortifyingType);
        }

        /// <summary>
        /// Shield Guy should select between all 4 moves if at least one ally is missing a shield and at least one ally shield is damaged
        /// </summary>
        [Test]
        public void Level1ShieldGuy_CorrectlyDeterminesViableMoves_AllMovesAreViable()
        {
            _chanceService.PushWhichEventsOccur(0); //have to set this up to prevent error

            const int shieldDefense = 2;
            IronBattleShield shield = new IronBattleShield(2, shieldDefense, 0);
            _ally1.SetBattleShield(shield);
            _ally1.BattleShield.DecrementHealth(shieldDefense + 1);

            _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(4, lastEventOccursArgs.Length);
            Assert.AreEqual(0.2, lastEventOccursArgs[0]);
            Assert.AreEqual(0.4, lastEventOccursArgs[1]);
            Assert.AreEqual(0.2, lastEventOccursArgs[2]);
            Assert.AreEqual(0.2, lastEventOccursArgs[3]);

            _chanceService.PushWhichEventsOccur(0);
            BattleMove returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            Assert.AreEqual(BattleMoveType.Attack, returnedMove.MoveType);


            _chanceService.PushWhichEventsOccur(1);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            Assert.IsTrue(returnedMove is ShieldMove);


            _chanceService.PushWhichEventsOccur(2);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            ShieldFortifyingMove shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Defense, shieldFortifyingMove.FortifyingType);


            _chanceService.PushWhichEventsOccur(3);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Health, shieldFortifyingMove.FortifyingType);
        }

        /// <summary>
        /// Shield Guy should only select between 3 moves if no ally shields need healing, but at least one ally still is unshielded
        /// </summary>
        [Test]
        public void Level1ShieldGuy_CorrectlyDeterminesViableMoves_NoHealingMove()
        {
            IronBattleShield shield = new IronBattleShield(1, 1, 0);
            _ally1.SetBattleShield(shield);

            _chanceService.PushWhichEventsOccur(0);
            BattleMove returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(3, lastEventOccursArgs.Length);
            Assert.AreEqual(0.2, lastEventOccursArgs[0]);
            Assert.AreEqual(0.6, lastEventOccursArgs[1]);
            Assert.AreEqual(0.2, lastEventOccursArgs[2]);

            Assert.AreEqual(BattleMoveType.Attack, returnedMove.MoveType);

            _chanceService.PushWhichEventsOccur(1);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            Assert.IsTrue(returnedMove is ShieldMove);

            _chanceService.PushWhichEventsOccur(2);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            ShieldFortifyingMove shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Defense, shieldFortifyingMove.FortifyingType);
        }

        /// <summary>
        /// Shield Guy should only select between 3 moves if all allies have shields and at least one is damaged
        /// </summary>
        [Test]
        public void Level1ShieldGuy_CorrectlyDeterminesViableMoves_NoShieldMove()
        {
            IronBattleShield shield = new IronBattleShield(3, 0, 0);
            _level1ShieldGuy.SetBattleShield(shield);
            _level1ShieldGuy.BattleShield.DecrementHealth(1);
            _ally1.SetBattleShield(shield);
            _ally2.SetBattleShield(shield);

            _chanceService.PushWhichEventsOccur(0);
            BattleMove returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(3, lastEventOccursArgs.Length);
            Assert.AreEqual(0.2, lastEventOccursArgs[0]);
            Assert.AreEqual(0.4, lastEventOccursArgs[1]);
            Assert.AreEqual(0.4, lastEventOccursArgs[2]);

            Assert.AreEqual(BattleMoveType.Attack, returnedMove.MoveType);

            _chanceService.PushWhichEventsOccur(1);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            ShieldFortifyingMove shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Defense, shieldFortifyingMove.FortifyingType);

            _chanceService.PushWhichEventsOccur(2);
            returnedMove = _level1ShieldGuy.SelectMove(_shieldGuyTeam, _humanTeam);
            shieldFortifyingMove = returnedMove as ShieldFortifyingMove;
            Assert.NotNull(shieldFortifyingMove);
            Assert.AreEqual(ShieldFortifyingType.Health, shieldFortifyingMove.FortifyingType);
        }

        #endregion move selection

        #region target selection

        /// <summary>
        /// Shield Guy should only try to target allies that do not have shields
        /// </summary>
        [Test]
        public void Level1ShieldGuy_SelectsAppropriateTarget_IronShieldMove([Values(0, 1, 2)] int numberOfAlliesWithShields)
        {
            for (int i = 0; i < numberOfAlliesWithShields; ++i)
            {
                _shieldGuyTeam.Fighters[i].SetBattleShield(new IronBattleShield(1, 0, 0));
            }

            _chanceService.PushWhichEventsOccur(1); //Will select iron shield move
            if (numberOfAlliesWithShields < 2)
            {
                _chanceService.PushWhichEventsOccur(0); //Select the target
            }

            BattleMoveWithTarget moveWithTarget = _level1ShieldGuy.SetupMove(_shieldGuyTeam, _humanTeam);

            double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

            if (numberOfAlliesWithShields < 2)
            {
                int numberOfAlliesWithoutShields = 3 - numberOfAlliesWithShields;
                double chance = 1.0/numberOfAlliesWithoutShields;

                Assert.AreEqual(numberOfAlliesWithoutShields, lastEventOccursArgs.Length);

                for (int i = 0; i < numberOfAlliesWithShields; ++i)
                {
                    Assert.AreEqual(chance, lastEventOccursArgs[i]);
                }
            }

            Assert.AreEqual(_shieldGuyTeam.Fighters[numberOfAlliesWithShields], moveWithTarget.Target);
        }

        /// <summary>
        /// Shield Guy should only try to target allies with shields when trying to fortify a shield
        /// </summary>
        [Test]
        public void Level1ShieldGuy_SelectsAppropriateTarget_ShieldFortifyingMove([Values(1, 2, 3)] int numberOfAlliesWithShield)
        {
            int selectedTargetIndex = numberOfAlliesWithShield - 1;
            IronBattleShield shield = new IronBattleShield(5, 0, 0);

            for (int i = 0; i < numberOfAlliesWithShield; ++i)
            {
                _shieldGuyTeam.Fighters[i].SetBattleShield(shield);
            }

            int selectedMoveIndex = numberOfAlliesWithShield == 3 ? 1 : 2;
            _chanceService.PushWhichEventsOccur(selectedMoveIndex);

            if (numberOfAlliesWithShield > 1)
            {
                _chanceService.PushWhichEventsOccur(selectedTargetIndex); //Select the target
            }
            BattleMoveWithTarget moveWithTarget = _level1ShieldGuy.SetupMove(_shieldGuyTeam, _humanTeam);

            if (numberOfAlliesWithShield > 1)
            {
                double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

                double chance = 1.0 / numberOfAlliesWithShield;

                Assert.AreEqual(numberOfAlliesWithShield, lastEventOccursArgs.Length);
                for (int i = 0; i < numberOfAlliesWithShield; ++i)
                {
                    Assert.AreEqual(chance, lastEventOccursArgs[i]);
                }
            }

            IFighter expectedTarget = _shieldGuyTeam.Fighters[selectedTargetIndex];
            Assert.AreEqual(expectedTarget, moveWithTarget.Target);
        }

        /// <summary>
        /// Shield Guy should only try to target allies with damaged shields when trying to heal a shield
        /// </summary>
        [Test]
        public void Level1ShieldGuy_SelectsAppropriateTarget_ShieldHealingMove([Values(1, 2, 3)] int numberOfAlliesWithDamagedShields)
        {
            IronBattleShield shield = new IronBattleShield(5, 0, 0);
            _shieldGuyTeam.Fighters.ForEach(f => f.SetBattleShield(shield));

            for (int i = 0; i < numberOfAlliesWithDamagedShields; ++i)
            {
                _shieldGuyTeam.Fighters[i].BattleShield.DecrementHealth(1);
            }

            _chanceService.PushWhichEventsOccur(2);

            if (numberOfAlliesWithDamagedShields > 1)
            {
                _chanceService.PushWhichEventsOccur(numberOfAlliesWithDamagedShields - 1); //Select the target
            }
            BattleMoveWithTarget moveWithTarget = _level1ShieldGuy.SetupMove(_shieldGuyTeam, _humanTeam);

            if (numberOfAlliesWithDamagedShields > 1)
            {
                double[] lastEventOccursArgs = _chanceService.LastEventOccursArgs;

                double chance = 1.0/ numberOfAlliesWithDamagedShields;

                Assert.AreEqual(numberOfAlliesWithDamagedShields, lastEventOccursArgs.Length);
                for (int i = 0; i < numberOfAlliesWithDamagedShields; ++i)
                {
                    Assert.AreEqual(chance, lastEventOccursArgs[i]);
                }
            }

            Assert.AreEqual(_shieldGuyTeam.Fighters[numberOfAlliesWithDamagedShields - 1], moveWithTarget.Target);
        }

        #endregion

        #region screen output

        [Test]

        public void BattleManager_PrintsCorrectExecutionText_ShieldFortifyingMove()
        {
            _level1ShieldGuy.SetBattleShield(new IronBattleShield(5, 1, 0));
            _chanceService.PushWhichEventsOccur(2, 0); //index 0 is attack, 1 is "equip with shield", and 2 is "fortify." Second index supplied so it won't error when selecting second turn's move

            _ally1.SetMove(_doNothingMove);
            _ally2.SetMove(_doNothingMove);

            _humanFighter.SetMove(_doNothingMove, 1);
            _humanFighter.SetMove(_runawayMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _shieldGuyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();
            
            Assert.AreEqual(1, outputs.Length);

            string displayName = _level1ShieldGuy.DisplayName;
            string expectedOutput = $"{displayName} strengthened {displayName}'s shield!\n";
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void BattleManager_PrintsCorrectExecutionText_ShieldHealingMove()
        {
            _level1ShieldGuy.SetBattleShield(new IronBattleShield(5, 1, 0));
            _level1ShieldGuy.BattleShield.DecrementHealth(1);
            _chanceService.PushWhichEventsOccur(3, 0); //index 0 is attack, 1 is "equip with shield", 2 is "fortify", 3 is "heal" Second index supplied so it won't error when selecting second turn's move

            _ally1.SetMove(_doNothingMove);
            _ally2.SetMove(_doNothingMove);

            _humanFighter.SetMove(_doNothingMove, 1);
            _humanFighter.SetMove(_runawayMove);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };

            _battleManager.Battle(_humanTeam, _shieldGuyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            string displayName = _level1ShieldGuy.DisplayName;
            string expectedOutput = $"{displayName} healed {displayName}'s shield!\n";
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        #endregion
    }
}
