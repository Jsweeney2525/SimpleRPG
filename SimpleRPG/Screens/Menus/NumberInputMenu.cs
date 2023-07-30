using System.Collections.Generic;
using SimpleRPG.Battle.Fighters;
using SimpleRPG.Battle.TerrainInteractables;
using SimpleRPG.Screens.Menus.MenuSelections;

namespace SimpleRPG.Screens.Menus
{
    public class NumberInputMenu : Menu
    {
        /// <summary>
        /// The inclusive minimum value allowed by the player
        /// </summary>
        protected int MinValue;

        /// <summary>
        /// The inclusive maximum value allowed by the player
        /// </summary>
        protected int MaxValue;

        public NumberInputMenu(string prompt, IInput input, IOutput output, int minValue, int maxValue) 
            : base(true, false, false, prompt, null, null, null, input, output)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override void Build(IFighter owner, Team ownTeam, Team enemyTeam, List<TerrainInteractable> terrainInteractables)
        {
        }

        protected override void PrintPromptHeader(string promptText = null)
        {
            promptText = promptText ?? PromptText;

            promptText = promptText.Replace(Globals.MaxValueReplacementString, MaxValue.ToString());
            promptText = promptText.Replace(Globals.MinValueReplacementString, MinValue.ToString());

            base.PrintPromptHeader(promptText);
        }

        public override MenuSelection GetInput()
        {
            NumberInputMenuSelection ret = null;
            var validRet = false;

            while (!validRet)
            {
                int retAsNumber;

                PrintPromptHeader();

                string input = _input.ReadInput();


                if (input == "")
                {
                    _output.WriteLineError("input is required");
                    _input.WaitAndClear(_output);
                }
                else if (!int.TryParse(input, out retAsNumber))
                {
                    _output.WriteLineError("input not recognized as a number");
                    _input.WaitAndClear(_output);
                }
                else if (retAsNumber < MinValue || retAsNumber > MaxValue)
                {
                    _output.WriteLineError($"input must be between {MinValue} and {MaxValue}, inclusive");
                    _input.WaitAndClear(_output);
                }
                else
                {
                    validRet = true;
                    ret = new NumberInputMenuSelection(retAsNumber);
                }
            }

            return ret;
        }
    }
}
