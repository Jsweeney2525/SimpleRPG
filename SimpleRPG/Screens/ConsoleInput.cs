using System;

namespace SimpleRPG.Screens
{
    public class ConsoleInput : IInput
    {
        public string ReadInput()
        {
            return Console.ReadLine();
        }

        public void WaitOnEnterKey()
        {
            Console.WriteLine("Please press the \"Enter\" key to continue");

            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
            } while (key.Key != ConsoleKey.Enter);
        }

        public void WaitAndClear(IOutput output)
        {
            WaitOnEnterKey();
            output.ClearScreen();
        }
    }
}
