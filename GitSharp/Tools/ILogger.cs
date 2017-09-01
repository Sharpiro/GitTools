using System;

namespace GitSharp.Tools
{
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message, ConsoleColor foregroundColor = ConsoleColor.DarkGreen);
        void Warning(string message);
        void Error(string message);
        void Error(Exception ex);
    }
}