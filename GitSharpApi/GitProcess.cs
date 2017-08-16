using System;

namespace GitSharpApi
{
    public sealed class GitProcess : StringResultProcess
    {
        public GitProcess(string workingDirectory = "", string fileName = "") : base(fileName, workingDirectory)
        {
            var programFiles = Environment.GetEnvironmentVariable("programw6432");
            var gitPath = $"{programFiles}\\git\\cmd\\git.exe";
            FileName = string.IsNullOrEmpty(fileName) ? gitPath : fileName;
        }
    }
}