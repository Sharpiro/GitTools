using System;
using System.Linq;

namespace GitSharp
{
    public class ExecutionFormatter
    {
        private GitSharpExecution _gitSharpExecution;

        public ExecutionFormatter(GitSharpExecution gitSharpExecution)
        {
            _gitSharpExecution = gitSharpExecution ?? throw new ArgumentNullException(nameof(gitSharpExecution));
        }


        public string Status()
        {
            var status = _gitSharpExecution.Status();
            return string.Join(Environment.NewLine, status.Select(i => i.FileInfo.Name));
        }
    }
}