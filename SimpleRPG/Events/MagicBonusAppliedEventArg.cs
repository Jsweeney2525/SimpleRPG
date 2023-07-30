using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Events
{
    public class MagicBonusAppliedEventArgs : EventArgs
    {
        public MagicStatType MagicStatType { get; }

        public MagicType MagicType { get; }
        
        public int BonusAmount { get; }

        public bool IsSecretStatBonus { get; }

        public MagicBonusAppliedEventArgs(MagicStatType magicStatType, MagicType magicType, int bonusAmount, bool isSecretStatBonus)
        {
            MagicStatType = magicStatType;
            MagicType = magicType;
            BonusAmount = bonusAmount;
            IsSecretStatBonus = isSecretStatBonus;
        }

    }
}
