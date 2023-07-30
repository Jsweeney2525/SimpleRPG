using System;

namespace SimpleRPG.Events
{
    public class LeveledUpEventArgs : EventArgs
    {
        public int NewLevel { get; private set; }

        /// <summary>
        /// How much the fighter's Health has been increased
        /// </summary>
        public int HealthBoost { get; private set; }

        /// <summary>
        /// How much the fighter's Mana has been increased
        /// </summary>
        public int ManaBoost { get; private set; }

        /// <summary>
        /// How much the fighter's Strength has been increased
        /// </summary>
        public int StrengthBoost { get; private set; }

        /// <summary>
        /// How much the fighter's Defense has been increased
        /// </summary>
        public int DefenseBoost { get; private set; }

        /// <summary>
        /// How much the fighter's Speed has been increased
        /// </summary>
        public int SpeedBoost { get; private set; }

        public LeveledUpEventArgs(int level, int healthBoost, int manaBoost, int strengthBoost, int defenseBoost, int speedBoost)
        {
            NewLevel = level;
            HealthBoost = healthBoost;
            ManaBoost = manaBoost;
            StrengthBoost = strengthBoost;
            DefenseBoost = defenseBoost;
            SpeedBoost = speedBoost;
        }
    }
}
