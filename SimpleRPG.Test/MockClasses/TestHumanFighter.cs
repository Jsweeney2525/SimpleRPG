using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Test.MockClasses
{
    public class TestHumanFighter : HumanFighter, ITestFighter
    {
        public TestHumanFighter(string name, int level, List<Spell> spells = null, List<SpecialMove> specialMoves = null ) :
            base(name, level, spells, specialMoves)
        {
            CurrentHealth = 1;
            MaxHealth = 1;
            MaxMana = 0;
            CurrentMana = 0;
            Strength = 1;
            Defense = 0;
            Speed = 0;
            Evade = 0;
            Luck = 0;
            MagicStrength = 0;
            MagicResistance = 0;

            _moves = new List<Tuple<BattleMove, int?>>();
            _moveTarget = this;
        }

        public void SetHealth(int maxHealth, int? currentHealth = null)
        {
            if (currentHealth == null)
            {
                currentHealth = maxHealth;
            }

            if (currentHealth < 0)
            {
                throw new ArgumentException("current Health must be a non-negative value");
            }
            if (maxHealth < 1)
            {
                throw new ArgumentException("max Health must be a positive value");
            }
            if (currentHealth > maxHealth)
            {
                throw new ArgumentException("current Health must be less than max health");
            }

            CurrentHealth = currentHealth.Value;
            MaxHealth = maxHealth;
        }

        public void SetMana(int maxMana, int? currentMana = null)
        {
            if (currentMana == null)
            {
                currentMana = maxMana;
            }

            if (maxMana < 0)
            {
                throw new ArgumentException("Max Mana must be a non-negative value");
            }
            if (currentMana < 0)
            {
                throw new ArgumentException("Current Mana must be a non-negative value");
            }
            if (currentMana > maxMana)
            {
                throw new ArgumentException("current mana must be less than max mana");
            }

            CurrentMana = currentMana.Value;
            MaxMana = maxMana;
        }

        public void SetStrength(int strength)
        {
            Strength = strength;
        }

        public void SetMagicStrength(int strength)
        {
            MagicStrength = strength;
        }

        public void SetMagicBonus(MagicType magicType, int strength)
        {
            MagicStrengthBonuses[magicType] = strength;
        }

        public void SetDefense(int defense)
        {
            Defense = defense;
        }

        public void SetMagicResistance(int resistance)
        {
            MagicResistance = resistance;
        }

        public void SetResistanceBonus(MagicType magicType, int resistance)
        {
            MagicResistanceBonuses[magicType] = resistance;
        }

        public void SetSpeed(int speed)
        {
            Speed = speed;
        }

        public void SetEvade(int evade)
        {
            Evade = evade;
        }

        public void SetLuck(int luck)
        {
            Luck = luck;
        }

        public void ResetElementalAffinity(MagicType magicType)
        {
            MagicAffinities[magicType] = FighterMagicRelationshipType.None;
        }

        public void SetElementalWeakness(MagicType magicType)
        {
            MagicAffinities[magicType] = FighterMagicRelationshipType.Weak;
        }

        public void SetElementalResistance(MagicType magicType)
        {
            MagicAffinities[magicType] = FighterMagicRelationshipType.Resistant;
        }

        public void SetElementalImmunity(MagicType magicType)
        {
            MagicAffinities[magicType] = FighterMagicRelationshipType.Immune;
        }

        private List<Tuple<BattleMove, int?>> _moves;
        private IFighter _moveTarget;

        /// <summary>
        /// hard codes which move will be returned when SelectMove() is called. 
        /// If this method has not been called, the default MenuInput from <see cref="HumanFighter"/> will be used
        /// </summary>
        /// <param name="move">The move to be returned when SelectMove() is called</param>
        /// <param name="numTurns">An option parameter to determine how many turns in a row a move should be used.
        /// Useful if different behavior is required from the second turn, for example 
        /// (e.g. cast a spell on turn 1 then run on turn 2, specify move as a Spell, numTurns as 1, then specify a run away move for turn 2)</param>
        public void SetMove(BattleMove move, int? numTurns = null)
        {
            _moves.Add(new Tuple<BattleMove, int?>(move, numTurns));
        }

        public BattleMove GetMove()
        {
            if (_moves.Count == 0)
            {
                throw new IndexOutOfRangeException("There are no moves in the test fighter's queue to be selected!");
            }

            Tuple<BattleMove, int?> nextMove = _moves[0];

            if (nextMove.Item2.HasValue)
            {
                int turnsLeft = nextMove.Item2.Value - 1;

                if (turnsLeft == 0)
                {
                    Tuple<BattleMove, int?>[] arr = _moves.ToArray();
                    Array.Copy(arr, 1, arr, 0, arr.Length - 1);
                    Array.Clear(arr, arr.Length - 1, 1);

                    _moves = arr.ToList();
                    _moves.RemoveAll(t => t == null);
                }
                else
                {
                    _moves[0] = new Tuple<BattleMove, int?>(nextMove.Item1, turnsLeft);
                }
            }

            return nextMove.Item1;
        }

        public bool HasMove()
        {
            return _moves.Count > 0;
        }

        public void SetMoveTarget(IFighter target)
        {
            _moveTarget = target;
        }

        public IFighter GetMoveTarget(BattleMove moveToExecute = null)
        {
            IFighter ret = null;

            if (_moveTarget != null)
            {
                ret = _moveTarget;
            }
            else if(moveToExecute != null && (moveToExecute.TargetType == TargetType.Self || moveToExecute.TargetType == TargetType.Field))
            {
                ret = this;
            }
            return ret;
        }

        /// <summary>
        /// Call this method to make the fighter die the next time TurnEnd is fired
        /// </summary>
        public void SetDeathOnTurnEndEvent()
        {
            this.TurnEnded += KilledOnTurnEnded;
        }

        private static void KilledOnTurnEnded(object sender, TurnEndedEventArgs e)
        {
            TestHumanFighter fighter = e.Fighter as TestHumanFighter;

            if (fighter == null)
            {
                throw new ArgumentException("KilledOnTurnEnd somehow called on class that is not a TestFighter!");
            }

            fighter.SetHealth(fighter.MaxHealth, 0);
            //TODO: should the basic "damage" method be called, and should it fire OnKilled if it brings health to 0?
            //Then, how do we ensure the proper event args are passed into the event? Or do we even care, since killed is empty?
            fighter.OnKilled(new KilledEventArgs());
        }

        /// <summary>
        /// Call this method to immediately kill a player
        /// </summary>
        public void DealMaxPhysicalDamage()
        {
            PhysicalDamage(MaxHealth);
        }
    }
}
