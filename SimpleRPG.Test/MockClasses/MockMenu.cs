using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Helpers;
using SimpleRPG.Screens;
using SimpleRPG.Screens.Menus;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Test.MockClasses
{
    public class MockMenu : Menu
    {
        private readonly Queue<MenuSelection> _menuSelections;

        /// <summary>
        /// This property is really dumb, used only to test that <see cref="GetInput(BattleMove)"/> is called correctly
        /// </summary>
        private readonly Queue<MenuSelection> _requiresBattleMoveMenuSelections;

        private bool EchoBattleMoveInput;

        public IMenu InnerMenu { get; private set; }

        public MockMenu(bool allowBackOption = false, bool allowHelpOption = false, bool allowStatusOption = false, string prompt = "", 
            string errorText = "", string helpText = "", List<MenuAction> menuActions = null, IInput input = null, IOutput output = null, 
            IChanceService chanceService = null, bool shuffleMenuItems = false, bool requiresBattleMoveInput = false) 
            : base(allowBackOption, allowHelpOption, allowStatusOption, prompt, errorText, helpText, menuActions, input, output, chanceService, shuffleMenuItems, requiresBattleMoveInput)
        {
            _menuSelections = new Queue<MenuSelection>();
            _requiresBattleMoveMenuSelections = new Queue<MenuSelection>();
            EchoBattleMoveInput = false;
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            Owner = owner;
            _hasBeenBuilt = true;
            InnerMenu?.Build(owner, ownTeam, enemyTeam, terrainInteractables);
        }

        public override MenuSelection GetInput()
        {
            MenuSelection ret;

            if (_menuSelections.Count == 0)
            {
                ret = InnerMenu?.GetInput() ?? base.GetInput();
            }
            else
            {
                PrintFullMenuPrompt();
                ret = _menuSelections.Dequeue();
            }

            return ret;
        }

        public override MenuSelection GetInput(BattleMove move, IMoveExecutor moveExecutor)
        {
            MenuSelection ret;

            if (EchoBattleMoveInput)
            {
                ret = new MenuSelection(move.Description, move, Owner, moveExecutor);
            }
            else
            {
                if (_requiresBattleMoveMenuSelections.Count > 0)
                {
                    PrintFullMenuPrompt();
                    ret = _requiresBattleMoveMenuSelections.Dequeue();
                }
                else if (_menuSelections.Count > 0)
                {
                    PrintFullMenuPrompt();
                    ret = _menuSelections.Dequeue();
                }
                else
                {
                    ret = InnerMenu?.GetInput() ?? base.GetInput();
                }
            }

            return ret;
        }

        protected void PrintFullMenuPrompt()
        {
            if (PromptText != null)
            {
                PrintFullMenuPrompt(MenuActions != null ? FilterAndShufflMenuActions() : new List<MenuAction>());
            }
        }

        public void SetInnerMenu(IMenu menu)
        {
            InnerMenu = menu;
        }

        public void SetNextSelection(params MenuSelection[] selections)
        {
            foreach (MenuSelection menuSelection in selections)
            {
                _menuSelections.Enqueue(menuSelection);
            }
        }

        public void SetNextBattleMoveRequiredSelection(params MenuSelection[] selections)
        {
            foreach (MenuSelection menuSelection in selections)
            {
                _requiresBattleMoveMenuSelections.Enqueue(menuSelection);
            }
        }

        public void SetEchoMode()
        {
            EchoBattleMoveInput = true;
        }

        public void SetMenuActions(IEnumerable<MenuAction> menuActions)
        {
            MenuActions = menuActions?.ToList() ?? new List<MenuAction>();
        }

        public void SetOutput(IOutput output)
        {
            _output = output;
        }

        public void SetPrompt(string prompt)
        {
            PromptText = prompt;
        }

        public void SetChanceService(IChanceService chanceService)
        {
            _chanceService = chanceService;
        }
    }
}
