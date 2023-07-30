using System;
using SimpleRPG.Screens;
using System.Collections.Generic;

namespace SimpleRPG.Test.MockClasses
{
    public enum MockOutputMessageType { Normal, Error };
    public class MockOutputMessage
    {
        public MockOutputMessageType Type { get; set; }

        public string Message { get; set; }

        public ConsoleColor? Color { get; set; }

        public MockOutputMessage(string message, MockOutputMessageType type, ConsoleColor? color)
        {
            Type = type;
            Message = message;
            Color = color;
        }
    }

    public class MockOutput : IOutput
    {
        private readonly List<MockOutputMessage> _outputs;

        private readonly List<int> _clearIndices;

        public MockOutput()
        {
            _outputs = new List<MockOutputMessage>();
            _clearIndices = new List<int>();
        }

        public void Push(string output, MockOutputMessageType type, ConsoleColor? color)
        {
            _outputs.Add(new MockOutputMessage(output, type, color));
        }

        public MockOutputMessage[] GetOutputs()
        {
            return _outputs.ToArray();
        }

        public int[] GetClearIndices()
        {
            return _clearIndices.ToArray();
        }

        public void ClearScreen()
        {
            _clearIndices.Add(_outputs.Count);
        }

        public void ClearMessages()
        {
            _outputs.Clear();
        }

        public void ClearClearIndices()
        {
            _clearIndices.Clear();
        }

        public void Write(string output, ConsoleColor? color = null)
        {
            Push(output, MockOutputMessageType.Normal, color);
        }

        public void Write(ColorString colorString)
        {
            List<ColorString> subStrings = colorString.SubStrings;
            int subStringLength = subStrings.Count;
            if (subStringLength == 0)
            {
                Push(colorString.Value, MockOutputMessageType.Normal, colorString.Color);
            }
            else
            {
                subStrings.ForEach(Write);
            }
        }

        public void WriteLine(string output, ConsoleColor? color = null)
        {
            Push(output + "\n", MockOutputMessageType.Normal, color);
        }

        public void WriteLine(ColorString colorString)
        {
            List<ColorString> subStrings = colorString.SubStrings;
            int subStringLength = subStrings.Count;
            if (subStringLength == 0)
            {
                Push(colorString.Value + "\n", MockOutputMessageType.Normal, colorString.Color);
            }
            else
            {
                for (var i = 0; i < subStringLength; ++i)
                {
                    ColorString subString = subStrings[i];

                    if (i == subStringLength - 1)
                    {
                        WriteLine(subString);
                    }
                    else
                    {
                        Write(subString);
                    }
                }
            }
        }

        public void WriteError(string output)
        {
            Push(output, MockOutputMessageType.Error, Globals.ErrorColor);
        }

        public void WriteLineError(string output)
        {
            Push(output + "\n", MockOutputMessageType.Error, Globals.ErrorColor);
        }
    }
}
