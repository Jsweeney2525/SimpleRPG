using SimpleRPG.Battle.BattleManager;

namespace SimpleRPG.Test.MockClasses
{
    public class SilentBattleConfiguration : BattleManagerBattleConfiguration
    {
        public SilentBattleConfiguration()
        {
            ShowIntroAndOutroMessages = false;
            ShowCastSpellMessages = false;
            ShowMagicalDamageMessages = false;
            ShowAttackMessages = false;
            ShowPhysicalDamageMessages = false;
            ShowDeathMessages = false;
            ShowExpAndLevelUpMessages = false;
            ShowShieldAddedMessage = false;
        }
    }
}
