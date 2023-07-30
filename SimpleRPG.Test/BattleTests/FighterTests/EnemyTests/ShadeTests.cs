using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Fighters.FighterGroupings;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    public class ShadeTests
    {
        private MockChanceService _chanceService;

        private Shade _shade1, _shade2, _shade3;
        private List<Shade> _shades;
        private ShadeFighterGrouping _shadeGrouping;
        private Team _shadeTeam;

        private int _malevolenceChargeIndex;
        private int _malevolenceChargeNoAbsorptionMoveIndex;
        private int _malevolenceAttackIndex;
        private int _malevolenceAttackNoAbsorptionMoveIndex;
        private int _darkFogIndex;
        private int _absorptionMoveIndex;

        private BattleMove _malevolenceChargeMove;
        private BattleMove _malevolenceAttackMove;
        private BattleMove _shadeAbsorbingMove;

        private TestHumanFighter _humanFighter;
        private Team _humanTeam;

        private readonly BattleMove _basicAttack = MoveFactory.Get(BattleMoveType.Attack);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
        private readonly BattleMove _doNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);

        private EventLogger _logger;
        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;

        private TestBattleManager _battleManager;

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);

            _chanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_chanceService);

            _shade1 = (Shade)FighterFactory.GetFighter(FighterType.Shade, 1);
            _shade2 = (Shade)FighterFactory.GetFighter(FighterType.Shade, 1);
            _shade3 = (Shade)FighterFactory.GetFighter(FighterType.Shade, 1);
            _shades = new List<Shade> { _shade1, _shade2, _shade3 };
            _shadeGrouping = new ShadeFighterGrouping(_chanceService, _shades.ToArray());
            _shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shadeGrouping);

            _humanFighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanFighter.SetSpeed(_shade1.Speed + 1);
            _humanTeam = new Team(_menuManager, _humanFighter);

            List<BattleMove> executableMoves = _shade1.GetExecutableMoves(_humanTeam);
            _absorptionMoveIndex = executableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.AbsorbShade);
            _malevolenceChargeIndex = executableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Special);
            _malevolenceAttackIndex = executableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.ConditionalPowerAttack);
            _darkFogIndex = executableMoves.FindIndex(bm => bm is StatusMove);

            _malevolenceChargeMove = executableMoves[_malevolenceChargeIndex];
            _malevolenceAttackMove = executableMoves[_malevolenceAttackIndex];
            _shadeAbsorbingMove = executableMoves[_absorptionMoveIndex];

            Shade fooShade = new Shade(1, _chanceService, 1);
            Team fooTeam = new Team(TestMenuManager.GetTestMenuManager(), fooShade);
            fooShade.SetTeam(fooTeam);
            List<BattleMove> fooExecutableAttacks = fooShade.GetExecutableMoves(_humanTeam);
            _malevolenceAttackNoAbsorptionMoveIndex =
                fooExecutableAttacks.FindIndex(bm => bm.MoveType == BattleMoveType.ConditionalPowerAttack);
            _malevolenceChargeNoAbsorptionMoveIndex =
                fooExecutableAttacks.FindIndex(bm => bm.MoveType == BattleMoveType.Special);

            _battleManager = new TestBattleManager(_chanceService, _input, _output);
        }

        [TearDown]
        public void TearDown()
        {
            _logger = null;
            _input = null;
            _output = null;
            _menuManager = null;
            _chanceService = null;

            _shade1 = null;
            _shade2 = null;
            _shade3 = null;
            _shadeGrouping = null;
            _shadeTeam = null;

            _humanFighter = null;
            _humanTeam = null;
        }

        [Test]
        public void FighterFactoryCorrectlyReturnsShade()
        {
            Shade returnedFighter = null;
            Assert.DoesNotThrow(() => returnedFighter = (Shade)FighterFactory.GetFighter(FighterType.Shade, 1));

            Assert.NotNull(returnedFighter);
        }

        #region Shade specific moves

        [Test]
        public void ShadeInitializesWithCorrectMoves()
        {
            BattleMove darkFogMove = _shade1.AvailableMoves.FirstOrDefault(m =>
            {
                bool found = false;

                StatusMove statusMove = m as StatusMove;

                if (statusMove != null)
                {
                    found = statusMove.Status is BlindStatus && 
                            statusMove.ExecutionText == $"draws a dark fog about {Globals.TargetReplaceText}" &&
                            statusMove.Accuracy == 60;
                }

                return found;
            });

            Assert.NotNull(darkFogMove);

            BattleMove shadeAbsorbingTechnique = _shade1.AvailableMoves.FirstOrDefault(m => m is ShadeAbsorbingMove);

            Assert.NotNull(shadeAbsorbingTechnique);

            BattleMove buildMalevolenceMove =
                _shade1.AvailableMoves.FirstOrDefault(m => 
                                                      m is SpecialMove && 
                                                      m.Description == "dark energy gather" &&
                                                      m.ExecutionText == "gathers dark energy");

            Assert.NotNull(buildMalevolenceMove);

            BattleMove malevolenceAttack =
                _shade1.AvailableMoves.FirstOrDefault(m => m is ConditionalPowerAttackBattleMove && m.ExecutionText == "unleashes their dark power!");

            Assert.NotNull(malevolenceAttack);
        }

        [Test]
        public void ShadeFiltersBasicAttackFromExecutableMoves()
        {
            List<BattleMove> availableMoves = _shade1.AvailableMoves;
            List<BattleMove> executableMoves = _shade1.GetExecutableMoves(_humanTeam);

            Assert.AreEqual(1, availableMoves.Count - executableMoves.Count);

            BattleMove filteredMove = availableMoves.FirstOrDefault(bm => !executableMoves.Contains(bm));

            Assert.AreEqual(BattleMoveType.Attack, filteredMove?.MoveType);
        }

        #region absorb move

        [Test]
        public void ShadeCorrectlyTargetsShade_AbsorbMove()
        {
            //arrange
            _chanceService.PushWhichEventOccurs(_absorptionMoveIndex);

            Team mixedTeam = new Team(TestMenuManager.GetTestMenuManager(), _shade1, _shade2, 
                FighterFactory.GetFighter(FighterType.Fairy, 1),
                FighterFactory.GetFighter(FighterType.Goblin, 1),
                FighterFactory.GetFighter(FighterType.Golem, 1),
                FighterFactory.GetFighter(FighterType.Ogre, 1));

            //act
            BattleMoveWithTarget moveWithTarget = null;
            //will throw if the filters do not appropriately filter out all non shades
            Assert.DoesNotThrow(() => moveWithTarget = _shade1.SetupMove(mixedTeam, _humanTeam));

            //assert
            Assert.True(moveWithTarget.Move is ShadeAbsorbingMove);
            Assert.AreEqual(_shade2, moveWithTarget.Target);
        }

        [Test]
        public void ShadeCorrectlyTargetsShadeWithLowestHealth_AbsorbMove()
        {
            //arrange
            _logger.Subscribe(_shade1, EventType.ShadeAbsorbed);

            _shade2.PhysicalDamage(_shade2.MaxHealth + _shade2.Defense - 1);

            _chanceService.PushWhichEventsOccur(
                _absorptionMoveIndex, //shade A's move
                _malevolenceChargeIndex,  //shade B's move
                _malevolenceChargeIndex, //shade C's move
                0, //absorption bonus
                _malevolenceChargeIndex, 
                _malevolenceChargeIndex, 
                _malevolenceChargeIndex);

            _humanFighter.SetMove(_doNothingMove, 1);
            _humanFighter.SetMove(_runawayMove);

            //act
            _battleManager.Battle(_humanTeam, _shadeTeam);

            //assert
            Assert.AreEqual(1, _logger.Logs.Count);

            Assert.AreEqual(_shade2, (_logger.Logs[0].E as ShadeAbsorbedEventArgs)?.AbsorbedShade);
        }

        [Test]
        public void AbsorbMove_CorrectlyAbsorbsShadeWhenExecuted()
        {
            //arrange
            _logger.Subscribe(EventType.ShadeAbsorbed, _shade1);
            BattleMove shadeAbsorbingTechnique = _shade1.AvailableMoves.FirstOrDefault(m => m is ShadeAbsorbingMove);
            BattleMoveWithTarget absorbMoveWithTarget = new BattleMoveWithTarget(shadeAbsorbingTechnique, _shade2, _shade1);

            _chanceService.PushWhichEventOccurs(0); //so the absorb method doesn't throw an error

            //act
            _shade1.ExecuteMove(_battleManager, absorbMoveWithTarget, _shadeTeam, _humanTeam, _output);

            //assert
            Assert.AreEqual(2, _shade1.ShadeExperience);
            List<EventLog> logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);
            EventLog log = logs[0];
            Assert.AreEqual(_shade1, log.Sender);
            Assert.AreEqual(_shade2, (log.E as ShadeAbsorbedEventArgs)?.AbsorbedShade);
            Assert.AreEqual(0, _shade2.CurrentHealth);
        }
        
        [Test]
        public void AbsorbMove_RemovedFromExecutableMoves_NoAllyShades()
        {
            List<BattleMove> executableMovesBefore = _shade1.GetExecutableMoves(_humanTeam);

            Team noShadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shade1, new Egg(MagicType.Fire));
            _shade1.SetTeam(noShadeTeam);

            List<BattleMove> executableMovesAfter = _shade1.GetExecutableMoves(_humanTeam);

            Assert.AreEqual(1, executableMovesBefore.Count - executableMovesAfter.Count);

            BattleMove notInAfter = executableMovesBefore.FirstOrDefault(m => !executableMovesAfter.Contains(m));

            Assert.NotNull(notInAfter);
            Assert.IsAssignableFrom<ShadeAbsorbingMove>(notInAfter);
        }

        #endregion absorb moves

        #region malevolence charge/attack

        [Test]
        public void MalevolenceCharge_CorrectlyRaisesMalevolenceAttackPower()
        {
            TestHumanFighter fighter2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanFighter.SetHealth(100);
            fighter2.SetHealth(100);
            
            Team shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shade1, _shade2);
            Team humanTeam = new Team(_menuManager, _humanFighter, fighter2);

            _humanFighter.SetMove(_doNothingMove, 3);
            _humanFighter.SetMove(_runawayMove);
            fighter2.SetMove(_doNothingMove);

            _chanceService.PushWhichEventsOccur(_malevolenceAttackIndex, 0, _malevolenceChargeIndex); //first round, _shade1 attacks HumanFighter1, _shade2 charges
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceChargeIndex); //second round, both shades charge
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceAttackIndex, 1); //third round, _shade1 charges again, _shade2 attacks
            _chanceService.PushAttackHitsNotCrit(2);

            _battleManager.Battle(humanTeam, shadeTeam);

            int fighter1Damage = _humanFighter.MaxHealth - _humanFighter.CurrentHealth;
            int fighter2Damage = fighter2.MaxHealth - fighter2.CurrentHealth;

            Assert.Greater(fighter2Damage, fighter1Damage);
        }

        [Test]
        public void MalevolenceCharge_CorrectlyCapsAtMaximumChargeAmount()
        {
            //arrange
            int maximumMalevolenceCharge = Shade.MaxMalevolenceLevel;

            List<BattleMove> executableMoves = _shade1.GetExecutableMoves(_humanTeam);
            BattleMove chargeMove = executableMoves.First(m => m.MoveType == BattleMoveType.Special);
            BattleMove attackMove = executableMoves.First(m => m.MoveType == BattleMoveType.ConditionalPowerAttack);

            _humanFighter.SetHealth(maximumMalevolenceCharge + 1);
            _humanFighter.SetDefense(_shade1.Strength);
            int totalTurnsCharging = maximumMalevolenceCharge*2;
            _humanFighter.SetMove(_runawayMove);

            for (var i = 0; i < totalTurnsCharging; ++i)
            {
                _battleManager.SetBattleMoveQueues(new BattleMoveQueue(new List<BattleMoveWithTarget>
                {
                    new BattleMoveWithTarget(chargeMove, _shade1, _shade1)
                }));
            }

            _battleManager.SetBattleMoveQueues(new BattleMoveQueue(new List<BattleMoveWithTarget>
                {
                    new BattleMoveWithTarget(attackMove, _humanFighter, _shade1)
                }));
            _chanceService.PushAttackHitsNotCrit();

            //Act
            _battleManager.Battle(_humanTeam, new Team(TestMenuManager.GetTestMenuManager(), _shade1));

            //Assert
            Assert.AreEqual(1, _humanFighter.CurrentHealth);
        }

        [Test]
        public void MalevolenceCharge_RemovedFromExecutableMoves_MaximumCharge()
        {
            List<BattleMove> executableMovesBefore = _shade1.GetExecutableMoves(_humanTeam);
            BattleMove chargeMove = executableMovesBefore.First(m => m.MoveType == BattleMoveType.Special);
            BattleMoveWithTarget chargeMoveWithTarget = new BattleMoveWithTarget(chargeMove, _shade1, _shade1);

            for (var i = 0; i < Shade.MaxMalevolenceLevel; ++i)
            {
                _shade1.ExecuteMove(_battleManager, chargeMoveWithTarget, _shadeTeam, _humanTeam, _output);
            }

            List<BattleMove> executableMovesAfter = _shade1.GetExecutableMoves(_humanTeam);

            Assert.AreEqual(1, executableMovesBefore.Count - executableMovesAfter.Count);

            BattleMove notInAfterList = executableMovesBefore.FirstOrDefault(m => !executableMovesAfter.Contains(m));
            Assert.AreEqual(chargeMove, notInAfterList);
        }


        [Test]
        public void MalevolenceCharge_CorrectlyAddedWhenShadeAbsorbed()
        {
            //arrange
            for (var i = 0; i < 2; ++i)
            {
                //the first shade will attack and miss for 2 turns, second shade will charge
                _chanceService.PushWhichEventsOccur(_malevolenceAttackIndex, _malevolenceChargeIndex);
                _chanceService.PushEventOccurs(false);
            }

            //third turn, first shade absorbs second
            _chanceService.PushWhichEventsOccur(_absorptionMoveIndex, _malevolenceChargeIndex);
            //have to set which bonus is set when Shade is absorbed
            _chanceService.PushWhichEventOccurs(0);
            //fourth turn, shade attacks and hits
            _chanceService.PushWhichEventOccurs(_malevolenceAttackNoAbsorptionMoveIndex);
            _chanceService.PushAttackHitsNotCrit();

            _humanFighter.SetDefense(_shade1.Strength);
            _humanFighter.SetHealth(3);
            _humanFighter.SetMove(_doNothingMove, 4);
            _humanFighter.SetMove(_runawayMove);

            Team shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shade1, _shade2);

            //act
            _battleManager.Battle(_humanTeam, shadeTeam);

            //assert
            Assert.AreEqual(1, _humanFighter.CurrentHealth);
        }

        [Test]
        public void MalevolenceCharge_CorrectlyCapsMalevolenceAfterAbsorption()
        {
            int maxMalevolenceLevel = Shade.MaxMalevolenceLevel;
            List<Shade> shadeFighters = new List<Shade> { _shade1, _shade2 };

            //arrange
            for (var i = 0; i < maxMalevolenceLevel; ++i)
            {
                List<BattleMoveWithTarget> battleMoves = shadeFighters.Select(s => new BattleMoveWithTarget(_malevolenceChargeMove, s, s)).ToList();
                _battleManager.SetBattleMoveQueues(new BattleMoveQueue(battleMoves));
            }

            //third turn, first shade absorbs second
            _battleManager.SetBattleMoveQueues(new BattleMoveQueue(new List<BattleMoveWithTarget> { new BattleMoveWithTarget(_shadeAbsorbingMove, _shade2, _shade1) }));
            //have to set which bonus is set when Shade is absorbed
            _chanceService.PushWhichEventOccurs(0);
            //fourth turn, shade attacks and hits
            _battleManager.SetBattleMoveQueues(new BattleMoveQueue(new List<BattleMoveWithTarget> { new BattleMoveWithTarget(_malevolenceAttackMove, _humanFighter, _shade1) }));
            _chanceService.PushAttackHitsNotCrit();

            _humanFighter.SetDefense(_shade1.Strength);
            _humanFighter.SetHealth(maxMalevolenceLevel * 2);


            _humanFighter.SetMove(_runawayMove);

            Team shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), shadeFighters.OfType<IFighter>().ToList());

            //act
            _battleManager.Battle(_humanTeam, shadeTeam);

            //assert
            Assert.AreEqual(maxMalevolenceLevel, _humanFighter.CurrentHealth);
        }

        [Test]
        public void MalevolenceCharge_CorrectlyResetAfterUsingMalevolenceAttack()
        {
            _chanceService.PushWhichEventsOccur(
                _malevolenceChargeNoAbsorptionMoveIndex,
                _malevolenceChargeNoAbsorptionMoveIndex, 
                _malevolenceAttackNoAbsorptionMoveIndex,
                _malevolenceAttackNoAbsorptionMoveIndex);
            _chanceService.PushAttackHitsNotCrit(2);
            
            _humanFighter.SetDefense(_shade1.Strength);
            _humanFighter.SetHealth(3);
            _humanFighter.SetMove(_doNothingMove, 4);
            _humanFighter.SetMove(_runawayMove);

            Team shadeTeam = new Team(TestMenuManager.GetTestMenuManager(), _shade1);

            _battleManager.Battle(_humanTeam, shadeTeam);

            Assert.AreEqual(1, _humanFighter.CurrentHealth);
        }

        [Test]
        public void Shade_MoreLikelyToAttackLessLikelyToCharge_TheMoreMalevolenceHasAlreadyBeenCharged()
        {
            List<double> chargeChances = new List<double>();
            List<double> attackChances = new List<double>();

            for (var i = 0; i < Shade.MaxMalevolenceLevel; ++i)
            {
                _chanceService.PushWhichEventOccurs(_malevolenceChargeIndex);

                BattleMoveWithTarget moveWithTarget = _shade1.SetupMove(_shadeTeam, _humanTeam);

                double[] lastEventOccurs = _chanceService.LastEventOccursArgs;
                chargeChances.Add(lastEventOccurs[_malevolenceChargeIndex]);
                attackChances.Add(lastEventOccurs[_malevolenceAttackIndex]);

                if (i > 0)
                {
                    Assert.Greater(attackChances[i], attackChances[i - 1]);
                    Assert.Less(chargeChances[i], chargeChances[i - 1]);
                }

                _shade1.ExecuteMove(_battleManager, moveWithTarget, _shadeTeam, _humanTeam, _output);
            }
        }

        #endregion

        #region Blindness move

        [Test]
        public void ShadeDoesNotTargetFoeAlreadyInflictedWithBlindness()
        {
            StatusMove darkFogMove = _shade1.GetExecutableMoves(_humanTeam)[_darkFogIndex] as StatusMove;

            _humanFighter.AddStatus(darkFogMove?.Status);
            Egg blindEgg = new Egg(MagicType.Fire);
            blindEgg.AddStatus(darkFogMove?.Status);
            Team humanTeam = new Team(TestMenuManager.GetTestMenuManager(), _humanFighter, blindEgg, new Egg(MagicType.Fire));

            _chanceService.PushWhichEventOccurs(_darkFogIndex);

            BattleMoveWithTarget moveWithTarget = null;

            //will throw if multiple targets
            Assert.DoesNotThrow(() => moveWithTarget = _shade1.SetupMove(_shadeTeam, humanTeam));

            Assert.NotNull(moveWithTarget);
            Assert.AreEqual(darkFogMove, moveWithTarget.Move);
            Assert.AreEqual(humanTeam.Fighters[2], moveWithTarget.Target);
        }

        [Test]
        public void ShadeDoesNotChooseDarkFog_AllEnemiesEitherDeadOrAlreadyBlinded()
        {
            StatusMove darkFogMove = _shade1.GetExecutableMoves(_humanTeam)[_darkFogIndex] as StatusMove;
            Egg egg = new Egg(MagicType.Fire);
            Team team = new Team(TestMenuManager.GetTestMenuManager(), _humanFighter, egg);

            List<BattleMove> executableMovesBefore = _shade1.GetExecutableMoves(team);

            _humanFighter.AddStatus(darkFogMove?.Status);
            egg.PhysicalDamage(egg.MaxHealth);

            List<BattleMove> executableMovesAfter = _shade1.GetExecutableMoves(team);

            Assert.AreEqual(1, executableMovesBefore.Count - executableMovesAfter.Count);

            BattleMove notInAfter = executableMovesBefore.FirstOrDefault(m => !executableMovesAfter.Contains(m));

            Assert.NotNull(notInAfter);
            Assert.AreEqual(darkFogMove, notInAfter);
        }

        #endregion

        #endregion Shade specific moves

        #region level tests

        [Test]
        public void ShadeExperienceSetter_CorrectlySetsShadeLevel()
        {
            for (int i = 1; i <= Shade.MaxShadeLevel; ++i)
            {
                int experienceForLevel = Shade.ExperienceForLevel[i];
                Shade shade = new Shade(1, _chanceService, experienceForLevel - 1); //1 xp away from leveling
                Assert.AreEqual(i - 1, shade.ShadeLevel);
                Shade absorbedShade = new Shade(1, _chanceService, 1);

                _chanceService.PushWhichEventOccurs(0); //which stat is boosted, needed to prevent out of range exceptions
                shade.AbsorbShade(absorbedShade);

                Assert.AreEqual(experienceForLevel, shade.ShadeExperience);
                Assert.AreEqual(i, shade.ShadeLevel);
            }
        }

        /// <summary>
        /// Basically tests that there isn't some error in logic that would cause a shade going from level 1 to level 3 to stop at 2
        /// </summary>
        [Test]
        public void ShadeExperienceSetter_CorrectlySkipsShadeLevels()
        {
            Shade shade = new Shade(1, _chanceService, 1);
            Shade absorbedShade = new Shade(1, _chanceService, Shade.ExperienceForLevel[Shade.MaxShadeLevel]);

            _chanceService.PushWhichEventOccurs(0);

            shade.AbsorbShade(absorbedShade);

            Assert.AreEqual(Shade.MaxShadeLevel, shade.ShadeLevel);
        }

        [Test]
        public void ShadeLevelUp_CorrectlyRaisesTransformEvent()
        {
            //arrange
            Shade shade = new Shade(1, _chanceService, Shade.ExperienceForLevel[Shade.MaxShadeLevel] - 1);
            Shade absorbedShade = new Shade(1, _chanceService, 1);
            _chanceService.PushWhichEventOccurs(0); //so AbsorbShade method doesn't throw an exception

            _logger.Subscribe(EventType.FighterTransformed, shade);
            string beforeBaseName = shade.BaseName;

            //act
            shade.AbsorbShade(absorbedShade);

            //assert
            List<EventLog> logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            FighterTransformedEventArgs e = log.E as FighterTransformedEventArgs;
            Assert.NotNull(e);
            
            Assert.AreEqual(beforeBaseName, e.PreTransformDisplayName);
            Assert.AreEqual($"a {shade.BaseName}", e.PostTransformDisplayName);
        }

        [Test]
        public void ShadeLevelUp_CorrectlyChangesDisplayName()
        {
            Shade weakShade = new Shade(1, _chanceService, 1);
            Shade strongShade = new Shade(1, _chanceService, 2);
            Shade powerfulShade = new Shade(1, _chanceService, 5);

            Assert.AreEqual("Shade", weakShade.DisplayName);
            Assert.AreEqual("Strong Shade", strongShade.DisplayName);
            Assert.AreEqual("Powerful Shade", powerfulShade.DisplayName);
        }

        #endregion level tests

        #region absorb tests

        [Test]
        public void ShadeCorrectlyHealsWhenAbsorbingAnotherShade([Range(0, 1)] int whichShadeIsHealed)
        {
            _chanceService.PushWhichEventsOccur(whichShadeIsHealed, 0); // second index for determining which stat will be boosted

            Shade selectedShade = whichShadeIsHealed == 0 ? _shade1 : _shade2;
            selectedShade.PhysicalDamage(selectedShade.MaxHealth + selectedShade.Defense - 1);

            Assert.AreEqual(1, selectedShade.CurrentHealth);

            _shade3.PhysicalDamage(_shade3.MaxHealth + _shade3.Defense);

            Assert.AreEqual(selectedShade.MaxHealth, selectedShade.CurrentHealth);
        }

        /// <summary>
        /// Currently, a shade can gain speed, evasiveness, or defense when absorbing another shade
        /// </summary>
        /// <param name="selectedBonusIndex">The index value for the bonuses</param>
        [Test]
        public void ShadeCorrectlyGainsBonusWhenAbsorbingAnotherShade([Range(0,2)] int selectedBonusIndex)
        {
            _chanceService.PushWhichEventOccurs(selectedBonusIndex);

            StatType boostedStat = Shade.AbsorptionBonuses[selectedBonusIndex];

            int valueBefore = _shade1.GetStatValue(boostedStat);

            _shade1.AbsorbShade(_shade2);

            int valueAfter = _shade1.GetStatValue(boostedStat);

            Assert.Greater(valueAfter, valueBefore);
        }

        [Test]
        public void ShadesShadeLevelCorrectlyIncreasesWhenAbsorbingAnotherShade([Range(1, 4)] int shadeLevel)
        {
            Shade shade1 = new Shade(1, _chanceService, 1);
            Shade shade2 = new Shade(1, _chanceService, shadeLevel);

            _chanceService.PushWhichEventOccurs(0); //determines which stat will be boosted, required to prevent exceptions, but not really important for the test

            shade1.AbsorbShade(shade2);

            Assert.AreEqual(shadeLevel + 1, shade1.ShadeExperience);
        }

        [Test]
        public void ShadesDisplayNameCorrectlyAlteredWhenAbsorbingAnotherShade([Values(1, 4)] int shadeLevel)
        {
            Shade shade1 = new Shade(1, _chanceService, 1);
            Shade shade2 = new Shade(1, _chanceService, shadeLevel);

            _chanceService.PushWhichEventOccurs(0); //determines which stat will be boosted, required to prevent exceptions, but not really important for the test

            shade1.AbsorbShade(shade2);

            string expectedDisplayName = shadeLevel == 1 ? "Strong Shade" : "Powerful Shade";
            Assert.AreEqual(expectedDisplayName, shade1.DisplayName);
        }

        #endregion 

        #region .Seal() method

        [Test]
        public void SealMethod_CorrectlyReducesHealth()
        {
            Assert.AreEqual(_shade1.MaxHealth, _shade1.CurrentHealth);

            _shade1.Seal();

            Assert.AreEqual(0, _shade1.CurrentHealth);
        }

        [Test]
        public void SealMethod_CorrectlyRaisesSealedEvent()
        {
            //Arrange
            _logger.Subscribe(_shade1, EventType.FighterSealed);

            //Act
            _shade1.Seal();

            //Assert
            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.FighterSealed, log.Type);

            FighterSealedEventArgs e = log.E as FighterSealedEventArgs;
            
            Assert.NotNull(e);
            Assert.AreEqual(_shade1, e.SealedFighter);
        }

        #endregion .Seal() method

        #region BattleManager tests

        [Test]
        public void BattleManager_AppropriatelyDisplaysAbsorbMessage()
        {
            _humanFighter.SetStrength(_shade1.MaxHealth + _shade1.Defense);
            _humanFighter.SetMove(_basicAttack, 1);
            _humanFighter.SetMoveTarget(_shade1);
            _humanFighter.SetMove(_runawayMove);
            _chanceService.PushAttackHitsNotCrit();

            //which moves will be selected by the shades
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceChargeIndex, _malevolenceChargeIndex);

            _chanceService.PushWhichEventsOccur(0, 0); //first is which remaining shade absorbs the fallen shade, second is which stat is boosted
            
            //define the expected string here, since the shade's display name will be updated after the absorption
            string expectedAbsorbMessage = $"{_shade1.DisplayName}'s essence was absorbed by {_shade2.DisplayName}!\n";

            _battleManager.Battle(_humanTeam, _shadeTeam, null, new SilentBattleConfiguration());

            MockOutputMessage[] outputs = _output.GetOutputs();
            
            MockOutputMessage absorbOutputMessage = outputs.FirstOrDefault(o => o.Message == expectedAbsorbMessage);

            Assert.NotNull(absorbOutputMessage);
        }

        [Test]
        public void BattleManager_AppropriatelyDisplaysTransformMessage()
        {
            _humanFighter.SetStrength(_shade1.MaxHealth + _shade1.Defense);
            _humanFighter.SetMove(_basicAttack, 1);
            _humanFighter.SetMoveTarget(_shade1);
            _humanFighter.SetMove(_runawayMove);
            _chanceService.PushAttackHitsNotCrit();

            //which moves will be selected by the shades
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceChargeIndex, _malevolenceChargeIndex);

            _chanceService.PushWhichEventsOccur(0, 0); //first is which remaining shade absorbs the fallen shade, second is which stat is boosted

            //define the expected string here, since the shade's display name will be updated after the absorption
            string displayNameBefore = _shade2.DisplayName;
            
            _battleManager.Battle(_humanTeam, _shadeTeam, null, new SilentBattleConfiguration());

            MockOutputMessage[] outputs = _output.GetOutputs();

            string expectedTransformMessage = $"{displayNameBefore} has transformed to become a {_shade2.BaseName}!\n";
            MockOutputMessage transformOutputMessage = outputs.FirstOrDefault(o => o.Message == expectedTransformMessage);

            Assert.NotNull(transformOutputMessage);
        }

        [Test]
        public void BattleManager_CorrectlyHandlesShadeAbsorbMove()
        {
            //arrange
            _logger.Subscribe(EventType.ShadeAbsorbed, _shade1);
            _humanFighter.SetMove(_doNothingMove, 1);
            _humanFighter.SetMove(_runawayMove);

            _chanceService.PushWhichEventsOccur(_absorptionMoveIndex, 0, _malevolenceChargeIndex, _malevolenceChargeIndex);
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceChargeIndex);

            //act
            Assert.DoesNotThrow(() => _battleManager.Battle(_humanTeam, _shadeTeam));

            //assert
            Assert.AreEqual(0, _shade2.CurrentHealth);

            Assert.AreEqual(1, _logger.Logs.Count);

            EventLog log = _logger.Logs[0];

            Assert.AreEqual(EventType.ShadeAbsorbed, log.Type);
            Assert.AreEqual(_shade2, ((ShadeAbsorbedEventArgs)log.E)?.AbsorbedShade);
        }

        [Test]
        public void BattleManager_CorrectlyPrintsShadeAbsorbMoveExecutionText()
        {
            //arrange
            ShadeAbsorbingMove absorbingMove = _shade1.GetExecutableMoves(_humanTeam).FirstOrDefault(m => m is ShadeAbsorbingMove) as ShadeAbsorbingMove;

            _humanFighter.SetMove(_doNothingMove, 1);
            _humanFighter.SetMove(_runawayMove);

            _chanceService.PushWhichEventsOccur(_absorptionMoveIndex, 0, _malevolenceChargeIndex, _malevolenceChargeIndex);
            _chanceService.PushWhichEventsOccur(_malevolenceChargeIndex, _malevolenceChargeIndex);

            SilentBattleConfiguration config = new SilentBattleConfiguration();

            //set up the output message now, before the display name chanegs from the absorption
            string expectedOutputMessage = $"{_shade1.DisplayName} {absorbingMove?.ExecutionText.Replace(Globals.TargetReplaceText, _shade2.DisplayName)}\n";

            //act
            _battleManager.Battle(_humanTeam, _shadeTeam, config: config);

            MockOutputMessage output = _output.GetOutputs()[0];
            
            Assert.AreEqual(expectedOutputMessage, output.Message);


        }

        #endregion
    }
}