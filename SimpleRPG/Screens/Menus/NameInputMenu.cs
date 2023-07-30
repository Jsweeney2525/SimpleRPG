using System.Collections.Generic;
using System.Linq;

namespace SimpleRPG.Screens.Menus
{
    //TODO: move this into an abstract class that has some way of validating the input, then returning the input in some way
    public class NameInputMenu
    {
        public const string DefaultPrompt = "Please select your character's name (limit 20 characters)";
        public const string NameRequiredErrorMessage = "A name must be specified";
        public const string NameTooLongErrorMessage = "The name cannot exceed 20 characters";
        public const string DefaultDisallowedNameErrorMessage = "That name is not allowed";
        public const int MaximumLength = 20;

        private IInput _input;
        private IOutput _output;

        public NameInputMenu(IInput input, IOutput output)
        {
            _input = input;
            _output = output;
        }

        public string GetName(string prompt = null, string disallowedErrorMessage = null, params string[] disallowedNames)
        {
            var ret = "";
            var validRet = false;
            List<string> disallowedNamesAsList = disallowedNames.Select(name => name.ToUpperInvariant()).ToList();

            while (!validRet)
            {
                _output.WriteLine(prompt ?? DefaultPrompt);
                ret = _input.ReadInput();
                ret = ret.Trim();

                if (ret == "")
                {
                    _output.WriteLineError(NameRequiredErrorMessage);
                    _input.WaitAndClear(_output);
                }
                else if (ret.Length > MaximumLength)
                {
                    _output.WriteLineError(NameTooLongErrorMessage);
                    _input.WaitAndClear(_output);
                }
                else if (disallowedNamesAsList.Contains(ret.ToUpperInvariant()))
                {
                    _output.WriteLineError(disallowedErrorMessage ?? DefaultDisallowedNameErrorMessage);
                    _input.WaitAndClear(_output);
                }
                else
                {
                    validRet = true;
                }
            }

            return ret;
        }
    }
}
