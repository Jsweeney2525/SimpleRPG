using System;
using SimpleRPG.Enums;

namespace SimpleRPG.Events
{
    public class StatBonusAppliedEventArgs : EventArgs
    {
        public StatType Stat { get; }

        public int BonusAmount { get; }

        public bool IsSecretStatBonus { get; }

        public StatBonusAppliedEventArgs(StatType stat, int bonusAmount, bool isSecretStatBonus)
        {
            Stat = stat;
            BonusAmount = bonusAmount;
            IsSecretStatBonus = isSecretStatBonus;
        }

    }
}
