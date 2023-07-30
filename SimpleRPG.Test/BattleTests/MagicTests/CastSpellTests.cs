using System;
using NUnit.Framework;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Test.MockClasses;
using SimpleRPG.Test.MockClasses.Enemies;
using SimpleRPG.Test.MockClasses.Events;
using SimpleRPG.Test.MockClasses.Factories;

namespace SimpleRPG.Test.BattleTests.MagicTests
{
    [TestFixture]
    public class CastSpellTests
    {
        private TestHumanFighter _human;
        private TestEnemyFighter _enemy;
        private TestTeam _humanTeam;
        private Team _enemyTeam;
        
        private DoNothingMove doNothing;
        private Spell _fireballSpell;

        private MockOutput _output;
        private MockInput _input;
        private TestMenuManager _menuManager;
        private MockChanceService _chanceService;
        private TestBattleManager _battleManager;
        private EventLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _output = new MockOutput();
            _input = new MockInput();
            _menuManager = new TestMenuManager(_input, _output);
            _chanceService = new MockChanceService();
            _battleManager = new TestBattleManager(_chanceService, _input, _output);
            _logger = new EventLogger();

            _human = (TestHumanFighter)TestFighterFactory.GetFighter(TestFighterType.TestHuman, 1);
            _enemy = (TestEnemyFighter) TestFighterFactory.GetFighter(TestFighterType.TestEnemy, 1);
            _humanTeam = new TestTeam(_human);
            _enemyTeam = new Team(_menuManager, _enemy);

            _human.SetSpeed(10);
            _enemy.SetSpeed(0);

            doNothing = new DoNothingMove();
            _enemy.SetMove(doNothing);

            _fireballSpell = SpellFactory.GetSpell(MagicType.Fire, 1);
        }

        //1)	Can the caster cast this spell? - done
            //a.  have they learned it? - done
            //b.	Do they have a status that prevents spell casting? - done
            //c.	do they have the mana? - done
                //i.Are there any statuses that effect the cost of that spell? - done
        //2)	What is the strength of the spell? - done
            //a.What is the player’s (inherent) bonus for that kind of spell? - done
            //b.What are the other temporary factors that would alter its power? - done
        //3)	Is the spell reflected?
            //0.Basic case- done
            //a.Is it possibly strengthened? Is there a “bounce back at twice the power” option?
            //b.What if it's reflected by both caster and target?
        //5)	How much damage is dealt?
            //a.What is the target’s (inherent) defense against that kind of spell? -done
            //b.What are the other temporary factors that would alter its damage (bonuses, statuses)? - statuses needs to be refactored 
            //c.Does the target have any status (weak, resistant, immune, absorbHP, absorbMP) that would affect damage dealt? - absorbs not implemented
                //i. What if they have multiple affinities? They're naturally weak but they have the ring of Butts that also grants them immunity?

        #region can the user cast the spell?
             
        [Test]
        public void CastSpell_CorrectlyChecksUserHasLearnedSelectedSpell()
        {
            var spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, 0, 5);

            _human.SetDeathOnTurnEndEvent();
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            Assert.False(_human.Spells.Contains(spell));

