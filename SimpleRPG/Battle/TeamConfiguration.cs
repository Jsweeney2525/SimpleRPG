using System.Collections.Generic;
using SimpleRPG.Enums;

namespace SimpleRPG.Battle
{
    public class EnemyConfiguration
    {
        public FighterType EnemyType { get; }

        public int Level { get; }

        public MagicType MagicType { get; }

        public EnemyConfiguration(FighterType enemyType, int level, MagicType magicType = MagicType.None)
        {
            EnemyType = enemyType;
            Level = level;
            MagicType = magicType;
        }
    }

    public class TeamConfiguration
    {
        public IEnumerable<EnemyConfiguration> Enemies { get; }

        public TeamConfiguration(params EnemyConfiguration[] enemies)
        {
            Enemies = enemies;
        }
    }
}
