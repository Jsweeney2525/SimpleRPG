using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Screens.Menus
{
    public class MultiTurnSelection
    {
        public MultiTurnBattleMove Move { get; }

        public IFighter Owner { get; }

        public int Index { get; protected set; }

        public IFighter Target { get; }

        public MultiTurnSelection(IFighter owner, MultiTurnBattleMove move, int index, IFighter target)
        {
            Move = move;
            Owner = owner;
            Index = index;
            Target = target;
        }

        public void IncrementIndex()
        {
            ++Index;
        }
    }

    public class MenuManager
    {
        protected Team _humanTeam;

        protected Team _enemyTeam;

        protected readonly IInput _input;

        protected readonly IOutput _output;

        protected readonly IMenuFactory _menuFactory;

        protected List<IMenu> _menus;

        protected MultiTurnSelection[] _multiTurnSelections;

        #region events

        public EventHandler<RanEventArgs> Ran { get; set; }

        public void OnRun(RanEventArgs e)
        {
            Ran?.Invoke(this, e);
        }

        #endregion

        public MenuManager(IInput input, IOutput output, IMenuFactory menuFactory)
        {
            _input = input;
            _output = output;
            _menuFactory = menuFactory;
        }

        public virtual void InitializeForBattle(Team myTeam, Team enemyTeam)
        {
            _humanTeam = myTeam;
            _enemyTeam = enemyTeam;

            int numHumanFighters = _humanTeam.GetHumanFighters().Count();
            _multiTurnSelections = new MultiTurnSelection[numHumanFighters];

            for (var i = 0; i < numHumanFighters; ++i)
            {
                _multiTurnSelections[i] = null;
            }
        }

        public virtual List<BattleMoveWithTarget> GetInputs()
        {
            List<BattleMoveWithTarget> inputs = new List<BattleMoveWithTarget>();

            List<BattleMoveWithTarget> humanInputs = GetInputs(_humanTeam.GetHumanFighters().ToList());
            inputs.AddRange(humanInputs);

            return inputs;
        }

        public virtual List<BattleMoveWithTarget> GetInputs(IList<HumanFighter> fighters, List<MenuAction> specialMenuActions = null)
        {
            BuildMenus(fighters, specialMenuActions);

            var length = _menus.Count;
            var ret = new List<BattleMoveWithTarget>(new BattleMoveWithTarget[length]);

            for (var i = 0; i < length;)
            {
                IFighter fighter = _menus[i].Owner;

                BattleMoveWithTarget multiTurnmove = FindAndProcessMultiTurnMove(fighter, _multiTurnSelections, i);

                if (multiTurnmove != null)
                {
                    ret[i] = multiTurnmove;
                    ++i;
                }
                else
                {
                    var menuSelection = _menus[i].GetInput();

                    switch (menuSelection.Description)
                    {
                        case "run":
                            OnRun(new RanEventArgs());
                            for (var j = 0; j < length; ++j)
                            {
                                ret[j] = null;
                            }
                            i = length;
                            break;
                        case "back":
                            --i;
                            break;
                        default:

                            var multiTurn = menuSelection.Move as MultiTurnBattleMove;
                            if (multiTurn != null)
                            {
                                _multiTurnSelections[i] = new MultiTurnSelection(fighter, multiTurn, 1,
                                    menuSelection.Target);

                                var move = multiTurn.Moves[0];

                                var target = move.TargetType == TargetType.Self ? fighter : menuSelection.Target;

                                ret[i] = new BattleMoveWithTarget(move, target, fighter);
                            }
                            else
                            {
                                ret[i] = menuSelection.Convert(fighter);
                            }

                            ++i;
                            break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Given a particular array of MultiTurnSelections, an index, and the moveOwner, either returns null (no multiTurn move for the given fighter),
        /// or returns the appropriate BattleMoveWithTarget, and updates the selectionArray
        /// </summary>
        /// <param name="moveOwner"></param>
        /// <param name="selectionArray"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected BattleMoveWithTarget FindAndProcessMultiTurnMove(IFighter moveOwner, MultiTurnSelection[] selectionArray, int index)
        {
            BattleMoveWithTarget ret = null;

            MultiTurnSelection multiTurnSelection = selectionArray.FirstOrDefault(mts => mts != null && mts.Owner == moveOwner);
            if (multiTurnSelection != null)
            {
                var multiTurnIndex = multiTurnSelection.Index;
                var move = multiTurnSelection.Move.Moves[multiTurnIndex];
                var target = (move.TargetType == TargetType.Self) ? moveOwner : multiTurnSelection.Target;

                ret = new BattleMoveWithTarget(move, target, moveOwner);

                multiTurnSelection.IncrementIndex();

                if (multiTurnSelection.Index >= multiTurnSelection.Move.Moves.Count)
                {
                    selectionArray[index] = null;
                }
            }

            return ret;
        }

        protected void BuildMenus(IList<HumanFighter> fighters, List<MenuAction> specialMenuActions = null, List<TerrainInteractable> terrainInteractables = null)
        {
            _menus = new List<IMenu>();

            var i = 0;
            foreach (var fighter in fighters.Where(f => f.IsAlive()))
            {
                var menu = _menuFactory.GetMenu(MenuType.BattleMenu, _input, _output, i != 0, specialMenuActions: specialMenuActions);
                menu.Build(fighter, _humanTeam, _enemyTeam, terrainInteractables ?? new List<TerrainInteractable>());
                _menus.Add(menu);
                ++i;
            }
        }

        protected MultiTurnSelection[] CopyMultiTurnSelectionArray(IList<HumanFighter> fighters)
        {
            int numFighters = fighters.Count;
            MultiTurnSelection[] copy = new MultiTurnSelection[numFighters];

            for (var j = 0; j < numFighters; ++j)
            {
                copy[j] = _multiTurnSelections.FirstOrDefault(mts => mts != null && mts.Owner == fighters[j]);
            }

            return copy;
        }
    }
}