            Assert.Throws<ArgumentException>(() => _battleManager.Battle(_humanTeam, _enemyTeam));
        }

        [Test]
        public void CastSpell_CorrectlyChecksUserHasEnoughMana([Values(4, 8)] int spellCost)
        {
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);

            int expectedMana = spellCost - 1;

            _human.SetMana(spellCost, expectedMana);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedMana, _human.CurrentMana);
        }

        [Test]
        public void CastSpell_CorrectlyChecksUserHasEnoughMana_SpellCostMultiplierStatus([Values(4, 8)] int spellCost,
            [Values(2, 3)] int costMultiplier1,
            [Values(null, 4)] int? costMultiplier2)
        {
            SpellCostMultiplierStatus status1 = new SpellCostMultiplierStatus(1, costMultiplier1);
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);

            int realCost = spellCost * costMultiplier1;

            if (costMultiplier2.HasValue)
            {
                realCost *= costMultiplier2.Value;
                SpellCostMultiplierStatus status2 = new SpellCostMultiplierStatus(1, costMultiplier2.Value);
                _human.AddStatus(status2);
            }

            int availableMana = realCost - 1;

            _human.SetMana(realCost, availableMana);
            _human.AddStatus(status1);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(availableMana, _human.CurrentMana);
        }

        [Test]
        public void CastSpell_CorrectlyChecksUsersMagicHasNotBeenSealed()
        {
            const int spellCost = 5;

            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);
            MagicSealedStatus status = new MagicSealedStatus(1);

            _human.SetMana(spellCost);
            _human.AddStatus(status);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(spellCost, _human.CurrentMana);
        }

        #endregion

        #region spell cost calculations

        [Test]
        public void CastSpell_CorrectlySpendsCastersMana([Values(1, 4, 8)] int spellCost)
        {
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);
            int expectedRemainingMana = 100 - spellCost;

            _human.SetMana(100);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedRemainingMana, _human.CurrentMana);
        }

        [Test]
        public void CastSpell_CorrectlySpendsCastersMana_SpellCostMultiplierStatus(
            [Values(1, 4, 8)] int spellCost,
            [Values(2.0, 3.0)] double multiplier,
            [Values(false, true)] bool teamStatus)
        {
            SpellCostMultiplierStatus status = new SpellCostMultiplierStatus(1, multiplier);
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);
            int expectedRemainingMana = 100 - (int)(spellCost * multiplier);

            _human.SetMana(100);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            if (teamStatus)
            {
                _humanTeam.AddStatus(status);
            }
            else
            {
                _human.AddStatus(status);
            }
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedRemainingMana, _human.CurrentMana);
        }

        [Test]
        public void CastSpell_CorrectlySpendsCastersMana_MultipleSpellCostMultiplierStatus(
            [Values(1, 4, 8)] int spellCost,
            [Values(2.0, 3.0)] double firstMultiplier,
            [Values(1.0, 4.0)] double secondMultiplier,
            [Values(false, true)] bool teamStatus)
        {
            SpellCostMultiplierStatus firstStatus = new SpellCostMultiplierStatus(1, firstMultiplier);
            SpellCostMultiplierStatus secondStatus = new SpellCostMultiplierStatus(1, secondMultiplier);
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);
            int expectedRemainingMana = 100 - (int)(spellCost * firstMultiplier * secondMultiplier);

            _human.SetMana(100);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);
            
            if (teamStatus)
            {
                _humanTeam.AddStatus(firstStatus);
                _humanTeam.AddStatus(secondStatus);
            }
            else
            {
                _human.AddStatus(firstStatus);
                _human.AddStatus(secondStatus);
            }

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedRemainingMana, _human.CurrentMana);
        }

        [Test]
        public void CastSpell_CorrectlySpendsCastersMana_BothTeamAndIndividualSpellCostMultiplierStatuses(
            [Values(2, 4, 8)] int spellCost,
            [Values(1.5, 3.0)] double firstMultiplier,
            [Values(1.0, 2.0)] double secondMultiplier)
        {
            SpellCostMultiplierStatus firstStatus = new SpellCostMultiplierStatus(1, firstMultiplier);
            SpellCostMultiplierStatus secondStatus = new SpellCostMultiplierStatus(1, secondMultiplier);
            Spell spell = new Spell("foo", MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, spellCost, 5);
            int expectedRemainingMana = 100 - (int)(spellCost * firstMultiplier * secondMultiplier);

            _human.SetMana(100);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _human.AddStatus(firstStatus);
            _humanTeam.AddStatus(secondStatus);

            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedRemainingMana, _human.CurrentMana);
        }

        #endregion

        #region reflect spell logic

        [Test]
        public void CastSpell_CorrectlyReflectsSpells([Values(MagicType.Fire, MagicType.Ice, MagicType.Lightning)] MagicType spellType,
            [Values(true, false)] bool reflectAll)
        {
            const int spellCost = 5;
            const int spellPower = 5;
            const int expectedRemainingHealth = 100 - spellPower;

            Spell spell = new Spell("foo", spellType, SpellType.Attack, TargetType.SingleEnemy, spellCost, spellPower);
            BattleMove runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

            MagicType reflectType = reflectAll ? MagicType.All : spellType;
            ReflectStatus status = new ReflectStatus(1, reflectType);

            _human.SetHealth(100);
            _human.SetMana(spellCost);
            _human.SetMagicStrength(0);
            _human.SetMagicBonus(spellType, 0);
            _human.SetMagicResistance(0);
            _human.SetResistanceBonus(spellType, 0);
            _human.AddSpell(spell);
            _human.SetMove(spell, 1);
            _human.SetMove(runawayMove);
            _human.SetMoveTarget(_enemy);

            _enemy.AddStatus(status);
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth, "enemy should still have full health, the spell should have been reflected!");
            Assert.AreEqual(expectedRemainingHealth, _human.CurrentHealth, "the human should have lost 5 health from being hit by their own spell!");
        }

        [Test, Sequential]
        public void CastSpell_CorrectlyDoesNotReflectsSpells_ReflectStatus_DoesNotMatchAttackingType(
            [Values(MagicType.Fire, MagicType.Ice, MagicType.Lightning)] MagicType spellType,
            [Values(MagicType.Wind, MagicType.Earth, MagicType.Water)] MagicType reflectType)
        {
            const int spellCost = 5;
            const int spellPower = 5;
            const int expectedRemainingHealth = 100 - spellPower;

            Spell spell = new Spell("foo", spellType, SpellType.Attack, TargetType.SingleEnemy, spellCost, spellPower);
            BattleMove runawayMove = MoveFactory.Get(BattleMoveType.Runaway);
            ReflectStatus status = new ReflectStatus(1, reflectType);

            _human.SetHealth(100);
            _human.SetMana(spellCost);
            _human.SetMagicStrength(0);
            _human.SetMagicBonus(spellType, 0);
            _human.SetMagicResistance(0);
            _human.SetResistanceBonus(spellType, 0);
            _human.AddSpell(spell);
            _human.SetMove(spell, 1);
            _human.SetMove(runawayMove);
            _human.SetMoveTarget(_enemy);

            _enemy.AddStatus(status);
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_human.MaxHealth, _human.CurrentHealth, "human should still have full health, the spell should not have been reflected!");
            Assert.AreEqual(expectedRemainingHealth, _enemy.CurrentHealth, "the enemy should have lost 5 health from being hit by the spell!");
        }

        [Test]
        public void CastSpell_CorrectlyReflectsSpells_WithCorrectMultiplier([Values(MagicType.Water, MagicType.Earth, MagicType.Wind)] MagicType spellType,
            [Values(true, false)] bool reflectAll)
        {
            const int spellCost = 5;
            const int spellPower = 5;
            const int expectedRemainingHealth = 100 - (spellPower * 2);

            Spell spell = new Spell("foo", spellType, SpellType.Attack, TargetType.SingleEnemy, spellCost, spellPower);
            BattleMove runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

            MagicType reflectType = reflectAll ? MagicType.All : spellType;
            ReflectStatus status = new ReflectStatus(1, reflectType, 2);

            _human.SetHealth(100);
            _human.SetMana(spellCost);
            _human.AddSpell(spell);
            _human.SetMove(spell, 1);
            _human.SetMove(runawayMove);
            _human.SetMoveTarget(_enemy);

            _enemy.AddStatus(status);
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth, "enemy should still have full health, the spell should have been reflected!");
            Assert.AreEqual(expectedRemainingHealth, _human.CurrentHealth, $"the human should have lost {spellPower * 2} health from being hit by their own spell reflected at double the power!");
        }

        [Test]
        public void CastSpell_SpellsVanishIfReflectedTwice([Values(MagicType.Fire, MagicType.Ice, MagicType.Lightning)] MagicType spellType,
            [Values(true, false)] bool reflectAll)
        {
            const int spellCost = 5;
            const int spellPower = 5;

            Spell spell = new Spell("foo", spellType, SpellType.Attack, TargetType.SingleEnemy, spellCost, spellPower);
            BattleMove runawayMove = MoveFactory.Get(BattleMoveType.Runaway);

            MagicType reflectType = reflectAll ? MagicType.All : spellType;
            ReflectStatus status = new ReflectStatus(1, reflectType);

            _human.SetHealth(100);
            _human.SetMana(spellCost);
            _human.AddStatus(status);
            _human.AddSpell(spell);
            _human.SetMove(spell, 1);
            _human.SetMove(runawayMove);
            _human.SetMoveTarget(_enemy);

            _enemy.AddStatus(status);
            _enemy.SetHealth(100);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(_enemy.MaxHealth, _enemy.CurrentHealth, "enemy should still have full health, the spell should have disappeared without hurting anyone!");
            Assert.AreEqual(_human.MaxHealth, _human.CurrentHealth, "the human should still have full health, the spell should have disappeared without hurting anyone");
        }

        #endregion

        /// <summary>
        /// Verifies damage output when caster has a particular magic strength, and a particular magic bonus, 
        /// and enemy has no resistance or bonuses/statuses
        /// </summary>
        [Test, Pairwise]
        public void CastSpell_CalculatesCorrectDamage(
            [Values(40, 80)] int magicStrength, 
            [Values(0, 6)] int magicBonus, 
            [Values(null, 0.25, 1.5)] double? individualMagicStrengthMultiplier,
            [Values(null, 0.5, 2.0)] double? teamMagicStrengthMultiplier,
            [Values(0, 4)] int resistance,
            [Values(0, 2)] int resistanceBonus,
            [Values(FighterMagicRelationshipType.Weak, FighterMagicRelationshipType.Resistant, FighterMagicRelationshipType.Immune)] FighterMagicRelationshipType relationshipType,
            [Values(MagicType.Ice, MagicType.All)] MagicType affinityMagicType,
            [Values(MagicType.Ice, MagicType.Lightning)] MagicType spellMagicType,
            [Values(false, true)] bool useAllTypeMultiplier
            )
        {
            const int maxHealth = 500;
            const int spellStrength = 10;
            int totalStrength = magicStrength + spellStrength + magicBonus;

            totalStrength = ApplyNullableMultiplier(totalStrength, individualMagicStrengthMultiplier);
            totalStrength = ApplyNullableMultiplier(totalStrength, teamMagicStrengthMultiplier);

            MagicType magicMultiplierType = (useAllTypeMultiplier) ? MagicType.All : spellMagicType;

            if (individualMagicStrengthMultiplier != null)
            {
                MagicMultiplierStatus status = new MagicMultiplierStatus(1, magicMultiplierType, individualMagicStrengthMultiplier.Value);
                _human.AddStatus(status);
            }

            if (teamMagicStrengthMultiplier != null)
            {
                MagicMultiplierStatus status = new MagicMultiplierStatus(1, magicMultiplierType, teamMagicStrengthMultiplier.Value);
                _humanTeam.AddStatus(status);
            }

            int totalResistance = resistance + resistanceBonus;

            int total = totalStrength - totalResistance;

            switch (relationshipType)
            {
                case FighterMagicRelationshipType.Immune:
                    _enemy.SetElementalImmunity(affinityMagicType);
                    if (affinityMagicType == MagicType.All || affinityMagicType == spellMagicType)
                    {
                        total = 0;
                    }
                    break;
                case FighterMagicRelationshipType.Resistant:
                    _enemy.SetElementalResistance(affinityMagicType);
                    if (affinityMagicType == MagicType.All || affinityMagicType == spellMagicType)
                    {
                        total /= 2;
                    }
                    break;
                case FighterMagicRelationshipType.Weak:
                    _enemy.SetElementalWeakness(affinityMagicType);
                    if (affinityMagicType == MagicType.All || affinityMagicType == spellMagicType)
                    {
                        total *= 2;
                    }
                    break;
            }

            int expectedRemainingHealth = (total >= maxHealth) ? 0 : maxHealth - total;

            var spell = new Spell("foo", spellMagicType, SpellType.Attack, TargetType.SingleEnemy, 0, spellStrength);

            _human.SetMagicStrength(magicStrength);
            _human.SetMagicBonus(spellMagicType, magicBonus);
            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(spell);
            _human.SetMove(spell);
            _human.SetMoveTarget(_enemy);

            _enemy.SetHealth(500);
            _enemy.SetMagicResistance(resistance);
            _enemy.SetResistanceBonus(spellMagicType, resistanceBonus);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            Assert.AreEqual(expectedRemainingHealth, _enemy.CurrentHealth);
        }

        private int ApplyNullableMultiplier(int magicStrength, double? multiplier)
        {
            int ret = magicStrength;

            if (multiplier != null)
            {
                ret = (int) (multiplier*magicStrength);
            }

            return ret;
        }

        [Test]
        public void CastMethod_AppropriatelyRaisesEvents([Values(false, true)] bool enemyDies)
        {
            var spellDamage = _fireballSpell.Power + _human.MagicStrength;
            int enemyHealth = spellDamage + (enemyDies ? 0 : 1);
            _enemy.SetHealth(enemyHealth); 

            _human.SetDeathOnTurnEndEvent();
            _human.AddSpell(_fireballSpell);
            _human.SetMana(_fireballSpell.Cost);
            _human.SetMove(_fireballSpell);
            _human.SetMoveTarget(_enemy);

            _logger.Subscribe(EventType.ManaLost, _human);
            _logger.Subscribe(EventType.SpellSuccessful, _human);
            _logger.Subscribe(EventType.EnemyKilled, _human);

            _logger.Subscribe(EventType.MagicalDamageTaken, _enemy);
            _logger.Subscribe(EventType.Killed, _enemy);

            _battleManager.Battle(_humanTeam, _enemyTeam);

            var logs = _logger.Logs;

            int expectedLogCount = enemyDies ? 5 : 3;

            Assert.AreEqual(expectedLogCount, logs.Count);

            int i = 0;

            //first event, mana spent
            EventLog log = logs[i++];
            Assert.AreEqual(EventType.ManaLost, log.Type);
            Assert.AreEqual(_human, log.Sender);
            ManaLostEventArgs e1 = log.E as ManaLostEventArgs;
            Assert.That(e1, Is.Not.Null);
            if (e1 != null)
            {
                Assert.AreEqual(_fireballSpell.Cost, e1.ManaSpent);
            }

            //second event, magical damage taken
            log = logs[i++];
            Assert.AreEqual(EventType.MagicalDamageTaken, log.Type);
            Assert.AreEqual(_enemy, log.Sender);
            MagicalDamageTakenEventArgs e2 = log.E as MagicalDamageTakenEventArgs;
            Assert.That(e2, Is.Not.Null);
            if (e2 != null)
            {
                Assert.AreEqual(spellDamage, e2.Damage);
                Assert.AreEqual(_fireballSpell.ElementalType, e2.MagicType);
            }

            //killed event sent by enemy, potentially
            if (enemyDies)
            {
                log = logs[i++];
                Assert.AreEqual(EventType.Killed, log.Type);
                Assert.AreEqual(_enemy, log.Sender);
                KilledEventArgs e3 = log.E as KilledEventArgs;
                Assert.That(e3, Is.Not.Null);
            }

            //Spell successful
            log = logs[i++];
            Assert.AreEqual(EventType.SpellSuccessful, log.Type);
            Assert.AreEqual(_human, log.Sender);
            SpellSuccessfulEventArgs e4 = log.E as SpellSuccessfulEventArgs;
            Assert.That(e4, Is.Not.Null);
            if (e4 != null)
            {
                Assert.AreEqual(_fireballSpell, e4.Spell);
                Assert.AreEqual(spellDamage, e4.DamageDealt);
                Assert.AreEqual(_enemy, e4.TargettedFoe);
            }

            //enemy killed, potentially
            if (enemyDies)
            {
                log = logs[i];
                Assert.AreEqual(EventType.EnemyKilled, log.Type);
                Assert.AreEqual(_human, log.Sender);
                EnemyKilledEventArgs e5 = log.E as EnemyKilledEventArgs;
                Assert.That(e5, Is.Not.Null);
                if (e5 != null)
                {
                    Assert.AreEqual(_enemy, e5.Enemy);
                }
            }
        }
    }
}
