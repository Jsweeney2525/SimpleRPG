using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests.EnemyTests
{
    [TestFixture]
    class MegaChickenTests
    {
        private MegaChicken _chicken;
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

        private string _castPrefix;

        private Tuple<MagicType, string>[] _level2Eggs =
        {
            new Tuple<MagicType, string>(MagicType.Fire, "blaze egg"),
            new Tuple<MagicType, string>(MagicType.Lightning, "thunder egg"),
            new Tuple<MagicType, string>(MagicType.Ice, "frost egg")
        };

        private Tuple<MagicType, string>[] _level3Eggs =
        {
            new Tuple<MagicType, string>(MagicType.Fire, "inferno egg"),
            new Tuple<MagicType, string>(MagicType.Lightning, "tempest egg"),
            new Tuple<MagicType, string>(MagicType.Ice, "blizzard egg")
        };

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            
            _chanceService = new MockChanceService();

            _battleManager = new TestBattleManager(_chanceService, _input, _output);

            TestFighterFactory.SetChanceService(_chanceService);

            _chicken = (MegaChicken)FighterFactory.GetFighter(FighterType.MegaChicken, 1);
            _enemyTeam = new Team(_menuManager, _chicken);

            _hero = (TestHumanFighter) TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Hero");
            _sidekick = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "SideKick");
            _humanTeam = new TestTeam(new List<HumanFighter> { _hero, _sidekick});

            _castPrefix = $"{_chicken.DisplayName} draws in the power of its magic eggs, sacrificing them and casts ";
        }

        [Test]
        public void ConstructorInitializesExpGiven()
        {
            Assert.AreEqual(1, _chicken.Level);
            Assert.AreEqual(25, _chicken.ExpGivenOnDefeat);
        }

        [Test]
        public void PhysicalDamageLimitedTo1Damage()
        {
            _chicken.PhysicalDamage(_chicken.MaxHealth - 1);

            Assert.AreEqual(_chicken.MaxHealth - 1, _chicken.CurrentHealth);
        }

        [Test]
        public void SelectMove_NoEggs_FullHealth()
        {
            var move = _chicken.SetupMove(_enemyTeam, _humanTeam);

            Assert.AreEqual(BattleMoveType.Special, move.Move.MoveType);
            Assert.AreEqual("lay egg", move.Move.Description);
        }

        [Test]
        public void SelectMove_NoEggs_HalfHealth()
        {
            _chicken.MagicalDamage(_chicken.MaxHealth/2 + 1, MagicType.Fire);

            var move = _chicken.SetupMove(_enemyTeam, _humanTeam);

            Assert.AreEqual(BattleMoveType.Special, move.Move.MoveType);
            Assert.AreEqual("lay 2 eggs", move.Move.Description);
        }

        [Test]
        public void LayEgg_CorrectlyAddsEggsToEnemyTeam()
        {
            _chanceService.PushWhichEventOccurs(0);

            Assert.AreEqual(1, _enemyTeam.Fighters.Count);

            var move = _chicken.SetupMove(_enemyTeam, _humanTeam);

            _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);

            Assert.AreEqual(2, _enemyTeam.Fighters.Count);
            var egg = _enemyTeam.Fighters[1];
            Assert.IsAssignableFrom<Egg>(egg);
            Assert.AreEqual(MagicType.Fire, ((Egg)egg).MagicType);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            Assert.AreEqual($"{_chicken.DisplayName} laid a fire egg!\n", outputs[0].Message);
        }

        [Test]
        public void LayTwoEggs_CorrectlyAddsEggsToEnemyTeam()
        {
            List<MagicType> eggMagicTypes = Globals.EggMagicTypes.ToList();
            _chanceService.PushWhichEventOccurs(eggMagicTypes.IndexOf(MagicType.Lightning));
            _chanceService.PushWhichEventOccurs(eggMagicTypes.IndexOf(MagicType.Ice));

            Assert.AreEqual(1, _enemyTeam.Fighters.Count);

            _chicken.MagicalDamage(_chicken.MaxHealth/2 + 1, MagicType.Ice);
            var move = _chicken.SetupMove(_enemyTeam, _humanTeam);

            _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);

            Assert.AreEqual(3, _enemyTeam.Fighters.Count);
            var egg1 = _enemyTeam.Fighters[1];
            Assert.IsAssignableFrom<Egg>(egg1);
            Assert.AreEqual(MagicType.Lightning, ((Egg)egg1).MagicType);

            var egg2 = _enemyTeam.Fighters[2];
            Assert.IsAssignableFrom<Egg>(egg2);
            Assert.AreEqual(MagicType.Ice, ((Egg)egg2).MagicType);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(2, outputs.Length);

            Assert.AreEqual($"{_chicken.DisplayName} laid a lightning egg!\n", outputs[0].Message);
            Assert.AreEqual($"{_chicken.DisplayName} laid an ice egg!\n", outputs[1].Message);
        }

        [Test]
        public void SelectMove_FullEggs()
        {
            BattleMoveWithTarget move;

            for (var i = 0; i < 3; ++i)
            {
                move = _chicken.SetupMove(_enemyTeam, _humanTeam);
                _chanceService.PushWhichEventOccurs(0);
                _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);
            }

            move = _chicken.SetupMove(_enemyTeam, _humanTeam);
            Assert.AreEqual(BattleMoveType.Special, move.Move.MoveType);
            Assert.AreEqual("cast eggs", move.Move.Description);
        }

        [Test]
        public void SelectMove_AnEggKilledBeforeItCouldBeCast()
        {
            BattleMoveWithTarget move;

            for (var i = 0; i < 3; ++i)
            {
                move = _chicken.SetupMove(_enemyTeam, _humanTeam);
                _chanceService.PushWhichEventOccurs(0);
                _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);
            }

            var egg = _enemyTeam.Fighters[1];
            egg.PhysicalDamage(1);
            Assert.IsFalse(egg.IsAlive());
            move = _chicken.SetupMove(_enemyTeam, _humanTeam);
            Assert.AreEqual(BattleMoveType.Special, move.Move.MoveType);
            Assert.AreEqual("lay egg", move.Move.Description);
        }

        [Test]
        public void CastEggs_CorrectlyRemovesEggsFromEnemyTeam_Exactly3Eggs()
        {
            BattleMoveWithTarget move;

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration();

            _battleManager.SetEnemyTeam(_enemyTeam);
            _battleManager.SetHumanTeam(_humanTeam);
            _battleManager.SetConfig(config);

            for (var i = 0; i < 3; ++i)
            {
                move = _chicken.SetupMove(_enemyTeam, _humanTeam);
                _chanceService.PushWhichEventOccurs(0);
                _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);
            }

            Assert.AreEqual(4, _enemyTeam.Fighters.Count);

            move = _chicken.SetupMove(_enemyTeam, _humanTeam);
            _chanceService.PushWhichEventOccurs(0); //have to select the target of the attack
            _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);

           Assert.AreEqual(1, _enemyTeam.Fighters.Count);
        }

        [Test]
        public void CastEggs_CorrectlyRemovesEggsFromEnemyTeam_MoreThan3Eggs()
        {
            BattleMoveWithTarget move;

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration();

            _battleManager.SetEnemyTeam(_enemyTeam);
            _battleManager.SetHumanTeam(_humanTeam);
            _battleManager.SetConfig(config);

            _chicken.MagicalDamage((_chicken.MaxHealth / 2) + 1, MagicType.Lightning);

            for (var i = 0; i < 2; ++i)
            {
                move = _chicken.SetupMove(_enemyTeam, _humanTeam);
                _chanceService.PushWhichEventOccurs(0);
                _chanceService.PushWhichEventOccurs(0);
                _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);
            }

            Assert.AreEqual(5, _enemyTeam.Fighters.Count);

            move = _chicken.SetupMove(_enemyTeam, _humanTeam);
            _chanceService.PushWhichEventOccurs(0); //have to select the target of the attack
            _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);

            Assert.AreEqual(2, _enemyTeam.Fighters.Count);
        }

        [Test, Sequential]
        public void CastEggs_Level3Spells([Range(0, 2)] int level3EggIndex)
        {
            Tuple<MagicType, string> level3EggCombo = _level3Eggs[level3EggIndex];
            string spellName = level3EggCombo.Item2;
            MagicType spellType = level3EggCombo.Item1;
            int typeIndex = Globals.EggMagicTypes.ToList().IndexOf(spellType);

            Spell spell = SpellFactory.GetSpell(spellName);
            int damage =  spell.Power + _chicken.MagicStrength;

            _hero.SetHealth(damage + 1);
            _hero.SetMove(_doNothingMove, 4);
            _hero.SetMove(_runawayMove);
            _hero.SetMoveTarget(_hero);

            _sidekick.SetMove(_doNothingMove);
            _sidekick.SetMoveTarget(_sidekick);

            //first 3 determine the egg type, the fourth is who the chicken is targetting
            _chanceService.PushWhichEventsOccur(typeIndex, typeIndex, typeIndex, 0);

            _battleManager.SuppressBattleIntroAndOutroMessages();
            _battleManager.Battle(_humanTeam, _enemyTeam);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(5, outputs.Length); // 1 for each egg lay, 1 for actual attack, 1 to output how much damage the fighter took

            Assert.AreEqual($"{_castPrefix}{spellName}!\n", outputs[3].Message);

            Assert.AreEqual(1, _hero.CurrentHealth);
        }

        [Test, Sequential]
        public void CastEggs_Level2Spells([Range(0, 2)] int level2EggIndex)
        {
            Tuple<MagicType, string> eggCombo = _level2Eggs[level2EggIndex];

            string spellName = eggCombo.Item2;
            MagicType spellType = eggCombo.Item1;

            Spell spell = SpellFactory.GetSpell(spellName);
            int damage = spell.Power + _chicken.MagicStrength;

            _hero.SetMove(_doNothingMove, 4);
            _hero.SetMove(_runawayMove);
            _hero.SetMoveTarget(_hero);

            _sidekick.SetHealth(damage + 1);
            _sidekick.SetMove(_doNothingMove);
            _sidekick.SetMoveTarget(_sidekick);

            int typeIndex = Globals.EggMagicTypes.ToList().IndexOf(spellType);
            _chanceService.PushWhichEventsOccur(typeIndex, typeIndex, (typeIndex + 1) % 3, 1); //the 3 egg types and targetting the sidekick

            _battleManager.SuppressBattleIntroAndOutroMessages();
            _battleManager.Battle(_humanTeam, _enemyTeam);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(5, outputs.Length); // 1 for each egg lay, 1 for actual attack, 1 to output how much damage the fighter took

            Assert.AreEqual(_castPrefix + spellName + "!\n", outputs[3].Message);

            Assert.AreEqual(1, _sidekick.CurrentHealth);
        }

        [Test]
        public void CastEggs_ChaosEgg()
        {
            Spell spell = SpellFactory.GetSpell("chaos egg");
            int damage = spell.Power + _chicken.MagicStrength;

            _hero.SetHealth(damage + 1);
            _hero.SetMove(_doNothingMove, 4);
            _hero.SetMove(_runawayMove);
            _hero.SetMoveTarget(_hero);

            _sidekick.SetHealth(damage + 1);
            _sidekick.SetMove(_doNothingMove);
            _sidekick.SetMoveTarget(_sidekick);

            _chanceService.PushWhichEventsOccur(0, 1, 2);
            
            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(7, outputs.Length); // 1 for each egg lay, 1 for actual attack, 3 to output how much damage each fighter took

            Assert.AreEqual($"{_castPrefix}chaos egg!\n", outputs[3].Message);

            Assert.AreEqual(1, _hero.CurrentHealth);
            Assert.AreEqual(1, _sidekick.CurrentHealth);
            Assert.AreEqual(_chicken.MaxHealth - damage, _chicken.CurrentHealth);
        }

        [Test]
        public void CastEggs_UnhappyPath_EggKilledBeforeMoveCanBeExecuted()
        {
            BattleMoveWithTarget move;

            for (var i = 0; i < 3; ++i)
            {
                move = _chicken.SetupMove(_enemyTeam, _humanTeam);
                _chanceService.PushWhichEventOccurs(0);
                _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);
            }

            move = _chicken.SetupMove(_enemyTeam, _humanTeam);
            var egg = _enemyTeam.Fighters[1];
            egg.PhysicalDamage(egg.MaxHealth);
            _chanceService.PushWhichEventOccurs(0);
            _chicken.ExecuteMove(_battleManager, move, _enemyTeam, _humanTeam, _output);

            //should lay an egg, not absorb the two remaining ones on the field.
            Assert.AreEqual(4, _enemyTeam.Fighters.Count);
        }
    }
}
