using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    internal class TeamEventTests
    {
        private HumanFighter _humanPlayer1, _humanPlayer2;
        private Team _humanTeam;
        private TestEnemyFighter _enemyPlayer1, _enemyPlayer2;
        private Team _enemyTeam;

        private EventLogger _logger;
        private MockInput _input;
        private MockOutput _output;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;

        private const string DefaultEnemyName = "Test";

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();
            _input = new MockInput();
            _output = new MockOutput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();

            TestFighterFactory.SetChanceService(_chanceService);

            _humanPlayer1 = (HumanFighter) TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, 1, "Ted");
            _humanPlayer2 = (HumanFighter) TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, 1, "Jed");
            _enemyPlayer1 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);
            _enemyPlayer2 = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, DefaultEnemyName);

            _enemyTeam = new Team(TestMenuManager.GetTestMenuManager(), _enemyPlayer1, _enemyPlayer2);
            _humanTeam = new Team(TestMenuManager.GetTestMenuManager(), _humanPlayer1, _humanPlayer2);

            _humanTeam.InitializeForBattle(_enemyTeam, _input, _output);
        }

        #region enemy team

        [Test]
        public void EnemyTeam_TeamDefeatedCorrectlyFiresAfter_AddingNewFighters()
        {
            _logger.Subscribe(EventType.TeamDefeated, _enemyTeam);

            _enemyTeam.Add((TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1));
            var fighters = _enemyTeam.Fighters;
            var count = fighters.Count;

            var i = 0;
            for (; i < count - 1; ++i)
            {
                fighters[i].PhysicalDamage(fighters[i].MaxHealth);
                Assert.AreEqual(0, _logger.Logs.Count);
            }

            fighters[i].PhysicalDamage(fighters[i].MaxHealth);
        
            var logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
        }

        [Test]
        public void EnemyTeam_TeamDefeatedCorrectlyFiresAfter_RemovingAFighter()
        {
            _logger.Subscribe(EventType.TeamDefeated, _enemyTeam);

            _enemyTeam.Remove(_enemyPlayer1);

            _enemyPlayer2.PhysicalDamage(_enemyPlayer2.MaxHealth);

            var logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
        }

        [Test]
        public void EnemyTeam_TestOnTeamDefeatedFiredWhenLastTeamMemberIsKilled_SingleFighterConstructorUsed()
        {
            var enemyFighter = (EnemyFighter)FighterFactory.GetFighter(FighterType.Goblin, 1);
            var enemyTeam = new Team(_menuManager, enemyFighter);

            _logger.Subscribe(EventType.TeamDefeated, enemyTeam);

            enemyFighter.PhysicalDamage(enemyFighter.MaxHealth);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
            Assert.AreEqual(enemyTeam, logs[0].Sender);
            var e = logs[0].E as TeamDefeatedEventArgs;
            Assert.That(e, Is.Not.Null);
            Assert.AreEqual(enemyTeam.Fighters.Count, e.Team.Fighters.Count);

            for (var i = 0; i < enemyTeam.Fighters.Count; ++i)
            {
                Assert.AreEqual(enemyTeam.Fighters[i], e.Team.Fighters[i]);
            }
        }

        [Test]
        public void EnemyTeam_TestOnTeamDefeatedFiredWhenLastTeamMemberIsKilled()
        {
            _logger.Subscribe(EventType.TeamDefeated, _enemyTeam);

            var fighters = _enemyTeam.Fighters;
            var count = fighters.Count;

            for (var i = 1; i <= count; ++i)
            {
                var fighter = fighters[i - 1];
                fighter.PhysicalDamage(fighter.MaxHealth);

                var logs = _logger.Logs;

                if (i < count)
                {
                    Assert.AreEqual(0, logs.Count);
                }
                else
                {
                    Assert.AreEqual(1, logs.Count);
                    Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
                    Assert.AreEqual(_enemyTeam, logs[0].Sender);
                    var e = logs[0].E as TeamDefeatedEventArgs;
                    Assert.That(e, Is.Not.Null);
                    Assert.AreEqual(_enemyTeam.Fighters.Count, e.Team.Fighters.Count);
                    
                    Assert.AreEqual(_enemyTeam, e.Team);
                }
            }
        }

        #endregion enemy team

        #region human team

        [Test]
        public void HumanTeam_TeamDefeatedCorrectlyFiresAfter_AddingNewFighters()
        {
            _logger.Subscribe(EventType.TeamDefeated, _humanTeam);

            _humanTeam.Add((TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1));
            var fighters = _humanTeam.Fighters;
            var count = fighters.Count;

            var i = 0;
            for (; i < count - 1; ++i)
            {
                fighters[i].PhysicalDamage(fighters[i].MaxHealth);
                Assert.AreEqual(0, _logger.Logs.Count);
            }

            fighters[i].PhysicalDamage(fighters[i].MaxHealth);

            var logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
        }

        [Test]
        public void HumanTeam_TeamDefeatedCorrectlyFiresAfter_RemovingAFighter()
        {
            _logger.Subscribe(EventType.TeamDefeated, _humanTeam);

            _humanTeam.Remove(_humanPlayer1);

            _humanPlayer2.PhysicalDamage(_humanPlayer2.MaxHealth);

            var logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
        }

        [Test]
        public void HumanTeam_TestOnTeamDefeatedFiredWhenLastTeamMemberIsKilled_SingleFighterConstructorUsed()
        {
            var humanFighter = (HumanFighter) FighterFactory.GetFighter(FighterType.HumanControlledPlayer, 1);
            var humanTeam = new Team(new TestMenuManager(new MockInput(), new MockOutput()), humanFighter);

            _logger.Subscribe(EventType.TeamDefeated, humanTeam);

            humanFighter.PhysicalDamage(humanFighter.MaxHealth);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
            Assert.AreEqual(humanTeam, logs[0].Sender);
            var e = logs[0].E as TeamDefeatedEventArgs;
            Assert.That(e, Is.Not.Null);
            Assert.AreEqual(humanTeam, e.Team);
        }

        [Test]
        public void HumanTeam_TestOnTeamDefeatedFiredWhenLastTeamMemberIsKilled()
        {
            _logger.Subscribe(EventType.TeamDefeated, _humanTeam);

            var fighters = _humanTeam.Fighters;
            var count = fighters.Count;

            for (var i = 1; i <= count; ++i)
            {
                var fighter = fighters[i - 1];
                fighter.PhysicalDamage(fighter.MaxHealth);

                var logs = _logger.Logs;

                if (i < count)
                {
                    Assert.AreEqual(0, logs.Count);
                }
                else
                {
                    Assert.AreEqual(1, logs.Count);
                    Assert.AreEqual(EventType.TeamDefeated, logs[0].Type);
                    Assert.AreEqual(_humanTeam, logs[0].Sender);
                    var e = logs[0].E as TeamDefeatedEventArgs;
                    Assert.That(e, Is.Not.Null);
                    Assert.AreEqual(_humanTeam, e.Team);
                }
            }
        }

        #endregion human team

        #region team

        [Test]
        public void FighterAddedEvent_FiredByAddMethod()
        {
            _logger.Subscribe(EventType.FighterAdded, _humanTeam);

            IFighter fighterToAdd = FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Fire);
            _humanTeam.Add(fighterToAdd);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.FighterAdded, log.Type);

            FighterAddedEventArgs e = log.E as FighterAddedEventArgs;
            
            Assert.NotNull(e);

            Assert.AreEqual(fighterToAdd, e.Fighter);
        }

        [Test]
        public void FighterAddedEvent_FiredByAddRangeMethod()
        {
            _logger.Subscribe(EventType.FighterAdded, _humanTeam);

            List<IFighter> fightersToAdd = new List<IFighter>
            {
                FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Fire),
                FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Lightning),
                FighterFactory.GetFighter(FighterType.Egg, 1, magicType: MagicType.Ice)
            };

            _humanTeam.AddRange(fightersToAdd);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(fightersToAdd.Count, logs.Count);

            List<FighterAddedEventArgs> eventArgs = logs.Select(log => log.E).OfType<FighterAddedEventArgs>().ToList();

            Assert.AreEqual(fightersToAdd.Count, eventArgs.Count);

            fightersToAdd.ForEach(f => Assert.NotNull(eventArgs.FirstOrDefault(args => args.Fighter == f)));
        }

        #endregion team
    }
}