using GitSharp;
using GitSharp.Tools;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GitSharpApi.Cmd
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            try
            {
                var process = Process.GetCurrentProcess();
                MoveWindow(process.MainWindowHandle, 447, 334, 1000, 550, true);
                const string workingDir = @"C:\Users\sharpiro\Desktop\temp\tempgitsharp";
                var execution = GitSharpExecution.Create(workingDir, logger, new Sha1Hasher(), new NodeFileParser(workingDir));
                var formatter = new ExecutionFormatter(execution, logger);
                var command = args.Single();
                switch (command.ToLower())
                {
                    case "init":
                        execution.Init();
                        break;
                    case "status":
                        formatter.Status();
                        break;
                    case "commit":
                        formatter.Commit();
                        break;
                    default: throw new ArgumentOutOfRangeException($"Invalid command '{command}'");
                }
                //if (!execution.IsKitRepo())
                //    execution.Init();
                //formatter.Status();
                //formatter.Commit();
                //logger.Debug("Press any key to exit...");
                //ReadKey();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}