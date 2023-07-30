using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;
using SimpleRPG.Test.MockClasses.Enemies;

namespace SimpleRPG.Test.MockClasses.Factories
{
    public enum TestFighterType
    {
        TestHuman = -1
        ,TestEnemy = 0
        ,HumanControlledPlayer = 1
        ,HumanControlledEnemy = 2
        ,Goblin = 3
        ,Golem = 4 //high defense, low attack, low speed
        ,Ogre = 5 //high attack, low speed, low defense
        ,Fairy = 6 //high speed, high evade, low attack, low defense
        ,MegaChicken = 7
        ,Egg = 8
        ,DancerBoss = 9
        ,Zombie = 10
        ,Warrior = 11
        ,ShieldGuy = 12
        ,Barbarian = 13
        ,Shade = 14
    }

    public class TestFighterFactory : FighterFactory
    {
        public static void SetChanceService(IChanceService service)
        {
            ChanceService = service;
        }

        public static void ResetChanceService()
        {
            ChanceService = Globals.ChanceService;
        }

        public static IFighter GetFighter(TestFighterType type, int level, string name = null)
        {
            IFighter ret;

            switch (type)
            {
                case TestFighterType.TestEnemy:
                    ret = new TestEnemyFighter(name ?? "Test", 1, 0, 1, 0, 0, 0, 0, ChanceService);
                    break;
                case TestFighterType.TestHuman:
                    TestHumanFighter humanFighter = new TestHumanFighter(name ?? "Test Player", level);
                    GodRelationshipManager?.InitializeForFighter(humanFighter);
                    ret = humanFighter;
                    break;
                default:
                    ret = GetFighter((FighterType) type, level, name);
                    break;
            }

            return ret;
        }
    }
}
