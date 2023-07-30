using SimpleRPG.Battle;

namespace SimpleRPG.Regions
{
    public class ScriptedBattlefieldConfiguration
    {
        public int BattleIndex { get; }

        public BattlefieldConfiguration BattlefieldConfig { get; }

        public ScriptedBattlefieldConfiguration(BattlefieldConfiguration battlefieldConfig, int battleIndex)
        {
            BattlefieldConfig = battlefieldConfig;
            BattleIndex = battleIndex;
        }
    }
}
