using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    public class WarriorTests
    {
        private TestHumanFighter _humanFighter;
        private Warrior _level1Warrior;
        private Warrior _level3Warrior;

        private Team _humanTeam;
        private Team _warriorTeam;

        private MockInput _input;
        private MockOutput _output;
        private MockChanceService _chanceService;
        private TestMenuManager _menuManager;

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _chanceService = new MockChanceService();
            _menuManager = new TestMenuManager(_input, _output);

            TestFighterFactory.SetChanceService(_chanceService);

            _humanFighter = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _humanTeam = new Team(_menuManager, _humanFighter);

            _level1Warrior = (Warrior)FighterFactory.GetFighter(FighterType.Warrior, 1);
            _level3Warrior = (Warrior)FighterFactory.GetFighter(FighterType.Warrior, 3);
            _warriorTeam = new Team(_menuManager, _level1Warrior);
        }

        [Test]
        public void WarriorClassConstructor_Level1_CorrectlyInitializesWith3Moves()
        {
            Assert.AreEqual(3, _level1Warrior.AvailableMoves.Count);
        }

        [Test]
        public void WarriorClass_Level1_CorrectlySetsUpSelectMove_NoAttackBoost()
        {
            _chanceService.PushWhichEventOccurs(0);

            List<BattleMove> availableMoves = _level1Warrior.AvailableMoves;
            int attackIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Attack);
            int evadeIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is AutoEvadeStatus);
            int attackBoostIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is StatMultiplierStatus);

            _level1Warrior.SelectMove(_warriorTeam, _humanTeam);

            double[] chances = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(0.25, chances[attackIndex]);
            Assert.AreEqual(0.25, chances[evadeIndex]);
            Assert.AreEqual(0.5, chances[attackBoostIndex]);
        }

        [Test]
        public void WarriorClass_Level1_CorrectlySetsUpSelectMove_WarriorHasAttackBoostStatus()
        {
            _chanceService.PushWhichEventOccurs(0);

            List<BattleMove> availableMoves = _level1Warrior.AvailableMoves;
            int attackIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Attack);
            int evadeIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is AutoEvadeStatus);
            int attackBoostIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is StatMultiplierStatus);

            StatusMove attackBoostMove = availableMoves[attackBoostIndex] as StatusMove ?? new StatusMove("", TargetType.Self, null);

            _level1Warrior.AddStatus(attackBoostMove.Status);

            _level1Warrior.SelectMove(_warriorTeam, _humanTeam);

            double[] chances = _chanceService.LastEventOccursArgs;

            double oneSixth = 1.0/6;
            Assert.AreEqual(2.0/3, chances[attackIndex]);
            Assert.AreEqual(oneSixth, chances[evadeIndex]);
            Assert.AreEqual(oneSixth, chances[attackBoostIndex]);
        }

        [Test]
        public void WarriorClassConstructor_Level3_CorrectlyInitializesWith4Moves()
        {
            Assert.AreEqual(4, _level3Warrior.AvailableMoves.Count);
        }

        private void GetLevel3MoveIndices(out int attackIndex, out int evadeIndex, out int evadeAndCounterIndex, out int attackBoostIndex)
        {
            List<BattleMove> availableMoves = _level3Warrior.AvailableMoves;
            attackIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Attack);
            evadeIndex = availableMoves.FindIndex(bm =>
            {
                if (bm.MoveType != BattleMoveType.Status)
                {
                    return false;
                }

                StatusMove statusMove = bm as StatusMove;

                AutoEvadeStatus status = statusMove?.Status as AutoEvadeStatus;

                if (status == null || status.ShouldCounterAttack)
                {
                    return false;
                }
                return true;
            });
            evadeAndCounterIndex = availableMoves.FindIndex(bm =>
            {
                if (bm.MoveType != BattleMoveType.Status)
                {
                    return false;
                }

                StatusMove statusMove = bm as StatusMove;

                AutoEvadeStatus status = statusMove?.Status as AutoEvadeStatus;

                if (status == null || !status.ShouldCounterAttack)
                {
                    return false;
                }
                return true;
            });
            attackBoostIndex = availableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is StatMultiplierStatus);
        }

        [Test]
        public void WarriorClass_Level3_CorrectlySetsUpSelectMove_NoAttackBoost()
        {
            _chanceService.PushWhichEventOccurs(0);

            int attackIndex, evadeIndex, evadeAndCounterIndex, attackBoostIndex;

            GetLevel3MoveIndices(out attackIndex, out evadeIndex, out evadeAndCounterIndex, out attackBoostIndex);

            _level3Warrior.SelectMove(_warriorTeam, _humanTeam);

            double[] chances = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(0.25, chances[attackIndex]);
            Assert.AreEqual(0.15, chances[evadeIndex]);
            Assert.AreEqual(0.10, chances[evadeAndCounterIndex]);
            Assert.AreEqual(0.50, chances[attackBoostIndex]);
        }

        [Test]
        public void WarriorClass_Level3_CorrectlySetsUpSelectMove_AttackBoost()
        {
            _chanceService.PushWhichEventOccurs(0);

            int attackIndex, evadeIndex, evadeAndCounterIndex, attackBoostIndex;

            GetLevel3MoveIndices(out attackIndex, out evadeIndex, out evadeAndCounterIndex, out attackBoostIndex);

            List<BattleMove> availableMoves = _level3Warrior.AvailableMoves;
            StatusMove attackBoostMove = availableMoves[attackBoostIndex] as StatusMove ?? new StatusMove("", TargetType.Self, null);

            _level3Warrior.AddStatus(attackBoostMove.Status);

            _level3Warrior.SelectMove(_warriorTeam, _humanTeam);

            double[] chances = _chanceService.LastEventOccursArgs;

            Assert.AreEqual(0.65, chances[attackIndex]);
            Assert.AreEqual(0.15, chances[evadeIndex]);
            Assert.AreEqual(0.10, chances[evadeAndCounterIndex]);
            Assert.AreEqual(0.10, chances[attackBoostIndex]);
        }
    }
}
