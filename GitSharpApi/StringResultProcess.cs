using System;
using System.Diagnostics;
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

        public Task<(int ExitCode, string ResultText)> ExecuteCommandAsync(string args)
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
                process.WaitForExit();
                var resultText = process.ExitCode == 0 ? process.StandardOutput.ReadToEnd() : process.StandardError.ReadToEnd();
                return (process.ExitCode, resultText);
            });
        }
    }
}