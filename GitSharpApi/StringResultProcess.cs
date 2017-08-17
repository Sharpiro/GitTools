using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GitSharpApi
{
    public class StringResultProcess
    {
        protected string FileName { get; set; }
        public string WorkingDirectory { get; set; }

        public StringResultProcess(string fileName, string workingDirectory = "")
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public Task<(int ExitCode, string ResultText)> ExecuteCommandAsync(string args, int timeoutSeconds = 10)
        {
            return Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo =
                {
                    FileName = FileName,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory
                }
                };
                process.Start();
                var outputText = process.StandardOutput.ReadToEnd();
                var errorText = process.StandardOutput.ReadToEnd();
                var exitSuccess = process.WaitForExit(timeoutSeconds * 1000);
                if (!exitSuccess) throw new InvalidOperationException($"The process failed to return within {timeoutSeconds} seconds");
                var resultText = outputText + errorText;
                return (process.ExitCode, resultText);
            });
        }
    }
}