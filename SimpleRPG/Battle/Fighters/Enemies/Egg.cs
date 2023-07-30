using System.Collections.Generic;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Enums;
using SimpleRPG.Events;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Egg: EnemyFighter, IMagicEnemy
    {
        public MagicType MagicType { get; }

        private static readonly BattleMove DoNothingMove = MoveFactory.Get(BattleMoveType.DoNothing);

        public Egg(MagicType type) 
            : base($"{type.ToString().ToLower()} egg", 1, 1, 0, 0, 0, 0, 0, 0, null)
        {
            MagicType = type;
            ExpGivenOnDefeat = 0;

            Killed += RemoveFromTeamOnDeath;

            _availableMoves = new List<BattleMove> { DoNothingMove };
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            return DoNothingMove;
        }

        private void RemoveFromTeamOnDeath(object sender, KilledEventArgs e)
        {
            Team.Remove(this);
        }
    }
}
