using System;
using SimpleRPG.Battle.Fighters.Enemies;
using SimpleRPG.Enums;

// | Monster     | HP          | MP      | Attack  | Defense | Speed   | Evade | Luck |
// ------------------------------------------------------------------------------------
// | Default     | level*5     | level*2 | Level   | level-1 | level   |   5   |  10  |
// | Goblin      | level*7     | level*3 | level+1 | level-1 | level   |   5   |  15  |
// | Fairy       | level*3     | level*4 | level   | level-2 | level+1 |   20  |  25  |
// | Golem       | level*4     | level*2 | level   | level-1 | level-1 |   0   |  10  |
// | Ogre        | level*5     | level*2 | level+2 | level-1 | level   |   5   |  10  |
// | Golem       | level*5     | level*2 | level   | level   | level   |   5   |  10  |
// | Ogre        | level*5     | level*2 | level   | level-1 | level   |   5   |  10  |
// | Warrior     | level*6     | level   | level+1 | level-1 | level   |   10  |  10  |
// | ShieldGuy   | level*3     | level   | level   | level-2 | level+1 |   5   |  10  |
// | Shade       | 2 + level*2 | level*2 | Level   | level-2 | level   |   5   |  10  |
// --------------------------------------------------------------------------------
// GOT TO DOCUMENT:
// | MegaChicken | level*3 | level   | level   | level-2 | level+1 |   5   |  10  |
// GOT TO IMPLEMENT:
// | Barbarian   | level*3 | level   | level   | level-2 | level+1 |   5   |  10  |

namespace SimpleRPG.Battle
{
    public static class LevelUpManager
    {
        private static readonly int[] ExperienceToLevel = {0, 0, 20, 60, 120, 200};

        public const int MAX_LEVEL = 5;

        public static int GetExpForLevel(int level)
        {
            if (level > MAX_LEVEL)
            {
                level = MAX_LEVEL;
            }

            return ExperienceToLevel[level];
        }

        #region health stat

        /// <summary>
        /// How much a fighter's health will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int HealthBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return 5;
        }

