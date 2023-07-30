using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class FighterEventTests
    {
        private TestHumanFighter _fighter;
        private TestEnemyFighter _enemy;
        private Spell _fireballSpell;
        private EventLogger _logger;

        [SetUp]
        public void Setup()
        {
            _fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _enemy = (TestEnemyFighter)TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1, "enemy");
            _logger = new EventLogger();
            _logger.SubscribeAll(_fighter);
            _logger.SubscribeAll(_enemy);
            _fireballSpell = SpellFactory.GetSpell(MagicType.Fire, 1);
            _fighter.AddSpell(_fireballSpell);
            _fighter.SetMana(_fireballSpell.Cost);
            _logger.ClearLogs();
        }

        [Test]
        public void OnPhysicalDamageTakenMethod_AppropriatelyFiresPhysicalDamageTakenEvent()
        {
            _fighter.OnDamageTaken(new PhysicalDamageTakenEventArgs(2));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(2, e.Damage);
        }

        [Test]
        public void OnMagicalDamageTakenMethod_AppropriatelyFiresMagicalDamageTakenEvent()
        {
            _fighter.OnMagicalDamageTaken(new MagicalDamageTakenEventArgs(2, MagicType.Fire));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.MagicalDamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as MagicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(2, e.Damage);
            Assert.AreEqual(MagicType.Fire, e.MagicType);
        }

        [Test]
        public void OnAttackSuccessfulMethod_AppropriatelyFiresAttackSuccessfulEvent()
        {
            _fighter.OnAttackSuccessful(new AttackSuccessfulEventArgs(_enemy, 2));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.AttackSuccessful, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as AttackSuccessfulEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(2, e.DamageDealt);
        }

        [Test]
        public void OnAttackMissedMethod_AppropriatelyFiresAttackMissedEvent()
        {
            _fighter.OnAttackMissed(new AttackMissedEventArgs(_enemy));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.AttackMissed, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as AttackMissedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_enemy, e.TargettedFoe);
        }

        [Test]
        public void OnCriticalAttackMethod_AppropriatelyFiresCriticalAttackEvent()
        {
            _fighter.OnCriticalAttack(new CriticalAttackEventArgs());

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.CriticalAttack, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as CriticalAttackEventArgs;
            Assert.That(e, Is.Not.Null);
        }

        [Test]
        public void OnManaSpentMethod_AppropriatelyFiresManaSpentEvent()
        {
            _fighter.OnManaLost(new ManaLostEventArgs(5));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaLost, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);

            var e = logs[0].E as ManaLostEventArgs;
            Assert.That(e, Is.Not.Null);

            if (e != null)
            {
                Assert.AreEqual(5, e.ManaSpent);
            }
        }

        [Test]
        public void OnManaRestoredMethod_AppropriatelyFiresManaSpentEvent()
        {
            _fighter.OnManaRestored(new ManaRestoredEventArgs(5));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaRestored, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);

            var e = logs[0].E as ManaRestoredEventArgs;
            Assert.That(e, Is.Not.Null);

            if (e != null)
            {
                Assert.AreEqual(5, e.ManaRestored);
            }
        }

        [Test]
        public void OnSpellSuccessfulMethod_AppropriatelyFiresSpellSuccessfullEvent()
        {
            _fighter.OnSpellSuccessful(new SpellSuccessfulEventArgs(_enemy, _fireballSpell, 10));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.SpellSuccessful, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as SpellSuccessfulEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_enemy, e.TargettedFoe);
            Assert.AreEqual(10, e.DamageDealt);
            Assert.AreEqual(_fireballSpell, e.Spell);
        }

        [Test]
        public void OnKilledMethod_AppropriatelyFiresKilledEvent()
        {
            _fighter.OnKilled(new KilledEventArgs());

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.Killed, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            KilledEventArgs e = logs[0].E as KilledEventArgs;
            Assert.That(e, Is.Not.Null);
        }

        [Test]
        public void OnEnemyKilledMethod_AppropriatelyFiresEnemyKilledEvent()
        {
            _enemy.OnEnemyKilled(new EnemyKilledEventArgs(_fighter));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.EnemyKilled, logs[0].Type);
            Assert.AreEqual(_enemy, logs[0].Sender);
            EnemyKilledEventArgs e = logs[0].E as EnemyKilledEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_fighter, e.Enemy);
        }

        [Test]
        public void OnTurnEndMethod_AppropriatelyFiresTurnEndEvent()
        {
            _fighter.OnTurnEnded(new TurnEndedEventArgs(_fighter));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.TurnEnded, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as TurnEndedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_fighter, e.Fighter);
        }

        [Test, Pairwise]
        public void RaiseStatMethod_AppropriatelyFiresEvent([Values(StatType.Strength, StatType.Defense, StatType.Speed)] StatType statToRaise,
            [Range(1,5)] int raiseAmount)
        {
            _fighter.RaiseStat(statToRaise, raiseAmount);

            Assert.AreEqual(1, _logger.Logs.Count);
        }

        [Test]
        public void DamageMethod_AppropriatelyRaisesEvents_WhenFighterSurvives()
        {
            _fighter.SetHealth(5);
            _fighter.PhysicalDamage(2);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(2, e.Damage);
        }

        [Test]
        public void DamageMethod_AppropriatelyRaisesEvents_WhenFighterDies()
        {
            _fighter.PhysicalDamage(_fighter.MaxHealth);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_fighter.MaxHealth, e.Damage);

            Assert.AreEqual(EventType.Killed, logs[1].Type);
            Assert.AreEqual(_fighter, logs[1].Sender);
            var e2 = logs[1].E as KilledEventArgs;
            Assert.That(e2, Is.Not.Null);
        }

        [Test]
        public void MagicalDamageMethod_AppropriatelyRaisesEvents_WhenFighterSurvives()
        {
            _fighter.SetHealth(4);
            _fighter.MagicalDamage(3, MagicType.Water);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.MagicalDamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as MagicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(3, e.Damage);
            Assert.AreEqual(MagicType.Water, e.MagicType);
        }

        [Test]
        public void MagicalDamageMethod_AppropriatelyRaisesEvents_WhenFighterDies()
        {
            _fighter.MagicalDamage(_fighter.MaxHealth + 2, MagicType.Ice);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            Assert.AreEqual(EventType.MagicalDamageTaken, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as MagicalDamageTakenEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(_fighter.MaxHealth, e.Damage);
            Assert.AreEqual(MagicType.Ice, e.MagicType);

            Assert.AreEqual(EventType.Killed, logs[1].Type);
            Assert.AreEqual(_fighter, logs[1].Sender);
            var e2 = logs[1].E as KilledEventArgs;
            Assert.That(e2, Is.Not.Null);
        }

        [Test]
        public void DrainManaMethod_AppropriatelyRaisesEvents_ManaDrainIsLessThanCurrentMana()
        {
            _fighter.SetMana(10);
            _fighter.DrainMana(8);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaLost, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ManaLostEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(8, e.ManaSpent);
            }
        }

        [Test]
        public void DrainManaMethod_AppropriatelyRaisesEvents_ManaDrainExceedsCurrentMana()
        {
            _fighter.SetMana(10);
            _fighter.DrainMana(25);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaLost, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ManaLostEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(10, e.ManaSpent);
            }
        }

        [Test]
        public void RestoreManaMethod_AppropriatelyRaisesEvents_ManaAmountIsLessThanMaxMana()
        {
            _fighter.SetMana(10, 0);
            _fighter.RestoreMana(8);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaRestored, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ManaRestoredEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(8, e.ManaRestored);
            }
        }

        [Test]
        public void RestoreManaMethod_AppropriatelyRaisesEvents_ManaAmountExceedsMaxMana()
        {
            _fighter.SetMana(10, 0);
            _fighter.RestoreMana(25);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ManaRestored, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ManaRestoredEventArgs;
            Assert.That(e, Is.Not.Null);
            if (e != null)
            {
                Assert.AreEqual(10, e.ManaRestored);
            }
        }

        [Test]
        public void AttackMethod_AppropriatelyRaisesEvents_WhenEnemySurvives()
        {
            var expectedDamage = _fighter.Strength - _enemy.Defense;
            _enemy.SetHealth(expectedDamage + 1);

            _fighter.Attack(_enemy);

            var logs = _logger.Logs;

            Assert.AreEqual(2, logs.Count);

            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(_enemy, logs[0].Sender);
            PhysicalDamageTakenEventArgs e1 = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e1);
            Assert.AreEqual(expectedDamage, e1.Damage);

            Assert.AreEqual(EventType.AttackSuccessful, logs[1].Type);
            Assert.AreEqual(_fighter, logs[1].Sender);
            AttackSuccessfulEventArgs e2 = logs[1].E as AttackSuccessfulEventArgs;
            Assert.NotNull(e2);
            Assert.AreEqual(expectedDamage, e2.DamageDealt);
        }

        [Test]
        public void AttackMethod_AppropriatelyRaisesEvents_WhenCriticalHit()
        {
            var expectedDamage = (_fighter.Strength - _enemy.Defense) * 2;
            _enemy.SetHealth(expectedDamage + 1);

            _fighter.Attack(_enemy, 2.0, true);

            var logs = _logger.Logs;

            Assert.AreEqual(3, logs.Count);

            Assert.AreEqual(EventType.CriticalAttack, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e1 = logs[0].E as CriticalAttackEventArgs;
            Assert.That(e1, Is.Not.Null);

            Assert.AreEqual(EventType.DamageTaken, logs[1].Type);
            Assert.AreEqual(_enemy, logs[1].Sender);
            var e2 = logs[1].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e2);
            Assert.AreEqual(expectedDamage, e2.Damage);

            Assert.AreEqual(EventType.AttackSuccessful, logs[2].Type);
            Assert.AreEqual(_fighter, logs[2].Sender);
            var e3 = logs[2].E as AttackSuccessfulEventArgs;
            Assert.NotNull(e3);
            Assert.AreEqual(expectedDamage, e3.DamageDealt);
        }

        [Test]
        public void AttackMethod_AppropriatelyRaisesEvents_WhenEnemyDies()
        {
            _fighter = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1, "Ted");
            _logger.SubscribeAll(_fighter);

            _fighter.SetStrength(_enemy.MaxHealth + 1);

            _fighter.Attack(_enemy);

            var logs = _logger.Logs;

            Assert.AreEqual(4, logs.Count);

            //first event, damage taken
            Assert.AreEqual(EventType.DamageTaken, logs[0].Type);
            Assert.AreEqual(_enemy, logs[0].Sender);
            PhysicalDamageTakenEventArgs e1 = logs[0].E as PhysicalDamageTakenEventArgs;
            Assert.NotNull(e1);
            Assert.AreEqual(_fighter.Strength, e1.Damage);

            //second event, killed - fired by enemy
            Assert.AreEqual(EventType.Killed, logs[1].Type);
            Assert.AreEqual(_enemy, logs[1].Sender);
            KilledEventArgs e2 = logs[1].E as KilledEventArgs;
            Assert.That(e2, Is.Not.Null);

            //third event, damageDealt to enemy
            Assert.AreEqual(EventType.AttackSuccessful, logs[2].Type);
            Assert.AreEqual(_fighter, logs[2].Sender);
            AttackSuccessfulEventArgs e3 = logs[2].E as AttackSuccessfulEventArgs;
            Assert.NotNull(e3);
            Assert.AreEqual(_enemy.MaxHealth, e3.DamageDealt);

            //fourth event, killed enemy
            Assert.AreEqual(EventType.EnemyKilled, logs[3].Type);
            Assert.AreEqual(_fighter, logs[3].Sender);
            EnemyKilledEventArgs e4 = logs[3].E as EnemyKilledEventArgs;
            Assert.NotNull(e4);
            Assert.AreEqual(_enemy, e4.Enemy);
        }

        [Test]
        public void AddStatus_AppropriatelyRaisesEvent()
        {
            StatMultiplierStatus status = new StatMultiplierStatus(2, StatType.Speed, 1.5);

            _fighter.AddStatus(status);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(_fighter, log.Sender);
            Assert.AreEqual(EventType.StatusAdded, log.Type);

            StatusAddedEventArgs args = log.E as StatusAddedEventArgs;

            Assert.NotNull(args);
            Assert.AreEqual(status, args.Status);
        }

        [Test]
        public void SetBattleShieldMethod_AppropriatelyRaisesBattleShieldAddedEvent()
        {
            IBattleShield shield = new IronBattleShield(1, 0, 0);

            _fighter.SetBattleShield((BattleShield) shield);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];
            Assert.AreEqual(EventType.ShieldAdded, log.Type);

            ShieldAddedEventArgs e = log.E as ShieldAddedEventArgs;
            Assert.NotNull(e);
            Assert.IsTrue(shield.AreEqual(e.BattleShield));
        }
    }
}
