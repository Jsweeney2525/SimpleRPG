using System;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;

namespace SimpleRPG.Battle.Fighters
{
    public class FighterFactory
    {
        protected static IChanceService ChanceService = Globals.ChanceService;

        public static IChanceService GetChanceService()
        {
            return ChanceService;
        }

        protected static IInput Input;

        public static void SetInput(IInput input)
        {
            Input = input;
        }

        protected static IOutput Output;

        public static void SetOutput(IOutput output)
        {
            Output = output;
        }

        protected static GodRelationshipManager GodRelationshipManager;

        public static void SetGodRelationshipManager(GodRelationshipManager godRelationshipManager)
        {
            GodRelationshipManager = godRelationshipManager;
        }

        public static Shade GetShade(int level,
            string name = null,
            int shadeLevel = 1)
        {
            return new Shade(level, ChanceService, shadeLevel);
        }

        public static IFighter GetFighter(FighterType type, 
            int level, 
            string name = null, 
            MagicType magicType = MagicType.None, 
            FighterClass fighterClass = FighterClass.None,
            IMenuFactory menuFactory = null)
        {
            IFighter ret;

            switch (type)
            {
                case FighterType.Goblin:
                    ret = new Goblin(level, ChanceService);
                    break;
                case FighterType.Golem: //high defense, low attack, low speed, low health
                    ret = new Golem(level, ChanceService);
                    break;
                case FighterType.Ogre: //high attack, low speed, low defense
                    ret = new Ogre(level, ChanceService);
                    break;
                case FighterType.Fairy: //high speed, high evade, low attack, low defense
                    ret = new Fairy(level, ChanceService);
                    break;
                case FighterType.HumanControlledPlayer:
                    ret = new HumanFighter(name ?? "Player", level);
                    GodRelationshipManager?.InitializeForFighter( (HumanFighter)ret );
                    break;
                case FighterType.HumanControlledEnemy:
                    ret = new HumanControlledEnemyFighter(name ?? "Player", Input, Output, menuFactory ?? Globals.MenuFactory);
                    break;
                case FighterType.MegaChicken:
                    ret = new MegaChicken(level, ChanceService);
                    break;
                case FighterType.Egg:
                    if (magicType == MagicType.None)
                    {
                        int magicIndex = ChanceService.WhichEventOccurs(Globals.EggMagicTypes.Length);
                        magicType = Globals.EggMagicTypes[magicIndex];
                    }
                    ret = new Egg(magicType);
                    break;
                case FighterType.DancerBoss:
                    ret = new DancerBoss(fighterClass, level, ChanceService);
                    break;
                case FighterType.Zombie:
                    ret = new Zombie(level, ChanceService);
                    break;
                case FighterType.Warrior:
                    ret = new Warrior(level, ChanceService);
                    break;
                case FighterType.ShieldGuy:
                    ret = new ShieldGuy(level, ChanceService);
                    break;
                case FighterType.Barbarian:
                    ret = new Barbarian(level, ChanceService);
                    break;
                case FighterType.Shade:
                    ret = GetShade(level, name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type,
                        "The specified type is invalid for FighterFactory to initialize!");
            }

            return ret;
        }
    }
}
