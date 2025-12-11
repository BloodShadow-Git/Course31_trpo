namespace BloodShadow.Core.Logger
{
    public abstract class Logger
    {
        public const ConsoleColor ERROR_FORE_COLOR = ConsoleColor.Red;
        public const ConsoleColor ERROR_BACK_COLOR = ConsoleColor.Black;

        public const ConsoleColor WARNING_FORE_COLOR = ConsoleColor.Yellow;
        public const ConsoleColor WARNING_BACK_COLOR = ConsoleColor.Black;

        public const ConsoleColor MESSAGE_FORE_COLOR = ConsoleColor.White;
        public const ConsoleColor MESSAGE_BACK_COLOR = ConsoleColor.Black;
        public string ReadLineError(object? mess) { return ColorReadLine(mess, ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public string ReadLineWarning(object? mess) { return ColorReadLine(mess, WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public string ReadLineMessage(object? mess) { return ColorReadLine(mess, MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public string ReadLineError() { return ColorReadLine(ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public string ReadLineWarning() { return ColorReadLine(WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public string ReadLineMessage() { return ColorReadLine(MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public string ReadError(object? mess) { return ColorRead(mess, ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public string ReadWarning(object? mess) { return ColorRead(mess, WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public string ReadMessage(object? mess) { return ColorRead(mess, MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public void WriteLineError(object? mess) { ColorWriteLine(mess, ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public void WriteLineWarning(object? mess) { ColorWriteLine(mess, WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public void WriteLineMessage(object? mess) { ColorWriteLine(mess, MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public void WriteLineError() { ColorWriteLine(ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public void WriteLineWarning() { ColorWriteLine(WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public void WriteLineMessage() { ColorWriteLine(MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public void WriteError(object? mess) { ColorWrite(mess, ERROR_FORE_COLOR, ERROR_BACK_COLOR); }
        public void WriteWarning(object? mess) { ColorWrite(mess, WARNING_FORE_COLOR, WARNING_BACK_COLOR); }
        public void WriteMessage(object? mess) { ColorWrite(mess, MESSAGE_FORE_COLOR, MESSAGE_BACK_COLOR); }

        public abstract string ColorReadLine(object? mess, ConsoleColor BC, ConsoleColor FC);
        public abstract string ColorReadLine(ConsoleColor BC, ConsoleColor FC);

        public abstract string ColorRead(object? mess, ConsoleColor BC, ConsoleColor FC);

        public abstract void ColorWriteLine(object? mess, ConsoleColor BC, ConsoleColor FC);
        public abstract void ColorWriteLine(ConsoleColor BC, ConsoleColor FC);

        public abstract void ColorWrite(object? mess, ConsoleColor BC, ConsoleColor FC);
    }
}
