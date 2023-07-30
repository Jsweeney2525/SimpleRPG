using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses
{
    public static class TestHelperExtensionMethods
    {
        public static bool IsCorrectType(this FighterType expectedType, IFighter fighter)
        {
            bool isCorrectType;

            switch (expectedType)
            {
                case FighterType.Fairy:
                    isCorrectType = fighter is Fairy;
                    break;
                case FighterType.Goblin:
                    isCorrectType = fighter is Goblin;
                    break;
                case FighterType.Warrior:
                    isCorrectType = fighter is Warrior;
                    break;
                case FighterType.Ogre:
                    isCorrectType = fighter is Ogre;
                    break;
                case FighterType.Golem:
                    isCorrectType = fighter is Golem;
                    break;
                case FighterType.HumanControlledPlayer:
                    isCorrectType = fighter is HumanFighter;
                    break;
                case FighterType.HumanControlledEnemy:
                    isCorrectType = fighter is HumanControlledEnemyFighter;
                    break;
                case FighterType.MegaChicken:
                    isCorrectType = fighter is MegaChicken;
                    break;
                case FighterType.Egg:
                    isCorrectType = fighter is Egg;
                    break;
                case FighterType.DancerBoss:
                    isCorrectType = fighter is DancerBoss;
                    break;
                case FighterType.Zombie:
                    isCorrectType = fighter is Zombie;
                    break;
                case FighterType.ShieldGuy:
                    isCorrectType = fighter is ShieldGuy;
                    break;
                case FighterType.Barbarian:
                    isCorrectType = fighter is Barbarian;
                    break;
                default:
                    throw new ArgumentException($"IsCorrectType() was not implemented to handle fighter type {expectedType}");
            }

            return isCorrectType;
        }
    }
}
