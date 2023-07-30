using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.Magic
{
    public static class SpellFactory
    {
        class SpellByLevel
        {
            public readonly Spell Spell;

            public readonly int Level;

            public SpellByLevel(Spell spell, int level)
            {
                Spell = spell;
                Level = level;
            }
        }

        private static readonly Dictionary<MagicType, string[]> _spellNameList = new Dictionary<MagicType, string[]>
        {
            {
                MagicType.Fire, new []{ "", "fireball", "blaze", "inferno" }
            }
            ,{
                MagicType.Water, new [] { "", "splash", "angry splash", "violent splash" }
            }
            ,{
                MagicType.Earth, new [] { "", "clay spike", "boulder", "earthquake" }
            }
            ,{
                MagicType.Wind, new [] { "", "gust", "air whip", "twister" }
            }
            ,{
                MagicType.Ice, new [] { "", "frost", "ice spike", "blizzard" }
            }
            ,{
                MagicType.Lightning, new [] { "", "spark", "shock", "storm" }
            }
        };

        private static readonly Dictionary<Type, List<SpellByLevel>> _masterSpellList = new Dictionary<Type, List<SpellByLevel>>
        {
            //TODO: maybe not unlocked by level up.
            {
                typeof(HumanFighter), new List<SpellByLevel>
                {
                    new SpellByLevel(GetSpell(MagicType.Fire, 1), 4)
                    ,new SpellByLevel(GetSpell(MagicType.Earth, 1), 4)
                    ,new SpellByLevel(GetSpell(MagicType.Water, 1), 4)
                    ,new SpellByLevel(GetSpell(MagicType.Wind, 1), 4)
                    ,new SpellByLevel(GetSpell(MagicType.Fire, 2), 5)
                    ,new SpellByLevel(GetSpell(MagicType.Earth, 2), 5)
                    ,new SpellByLevel(GetSpell(MagicType.Water, 2), 5)
                    ,new SpellByLevel(GetSpell(MagicType.Wind, 2), 5)
                }
            }
            ,{
                typeof(Fairy), new List<SpellByLevel>
                {
                    new SpellByLevel(GetSpell("fairy wind"), 1),
                    new SpellByLevel(GetSpell("fairy wish"), 1)
                }
            }
            ,{
                typeof(MegaChicken), new List<SpellByLevel>
                {
                    //TODO: add some unique effects to these spells once they can do more than just attack
                    //TODO: determine correct target types once these spells are fleshed out
                    new SpellByLevel(GetSpell("inferno egg"), 1)
                    ,new SpellByLevel(GetSpell("blaze egg"), 1)
                    ,new SpellByLevel(GetSpell("tempest egg"), 1)
                    ,new SpellByLevel(GetSpell("thunder egg"), 1)
                    ,new SpellByLevel(GetSpell("blizzard egg"), 1)
                    ,new SpellByLevel(GetSpell("frost egg"), 1)
                    ,new SpellByLevel(GetSpell("chaos egg"), 1)
                }
            }
        };

        public static Spell GetSpell(string name)
        {
            Spell ret;

            switch (name)
            {
                default:
                    throw new ArgumentException($"No spell exists called {name}!");
                case "fairy wind":
                    ret = new Spell(name, MagicType.Wind, SpellType.Attack, TargetType.SingleEnemy, 2, 1);
                    break;
                case "fairy wish":
                    ret = new Spell(name, MagicType.Wind, SpellType.RestoreMana, TargetType.SingleAllyOrSelf, 0, 0);
                    break;
                case "inferno egg":
                    ret = new Spell(name, MagicType.Fire, SpellType.Attack, TargetType.SingleEnemy, 0, 5);
                    break;
                case "blaze egg":
                    ret = new Spell(name, MagicType.Fire, SpellType.Attack, TargetType.SingleEnemy, 0, 2);
                    break;
                case "tempest egg":
                    ret = new Spell(name, MagicType.Lightning, SpellType.Attack, TargetType.SingleEnemy, 0, 5);
                    break;
                case "thunder egg":
                    ret = new Spell(name, MagicType.Lightning, SpellType.Attack, TargetType.SingleEnemy, 0, 2);
                    break;
                case "blizzard egg":
                    ret = new Spell(name, MagicType.Lightning, SpellType.Attack, TargetType.SingleEnemy, 0, 5);
                    break;
                case "frost egg":
                    ret = new Spell(name, MagicType.Ice, SpellType.Attack, TargetType.SingleEnemy, 0, 2);
                    break;
                case "chaos egg":
                    ret = new Spell(name, MagicType.None, SpellType.Attack, TargetType.SingleEnemy, 0, 2);
                    break;
            }

            return ret;
        }



        public static Spell GetSpell(MagicType type, int powerLevel)
        {
            var cost = 0;
            var power = 0;
            string name;
            var spellNotFoundException = new ArgumentException($"No spell exists for type {type} for powerLevel {powerLevel}!");

            if (!_spellNameList.ContainsKey(type))
            {
                throw spellNotFoundException;
            }
            else
            {
                var spellNames = _spellNameList[type];

                if (powerLevel < 1 || powerLevel >= spellNames.Length)
                {
                    throw spellNotFoundException;
                }
                else
                {
                    name = spellNames[powerLevel];
                }
            }

            switch (powerLevel)
            {
                case 1:
                    cost = 2;
                    power = 1;
                    break;
                case 2:
                    cost = 5;
                    power = 3;
                    break;
            }

            var ret = new Spell(name, type, SpellType.Attack, TargetType.SingleEnemy, cost, power);

            return ret;
        }

        /// <summary>
        /// Gets all the spells a particular class learns by level up by the time they reach a particular level.
        /// Optionally only returns the list of spells they would learn within a range of levels.
        /// (e.g. if FooFighter learns a new spell at levels 2-5, and min level is specified at 4, only 2 spells are returned)
        /// </summary>
        /// <typeparam name="T">The Fighter class that will learn the spells</typeparam>
        /// <param name="level">The max level for the range of spells they will learn</param>
        /// <param name="minLevel">The min level for the range of spells they will learn.
        /// Omit this to get all spells learned by the given level</param>
        /// <param name="fighterClass">An optional parameter that may determine what spells a fighter may learn depending on their class</param>
        /// <returns>all the spells a particular class learns by level up between one level and another</returns>
        public static List<Spell> GetSpellsByLevel<T>(int level, int minLevel = 1, FighterClass fighterClass = FighterClass.None) where T : IFighter
        {
            var ret = new List<Spell>();

            var type = typeof (T);

            if (_masterSpellList.ContainsKey(type))
            {
                var list = _masterSpellList[type];
                
                ret.AddRange(list.Where(sbl => sbl.Level >= minLevel && sbl.Level <= level).Select(sbl => sbl.Spell));
            }

            return ret;
        }
    }
}