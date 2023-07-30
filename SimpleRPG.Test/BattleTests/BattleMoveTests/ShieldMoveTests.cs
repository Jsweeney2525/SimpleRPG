using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ShieldMoveTests
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

        private static Type[] _shieldTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                              from assemblyType in domainAssembly.GetTypes()
                                              where assemblyType.IsSubclassOf(typeof(BattleShield))
                                              select assemblyType).ToArray();

        public static IEnumerable<BattleShield> _shields = _shieldTypes.Select(GetShieldFromType);

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
        public void CopyMethod_ReturnsAppropriateResponse([Values(MagicType.Fire, MagicType.Water)] MagicType shieldMagicType, [Values(1, 7)] int shieldHealth,
            [Values(2, 4)] int shieldDefense)
        {
            ElementalBattleShield shield = new ElementalBattleShield(shieldHealth, shieldDefense, 0, MagicType.Lightning);
            ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, null, shield);

            ShieldMove copy = new ShieldMove(shieldMove);

            Assert.AreNotEqual(shieldMove.Shield, copy.Shield);
            Assert.IsTrue(shieldMove.Shield.AreEqual(copy.Shield));
        }

        [Test]
        public void BattleManager_CorrectlySetsShield_WhenExecutingShieldMove()
        {
            const int shieldDefense = 5;
            const int shieldHealth = 1;
            ElementalBattleShield shield = new ElementalBattleShield(shieldHealth, shieldDefense, 0, MagicType.Lightning);
            ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, null, shield);

            _humanFighter.SetSpeed(1);
            _humanFighter.SetMove(shieldMove, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetStrength(shieldHealth + shieldDefense);
            _enemy.SetMove(_basicAttackMove);
            _chanceService.PushEventsOccur(true, false); //attack hits, is not a crit

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_humanFighter.MaxHealth, _humanFighter.CurrentHealth);
        }

        [Test]
        public void BattleManager_CorrectlyPrintsMessages_ElementalBattleShield([Values("eats pudding", null)] string executionMessage, 
            [Values(MagicType.Ice, MagicType.Fire, MagicType.Earth, MagicType.Lightning)] MagicType shieldMagicType)
        {
            const int shieldDefense = 5;
            const int shieldHealth = 1;
            ElementalBattleShield shield = new ElementalBattleShield(shieldHealth, shieldDefense, 0, shieldMagicType);
            ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, executionMessage, shield);

            _humanFighter.SetSpeed(1);
            _humanFighter.SetMove(shieldMove, 1);
            _humanFighter.SetMove(_runawayMove);

            _enemy.SetStrength(shieldHealth + shieldDefense);
            _enemy.SetMove(_basicAttackMove);
            _chanceService.PushEventsOccur(true, false); //attack hits, is not a crit

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowAttackMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            MockOutputMessage[] outputs = _output.GetOutputs();

            int expectedLength = 3; //damage taken, "equipped with shield," and "shield destroyed" messages
            if (executionMessage != null)
            {
                expectedLength++;
            }
            Assert.AreEqual(expectedLength, outputs.Length);

            int i = 0;

            if (executionMessage != null)
            {
                Assert.AreEqual($"{_humanFighter.DisplayName} {executionMessage}!\n", outputs[i++].Message);
            }

            string aOrAn = shieldMagicType == MagicType.Ice || shieldMagicType == MagicType.Earth ? "an" : "a";
            Assert.AreEqual($"{_humanFighter.DisplayName} was equipped with {aOrAn} {shieldMagicType.ToString().ToLower()} elemental battle shield!\n", outputs[i].Message);
        }

        [Test]
        public void BattleManager_PrintsMessage_SmokeTest()
        {
            foreach (BattleShield shield in _shields)
            {
                ShieldMove shieldMove = new ShieldMove("foo", TargetType.Self, null, shield);

                _humanFighter.SetSpeed(1);
                _humanFighter.SetMove(shieldMove, 1);
                _humanFighter.SetMove(_runawayMove);

                _enemy.SetMove(_doNothing);

                BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
                {
                    ShowIntroAndOutroMessages = false,
                    ShowAttackMessages = false
                };

                _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

                MockOutputMessage[] outputs = _output.GetOutputs();

                Assert.AreEqual(1, outputs.Length);
            }
        }

        private static BattleShield GetShieldFromType(Type t)
        {
            BattleShield ret = null;

            if (t == typeof(IronBattleShield))
            {
                ret = new IronBattleShield(1, 0, 0);
            }
            else if (t == typeof(ElementalBattleShield))
            {
                ret = new ElementalBattleShield(1, 0, 0, MagicType.Fire);
            }
            else
            {
                throw new NotImplementedException($"GetShieldFromType() does not yet know how to handle type '{t}'");
            }

            return ret;
        }
    }
}
