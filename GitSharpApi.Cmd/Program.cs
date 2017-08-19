using GitSharp;
using GitSharp.Tools;
using System;
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
                const string workingDir = @"C:\Users\sharpiro\Desktop\temp\tempgitsharp";
                var execution = new GitSharpExecution(workingDir, logger, new Sha1Hasher());
                var formatter = new ExecutionFormatter(execution);
                if (!execution.IsGitRepo())
                    execution.Init();
                var output = formatter.Status();
                logger.Info(output);
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
    }
}