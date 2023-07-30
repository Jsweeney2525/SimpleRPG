using SimpleRPG.Screens;
using System.Collections.Generic;

namespace SimpleRPG.Test.MockClasses
{
    public class MockInput : IInput
    {
        private List<string> _inputs;
        private int _index;

        public MockInput()
        {
            _inputs = new List<string>();
            _index = 0;
        }

        public MockInput(string[] inputs) : this()
        {
            _inputs.AddRange(inputs);
        }

        public void Push(string input)
        {
            _inputs.Add(input);
        }

        public void Push(List<string> inputs)
        {
            _inputs.AddRange(inputs);
        }

        public void Push(params string[] inputs)
        {
            _inputs.AddRange(inputs);
        }

        public string ReadInput()
        {
            if (_index < _inputs.Count)
            {
                return _inputs[_index++];
            }
            return "";
        }

        public void WaitOnEnterKey() { }

        public void WaitAndClear(IOutput output)
        {
            WaitOnEnterKey();
            output.ClearScreen();
        }
    }
}
