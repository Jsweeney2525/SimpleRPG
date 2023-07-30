using System;
using System.Collections.Generic;

namespace SimpleRPG.Screens
{
    public class ConsoleOutput : IOutput
    {
        public void ClearScreen()
        {
            Console.Clear();
        }

        public void Write(string output, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.Write(output);
            Console.ResetColor();
        }

        public void Write(ColorString colorString)
        {
            List<ColorString> subStrings = colorString.SubStrings;
            int subStringLength = subStrings.Count;
            if (subStringLength == 0)
            {
                Console.ForegroundColor = colorString.Color;
                Console.Write(colorString.Value);
                Console.ResetColor();
            }
            else
            {
                subStrings.ForEach(Write);
            }
        }

        public void WriteLine(string output, ConsoleColor? color = null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }
            Console.WriteLine(output);
            Console.ResetColor();
        }

        public void WriteLine(ColorString colorString)
        {
            List<ColorString> subStrings = colorString.SubStrings;
            int subStringLength = subStrings.Count;
            if (subStringLength == 0)
            {
                Console.ForegroundColor = colorString.Color;
                Console.WriteLine(colorString.Value);
                Console.ResetColor();
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
            Console.ForegroundColor = Globals.ErrorColor;
            Console.Write(output);
            Console.ResetColor();
        }

        public void WriteLineError(string output)
        {
            Console.ForegroundColor = Globals.ErrorColor;
            Console.WriteLine(output);
            Console.ResetColor();
        }
    }
}
