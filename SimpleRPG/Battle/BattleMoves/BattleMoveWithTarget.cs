using SimpleRPG.Battle.Fighters;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Battle.BattleMoves
{
    public class BattleMoveWithTarget
    {
        public BattleMove Move { get; }

        public IFighter Target { get; private set; }

        public IFighter Owner { get; }

        /// <summary>
        /// Supplied on occasion if the logic to execute the <see cref="Move"/> lies in a specific component.
        /// </summary>
        public IMoveExecutor MoveExecutor { get; }

        public BattleMoveWithTarget(BattleMove move, IFighter target, IFighter owner, IMoveExecutor moveExecutor = null)
        {
            Move = move;
            Target = target;
            Owner = owner;
            MoveExecutor = moveExecutor;
        }

        public BattleMoveWithTarget(MenuSelection selection, IFighter owner)
        {
            Move = selection.Move;
            Target = selection.Target;
            Owner = owner;
            MoveExecutor = selection.MoveExecutor;
        }

        //TODO: should this take a bunch of transform functions from the battleManager, allowing the BattleMoveWIthTarget to house this logic while alos allowing for statuses?
        /*public bool CanBeExecuted(_battleManager.BattleManager battleManager)
        {
            bool ret;

            if (!Owner.IsAlive())
            {
                ret = false;
            }
            else if (Move.MoveType == BattleMoveType.Spell)
            {
                var spell = Move as Spell;

                if (spell == null)
                {
                    throw new ArgumentException($"The battle move {Move.Description} is classified as a spell type, but cannot be cast as a spell!");
                }

                if (!Owner.HasSpell(spell))
                {
                    throw new ArgumentException($"Fighter {Owner.DisplayName} cannot cast {spell.Description}, they have not learned this spell!");
                }

                if (Owner.Statuses.Exists(s => s is MagicSealedStatus))
                {
                    ret = false;
                }
                else
                {
                    int realCost = battleManager.CalculateSpellCost(Owner, spell);

                    ret = realCost <= Owner.CurrentMana;
                }
            }
            else
            {
                ret = true;
            }

            return ret;
        }*/

        public void Retarget(IFighter newTarget)
        {
            Target = newTarget;
        }
    }
}
