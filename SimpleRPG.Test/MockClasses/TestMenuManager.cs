using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Enums;
using SimpleRPG.Events;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;

namespace SimpleRPG.Test.MockClasses
{
    public class TestMenuManager : MenuManager
    {
        protected MultiTurnSelection[] _testFigherMultiTurnSelections;

        public static TestMenuManager GetTestMenuManager()
        {
            return new TestMenuManager(new MockInput(), new MockOutput());
        }

        public TestMenuManager(IInput input, IOutput output, IMenuFactory menuFactory = null)
            : base(input, output, menuFactory ?? Globals.MenuFactory)
        {   
        }

        public override void InitializeForBattle(Team myTeam, Team enemyTeam)
        {
            base.InitializeForBattle(myTeam, enemyTeam);

            List<ITestFighter> testFighters = myTeam.GetHumanFighters().OfType<ITestFighter>().ToList();

            _testFigherMultiTurnSelections = new MultiTurnSelection[testFighters.Count];
        }

        public override List<BattleMoveWithTarget> GetInputs(IList<HumanFighter> fighters, List<MenuAction> specialMenuActions = null)
        {
            //separate given list into testFighters with pre-selected moves and those that take the typical path of menus and IInput's input
            List<ITestFighter> testFightersWithMoves = new List<ITestFighter>();

            List<HumanFighter> regularInputFighters = new List<HumanFighter>();

            foreach (HumanFighter fighter in fighters)
            {
                ITestFighter testFighter = fighter as ITestFighter;

                if (testFighter == null)
                {
                    regularInputFighters.Add(fighter);
                }
                else if (!testFighter.HasMove())
                {
                    regularInputFighters.Add(fighter);
                }
                else
                {
                    testFightersWithMoves.Add(testFighter);
                }
            }

            List<BattleMoveWithTarget> inputs = new List<BattleMoveWithTarget>();

            List<BattleMoveWithTarget> regularInputs = base.GetInputs(regularInputFighters, specialMenuActions);

            inputs.AddRange(regularInputs);

            int testFighterLength = testFightersWithMoves.Count;

            List<BattleMoveWithTarget> testFighterInputs = new List<BattleMoveWithTarget>();

            for (var i = 0; i < testFighterLength; ++i)
            {
                testFighterInputs.Add(null);
            }

            for (var i = 0; i < testFighterLength; ++i)
            {
                ITestFighter testFighter = testFightersWithMoves[i];
                IFighter testFighterAsFighter = testFighter as IFighter;

                if (testFighterAsFighter == null)
                {
                    throw new InvalidOperationException("this should be impossible! Somehow a HumanFighter was castable to type ITestFighter, but not IFighter");
                }

                BattleMoveWithTarget multiTurnMove = FindAndProcessMultiTurnMove(testFighterAsFighter,
                    _testFigherMultiTurnSelections, i); 

                if (multiTurnMove != null)
                {
                    testFighterInputs[i] = multiTurnMove;
                }
                else
                {
                    var move = testFighter.GetMove();

                    if (move.MoveType == BattleMoveType.Runaway)
                    {
                        OnRun(new RanEventArgs());
                        for (var j = 0; j < testFighterLength; ++j)
                        {
                            testFighterInputs[j] = null;
                        }
                        break;
                    }
                    else
                    {
                        IFighter target = testFighter.GetMoveTarget();
                        BattleMoveWithTarget processedMove = ProcessMove(move, i, testFighter as IFighter, target);
                        testFighterInputs[i] = processedMove;
                    }
                }
            }

            inputs.AddRange(testFighterInputs);

            return inputs;
        }

        protected BattleMoveWithTarget ProcessMove(BattleMove move, int index, IFighter fighter, IFighter originalTarget)
        {
            BattleMoveWithTarget ret;
            var multiTurn = move as MultiTurnBattleMove;
            if (multiTurn != null)
            {
                _testFigherMultiTurnSelections[index] = new MultiTurnSelection(fighter, multiTurn, 1,
                    originalTarget);

                var firstMove = multiTurn.Moves[0];

                var moveTarget = firstMove.TargetType == TargetType.Self ? fighter : originalTarget;

                ret = new BattleMoveWithTarget(move, moveTarget, fighter);
            }
            else
            {
                ret = new BattleMoveWithTarget(move, originalTarget, fighter);
            }

            return ret;
        }
    }
}
