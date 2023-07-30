using System;
using System.Collections.Generic;
using System.Linq;
using SimpleRPG.Battle;
using SimpleRPG.Battle.BattleMoves;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Helpers;
using SimpleRPG.Screens.Menus.MenuActions;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class Menu : IMenu
    {
        protected bool _allowBackOption;

        protected bool _allowHelpOption;

        protected bool _allowStatusOption;

        protected bool _shuffleInputsFlag;

        /// <summary>
        /// If this value is true (defaults to false),
        /// <see cref="GetInput(BattleMove)"/> should be called
        /// </summary>
        public bool RequiresBattleMoveInput { get; protected set; }

        public string PromptText { get; protected set; }
                                        
        public string ErrorText { get;  protected set; }
                                        
        public string HelpText { get;   protected set; }

        public List<MenuAction> MenuActions { get; protected set; }

        protected IInput _input;

        protected IOutput _output;

        protected IChanceService _chanceService;

        public IFighter Owner { get; protected set; }

        protected Team _ownTeam;

        protected Team _enemyTeam;

        protected List<TerrainInteractable> _terrainInteractables;

        protected bool _hasBeenBuilt;

        public Menu(bool allowBackOption, 
            bool allowHelpOption, 
            bool allowStatusOption, 
            string prompt, 
            string errorText, 
            string helpText, 
            List<MenuAction> menuActions, 
            IInput input, 
            IOutput output, 
            IChanceService chanceService = null, 
            bool shuffleMenuItems = false,
            bool requiresBattleMoveInput = false)
        {
            _hasBeenBuilt = false;
            _allowBackOption = allowBackOption;
            _allowHelpOption = allowHelpOption;
            _allowStatusOption = allowStatusOption;
            _shuffleInputsFlag = shuffleMenuItems;
            RequiresBattleMoveInput = requiresBattleMoveInput;

            PromptText = prompt;
            ErrorText = errorText ?? Globals.GenericErrorMessage;
            HelpText = helpText;

            MenuActions = menuActions;

            _input = input;
            _output = output;
            _chanceService = chanceService ?? Globals.ChanceService;
        }

        /// <summary>
        /// Call this when help and status flags should be true, and shuffle should be false, and options should not be shuffled
        /// </summary>
        /// <param name="allowBackOption"></param>
        /// <param name="prompt"></param>
        /// <param name="helpText"></param>
        /// <param name="menuActions"></param>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="requiresBattleMoveInput"></param>
        public Menu(bool allowBackOption, string prompt, string helpText, List<MenuAction> menuActions,
            IInput input, IOutput output, bool requiresBattleMoveInput = false)
            : this(allowBackOption, true, true, prompt, null, helpText, menuActions, input, output, null, false, requiresBattleMoveInput)
        {

        }

        //TODO: don't need ownTeam input, can be keyed off of owner's Team property
        public virtual void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
            Owner = owner;
            _ownTeam = ownTeam;
            _enemyTeam = enemyTeam;
            _terrainInteractables = terrainInteractables;

            _hasBeenBuilt = true;

            foreach (var menuAction in MenuActions)
            {
                menuAction.SubMenu?.Build(owner, ownTeam, enemyTeam, terrainInteractables);
            }
        }

        protected List<MenuAction> FilterAndShufflMenuActions()
        {
            List<MenuAction> nonHiddenMenuActions = MenuActions.Where(ma => !ma.IsHidden).ToList();

            if (_shuffleInputsFlag)
            {
                nonHiddenMenuActions = _chanceService.Shuffle(nonHiddenMenuActions).ToList();
            }

            return nonHiddenMenuActions;
        }

        public virtual MenuSelection GetInput()
        {
            if (!_hasBeenBuilt)
            {
                throw new InvalidOperationException("This menu has not yet been built!");
            }

            string input;
            bool continuer = true;
            MenuSelection ret = null;

            List<MenuAction> nonHiddenMenuActions = FilterAndShufflMenuActions();

            do
                {
                var showError = false;

                PrintFullMenuPrompt(nonHiddenMenuActions);

                input = _input.ReadInput().ToLower();
                var selectedAction = MenuActions.SingleOrDefault(ma => 
                ma.CommandText.ToLower() == input || 
                //only check against AltText if AltText is non-empty
                (ma.AltText?.ToLower() == input)
                );

                if (selectedAction == null)
                {
                    if (input == "help" && _allowHelpOption)
                    {
                        _output.ClearScreen();
                        _output.WriteLine(HelpText);
                        _input.WaitAndClear(_output);
                    }
                    else if (input == "status" && _allowStatusOption)
                    {
                        _output.ClearScreen();
                        PrintStatusScreens();
                        _input.WaitAndClear(_output);
                    }
                    else if (input == "back" && _allowBackOption)
                    {
                        ret = new MenuSelection("back", null, null);
                        continuer = false;
                        _output.ClearScreen();
                    }
                    else
                    {
                        int numInput;
                        if (!int.TryParse(input, out numInput)) //not in the list of menu options, not a valid number
                        {
                            showError = true;
                        }
                        else if (numInput < 1 || numInput > nonHiddenMenuActions.Count) //invalid bounds
                        {
                            showError = true;
                        }
                        else
                        {
                            selectedAction = nonHiddenMenuActions[numInput - 1];
                        }
                    }
                }
                else if (selectedAction.IsDisabled)
                {
                    showError = true;
                }

                if (showError)
                {
                    _output.WriteLineError(ErrorText);
                    _input.WaitAndClear(_output);
                }
                else if (selectedAction != null)
                {
                    _output.ClearScreen();
                    IMenu subMenu = selectedAction.SubMenu;
                    if (subMenu == null)
                    {
                        continuer = false;
                        ret = selectedAction.ConvertToMenuSelection();
                    }
                    else
                    {
                        ret = GetSubMenuInput(subMenu, selectedAction);

                        continuer = (ret.Description == "back");                  
                    }
                }
            } while (continuer);

            return ret;
        }

        /// <summary>
        /// Call this method when the action has already been selected, but further input is required
        /// (e.g. "Attack" has already been selected, so the move is "Attack", but a target is missing)
        /// </summary>
        /// <param name="move"></param>
        /// <param name="moveExecutor">In some special cases, the specific executor of the action must be specified, in most cases it will be null</param>
        /// <returns></returns>
        public virtual MenuSelection GetInput(BattleMove move, IMoveExecutor moveExecutor)
        {
            return GetInput();
        }

        protected virtual MenuSelection GetSubMenuInput(IMenu subMenu, MenuAction selectedAction)
        {
            MenuSelection ret;

            //TargetMenu targetMenu = subMenu as TargetMenu;
            ConfirmationMenu confirmationMenu = subMenu as ConfirmationMenu;

            //if (targetMenu != null)
            //{
            //    ret = subMenu.GetInput(selectedAction.BattleMove);
            //}
            //else if (confirmationMenu != null)
            if (confirmationMenu != null)
            {
                ret = confirmationMenu.GetInput(selectedAction);
            }
            else
            {
                ret = subMenu.RequiresBattleMoveInput ? subMenu.GetInput(selectedAction.BattleMove, selectedAction.MoveExecutor) : subMenu.GetInput();
            }

            return ret;
        }

        protected virtual void PrintPromptHeader(string promptText = null)
        {
            promptText = promptText ?? PromptText;
            promptText = promptText.Replace(Globals.OwnerReplaceText, Owner?.DisplayName);
            _output.WriteLine(promptText);
        }

        /// <summary>
        /// Prints the prompt, the menu actions, and any additional options (e.g. status, back, help)
        /// </summary>
        /// <param name="actionsToDisplay">The actions that will be displayed. This should already have filtered out the hidden actions, and have been shuffled</param>
        protected void PrintFullMenuPrompt(IEnumerable<MenuAction> actionsToDisplay)
        {
            PrintPromptHeader();

            var i = 1;

            //IEnumerable<MenuAction> actionsToDisplay = MenuActions.Where(ma => !ma.IsHidden);
            //
            //if (_shuffleInputsFlag)
            //{
            //    actionsToDisplay = _chanceService.Shuffle(MenuActions);
            //}

            foreach (var action in actionsToDisplay)
            {
                ConsoleColor? color = null;

                if (action.IsDisabled)
                {
                    color = Globals.DisabledColor;
                }

                _output.WriteLine(i++ + ". " + action.DisplayText, color);
            }

            if (_allowStatusOption)
            {
                _output.WriteLine("(or check 'status')");
            }

            if (_allowBackOption)
            {
                _output.WriteLine("('back' to return to the previous screen)");
            }

            if(_allowHelpOption)
            {
                _output.WriteLine("(or type 'help' for help)");
            }
        }

        protected void PrintStatusScreens()
        {
            foreach (var player in _ownTeam.Fighters)
            {
                _output.WriteLine($"Fighter: {player.DisplayName}");
                _output.WriteLine($"HP: {player.CurrentHealth} / {player.MaxHealth}");
                _output.WriteLine($"Mana: {player.CurrentMana} / {player.MaxMana}");
                _output.WriteLine($"Strength: {player.Strength}");
                _output.WriteLine($"Defense: {player.Defense}");
                _output.WriteLine($"Speed: {player.Speed}");
                _input.WaitAndClear(_output);
            }

            _output.WriteLine("Remaining Foes:");
            foreach (var enemy in _enemyTeam.Fighters.Where(e => e.IsAlive()))
            {
                _output.WriteLine($"{enemy.DisplayName}");
            }

            if (_terrainInteractables.Count > 0)
            {
                _input.WaitAndClear(_output);
                _output.WriteLine("Other details:");

                foreach (TerrainInteractable terrainInteractable in _terrainInteractables)
                {
                    _output.WriteLine(terrainInteractable.GetFullDisplayString());
                }
            }
        }
    }
}
