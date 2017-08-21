using System;
using static System.Console;

namespace GitSharp.Tools
{
    public class ConsoleLogger : ILogger
    {
        public void Debug(string message)
        {
            ForegroundColor = ConsoleColor.DarkGray;
            WriteLine(message);
            ForegroundColor = ConsoleColor.Gray;
        }

        public void Error(string message)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(message);
            ForegroundColor = ConsoleColor.Gray;
        }

        public void Error(Exception ex)
        {
            ForegroundColor = ConsoleColor.Red;
            WriteLine(ex);
            ForegroundColor = ConsoleColor.Gray;
        }

        public void Info(string message, ConsoleColor foregroundColor = ConsoleColor.DarkGreen)
        {
            ForegroundColor = ConsoleColor.DarkGreen;
            WriteLine(message);
            ForegroundColor = ConsoleColor.Gray;
        }

        public void Warning(string message)
        {
            ForegroundColor = ConsoleColor.Yellow;
            WriteLine(message);
            ForegroundColor = ConsoleColor.Gray;
        }
    }
}