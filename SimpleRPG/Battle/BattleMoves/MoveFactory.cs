using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves.BattleMoveEffects;
using SimpleRPG.Battle.BattleMoves.ConditionalBattleMoves;
using SimpleRPG.Battle.BattleShields;
using SimpleRPG.Battle.FieldEffects;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle.BattleMoves
{
    public static class MoveFactory
    {
        class MoveByLevel
        {
            public readonly BattleMove Move;

            public readonly int Level;

            public readonly FighterClass FighterClass;

            public MoveByLevel(BattleMove move, int level, FighterClass fighterClass = FighterClass.None)
            {
                Move = move;
                Level = level;
                FighterClass = fighterClass;
            }
        }

        private static readonly BattleMove DoNothing = new DoNothingMove();
        private static readonly BattleMove Attack = new AttackBattleMove("attack", TargetType.SingleEnemy, 100, 10);
        private static readonly BattleMove Runaway = new BattleMove("runaway", BattleMoveType.Runaway, TargetType.Self, 1, "ran away!");
        private static readonly NeverMissBattleMoveEffect UnconditionalNeverMissEffect = new NeverMissBattleMoveEffect();

        private static readonly Dictionary<Type, List<MoveByLevel>> MasterMoveList = new Dictionary
            <Type, List<MoveByLevel>>
        {
            {
                typeof (Goblin), new List<MoveByLevel>
                {
                    new MoveByLevel(Get(BattleMoveType.MultiTurn, "goblin punch") as SpecialMove, 1)
                }
            }
            , {
                typeof(DancerBoss), new List<MoveByLevel>
                {
                    new MoveByLevel(Get(BattleMoveType.Dance, "fire dance") as DanceMove, 1, FighterClass.BossDancerKiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "water dance") as DanceMove, 1, FighterClass.BossDancerKiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "wind dance") as DanceMove, 1, FighterClass.BossDancerKiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "earth dance") as DanceMove, 1, FighterClass.BossDancerKiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "heart dance") as DanceMove, 1, FighterClass.BossDancerRiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "soul dance") as DanceMove, 1, FighterClass.BossDancerRiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "mind dance") as DanceMove, 1, FighterClass.BossDancerRiki)
                    , new MoveByLevel(Get(BattleMoveType.Dance, "danger dance") as DanceMove, 1, FighterClass.BossDancerRiki)
                }
            }
            ,
            {
                typeof(Warrior), new List<MoveByLevel>
                {
                    new MoveByLevel(Get(BattleMoveType.Status, "warrior's cry") as SpecialMove, 1)
                    ,new MoveByLevel(Get(BattleMoveType.Status, "evade") as SpecialMove, 1)
                    ,new MoveByLevel(Get(BattleMoveType.Status, "evadeAndCounter") as SpecialMove, 3)
                }
            }
            ,
            {
                typeof(ShieldGuy), new List<MoveByLevel>
                {
                    new MoveByLevel(Get(BattleMoveType.Shield, "iron shield"), 1)
                    ,new MoveByLevel(Get(BattleMoveType.ShieldFortifier, "heal shield"), 1)
                    ,new MoveByLevel(Get(BattleMoveType.ShieldFortifier, "strengthen shield"), 1)
                }
            }
            ,
            {
                typeof(Shade), new List<MoveByLevel>
                {
                    new MoveByLevel(Get(BattleMoveType.Status, "dark fog"), 1)
                    ,new MoveByLevel(Get(BattleMoveType.AbsorbShade), 1)
                    ,new MoveByLevel(Get(BattleMoveType.Special, "dark energy gather"), 1)
                    ,new MoveByLevel(Get(BattleMoveType.ConditionalPowerAttack, "malevolence attack"), 1)
                }
            }
        };

        public static BattleMove Get(BattleMoveType type, string description = null)
        {
            BattleMove ret;
            var notFoundException = new ArgumentException($"No move exists with type {type} and description '{description}'!");

            switch (type)
            {
                case BattleMoveType.Attack:
                    switch (description)
                    {
                        case null:
                        case "attack":
                            ret = Attack;
                            break;
                        case "goblin punch":
                            ret = new AttackBattleMove(description, TargetType.SingleEnemy, 100, 75, executionText: $"throws a goblin punch at {Globals.TargetReplaceText}");
                            break;
                        case "feint":
                            NotEvadedBattleCondition notEvadedCondition = new NotEvadedBattleCondition();
                            AttackBoostBattleMoveEffect attackBoostEffect = new AttackBoostBattleMoveEffect(0.25, notEvadedCondition);
                            CannotBeEvadedBattleMoveEffect cannotBeEvadedEffect = new CannotBeEvadedBattleMoveEffect();
                            BattleMoveEffect[] effects = { attackBoostEffect, cannotBeEvadedEffect, UnconditionalNeverMissEffect };
                            ret = new AttackBattleMove(description, TargetType.SingleEnemy, 100, 0, executionText: "attacks [target] with a feint", effects: effects);
                            break;
                        default:
                            throw notFoundException;
                    }
                    break;
                case BattleMoveType.ConditionalPowerAttack:
                    ret = new ConditionalPowerAttackBattleMove("malevolence attack", TargetType.SingleEnemy, 100, 10, executionText: "unleashes their dark power!");
                    break;
                case BattleMoveType.Runaway:
                    ret = Runaway;
                    break;
                case BattleMoveType.DoNothing:
                    switch (description)
                    {
                        case null:
                            ret = DoNothing;
                            break;
                        case "goblin punch charge":
                            ret = new DoNothingMove("prepares to unleash its fury");
                            break;
                        default:
                            throw notFoundException;
                    }
                    break;
                case BattleMoveType.Special:
                    switch (description)
                    {
                        case "dark energy gather":
                            ret = new SpecialMove(description, BattleMoveType.Special, TargetType.Self, "gathers dark energy");
                            break;
                        default:
                            throw notFoundException;
                    }
                    break;
                case BattleMoveType.MultiTurn:
                    if (description == "goblin punch")
                    {
                        ret = new MultiTurnBattleMove(description, TargetType.SingleEnemy, 
                            Get(BattleMoveType.DoNothing, "goblin punch charge"),
                            Get(BattleMoveType.Attack, description));
                    }
                    else
                    {
                        throw notFoundException;
                    }
                    break;
                case BattleMoveType.Dance:
                    switch (description)
                    {
                        default:
                            throw notFoundException;
                        case "fire dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Fire,
                                new List<FieldEffect>
                                {
                                    new MagicMultiplierFieldEffect(TargetType.OwnTeam, "fire dance", MagicType.Fire, (4.0/3.0))
                                },
                                new DoNothingMove("performs the fire dance"),
                                new DoNothingMove("continues to perform the fire dance"));
                            break;
                        case "water dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Water,
                                new List<FieldEffect>
                                {
                                    new MagicMultiplierFieldEffect(TargetType.OwnTeam, "water dance", MagicType.Water, (4.0/3.0))
                                },
                                new DoNothingMove("performs the water dance"),
                                new DoNothingMove("continues to perform the water dance"));
                            break;
                        case "wind dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Wind,
                                new List<FieldEffect>
                                {
                                    new MagicMultiplierFieldEffect(TargetType.OwnTeam, "wind dance", MagicType.Wind, (4.0/3.0))
                                },
                                new DoNothingMove("performs the wind dance"),
                                new DoNothingMove("continues to perform the wind dance"));
                            break;
                        case "earth dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Earth,
                                new List<FieldEffect>
                                {
                                    new MagicMultiplierFieldEffect(TargetType.OwnTeam, "earth dance", MagicType.Earth, (4.0/3.0))
                                },
                                new DoNothingMove("performs the earth dance"),
                                new DoNothingMove("continues to perform the earth dance"));
                            break;
                        case "heart dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Heart,
                                new List<FieldEffect>
                                {
                                    new StatMultiplierFieldEffect(TargetType.OwnTeam, "heart dance", StatType.Defense, (5/100.0))
                                },
                                new DoNothingMove("performs the heart dance"),
                                new DoNothingMove("continues to perform the heart dance"));
                            break;
                        case "soul dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Soul,
                                new List<FieldEffect>
                                {
                                    new MagicMultiplierFieldEffect(TargetType.OwnTeam, "soul dance", MagicType.All, (5/100.0))
                                },
                                new DoNothingMove("performs the soul dance"),
                                new DoNothingMove("continues to perform the soul dance"));
                            break;
                        case "mind dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Mind,
                                new List<FieldEffect>
                                {
                                    new StatMultiplierFieldEffect(TargetType.OwnTeam, "mind dance", StatType.Evade, (5.0/100.0), 1)
                                },
                                new DoNothingMove("performs the mind dance"),
                                new DoNothingMove("continues to perform the mind dance"));
                            break;
                        case "danger dance":
                            ret = new DanceMove(description, TargetType.OwnTeam, 2, DanceEffectType.Danger,
                                new List<FieldEffect>
                                {
                                    new StatMultiplierFieldEffect(TargetType.OwnTeam, "danger dance", StatType.Strength, (5.0/100.0), 1)
                                },
                                new DoNothingMove("performs the danger dance"),
                                new DoNothingMove("continues to perform the danger dance"));
                            break;
                    }
                    break;
                case BattleMoveType.Status:
                    Status status;
                    switch (description)
                    {
                        default:
                            throw notFoundException;
                        case "warrior's cry":
                            status = new StatMultiplierStatus(4, StatType.Strength, 2);
                            ret = new StatusMove(description, TargetType.Self, status, "unleashes their warrior cry!");
                            break;
                        case "evade":
                            status = new AutoEvadeStatus(1, false);
                            ret = new StatusMove(description, TargetType.Self, status, "takes a ready stance", 1);
                            break;
                        case "evadeAndCounter":
                            status = new AutoEvadeStatus(1, true);
                            ret = new StatusMove(description, TargetType.Self, status, "prepares to counter", 1);
                            break;
                        case "dark fog":
                            status = new BlindStatus(2);
                            ret = new StatusMove(description, TargetType.SingleEnemy, status, $"draws a dark fog about {Globals.TargetReplaceText}", accuracy: 60);
                            break;
                    }
                    break;
                case BattleMoveType.Shield:
                    switch (description)
                    {
                        case "iron shield":
                            IBattleShield shield = new IronBattleShield(5, 2, 0);
                            ret = new ShieldMove(description, TargetType.SingleAllyOrSelf, "created an iron shield", shield);
                            break;
                        default:
                            throw notFoundException;
                    }
                    break;
                case BattleMoveType.ShieldFortifier:
                    switch (description)
                    {
                        case "heal shield":
                            ret = new ShieldFortifyingMove(description, TargetType.SingleAllyOrSelf, $"healed {Globals.TargetReplaceText}'s shield", ShieldFortifyingType.Health, 5);
                            break;
                        case "strengthen shield":
                            ret = new ShieldFortifyingMove(description, TargetType.SingleAllyOrSelf, $"strengthened {Globals.TargetReplaceText}'s shield", ShieldFortifyingType.Defense, 5);
                            break;
                        default:
                            ret = null;
                            break;
                    }
                    break;
                case BattleMoveType.ShieldBuster:
                    if (string.IsNullOrEmpty(description))
                    {
                        description = "Shield buster";
                    }
                    switch (description)
                    {
                        case "Shield buster":
                            ret = new ShieldBusterMove(description, TargetType.SingleEnemy, "uses the shield buster on [target]");
                            break;
                        case "Super shield buster":
                            ret = new ShieldBusterMove(description, TargetType.SingleEnemy, "uses the super shield buster on [target]", 1);
                            break;
                        default:
                            throw notFoundException;
                    }
                    
                    break;
                case BattleMoveType.BellMove:
                    switch (description)
                    {
                        case "seal shade":
                        default:
                            ret = Get(BellMoveType.SealMove);
                            break;
                        case "control shade":
                            ret = Get(BellMoveType.ControlMove);
                            break;
                    }
                    break;
                case BattleMoveType.AbsorbShade:
                    ret = new ShadeAbsorbingMove("absorb shade", $"has given into malice, targetting {Globals.TargetReplaceText}!");
                    break;
                default:
                    throw notFoundException;
            }

            return ret;
        }

        public static BellMove Get(BellMoveType moveType)
        {
            string description = "";
            string executionText = "";
            switch (moveType)
            {
                case BellMoveType.SealMove:
                    description = "seal shade";
                    executionText = $"attempts to seal {Globals.TargetReplaceText}";
                    break;
                case BellMoveType.ControlMove:
                    description = "control shade";
                    executionText = $"attempts to dominate {Globals.TargetReplaceText}";
                    break;
            }

            BellMove ret = new BellMove(description, moveType, TargetType.SingleEnemy, executionText: executionText);

            return ret;
        }

        /// <summary>
        /// Gets all the battle moves a particular class learns by level up by the time they reach a particular level.
        /// Optionally only returns the list of spells they would learn within a range of levels.
        /// (e.g. if FooFighter learns a new spell at levels 2-5, and min level is specified at 4, only 2 spells are returned)
        /// </summary>
        /// <typeparam name="T">The Fighter class that will learn the battle moves</typeparam>
        /// <param name="level">The max level for the range of battle moves they will learn</param>
        /// <param name="minLevel">The min level for the range of battle movess they will learn.
        /// Omit this to get all battle moves learned by the given level</param>
        /// <param name="fighterClass">An optional parameter that may determine what spells a fighter may learn depending on their class</param>
        /// <returns>all the <see cref="BattleMove"/> a particular class learns by level up between one level and another</returns>
        public static List<BattleMove> GetMovesByLevel<T>(int level, int minLevel = 1, FighterClass fighterClass = FighterClass.None) where T : IFighter
        {
            var ret = new List<BattleMove>();

            var type = typeof(T);

            if (MasterMoveList.ContainsKey(type))
            {
                var list = MasterMoveList[type];

                ret.AddRange(list.Where(sbl => 
                sbl.Level >= minLevel 
                && sbl.Level <= level 
                && ((sbl.FighterClass == FighterClass.None) || fighterClass == sbl.FighterClass))
                .Select(sbl => sbl.Move));
            }

            return ret;
        }
    }
}
