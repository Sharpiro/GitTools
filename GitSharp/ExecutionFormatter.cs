using GitSharp.Tools;
using System;
using System.Linq;

namespace GitSharp
{
    public class ExecutionFormatter
    {
        private GitSharpExecution _gitSharpExecution;
        public ILogger _logger { get; }

        public ExecutionFormatter(GitSharpExecution gitSharpExecution, ILogger logger)
        {
            _gitSharpExecution = gitSharpExecution ?? throw new ArgumentNullException(nameof(gitSharpExecution));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Status()
        {
            var status = _gitSharpExecution.Status();
            _logger.Info("Status");
            string output;
            if (!status.Any())
                output = "No deltas";
            else
                output = string.Join(Environment.NewLine, status.Select(i => $"\t{i.Node.RelativePath}({i.DeltaType})"));
            _logger.Info(output);
        }

        public void Commit()
        {
            var status = _gitSharpExecution.Commit();
            _logger.Info("Commit");
            string output;
            if (!status.Any())
                output = "No deltas";
            else
                output = string.Join(Environment.NewLine, status.Select(i => $"\t{i.Node.RelativePath}({i.DeltaType})"));
            _logger.Info(output);
        }
    }
}