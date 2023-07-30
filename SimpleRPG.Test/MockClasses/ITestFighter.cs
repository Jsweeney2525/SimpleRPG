using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;

namespace SimpleRPG.Test.MockClasses
{
    public interface ITestFighter
    {
        #region stats

        void SetHealth(int maxHealth, int? currentHealth = null);

        void SetMana(int maxMana, int? currentMana = null);

        void SetStrength(int strength);

        void SetMagicStrength(int strength);

        void SetMagicBonus(MagicType magicType, int strength);

        void SetDefense(int defense);

        void SetMagicResistance(int resistance);

        void SetResistanceBonus(MagicType magicType, int resistance);

        void SetSpeed(int speed);

        void SetEvade(int evade);

        void SetLuck(int luck);

        #endregion

        #region magic affinities

        /// <summary>
        /// Ensures the TestFighter has <see cref="FighterMagicRelationshipType"/> "None" for a given element
        /// </summary>
        /// <param name="magicType"></param>
        void ResetElementalAffinity(MagicType magicType);

        void SetElementalWeakness(MagicType magicType);

        void SetElementalResistance(MagicType magicType);

        void SetElementalImmunity(MagicType magicType);

        #endregion

        #region battle mocks

        /// <summary>
        /// hard codes which move will be returned when SelectMove() is called. 
        /// If this method has not been called, the default MenuInput from <see cref="HumanFighter"/> will be used
        /// </summary>
        /// <param name="move">The move to be returned when SelectMove() is called</param>
        /// <param name="numTurns">An option parameter to determine how many turns in a row a move should be used.
        /// Useful if different behavior is required from the second turn, for example 
        /// (e.g. cast a spell on turn 1 then run on turn 2, specify move as a Spell, numTurns as 1, then specify a run away move for turn 2)</param>
        void SetMove(BattleMove move, int? numTurns = null);

        BattleMove GetMove();

        bool HasMove();

        void SetMoveTarget(IFighter target);

        IFighter GetMoveTarget(BattleMove moveToExecute = null);

        /// <summary>
        /// Call this method to make the fighter die the next time TurnEnd is fired
        /// </summary>
        void SetDeathOnTurnEndEvent();

        /// <summary>
        /// Call this method to immediately kill a player
        /// </summary>
        void DealMaxPhysicalDamage();

        #endregion
    }
}
