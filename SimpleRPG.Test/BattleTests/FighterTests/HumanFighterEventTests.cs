using System.Collections.Generic;
using NUnit.Framework;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.FighterTests
{
    [TestFixture]
    class HumanFighterEventTests
    {
        private HumanFighter _fighter;
        private EventLogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new EventLogger();

            _fighter = (HumanFighter)TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, 1, "Ted");
            _logger.SubscribeAll(_fighter);
        }

        #region On____MethodFirestEvents

        [Test]
        public void OnExpGainedMethod_AppropriatelyFiresExpGainedEvent()
        {
            _fighter.OnExpGained(new ExpGainedEventArgs(10));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ExpGained, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ExpGainedEventArgs;
            Assert.That(e, Is.Not.Null);
            Assert.AreEqual(10, e.AmountGained);
        }

        [Test]
        public void OnLeveledUp_AppropriatelyFiresLeveledUpEvent()
        {
            const int level = 2;
            const int health = 5;
            const int mana = 2;
            const int strength = 1;
            const int defense = 1;
            const int speed = 1;

            _fighter.OnLeveledUp(new LeveledUpEventArgs(level, health, mana, strength, defense, speed));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.LeveledUp, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as LeveledUpEventArgs;
            Assert.That(e, Is.Not.Null);

            if (e != null)
            {
                Assert.AreEqual(level, e.NewLevel);
                Assert.AreEqual(health, e.HealthBoost);
                Assert.AreEqual(mana, e.ManaBoost);
                Assert.AreEqual(strength, e.StrengthBoost);
                Assert.AreEqual(defense, e.DefenseBoost);
                Assert.AreEqual(speed, e.SpeedBoost);
            }
        }

        [Test]
        public void OnLearnedSpell_AppropriatelyFiresLearnedSpellEvent()
        {
            var spell = SpellFactory.GetSpell(MagicType.Fire, 1);
            _fighter.OnSpellsLearned(new SpellsLearnedEventArgs(spell));

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.SpellLearned, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as SpellsLearnedEventArgs;
            Assert.That(e, Is.Not.Null);

            if (e != null)
            {
                Assert.AreEqual(spell, e.SpellsLearned[0]);
            }
        }

        #endregion On____MethodFirestEvents

        [Test]
        public void GainExpMethod_AppropriatelyRaisesEvents_WhenFighterDoesNotLevelUp()
        {
            var expAmount = LevelUpManager.GetExpForLevel(_fighter.Level + 1) - 1;
            _fighter.GainExp(expAmount);
        
            var logs = _logger.Logs;
        
            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.ExpGained, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as ExpGainedEventArgs;
            Assert.That(e, Is.Not.Null);
            Assert.AreEqual(expAmount, e.AmountGained);
        }

        [Test]
        public void GainExpMethod_AppropriatelyRaisesEvents_WhenFighterGainsOneLevel([Range(1, 4)] int initialLevel)
        {
            int nextLevel = initialLevel + 1;
            _fighter = (HumanFighter)TestFighterFactory.GetFighter(TestFighterType.HumanControlledPlayer, initialLevel, "Ted");
            _logger.SubscribeAll(_fighter);

            var expAmount = LevelUpManager.GetExpForLevel(nextLevel);
            var spellsLearned = SpellFactory.GetSpellsByLevel<HumanFighter>(nextLevel, nextLevel);
            _fighter.GainExp(expAmount);

            var logs = _logger.Logs;

            int expectedLogCount = 2 + (spellsLearned.Count > 0 ? 1 : 0);

            Assert.AreEqual(expectedLogCount, logs.Count); //exp gained, level up, potentially spells learned

            var i = 0;
            var log = logs[i++];
            Assert.AreEqual(EventType.ExpGained, log.Type);
            Assert.AreEqual(_fighter, log.Sender);
            var e1 = log.E as ExpGainedEventArgs;
            Assert.That(e1, Is.Not.Null);
            if (e1 != null)
            {
                Assert.AreEqual(expAmount, e1.AmountGained);
            }

            log = logs[i++];
            Assert.AreEqual(EventType.LeveledUp, log.Type);
            Assert.AreEqual(_fighter, log.Sender);
            var e2 = log.E as LeveledUpEventArgs;
            Assert.That(e2, Is.Not.Null);
            if (e2 != null)
            {
                Assert.AreEqual(nextLevel, e2.NewLevel);
                Assert.AreEqual(LevelUpManager.HealthBoostByLevel(nextLevel), e2.HealthBoost);
                Assert.AreEqual(LevelUpManager.ManaBoostByLevel(nextLevel), e2.ManaBoost);
                Assert.AreEqual(LevelUpManager.StrengthBoostByLevel(nextLevel), e2.StrengthBoost);
                Assert.AreEqual(LevelUpManager.DefenseBoostByLevel(nextLevel), e2.DefenseBoost);
                Assert.AreEqual(LevelUpManager.SpeedBoostByLevel(nextLevel), e2.SpeedBoost);
            }

            if (spellsLearned.Count > 0)
            {
                log = logs[i];

                Assert.AreEqual(EventType.SpellLearned, log.Type);
                Assert.AreEqual(_fighter, log.Sender);
                var e3 = log.E as SpellsLearnedEventArgs;
                Assert.That(e3, Is.Not.Null);
                if (e3 == null)
                {
                    return;
                }

                foreach (var spell in spellsLearned)
                {
                    Assert.IsTrue(e3.SpellsLearned.Contains(spell));
                }
            }
        }

        [Test]
        public void GainExpMethod_AppropriatelyRaisesEvents_WhenFighterGainsMultipleLevels([Values(2, 3)] int numberOfLevelsToIncrease)
        {
            int currentLevel = _fighter.Level;
            int nextLevel = currentLevel + 1;
            int targetLevel = currentLevel + numberOfLevelsToIncrease;
            var expAmount = LevelUpManager.GetExpForLevel(targetLevel);

            int expectedLogsLength = numberOfLevelsToIncrease + 1; //expGained event, one for each level up

            int i = 0;
            for (int level = currentLevel; i < numberOfLevelsToIncrease; ++i, ++level)
            {
                var spellsLearned = SpellFactory.GetSpellsByLevel<HumanFighter>(level + 1, level + 1);
                if (spellsLearned.Count > 0) //SpellsLeaned event will fire
                {
                    expectedLogsLength++;
                }
            }
            
            _fighter.GainExp(expAmount);
            
            var logs = _logger.Logs;
            Assert.AreEqual(expectedLogsLength, logs.Count);

            i = 0;
            var log = logs[i++];
            Assert.AreEqual(EventType.ExpGained, log.Type);
            Assert.AreEqual(_fighter, log.Sender);

            ExpGainedEventArgs e1 = log.E as ExpGainedEventArgs;
            Assert.That(e1, Is.Not.Null);
            if (e1 != null)
            {
                Assert.AreEqual(expAmount, e1.AmountGained);
            }

            while(i < expectedLogsLength)
            {
                log = logs[i++];
                Assert.AreEqual(EventType.LeveledUp, log.Type);
                Assert.AreEqual(_fighter, log.Sender);
                LeveledUpEventArgs e2 = log.E as LeveledUpEventArgs;
                Assert.That(e2, Is.Not.Null);
                if (e2 != null)
                {
                    Assert.AreEqual(nextLevel, e2.NewLevel);
                    Assert.AreEqual(LevelUpManager.HealthBoostByLevel(nextLevel), e2.HealthBoost);
                    Assert.AreEqual(LevelUpManager.ManaBoostByLevel(nextLevel), e2.ManaBoost);
                    Assert.AreEqual(LevelUpManager.StrengthBoostByLevel(nextLevel), e2.StrengthBoost);
                    Assert.AreEqual(LevelUpManager.DefenseBoostByLevel(nextLevel), e2.DefenseBoost);
                    Assert.AreEqual(LevelUpManager.SpeedBoostByLevel(nextLevel), e2.SpeedBoost);
                }

                List<Spell> spellsLearned = SpellFactory.GetSpellsByLevel<HumanFighter>(nextLevel, nextLevel++);

                if (spellsLearned.Count > 0)
                {
                    log = logs[i++];

                    foreach (var spell in spellsLearned)
                    {
                        Assert.AreEqual(EventType.SpellLearned, log.Type);
                        Assert.AreEqual(_fighter, log.Sender);
                        var e3 = log.E as SpellsLearnedEventArgs;
                        Assert.That(23, Is.Not.Null);
                        if (e3 != null)
                        {
                            Assert.IsTrue(e3.SpellsLearned.Contains(spell));
                        }
                    }
                }
            }
        }

        [Test]
        public void AddSpellMethod_AppropriatelyRaisesEvents()
        {
            var spell = SpellFactory.GetSpell(MagicType.Fire, 1);
            _fighter.AddSpell(spell);

            var logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);
            Assert.AreEqual(EventType.SpellLearned, logs[0].Type);
            Assert.AreEqual(_fighter, logs[0].Sender);
            var e = logs[0].E as SpellsLearnedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(spell, e.SpellsLearned[0]);
        }

        [Test]
        public void AddMove_AppropriatelyRaisesEvent_NonSpecialMove()
        {
            BattleMove doNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);
            _fighter.AddMove(doNothingMove);

            List<EventLog> logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];
            Assert.AreEqual(EventType.MoveLearned, log.Type);

            MoveLearnedEventArgs e = log.E as MoveLearnedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(doNothingMove, e.Move);
        }

        [Test]
        public void AddMove_AppropriatelyRaisesEvent_SpecialMove()
        {
            BattleMove shieldMove = MoveFactory.Get(BattleMoveType.Shield, "iron shield");
            _fighter.AddMove(shieldMove);

            List<EventLog> logs = _logger.Logs;
            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];
            Assert.AreEqual(EventType.MoveLearned, log.Type);

            MoveLearnedEventArgs e = log.E as MoveLearnedEventArgs;
            Assert.NotNull(e);
            Assert.AreEqual(shieldMove, e.Move);
        }

        [Test]
        public void AddStatBonus_AppropriatelyRasiesEvent([Values]StatType statType, [Values(1, 5)] int bonusAmount, [Values] bool isSecret)
        {
            _fighter.AddStatBonus(statType, bonusAmount, isSecret);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.StatBonusApplied, log.Type);

            StatBonusAppliedEventArgs e = log.E as StatBonusAppliedEventArgs;

            Assert.NotNull(e);
            Assert.AreEqual(statType, e.Stat);
            Assert.AreEqual(bonusAmount, e.BonusAmount);
            Assert.AreEqual(isSecret, e.IsSecretStatBonus);
        }

        [Test]
        public void AddStatBonus_DefaultParameters_NotSecretStatBoost()
        {
            _fighter.AddStatBonus(StatType.Strength, 1);
            
            EventLog log = _logger.Logs[0];
            StatBonusAppliedEventArgs e = log.E as StatBonusAppliedEventArgs;

            Assert.NotNull(e);
            Assert.IsFalse(e.IsSecretStatBonus);
        }

        [Test, Pairwise]
        public void AddMagicBonus_AppropriatelyRasiesEvent([Values]MagicStatType magicStatType, [Values]MagicType magicType, [Values(1, 5)] int bonusAmount, [Values] bool isSecret)
        {
            _fighter.AddMagicBonus(magicStatType, magicType, bonusAmount, isSecret);

            List<EventLog> logs = _logger.Logs;

            Assert.AreEqual(1, logs.Count);

            EventLog log = logs[0];

            Assert.AreEqual(EventType.MagicBonusApplied, log.Type);

            MagicBonusAppliedEventArgs e = log.E as MagicBonusAppliedEventArgs;

            Assert.NotNull(e);
            Assert.AreEqual(magicStatType, e.MagicStatType);
            Assert.AreEqual(magicType, e.MagicType);
            Assert.AreEqual(bonusAmount, e.BonusAmount);
            Assert.AreEqual(isSecret, e.IsSecretStatBonus);
        }

        [Test]
        public void AddMagicBonus_DefaultParameters_NotSecretStatBoost()
        {
            _fighter.AddStatBonus(StatType.Strength, 1);

            EventLog log = _logger.Logs[0];
            StatBonusAppliedEventArgs e = log.E as StatBonusAppliedEventArgs;

            Assert.NotNull(e);
            Assert.IsFalse(e.IsSecretStatBonus);
        }
    }
}
