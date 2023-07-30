using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Magic;
using SimpleRPG.Battle.Statuses;
using SimpleRPG.Enums;
using SimpleRPG.Helpers;

namespace SimpleRPG.Battle.Fighters.Enemies
{
    public class Warrior : EnemyFighter
    {
        protected int _attackIndex;

        protected int _attackBoostIndex;

        protected int _evadeIndex;

        protected int _evadeAndCounterIndex;

        protected StatusMove _attackBoostMove;

        public Warrior(int level, IChanceService chanceService, string name = null) :
            base(name ?? "Warrior"
                , level
                , LevelUpManager.GetHealthByLevel<Warrior>(level)
                , LevelUpManager.GetManaByLevel<Warrior>(level)
                , LevelUpManager.GetStrengthByLevel<Warrior>(level)
                , LevelUpManager.GetDefenseByLevel<Warrior>(level)
                , LevelUpManager.GetSpeedByLevel<Warrior>(level)
                , LevelUpManager.GetEvadeByLevel<Warrior>(level)
                , LevelUpManager.GetLuckByLevel<Warrior>(level)
                , chanceService
                , SpellFactory.GetSpellsByLevel<Warrior>(level)
                , MoveFactory.GetMovesByLevel<Warrior>(level))
        {
            _attackIndex = AvailableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Attack);
            _evadeIndex = AvailableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is AutoEvadeStatus && !((AutoEvadeStatus)((StatusMove) bm).Status).ShouldCounterAttack);
            _evadeAndCounterIndex = AvailableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is AutoEvadeStatus && ((AutoEvadeStatus)((StatusMove)bm).Status).ShouldCounterAttack);
            _attackBoostIndex = AvailableMoves.FindIndex(bm => bm.MoveType == BattleMoveType.Status && (bm as StatusMove)?.Status is StatMultiplierStatus);

            _attackBoostMove = AvailableMoves[_attackBoostIndex] as StatusMove;
        }

        public override BattleMove SelectMove(Team ownTeam, Team enemyTeam)
        {
            double[] chancesArray = GenerateMoveChances();

            int whichMoveIndex = ChanceService.WhichEventOccurs(chancesArray);

            BattleMove ret = AvailableMoves[whichMoveIndex];

            return ret;
        }

        protected double[] GenerateMoveChances()
        {
            bool hasAttackBoostStatus = Statuses.Find(s => s.AreEqual(_attackBoostMove?.Status)) != null;

            double[] chancesArray = new double[AvailableMoves.Count];

            if (AvailableMoves.Count == 3)
            {
                chancesArray[_attackIndex] = hasAttackBoostStatus ? 2.0/3 : 0.25;
                chancesArray[_evadeIndex] = hasAttackBoostStatus ? 1.0/6 : 0.25;
                chancesArray[_attackBoostIndex] = hasAttackBoostStatus ? 1.0/6 : 0.5;
            }
            else //note: assumes Warrior can only be in one of two states, 3 moves (for level 1-2) and 4 moves (level 3+)
            {
                chancesArray[_attackIndex] = hasAttackBoostStatus ? .65 : .25;
                chancesArray[_evadeIndex] = .15;
                chancesArray[_evadeAndCounterIndex] = .10;
                chancesArray[_attackBoostIndex] = hasAttackBoostStatus ? .10 : .5;
            }

            return chancesArray;
        }
    }
}
