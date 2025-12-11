using System;

namespace BloodShadow.Core.Logger
{
    class ConsoleLogger : Logger
    {
        public override string ColorReadLine(object? mess, ConsoleColor BC, ConsoleColor FC)
        {
            ColorWriteLine(mess, BC, FC);
            return Console.ReadLine();
        }
        public override string ColorReadLine(ConsoleColor BC, ConsoleColor FC)
        {
            ColorWriteLine(BC, FC);
            return Console.ReadLine();
        }

        public override string ColorRead(object? mess, ConsoleColor BC, ConsoleColor FC)
        {
            ColorWrite(mess, BC, FC);
            return Console.ReadLine();
        }

        public override void ColorWriteLine(object? mess, ConsoleColor BC, ConsoleColor FC)
        {
            ConsoleColor cBC = Console.BackgroundColor;
            ConsoleColor cFC = Console.ForegroundColor;
            Console.BackgroundColor = BC;
            Console.ForegroundColor = FC;
            Console.WriteLine(mess);
            Console.ForegroundColor = cFC;
            Console.BackgroundColor = cBC;
        }
        public override void ColorWriteLine(ConsoleColor BC, ConsoleColor FC)
        {
            ConsoleColor cBC = Console.BackgroundColor;
            ConsoleColor cFC = Console.ForegroundColor;
            Console.BackgroundColor = BC;
            Console.ForegroundColor = FC;
            Console.WriteLine();
            Console.ForegroundColor = cFC;
            Console.BackgroundColor = cBC;
        }

        public override void ColorWrite(object? mess, ConsoleColor BC, ConsoleColor FC)
        {
            ConsoleColor cBC = Console.BackgroundColor;
            ConsoleColor cFC = Console.ForegroundColor;
            Console.BackgroundColor = BC;
            Console.ForegroundColor = FC;
            Console.Write(mess);
            Console.ForegroundColor = cFC;
            Console.BackgroundColor = cBC;
        }
    }
}