        public static int GetHealthByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return level * 5;
        }

        public static int GetHealthByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof (T);
            int ret;

            if (type == typeof(Fairy))
            {   //reduced health to make up for high speed and evasion
                ret = level*3;
            }
            else if (type == typeof(Golem))
            {
                //reduced health to make up for high defense
                ret = level * 4;
            }
            else if (type == typeof(Shade))
            {
                ret = 2 + (level*2);
            }
            else if (type == typeof(Goblin))
            {
                //increased health- goblin's specialty to make up for high defense
                ret = level*7;
            }
            else if (type == typeof(MegaChicken))
            {
                //note: may need to update this is anything other 
                ret = 15;
            }
            else if (type == typeof(Warrior))
            {
                ret = level*6;
            }
            else if (type == typeof(Barbarian))
            {
                ret = 40;
            }
            else if (type == typeof(ShieldGuy))
            {
                ret = level*3;
            }
            else
            {
                ret = level*5;
            }

            return ret;
        }

        #endregion

        #region mana stat

        /// <summary>
        /// How much a fighter's mana will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int ManaBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return 2;
        }

        public static int GetManaByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return 2 + (level * 2);
        }

        public static int GetManaByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                ret = 6 + (4 * level);
            }
            else if (type == typeof(Goblin))
            {
                ret = (level/3) * 2;
            }
            else if (type == typeof(Golem))
            {
                ret = 0;
            }
            else if (type == typeof(Ogre))
            {
                ret = level + ((level - 1) / 4);
            }

            else if (type == typeof(Ogre))
            {
                ret = 5 + (3*level);
            }
            else if (type == typeof(MegaChicken))
            {
                ret = level * 5;
            }*/
            if (type == typeof(Fairy))
            {
                ret = level*4;
            }
            else if (type == typeof(Goblin))
            {
                ret = level * 3;
            }
            else if (type == typeof(Warrior) || type == typeof(ShieldGuy) || type == typeof(Barbarian))
            {
                ret = level;
            }
            else
            {
                ret = level*2;
            }

            return ret;
        }

        #endregion

        #region strength stat

        /// <summary>
        /// How much a fighter's strength will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int StrengthBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return 1;
        }

        public static int GetStrengthByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return level + 1;
        }

        public static int GetStrengthByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                ret = level;
            }
            else if (type == typeof(Goblin))
            {
                ret = 1 + (2 * level);
            }
            else if (type == typeof(Golem))
            {
                //1 plus level, plus an extra 3 every 5 levels
                ret = 1 + level + (3 * (level / 5));
            }
            else if (type == typeof(Ogre))
            {
                //base of 3, 2 per level plus a bonus each 4
                ret = 3 + (2 * level) + (level / 4);
            }
            else if (type == typeof(MegaChicken))
            {
                ret = 0;
            }
            else if (type == typeof(Warrior))
            {
                ret = 6 + level*2;
            }*/
            if (type == typeof(Ogre))
            {
                ret = level + 2;
            }
            else if (type == typeof(Goblin) || type == typeof(Warrior))
            {
                ret = level + 1;
            }
            else if (type == typeof(Barbarian))
            {
                ret = level + 3;
            }
            else
            {
                ret = level;
            }

            return ret;
        }

        #endregion

        #region defense stat

        /// <summary>
        /// How much a fighter's defense will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int DefenseBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            return level == 1 ? 0 : 1; //((level % 3) == 0) ? 0 : 1;
        }

        public static int GetDefenseByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //goes up by 1 every 2 out of 3 levels. (level 1- 1 def, level 2- 2 def, level 3- 2 def)
            //var div = level/3;
            //return (level % 3) + (div * 2);

            return Math.Max(0, level - 1);
        }

        public static int GetDefenseByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                ret = (level / 2) - 1;
            }
            else if (type == typeof(Goblin))
            {
                //base of 2, then up by 2 every third level level
                ret = 2 + (2 * (level/3));
            }
            else if (type == typeof(Golem))
            {
                //3 every level
                ret = 3 * level;
            }
            else if (type == typeof(Ogre))
            {
                ret = level / 2;
            }
            else if (type == typeof(MegaChicken))
            {
                ret = level;
            }
            else if (type == typeof(Warrior))
            {
                ret = level + 2;
            }*/
            if (type == typeof(Fairy) || type == typeof(ShieldGuy) || type== typeof(Shade))
            {
                ret = Math.Max(0, level - 2);
            }
            else if (type == typeof(Golem) || type == typeof(Barbarian))
            {
                ret = level;
            }
            else
            {
                ret = Math.Max(0, level - 1);
            }

            return ret;
        }

        #endregion

        #region Speed stat

        /// <summary>
        /// How much a fighter's speed will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int SpeedBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //return ((level % 2) == 0) ? 1 : 0;
            return 1;
        }

        public static int GetSpeedByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //base of 2 and goes up by 1 every even level (level 1- 2, level 2- 3, level 3- 3, level 4- 4)
            //return (level / 2) + 2;

            return level;
        }

        public static int GetSpeedByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                ret = 2 + (3*level);
            }
            else if (type == typeof(Goblin))
            {
                ret = level;
            }
            else if (type == typeof(Golem))
            {
                //base of 1, 1 every level with a +1 bonus every 4 levels
                ret = 1 + level + (1 * (level / 4));
            }
            else if (type == typeof(Ogre))
            {
                ret = 2 * (level / 3);
            }
            else if (type == typeof(MegaChicken))
            {
                ret = 4 + (2 * level);
            }*/
            if (type == typeof(Fairy) || type == typeof(ShieldGuy))
            {
                ret = level + 1;
            }
            else if (type == typeof(Golem))
            {
                ret = Math.Max(0, level - 1);
            }
            else
            {
                ret = level;
            }

            return ret;
        }

        #endregion

        #region evade stat

        /// <summary>
        /// How much a fighter's evade will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int EvadeBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //return ((level % 5) == 0) ? 2 : 0;
            return 0;
        }

        public static int GetEvadeByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //base of 10, then goes up by 2 every 5 levels
            //return ((level / 5) * 2) + 10;

            return 5;
        }

        public static int GetEvadeByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                ret = 50 + ((level < 10) ? 0 : 15 );
            }
            else if (type == typeof(Goblin))
            {
                //base of 5, but up 3 every 5 levels
                ret = 5 + (3 * (level/5));
            }
            else if (type == typeof(Golem) ||
                     type == typeof(Ogre))
            {
                //5 every 10 levels
                ret = 5 * (level / 10);

                if (ret > 30)
                {
                    ret = 30;
                }
            }
            else if (type == typeof(MegaChicken))
            {
                ret = 20 + (5 * (level / 3));
                if (ret > 50)
                {
                    ret = 50;
                }
            }*/
            if (type == typeof(Fairy))
            {
                ret = 20;
            }
            else if (type == typeof(Golem))
            {
                ret = 0;
            }
            else if (type == typeof(Warrior) || type == typeof(Barbarian))
            {
                ret = 10;
            }
            else
            {
                ret = 5;
            }

            return ret;
        }

        #endregion

        #region luck stat

        /// <summary>
        /// How much a fighter's luck will be boosted when leveling up from (level - 1) to level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="fighterClass"></param>
        /// <returns></returns>
        public static int LuckBoostByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //return ((level % 3) == 0) ? 1 : 0;

            return 0;
        }

        public static int GetLuckByLevel(int level, FighterClass fighterClass = FighterClass.None)
        {
            //base of 5, then up by 1 every 3 levels
            //return (level / 3) + 5;

            return 10;
        }

        public static int GetLuckByLevel<T>(int level, FighterClass fighterClass = FighterClass.None) where T : EnemyFighter
        {
            var type = typeof(T);
            int ret;

            /*if (type == typeof(Fairy))
            {
                //base of 15, up by 10 every 5 levels
                ret = 15 + ((level /5) * 10);
                if (ret > 90)
                {
                    ret = 90;
                }
            }
            else if (type == typeof(Goblin))
            {
                //up by 10 every 7 levels
                ret = 10 * (level / 7);

                if (ret > 50)
                {
                    ret = 50;
                }
            }
            else if (type == typeof(Golem))
            {
                ret = 0;
            }
            else if (type == typeof(Ogre))
            {
                ret = 20 + (4 * (level/5));
            }
            else if (type == typeof(MegaChicken))
            {
                ret = 0;
            }*/
            if (type == typeof(Fairy))
            {
                ret = 25;
            }
            else if (type == typeof(Goblin))
            {
                //slight boost over default
                ret = 15;
            }
            else
            {
                ret = 10;
            }

            return ret;
        }

        #endregion
    }
}
