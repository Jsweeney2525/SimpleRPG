using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleManager;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests
{
    [TestFixture]
    class BattleManagerTests
    {
        //TODO: A lot of these tests were testing not only the outcomes of the tests, but events being fired as well. Those should be their own tests!
        private TestHumanFighter _humanPlayer1, _humanPlayer2;
        private TestTeam _humanTeam;
        private TestEnemyFighter _enemyPlayer1, _enemyPlayer2;
        private Team _enemyTeam;

        private readonly Spell _fireball = SpellFactory.GetSpell(MagicType.Fire, 1);
        private readonly BattleMove _basicAttackMove = MoveFactory.Get(BattleMoveType.Attack);
        private readonly DoNothingMove _doNothingMove = (DoNothingMove)MoveFactory.Get(BattleMoveType.DoNothing);
        private readonly BattleMove _runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

        private MockInput _input;
        private MockOutput _output;
        private MockMenuFactory _menuFactory;
        private TestMenuManager _menuManager;
        private MockChanceService _mockChanceService;
        private BattleManager _battleManager;

        private EventLogger _logger;

        [SetUp]
        public void Setup()
        {
            _input = new MockInput();
            _output = new MockOutput();
            _menuFactory = new MockMenuFactory();
            _menuManager = new TestMenuManager(_input, _output, _menuFactory);
            _logger = new EventLogger();
            _mockChanceService = new MockChanceService();
            TestFighterFactory.SetChanceService(_mockChanceService);

            _battleManager = new BattleManager(_mockChanceService, _input, _output);

            _humanPlayer1 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _logger.SubscribeAll(_humanPlayer1);

            _humanPlayer2 = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Jed");
            _logger.SubscribeAll(_humanPlayer2);

            _humanPlayer2.AddSpell(_fireball);
            _humanPlayer2.SetMana(_fireball.Cost);
            _logger.ClearLogs();

            _humanPlayer2.TurnEnded += TurnEndedEvents.RestoreManaOnTurnEnd;

            _enemyPlayer1 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _logger.SubscribeAll(_enemyPlayer1);

            _enemyPlayer2 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _logger.SubscribeAll(_enemyPlayer2);

            _humanTeam = new TestTeam(_menuManager, _humanPlayer1, _humanPlayer2);

            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1, _enemyPlayer2);
        }

        private void TestPlayer1Events(List<EventLog> logs, int startingIndex, bool shouldHaveKilled, EnemyFighter enemy = null)
        {
            if (enemy == null)
            {
                enemy = _enemyPlayer1;
            }

            Assert.AreEqual(EventType.DamageTaken, logs[startingIndex].Type);
            Assert.AreEqual(enemy, logs[startingIndex].Sender);
            var damageTakenArgs = logs[startingIndex].E as PhysicalDamageTakenEventArgs;
            Assert.That(damageTakenArgs, Is.Not.Null);
            if (damageTakenArgs != null) {  Assert.AreEqual(_humanPlayer1.Strength, damageTakenArgs.Damage); }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.Killed, logs[startingIndex].Type);
                Assert.AreEqual(enemy, logs[startingIndex].Sender);
                var killedArgs = logs[startingIndex].E as KilledEventArgs;
                Assert.That(killedArgs, Is.Not.Null);
                ++startingIndex;
            }

            Assert.AreEqual(EventType.AttackSuccessful, logs[startingIndex].Type);
            Assert.AreEqual(_humanPlayer1, logs[startingIndex].Sender);
            var attackSuccessfulArgs = logs[startingIndex].E as AttackSuccessfulEventArgs;
            Assert.That(attackSuccessfulArgs, Is.Not.Null);
            if (attackSuccessfulArgs != null)
            {
                Assert.AreEqual(enemy, attackSuccessfulArgs.TargettedFoe);
                Assert.AreEqual(_humanPlayer1.Strength, attackSuccessfulArgs.DamageDealt);
            }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.EnemyKilled, logs[startingIndex].Type);
                Assert.AreEqual(_humanPlayer1, logs[startingIndex].Sender);
                var enemyKilledArgs = logs[startingIndex].E as EnemyKilledEventArgs;
                Assert.That(enemyKilledArgs, Is.Not.Null);
                if (enemyKilledArgs != null) { Assert.AreEqual(enemy, enemyKilledArgs.Enemy); }
            }
        }

        private void TestPlayer2Events(List<EventLog> logs, int startingIndex, bool shouldHaveKilled, EnemyFighter enemy = null)
        {
            if (enemy == null)
            {
                enemy = _enemyPlayer2;
            }

            Assert.AreEqual(EventType.ManaLost, logs[startingIndex].Type);
            Assert.AreEqual(_humanPlayer2, logs[startingIndex].Sender);
            var manaLostEventArgs = logs[startingIndex].E as ManaLostEventArgs;
            Assert.That(manaLostEventArgs, Is.Not.Null);
            if (manaLostEventArgs != null) { Assert.AreEqual(_fireball.Cost, manaLostEventArgs.ManaSpent); }
            ++startingIndex;

            Assert.AreEqual(EventType.MagicalDamageTaken, logs[startingIndex].Type);
            Assert.AreEqual(enemy, logs[startingIndex].Sender);
            var damageTakenArgs = logs[startingIndex].E as MagicalDamageTakenEventArgs;
            Assert.That(damageTakenArgs, Is.Not.Null);
            if (damageTakenArgs != null)
            {
                Assert.AreEqual(_humanPlayer2.MagicStrength + _fireball.Power, damageTakenArgs.Damage);
                Assert.AreEqual(_fireball.ElementalType, damageTakenArgs.MagicType);
            }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.Killed, logs[startingIndex].Type);
                Assert.AreEqual(enemy, logs[startingIndex].Sender);
                var killedArgs = logs[startingIndex].E as KilledEventArgs;
                Assert.That(killedArgs, Is.Not.Null);
                ++startingIndex;
            }

            Assert.AreEqual(EventType.SpellSuccessful, logs[startingIndex].Type);
            Assert.AreEqual(_humanPlayer2, logs[startingIndex].Sender);
            var spellSuccessfulArgs = logs[startingIndex].E as SpellSuccessfulEventArgs;
            Assert.That(spellSuccessfulArgs, Is.Not.Null);
            if (spellSuccessfulArgs != null)
            {
                Assert.AreEqual(enemy, spellSuccessfulArgs.TargettedFoe);
                var magicDamageDealt = _humanPlayer2.MagicStrength + SpellFactory.GetSpell(MagicType.Fire, 1).Power;
                Assert.AreEqual(magicDamageDealt, spellSuccessfulArgs.DamageDealt);
                Assert.AreEqual("fireball", spellSuccessfulArgs.Spell.Description);
            }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.EnemyKilled, logs[startingIndex].Type);
                Assert.AreEqual(_humanPlayer2, logs[startingIndex].Sender);
                var enemyKilledArgs = logs[startingIndex].E as EnemyKilledEventArgs;
                Assert.That(enemyKilledArgs, Is.Not.Null);
                if (enemyKilledArgs != null) { Assert.AreEqual(enemy, enemyKilledArgs.Enemy); }
            }
        }

        private void TestEnemyPlayer1Events(List<EventLog> logs, int startingIndex, bool shouldHaveKilled)
        {
            Assert.AreEqual(EventType.DamageTaken, logs[startingIndex].Type);
            Assert.AreEqual(_humanPlayer1, logs[startingIndex].Sender);
            var damageTakenArgs = logs[startingIndex].E as PhysicalDamageTakenEventArgs;
            Assert.That(damageTakenArgs, Is.Not.Null);
            if (damageTakenArgs != null) { Assert.AreEqual(_enemyPlayer1.Strength - _humanPlayer1.Defense, damageTakenArgs.Damage); }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.Killed, logs[startingIndex].Type);
                Assert.AreEqual(_humanPlayer1, logs[startingIndex].Sender);
                var killedArgs = logs[startingIndex].E as KilledEventArgs;
                Assert.That(killedArgs, Is.Not.Null);
                ++startingIndex;
            }

            Assert.AreEqual(EventType.AttackSuccessful, logs[startingIndex].Type);
            Assert.AreEqual(_enemyPlayer1, logs[startingIndex].Sender);
            var attackSuccessfulArgs = logs[startingIndex].E as AttackSuccessfulEventArgs;
            Assert.That(attackSuccessfulArgs, Is.Not.Null);
            if (attackSuccessfulArgs != null)
            {
                Assert.AreEqual(_humanPlayer1, attackSuccessfulArgs.TargettedFoe);
                Assert.AreEqual(_enemyPlayer1.Strength, attackSuccessfulArgs.DamageDealt);
            }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.EnemyKilled, logs[startingIndex].Type);
                Assert.AreEqual(_enemyPlayer1, logs[startingIndex].Sender);
                var enemyKilledArgs = logs[startingIndex].E as EnemyKilledEventArgs;
                Assert.That(enemyKilledArgs, Is.Not.Null);
                if (enemyKilledArgs != null) {  Assert.AreEqual(_humanPlayer1, enemyKilledArgs.Enemy); }
            }
        }

        private void TestEnemyPlayer2Events(List<EventLog> logs, int startingIndex, bool shouldHaveKilled)
        {
            Assert.AreEqual(EventType.DamageTaken, logs[startingIndex].Type);
            Assert.AreEqual(_humanPlayer2, logs[startingIndex].Sender);
            var damageTakenArgs = logs[startingIndex].E as PhysicalDamageTakenEventArgs;
            Assert.That(damageTakenArgs, Is.Not.Null);
            if (damageTakenArgs != null) { Assert.AreEqual(_enemyPlayer2.Strength, damageTakenArgs.Damage); }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.Killed, logs[startingIndex].Type);
                Assert.AreEqual(_humanPlayer2, logs[startingIndex].Sender);
                var killedArgs = logs[startingIndex].E as KilledEventArgs;
                Assert.That(killedArgs, Is.Not.Null);
                ++startingIndex;
            }

            Assert.AreEqual(EventType.AttackSuccessful, logs[startingIndex].Type);
            Assert.AreEqual(_enemyPlayer2, logs[startingIndex].Sender);
            var attackSuccessfulArgs = logs[startingIndex].E as AttackSuccessfulEventArgs;
            Assert.That(attackSuccessfulArgs, Is.Not.Null);
            if (attackSuccessfulArgs != null)
            {
                Assert.AreEqual(_humanPlayer2, attackSuccessfulArgs.TargettedFoe);
                Assert.AreEqual(_enemyPlayer2.Strength, attackSuccessfulArgs.DamageDealt);
            }
            ++startingIndex;

            if (shouldHaveKilled)
            {
                Assert.AreEqual(EventType.EnemyKilled, logs[startingIndex].Type);
                Assert.AreEqual(_enemyPlayer2, logs[startingIndex].Sender);
                var enemyKilledArgs = logs[startingIndex].E as EnemyKilledEventArgs;
                Assert.That(enemyKilledArgs, Is.Not.Null);
                if (enemyKilledArgs != null) { Assert.AreEqual(_humanPlayer2, enemyKilledArgs.Enemy); }
            }
        }

        [Test]
        public void TestBattle_CorrectEventsFired_EnemiesDefeatedImmediately()
        {
            var player1DamageDealt = _humanPlayer1.Strength;
            _enemyPlayer1.SetHealth(player1DamageDealt);

            var player2DamageDealt = _humanPlayer2.MagicStrength + _fireball.Power;
            _enemyPlayer2.SetHealth(player2DamageDealt);

            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventsOccur(0, 1);

            //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
            _mockChanceService.PushEventsOccur(true, false, true, false);

            _input.Push(new List<string> { "fight", "attack", "1" });
            _input.Push(new List<string> { "fight", "magic", "fireball", "2" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            var logs = _logger.Logs;

            //5 events for first player's turn (damageDone, killed, attack/spell successful, enemy killed, turn end)
            //6 events for second player's turn (those same 5 + manaLost)
            //plus at the end of second player's turn RestoreMana() is called, which raises a ManaRestored event
            //then, 2 more expGained events
            Assert.AreEqual(14, logs.Count);

            TestPlayer1Events(logs, 0, true);
            TestPlayer2Events(logs, 5, true);
        }

        [Test]
        public void SelectNewTarget_SingleEnemyTargetType()
        {
            _humanPlayer1.SetMove(_doNothingMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);

            _humanPlayer2.SetMove(_basicAttackMove);
            _humanPlayer2.SetMoveTarget(_enemyPlayer1);
            _mockChanceService.PushAttackHitsNotCrit();

            _enemyPlayer1.DealMaxPhysicalDamage();
            _enemyPlayer2.SetMove(_doNothingMove);
            _enemyPlayer2.SetHealth(_humanPlayer2.Strength + 1);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _enemyPlayer2.CurrentHealth);
        }

        [Test]
        public void SelectNewTarget_SingleAllyTargetType()
        {
            TestEnemyFighter enemy3 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _enemyTeam = new Team(_menuManager, enemy3, _enemyPlayer1, _enemyPlayer2);

            AttackBattleMove attackAlly = new AttackBattleMove("fake move", TargetType.SingleAlly, 100, 0);
            
            enemy3.SetMove(attackAlly);
            enemy3.SetMoveTarget(_enemyPlayer1);
            _mockChanceService.PushAttackHitsNotCrit();

            _enemyPlayer1.DealMaxPhysicalDamage();

            _enemyPlayer2.SetHealth(enemy3.Strength + 1);
            _enemyPlayer2.SetMove(_doNothingMove);

            _humanPlayer1.SetMove(_doNothingMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);

            _humanPlayer2.SetMove(_doNothingMove);
            _humanPlayer2.SetMoveTarget(_humanPlayer2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, _enemyPlayer2.CurrentHealth);
        }

        [Test]
        public void SelectNewTarget_SingleAllyOrSelfTargetType()
        {
            TestEnemyFighter enemy3 = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "Enemy");
            _enemyTeam = new Team(_menuManager, enemy3, _enemyPlayer1, _enemyPlayer2);

            AttackBattleMove attackAllyOrSelf = new AttackBattleMove("fake move", TargetType.SingleAllyOrSelf, 100, 0);

            enemy3.SetMove(attackAllyOrSelf);
            enemy3.SetMoveTarget(_enemyPlayer1);
            _mockChanceService.PushAttackHitsNotCrit();
            enemy3.SetHealth(enemy3.Strength + 1);

            _enemyPlayer1.DealMaxPhysicalDamage();
            
            _enemyPlayer2.SetMove(_doNothingMove);

            _humanPlayer1.SetMove(_doNothingMove, 1);
            _humanPlayer1.SetMove(_runawayMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);

            _humanPlayer2.SetMove(_doNothingMove);
            _humanPlayer2.SetMoveTarget(_humanPlayer2);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(1, enemy3.CurrentHealth);
        }

        [Test]
        public void TestBattle_EnemiesGiveExpWhenDefeated([Values(1, 3)] int numberOfEnemies)
        {
            _humanPlayer1.SetStrength(1);
            _humanPlayer2.SetStrength(1);

            const int expPerEnemy = 5;

            var enemies = new List<IFighter>();
            for (var i = 0; i < numberOfEnemies; ++i)
            {
                var enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
                enemy.SetHealth(1);
                //make the enemy do nothing if it does get to attack
                enemy.SetMove(MoveFactory.Get(BattleMoveType.DoNothing));
                enemy.SetExpGiven(expPerEnemy);
                enemies.Add(enemy);

                //set the human player actions
                _input.Push(new List<string> { "fight", "attack", "1" });
                //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
                _mockChanceService.PushEventOccurs(true);
                _mockChanceService.PushEventOccurs(false);
            }

            if ((numberOfEnemies%2) == 1)
            {
                //need an even number of inputs, or else the GetInputs will break
                _input.Push(new List<string> { "fight", "attack", "1" });
            }

            Team enemyTeam = new Team(_menuManager, enemies);

            _battleManager.Battle(_humanTeam, enemyTeam);

            Assert.AreEqual(numberOfEnemies * expPerEnemy, _humanPlayer1.CurrentExp);
            Assert.AreEqual(numberOfEnemies * expPerEnemy, _humanPlayer2.CurrentExp);
        }

        [Test]
        public void TestBattle_ClearsExpTotalIfBattleDoesNotEndInVictory()
        {
            _humanPlayer1.SetStrength(1);
            _humanPlayer2.SetStrength(1);

            var enemies = new List<IFighter>();
            const int expPerEnemy = 5;
            
            for (var i = 0; i < 3; ++i)
            {
                var enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
                enemy.SetHealth(1);
                //make the enemy do nothing if it does get to attack
                enemy.SetMove(MoveFactory.Get(BattleMoveType.DoNothing));
                enemy.SetExpGiven(expPerEnemy * 2);
                enemies.Add(enemy);

                //set the human player actions
                _input.Push(new List<string> { "fight", "attack", "1" });
                //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
                _mockChanceService.PushEventOccurs(true);
                _mockChanceService.PushEventOccurs(false);
            }

            _input.Push(new List<string> { "run", "yes" });

            Team enemyTeam = new Team(_menuManager, enemies);

            _battleManager.Battle(_humanTeam, enemyTeam);

            Assert.AreEqual(0, _humanPlayer1.CurrentExp);
            Assert.AreEqual(0, _humanPlayer2.CurrentExp);
            Assert.IsFalse(enemies[0].IsAlive());
            Assert.IsFalse(enemies[1].IsAlive());

            //simply reviving the dead enemies means they will all have 2 "OnEnemyDefeated" listeners assigned to them.
            //recreating the list will prevent double registering
            enemies = new List<IFighter>();
            for (var i = 0; i < 3; ++i)
            {
                var enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
                enemy.SetHealth(1);
                //make the enemy do nothing if it does get to attack
                enemy.SetMove(MoveFactory.Get(BattleMoveType.DoNothing));
                enemy.SetExpGiven(expPerEnemy);
                enemies.Add(enemy);

                //set the human player actions
                _input.Push(new List<string> { "fight", "attack", "1" });
                //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
                _mockChanceService.PushEventOccurs(true);
                _mockChanceService.PushEventOccurs(false);
            }

            //need this junk one extra time for _humanPlayer2 during the second turn
            //set the human player actions
            _input.Push(new List<string> { "fight", "attack", "1" });
            //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
            _mockChanceService.PushEventOccurs(true);
            _mockChanceService.PushEventOccurs(false);

            enemyTeam = new Team(_menuManager, enemies);

            _battleManager.Battle(_humanTeam, enemyTeam);

            Assert.AreEqual(3 * expPerEnemy, _humanPlayer1.CurrentExp); //will be expPerEnemy * 7 if it didn't clear old exp counter
            Assert.AreEqual(3 * expPerEnemy, _humanPlayer2.CurrentExp);
        }

        [Test]
        public void TestBattle_DoNothingActionDoesNotDisplayText_IfMoveHasNoMessage()
        {
            _humanTeam = new TestTeam(_menuManager, _humanPlayer1);

            var enemyList = new List<IFighter>();

            const int numEggs = 8;
            for (var i = 0; i < numEggs; ++i)
            {
                var egg = (Egg)FighterFactory.GetFighter(FighterType.Egg, 0, null, MagicType.Fire);
                enemyList.Add(egg);

                _mockChanceService.PushEventsOccur(true, false);
                _input.Push(new List<string> { "fight", "attack", "1" });
            }

            _enemyTeam = new Team(_menuManager, enemyList);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = true
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var clearIndices = _output.GetClearIndices();

            const int clearsPerRound = 4; //main menu, attack type selection menu, targetting menu, "Player attacks enemy!" message
            
            const int expectedClearCount = clearsPerRound*numEggs + 3; //have to add 3 for: battle intro, victory message, exp gained
            Assert.AreEqual(expectedClearCount, clearIndices.Length);
        }

        [Test]
        public void TestBattle_DoNothingActionDisplaysMovesMessageText()
        {
            const string doNothingMessage = "hoots and hollers";
            DoNothingMove doNothingMove = new DoNothingMove(doNothingMessage);

            TestEnemyFighter enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            enemy.SetMove(doNothingMove);
            enemy.SetSpeed(1);

            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);
            _humanPlayer1.SetDeathOnTurnEndEvent();

            _humanTeam = new TestTeam(_menuManager, _humanPlayer1);
            _enemyTeam = new Team(_menuManager, enemy);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowDeathMessages = false
            };
            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length, "The message should have been displayed, but it was not");
            Assert.AreEqual($"{enemy.DisplayName} { doNothingMessage }\n", outputs[0].Message);

            var clearIndices = _output.GetClearIndices();

            Assert.AreEqual(1, clearIndices.Length, "The WaitAndClear method should be called after the do nothing message is displayed");
        }

        [Test]
        public void TestBattle_EnemiesGetToAttack()
        {
            //set up player 1's attack and enemy's health
            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);

            int player1DamageDealt = _humanPlayer1.Strength - _enemyPlayer1.Defense;
            _enemyPlayer1.SetHealth(player1DamageDealt * 2);
            
            //set up player 2's spell and enemy's health
            _humanPlayer2.SetMove(_fireball);
            _humanPlayer2.SetMoveTarget(_enemyPlayer2);

            int player2DamageDealt = _humanPlayer2.MagicStrength + _fireball.Power;
            _enemyPlayer2.SetHealth(player2DamageDealt * 2);

            //set up player 1's health
            int enemy1DamageDealt = _enemyPlayer1.Strength - _humanPlayer1.Defense;
            _humanPlayer1.SetHealth(enemy1DamageDealt * 2);
            _humanPlayer1.SetSpeed(_enemyPlayer1.Speed + 1);

            //set up player 2's health
            int enemy2DamageDealt = _enemyPlayer2.Strength - _humanPlayer2.Defense;
            _humanPlayer2.SetHealth(enemy2DamageDealt * 2);
            _humanPlayer2.SetSpeed(_enemyPlayer2.Speed + 1);

            //set up the enemy's targets for the first 2 turns
            _mockChanceService.PushWhichEventsOccur(0, 1, 0, 1);

            //set up the "attack hits" and "critical hit" events for all 6 attacks, all hit, none are crits
            for (var i = 0; i < 6; ++i)
            {
                _mockChanceService.PushEventsOccur(true, false);
            }

            _battleManager.Battle(_humanTeam, _enemyTeam);

            var logs = _logger.Logs;

            Assert.AreEqual(28, logs.Count); //26 events detailed below, then 2 more ExpGained at the end of battle

            TestPlayer1Events(logs, 0, false); //player 1- damage taken, attack successful
            Assert.AreEqual(EventType.TurnEnded, logs[2].Type);
            TestPlayer2Events(logs, 3, false); //player2- mana spent, damage taken, spell successful
            Assert.AreEqual(EventType.TurnEnded, logs[6].Type);
            Assert.AreEqual(EventType.ManaRestored, logs[7].Type); //player 2's mana restored inside turn end event
            TestEnemyPlayer1Events(logs, 8, false); //enemy 1- damage taken, attack successful
            Assert.AreEqual(EventType.TurnEnded, logs[10].Type);
            TestEnemyPlayer2Events(logs, 11, false); //enemy 2- damage taken, attack successful
            Assert.AreEqual(EventType.TurnEnded, logs[13].Type);
            TestPlayer1Events(logs, 14, true); //player 1 - damage taken, killed, attack successful, enemy killed
            Assert.AreEqual(EventType.TurnEnded, logs[18].Type);
            TestPlayer2Events(logs, 19, true); //player 2- mana lost, damage taken, killed, attack successful, enemy killed
            Assert.AreEqual(EventType.TurnEnded, logs[24].Type);
            Assert.AreEqual(EventType.ManaRestored, logs[25].Type); //player 2's mana restored inside turn end event
        }

        [Test]
        public void ExecuteAttackMethod_DoesNotThrowException_DefenseExceedsAttack()
        {
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1);
            _humanTeam = new TestTeam(_humanPlayer1);

            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);
            _humanPlayer1.SetStrength(_enemyPlayer1.Defense + _enemyPlayer1.MaxHealth);
            _humanPlayer1.SetDefense(_enemyPlayer1.Strength + 5);
            
            _enemyPlayer1.SetSpeed(_humanPlayer1.Speed + 5);
            _enemyPlayer1.SetMove(_basicAttackMove);
            _enemyPlayer1.SetMoveTarget(_humanPlayer1);

            _mockChanceService.PushEventsOccur(true, false, true, false); //attacks hit and are not crits

            Assert.DoesNotThrow(() => _battleManager.Battle(_humanTeam, _enemyTeam));

            Assert.AreEqual(_humanPlayer1.MaxHealth, _humanPlayer1.CurrentHealth);
        }

        private void CustomAttackMessage_Setup(string executionText)
        {
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1);
            _humanTeam = new TestTeam(_humanPlayer1);

            AttackBattleMove customAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, executionText: executionText);

            _humanPlayer1.SetMove(customAttack);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);
            _mockChanceService.PushEventsOccur(true, false); //attack hits, not a crit

            _enemyPlayer1.SetMove(_doNothingMove);


            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowDeathMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowExpAndLevelUpMessages = false
            };

            _battleManager.Battle(_humanTeam, _enemyTeam, config: config);
        }

        [Test]
        public void ExecuteAttackMethod_PrintsCustomMessage_ExecutionTextNotNull()
        {
            string executionText = "throws a baseball";

            CustomAttackMessage_Setup(executionText);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            string expectedOutput = $"{_humanPlayer1.DisplayName} {executionText}!\n";
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void ExecuteAttackMethod_PrintsCustomMessage_ExecutionTextContainsReplacementString()
        {
            string executionText = $"throws a baseball at {Globals.TargetReplaceText}";

            CustomAttackMessage_Setup(executionText);

            MockOutputMessage[] outputs = _output.GetOutputs();

            Assert.AreEqual(1, outputs.Length);

            string expectedOutput = $"{_humanPlayer1.DisplayName} throws a baseball at {_enemyPlayer1.DisplayName}!\n";
            Assert.AreEqual(expectedOutput, outputs[0].Message);
        }

        [Test]
        public void ExecuteSpellMethod_DoesNotThrowException_ResistanceExceedsMagicStrength()
        {
            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1);
            _humanTeam = new TestTeam(_humanPlayer1);

            _humanPlayer1.SetMove(_basicAttackMove);
            _humanPlayer1.SetMoveTarget(_enemyPlayer1);
            _humanPlayer1.SetStrength(_enemyPlayer1.Defense + _enemyPlayer1.MaxHealth);
            _humanPlayer1.SetMagicResistance(_enemyPlayer1.MagicStrength + _fireball.Power + 5);

            _enemyPlayer1.SetSpeed(_humanPlayer1.Speed + 5);
            _enemyPlayer1.SetMana(_fireball.Cost);
            _enemyPlayer1.AddSpell(_fireball);
            _enemyPlayer1.SetMove(_fireball);
            _enemyPlayer1.SetMoveTarget(_humanPlayer1);

            _mockChanceService.PushEventsOccur(true, false); //attack hits and are not crits

            Assert.DoesNotThrow(() => _battleManager.Battle(_humanTeam, _enemyTeam));

            Assert.AreEqual(_humanPlayer1.MaxHealth, _humanPlayer1.CurrentHealth);
        }

        [Test]
        public void TestBattle_HumanPlayersRun([Values (1, 2)] int whoRuns)
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            for (var i = 1; i <= 2; ++i)
            {
                List<string> inputs = i == whoRuns ? new List<string> { "run", "y" } : new List<string> { "fight", "attack", "1" };

                _input.Push(inputs);
            }

            _battleManager.Battle(_humanTeam, _enemyTeam);

            var logs = _logger.Logs;

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public void TestBattle_SpeedAltersMoveOrder()
        {
            TestHumanFighter humanPlayer = new TestHumanFighter("Hero", 1);

            _logger.SubscribeAll(humanPlayer);

            TestTeam humanTeam = new TestTeam(humanPlayer);

            TestEnemyFighter enemyPlayer = new TestEnemyFighter("Enemy", humanPlayer.Strength, 0, 1, 0, 2, 0, 0, _mockChanceService);
            enemyPlayer.SetSpeed(humanPlayer.Speed + 1);
            enemyPlayer.SetStrength(humanPlayer.MaxHealth - 1);

            _logger.SubscribeAll(enemyPlayer);

            Team team = new Team(_menuManager, enemyPlayer);
            
            humanPlayer.SetMove(_basicAttackMove);
            humanPlayer.SetMoveTarget(enemyPlayer);

            //set up the "attack hits" events for the first 2 attacks, make sure they do connect, but that neither are crits
            _mockChanceService.PushEventsOccur(true, false, true, false);

            _battleManager.Battle(humanTeam, team);

            var logs = _logger.Logs;

            //first 3 are enemy's turn - damage dealt, attack successful, turn ended
            //Then 5 more for human - damage dealt, killed, attack successful, enemy killed, turn ended
            //Finally, 1 more for ExpGained on battle end
            Assert.AreEqual(9, logs.Count); 

            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(humanPlayer, logs[0].Sender);

            Assert.AreEqual(EventType.AttackSuccessful, logs[1].Type);
            Assert.AreEqual(enemyPlayer, logs[1].Sender);

            Assert.AreEqual(EventType.TurnEnded, logs[2].Type);
            Assert.AreEqual(enemyPlayer, logs[2].Sender);

            Assert.AreEqual(EventType.DamageTaken, logs[3].Type);
            Assert.AreEqual(enemyPlayer, logs[3].Sender);

            Assert.AreEqual(EventType.Killed, logs[4].Type);
            Assert.AreEqual(enemyPlayer, logs[4].Sender);

            Assert.AreEqual(EventType.AttackSuccessful, logs[5].Type);
            Assert.AreEqual(humanPlayer, logs[5].Sender);

            Assert.AreEqual(EventType.EnemyKilled, logs[6].Type);
            Assert.AreEqual(humanPlayer, logs[6].Sender);

            Assert.AreEqual(EventType.TurnEnded, logs[7].Type);
            Assert.AreEqual(humanPlayer, logs[7].Sender);
        }

        [Test]
        public void TestBattle_PriorityTrumpsSpeed_WhenDeterminingMoveOrder()
        {
            TestHumanFighter humanPlayer = new TestHumanFighter("Hero", 1);
            TestTeam humanTeam = new TestTeam(humanPlayer);

            TestEnemyFighter enemyPlayer = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            Team enemyTeam = new Team(_menuManager, enemyPlayer);

            AttackBattleMove highPriorityAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0, priority: 1);
            humanPlayer.SetMove(highPriorityAttack);
            humanPlayer.SetMoveTarget(enemyPlayer);
            humanPlayer.SetStrength(enemyPlayer.MaxHealth + enemyPlayer.Defense);

            AttackBattleMove lowPriorityAttack = new AttackBattleMove("foo", TargetType.SingleEnemy, 100, 0);
            enemyPlayer.SetMove(lowPriorityAttack);
            enemyPlayer.SetMoveTarget(humanPlayer);
            enemyPlayer.SetSpeed(humanPlayer.Speed + 1);
            enemyPlayer.SetStrength(humanPlayer.MaxHealth + humanPlayer.Defense);

            //set up the "attack hits" events for the first 2 attacks, make sure they do connect, but that neither are crits
            _mockChanceService.PushEventsOccur(true, false);

            _battleManager.Battle(humanTeam, enemyTeam);

            Assert.AreEqual(humanPlayer.MaxHealth, humanPlayer.CurrentHealth);
            Assert.AreEqual(0, enemyPlayer.CurrentHealth);
        }

        [Test]
        public void TestAccuracyStat()
        {
            var humanPlayer = new TestHumanFighter("Hero", 1);

            var humanTeam = new Team(_menuManager, humanPlayer);

            //enemy fighter speed is higher than human player
            var enemyPlayer = new TestEnemyFighter("Enemy", 2, 0, 1, 0, 2, 10, 0, _mockChanceService);
            enemyPlayer.SetHealth(humanPlayer.Strength - enemyPlayer.Defense);
            //enemy attacks first
            enemyPlayer.SetSpeed(humanPlayer.Speed + 1);

            Team enemyTeam = new Team(_menuManager, enemyPlayer);

            //enemy attack misses, fighter attack hits but is not a crit
            _mockChanceService.PushEventsOccur(false, true, false);

            _input.Push(new List<string> { "fight", "attack", "1" });

            _battleManager.Battle(humanTeam, enemyTeam);

            var lastChanceVals = _mockChanceService.LastChanceVals;

            //verify the correct chance of evade for enemy attack
            Assert.AreEqual((100.0 - humanPlayer.Evade) / 100.0, lastChanceVals[0]);

            //verify the correct chance of evade for fighter attack
            Assert.AreEqual((100.0 - enemyPlayer.Evade) / 100.0, lastChanceVals[1]);

            Assert.AreEqual(humanPlayer.MaxHealth, humanPlayer.CurrentHealth);
        }

        [Test]
        public void TestCritEvent()
        {
            var humanPlayer = new TestHumanFighter("Hero", 1);

            _logger.Subscribe(EventType.AttackSuccessful, humanPlayer);
            _logger.Subscribe(EventType.CriticalAttack, humanPlayer);
            _logger.Subscribe(EventType.EnemyKilled, humanPlayer);
            _logger.Subscribe(EventType.DamageTaken, humanPlayer);
            _logger.Subscribe(EventType.Killed, humanPlayer);

            var humanTeam = new Team(_menuManager, humanPlayer);

            //enemy fighter speed is higher than human player
            var enemyPlayer = new TestEnemyFighter("Enemy", humanPlayer.Strength * 2, 0, 1, 0, 0, 10, 0, _mockChanceService);

            _logger.Subscribe(EventType.AttackSuccessful, enemyPlayer);
            _logger.Subscribe(EventType.AttackMissed, enemyPlayer);
            _logger.Subscribe(EventType.EnemyKilled, enemyPlayer);
            _logger.Subscribe(EventType.DamageTaken, enemyPlayer);
            _logger.Subscribe(EventType.Killed, enemyPlayer);

            var enemyTeam = new Team(_menuManager, enemyPlayer);
            
            //fighter attack hits
            _mockChanceService.PushEventOccurs(true);
            //fighter attack is a critical hit
            _mockChanceService.PushEventOccurs(true);

            _input.Push(new List<string> { "fight", "attack", "1" });

            _battleManager.Battle(humanTeam, enemyTeam);

            var logs = _logger.Logs;

            Assert.AreEqual(5, logs.Count); //critical hit, damage taken, killed, attack successful, enemy killed

            EventLog log = logs[0];

            Assert.AreEqual(EventType.CriticalAttack, log.Type);
            Assert.AreEqual(humanPlayer, log.Sender);
        }

        [Test]
        public void DisplaysExpGainedMessageOnPlayerExpGained([Values(1, 2)] int numPlayers)
        {
            const int enemyHealth = 5;
            var players = new List<HumanFighter>();
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);

            for (var i = 0; i < numPlayers; ++i)
            {
                var player = new TestHumanFighter("Hero", 1);
                _logger.SubscribeAll(player);
                player.SetStrength(enemyHealth);

                players.Add(player);
                player.SetMove(attack);

                _input.Push(new List<string> {"fight", "attack", "1"});
            }

            var humanTeam = new TestTeam(_menuManager, players);

            //enemy fighter speed is higher than human player
            var enemyPlayer = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _logger.SubscribeAll(enemyPlayer);
            enemyPlayer.SetHealth(enemyHealth);
            var nextLevel = players[0].Level + 1;
            var expToNextLevel = LevelUpManager.GetExpForLevel(nextLevel);
            var expGiven = expToNextLevel - 1;
            enemyPlayer.SetExpGiven(expGiven);
            enemyPlayer.SetMove(MoveFactory.Get(BattleMoveType.DoNothing));

            var enemyTeam = new Team(_menuManager, enemyPlayer);

            players.OfType<TestHumanFighter>().ToList().ForEach(f => f.SetMoveTarget(enemyPlayer));

            //fighter attack hits, but is not a crit
            _mockChanceService.PushEventsOccur(true, false);

            _battleManager.Battle(humanTeam, enemyTeam);

            var outputs = _output.GetOutputs();
            var length = outputs.Length;

            for (int i = length - numPlayers, j = 0; i < length; ++i, ++j)
            {
                Assert.AreEqual($"{players[j].DisplayName} gained {expGiven} experience points!\n", outputs[i].Message);
            }
        }

        [Test]
        public void DisplaysLevelUpMessageOnPlayerLeveledUp([Values(1, 2)] int numPlayers)
        {
            List<HumanFighter> players = new List<HumanFighter>();
            const int enemyHealth = 5;
            BattleMove attack = MoveFactory.Get(BattleMoveType.Attack);

            int i = 0;
            for (; i < numPlayers; ++i)
            {
                TestHumanFighter player = new TestHumanFighter("Hero", 1);
                player.SetStrength(enemyHealth);
                player.SetMove(attack);
                _logger.SubscribeAll(player);

                players.Add(player);
            }

            TestTeam humanTeam = new TestTeam(_menuManager, players);

            //enemy fighter speed is higher than human player
            var enemyPlayer = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            enemyPlayer.SetHealth(enemyHealth);
            var nextLevel = players[0].Level + 1;
            var expToNextLevel = LevelUpManager.GetExpForLevel(nextLevel);
            var expGiven = expToNextLevel;
            enemyPlayer.SetExpGiven(expGiven);
            enemyPlayer.SetMove(MoveFactory.Get(BattleMoveType.DoNothing));

            var enemyTeam = new Team(_menuManager, enemyPlayer);

            var spellsLearned = SpellFactory.GetSpellsByLevel<HumanFighter>(nextLevel, nextLevel);
            players.OfType<TestHumanFighter>().ToList().ForEach(f => f.SetMoveTarget(enemyPlayer));

            //fighter attack hits, not a crit
            _mockChanceService.PushEventsOccur(true, false);

            BattleManagerBattleConfiguration config = new BattleManagerBattleConfiguration
            {
                ShowIntroAndOutroMessages = false,
                ShowAttackMessages = false,
                ShowPhysicalDamageMessages = false,
                ShowDeathMessages = false
            };

            _battleManager.Battle(humanTeam, enemyTeam, config: config);

            var outputs = _output.GetOutputs();
            var clears = _output.GetClearIndices();
            var clearLength = clears.Length;
            var outputLength = outputs.Length;

            int numMessagesPerPlayer = 12; //gained exp, grew to level 2, plus 5 stats both boost level and new total
            numMessagesPerPlayer += spellsLearned.Count; //one message for each spell learned

            int numMessagesTotal = numMessagesPerPlayer*numPlayers;

            Assert.AreEqual(numMessagesTotal, outputLength);

            int numClearsPerPlayers = 3; //exp gained, level up (increment of each stat), level up (new stat total)
            int numClearsTotal = numClearsPerPlayers*numPlayers + 1; //have to add one extra for the wait and clear after the attack is executed

            Assert.AreEqual(numClearsTotal, clearLength);

            i = 0;
            int clearIndex = 1;
            for (int j = 0; j < numPlayers; ++j)
            {
                var player = players[j];
                string displayName = player.DisplayName;
                var level = player.Level;

                Assert.AreEqual($"{player.DisplayName} gained {expGiven} experience points!\n", outputs[i++].Message);

                Assert.AreEqual(i, clears[clearIndex++]);

                Assert.AreEqual($"{displayName} grew to level {player.Level}!\n", outputs[i++].Message);
                Assert.AreEqual($"+{LevelUpManager.HealthBoostByLevel(level)} Health!\n", outputs[i++].Message);
                Assert.AreEqual($"+{LevelUpManager.ManaBoostByLevel(level)} Mana!\n", outputs[i++].Message);
                Assert.AreEqual($"+{LevelUpManager.StrengthBoostByLevel(level)} Strength!\n", outputs[i++].Message);
                Assert.AreEqual($"+{LevelUpManager.DefenseBoostByLevel(level)} Defense!\n", outputs[i++].Message);
                Assert.AreEqual($"+{LevelUpManager.SpeedBoostByLevel(level)} Speed!\n", outputs[i++].Message);

                Assert.AreEqual(i, clears[clearIndex++]);

                Assert.AreEqual($"Max Health: {player.MaxHealth}\n", outputs[i++].Message);
                Assert.AreEqual($"Max Mana: {player.MaxMana}\n", outputs[i++].Message);
                Assert.AreEqual($"Strength: {player.Strength}\n", outputs[i++].Message);
                Assert.AreEqual($"Defense: {player.Defense}\n", outputs[i++].Message);
                Assert.AreEqual($"Speed: {player.Speed}\n", outputs[i++].Message);

                Assert.AreEqual(i, clears[clearIndex++]);

                foreach (var spell in spellsLearned)
                {
                    Assert.AreEqual($"{displayName} learned the '{spell.Description}' spell!\n", outputs[i++].Message);
                }

                if (spellsLearned.Count > 0)
                {
                    Assert.AreEqual(i, clears[clearIndex++]);
                }
            }
        }

        [Test]
        public void TestBattle_ReturnsVictoryOnVictory()
        {
            var player1DamageDealt = _humanPlayer1.Strength;
            _enemyPlayer1.SetHealth(player1DamageDealt);

            var player2DamageDealt = _humanPlayer2.MagicStrength + _fireball.Power;
            _enemyPlayer2.SetHealth(player2DamageDealt);

            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
            _mockChanceService.PushEventOccurs(true);
            _mockChanceService.PushEventOccurs(false);
            _mockChanceService.PushEventOccurs(true);
            _mockChanceService.PushEventOccurs(false);

            _input.Push(new List<string> { "fight", "attack", "1" });
            _input.Push(new List<string> { "fight", "magic", "fireball", "2" });

            BattleEndStatus status = _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(BattleEndStatus.Victory, status);
        }

        [Test]
        public void TestBattle_ReturnsDefeatOnDefeat()
        {
            var maxHumanSpeed = Math.Max(_humanPlayer1.Speed, _humanPlayer2.Speed);

            var enemy1DamageDealt = _enemyPlayer1.Strength - _humanPlayer1.Defense;
            _humanPlayer1.SetHealth(enemy1DamageDealt);
            _enemyPlayer1.SetSpeed(maxHumanSpeed + 1);

            var enemy2DamageDealt = _enemyPlayer2.Strength - _humanPlayer2.Defense;
            _humanPlayer2.SetHealth(enemy2DamageDealt);
            _enemyPlayer2.SetSpeed(maxHumanSpeed + 1);

            //set up the "attack hits" and "is critical" events, make sure they do connect but neither are crits
            _mockChanceService.PushEventOccurs(true);
            _mockChanceService.PushEventOccurs(false);
            _mockChanceService.PushEventOccurs(true);
            _mockChanceService.PushEventOccurs(false);

            //set up the targetting system on the enemy players
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "fight", "attack", "1" });
            _input.Push(new List<string> { "fight", "magic", "fireball", "2" });

            BattleEndStatus status = _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(BattleEndStatus.Defeat, status);
        }

        [Test]
        public void TestBattle_ReturnsRanOnRan()
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "run", "yes" });

            BattleEndStatus status = _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(BattleEndStatus.Ran, status);
        }

        [Test]
        public void TestBattle_ReturnsVictoryOnEnemyRan()
        {
            _enemyPlayer1.SetMove(_runawayMove);
            _enemyPlayer2.SetMove(_doNothingMove);

            _humanPlayer1.SetMove(_doNothingMove);
            _humanPlayer1.SetMoveTarget(_humanPlayer1);

            _humanPlayer2.SetMove(_doNothingMove);
            _humanPlayer2.SetMoveTarget(_humanPlayer2);

            BattleEndStatus status = _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(BattleEndStatus.Victory, status);
        }

        [Test]
        public void EventsRemovedWhenBattleFinished()
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventsOccur(0, 1);
            _input.Push(new List<string> { "run", "yes" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.IsNull(_enemyTeam.TeamDefeated);
            Assert.IsNull(_humanTeam.TeamDefeated);
        }

        [Test]
        public void EventsRemovedWhenBattleFinished_EnemyFighterKilled()
        {
            //just a sanity check assertion
            Assert.AreEqual(0, _humanPlayer1.CurrentExp);

            _enemyPlayer1.SetExpGiven(100);
            _enemyPlayer2.SetExpGiven(100);

            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "run", "y" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(0, _humanPlayer1.CurrentExp);

            foreach (var fighter in _enemyTeam.Fighters)
            {
                fighter.PhysicalDamage(fighter.MaxHealth);
            }

            Assert.AreNotEqual(200, _humanPlayer1.CurrentExp);
            Assert.AreEqual(0, _humanPlayer1.CurrentExp);
        }

        [Test]
        public void EventsRemovedWhenBattleFinished_ExpGained()
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "run", "y" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            _output.ClearMessages();

            _humanPlayer1.GainExp(10);

            var outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void EventsRemovedWhenBattleFinished_LeveledUp()
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "run", "y" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            _output.ClearMessages();

            _humanPlayer1.OnLeveledUp(new LeveledUpEventArgs(2, 1, 1, 1, 1, 1));

            var outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void EventsRemovedWhenBattleFinished_SpellLearned()
        {
            //set up the targetting system on the enemy players- required even though they don't get to attack,
            //since it's used during the setup stage
            _mockChanceService.PushWhichEventOccurs(0);
            _mockChanceService.PushWhichEventOccurs(1);

            _input.Push(new List<string> { "run", "y" });

            _battleManager.Battle(_humanTeam, _enemyTeam);

            _output.ClearMessages();

            _humanPlayer1.AddSpell(SpellFactory.GetSpell(MagicType.Fire, 1));

            var outputs = _output.GetOutputs();

            Assert.AreEqual(0, outputs.Length);
        }

        [Test]
        public void BattleManagerCorrectlyGeneratesSpecialMenuOptions_FromTerrainInteractables()
        {
            List<MenuAction> menuActions = new List<MenuAction>
            {
                new MenuAction("wait")
            };

            List<TerrainInteractable> terrainInteractables = new List<TerrainInteractable>
            {
                new TestTerrainInteractable(menuActions)
            };

            List<MockMenu> mockMenus = new List<MockMenu>
            {
                new MockMenu(),
                new MockMenu()
            };

            _menuFactory.SetMenu(MenuType.BattleMenu, mockMenus.OfType<IMenu>().ToArray());

            _input.Push("run", "y", "run", "y");

            _battleManager.Battle(_humanTeam, _enemyTeam, terrainInteractables);

            foreach (MockMenu menu in mockMenus)
            {
                List<MenuAction> builtMenuActions = (menu.InnerMenu as Menu)?.MenuActions ?? new List<MenuAction>();

                Assert.AreEqual(4, builtMenuActions.Count);

                foreach (MenuAction menuAction in builtMenuActions)
                {
                    Assert.Contains(menuAction, builtMenuActions);
                }
            }
        }
    }
}
