using System;

namespace SimpleRPG.Screens
{
    public interface IOutput
    {
        void ClearScreen();

        void Write(ColorString output);

        void Write(string output, ConsoleColor? color = null);

        void WriteLine(ColorString output);

        void WriteLine(string output, ConsoleColor? color = null);

        void WriteError(string output);

        void WriteLineError(string output);
    }
}
