using GitSharp;
using GitSharp.Tools;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static System.Console;

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
                var execution = GitSharpExecution.Create(workingDir, logger, new Sha1Hasher(), new NodeParser(workingDir));
                var formatter = new ExecutionFormatter(execution, logger);
                if (!execution.IsKitRepo())
                    execution.Init();
                //formatter.Status();
                formatter.Commit();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                logger.Debug("Press any key to exit...");
                ReadKey();
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    }
}