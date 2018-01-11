using PoolObserver.Common.Constants;
using System;

namespace PoolObserver.Bot.Managers
{
    public static class LoggingManager
    {
        private static void LogToConsole(string text, LogType type)
        {
            LogDateToConsole();
            SetConsoleColor(type);
            Console.Write(string.Format("{0}\n", text));
        }

        private static void LogDateToConsole()
        {
            SetConsoleColor(LogType.Default);
            Console.Write(string.Format("[{0} {1}] ", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString()));
        }

        private static void SetConsoleColor(LogType type)
        {
            switch (type)
            {
                case LogType.Event:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    }
                case LogType.Error:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    }
                case LogType.Success:
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    }
                case LogType.Warning:
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    }
                default:
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    }
            }
        }

        public static void LogEvent(string text, LogType type = LogType.Default)
        {
            LogToConsole(text, type);
        }
    }
}
