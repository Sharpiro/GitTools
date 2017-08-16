using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitSharpApi.Models;

namespace GitSharpApi
{
    public class GitApi
    {
        private readonly GitProcess _gitProcess;

        public GitApi(GitProcess gitProcess)
        {
            _gitProcess = gitProcess ?? throw new ArgumentNullException(nameof(gitProcess));
        }

        public Task<(int ExitCode, string ResultText)> GetStatusAsync()
        {
            const string args = "status";
            return _gitProcess.ExecuteCommandAsync(args);
        }

        public async Task<string> GetRootDirectoryAsync()
        {
            const string args = "rev-parse --show-toplevel";
            var response = await _gitProcess.ExecuteCommandAsync(args);
            return Path.GetFullPath(response.ResultText);
        }

        public async Task<IEnumerable<string>> GetObjectsAsync()
        {
            var rootDirectory = await GetRootDirectoryAsync();
            var objectsDirectory = new DirectoryInfo($"{rootDirectory}\\.git\\objects");
            if (!objectsDirectory.Exists) throw new DirectoryNotFoundException($"Unable to locate objects directory @ '{objectsDirectory.FullName}'");
            var objects = objectsDirectory.GetFiles("*.*", SearchOption.AllDirectories).Select(f =>
            {
                var parent = f.Directory?.Name;
                if (parent == null) throw new DirectoryNotFoundException($"Unable to determine parent directory for file '{f.FullName}'");
                return $"{parent}{f.Name}";
            });
            return objects;
        }

        public async Task<string> CatFileAsync(string sha1)
        {
            var args = $"cat-file -p {sha1}";
            var response = await _gitProcess.ExecuteCommandAsync(args);
            return response.ResultText;
        }

        public async Task<IEnumerable<Commit>> GetCommits(int limit = 1000)
        {
            var args = $"log -{limit} --pretty=format:\"%H,%an,%ad,%s\" --date=iso-strict";
            var response = await _gitProcess.ExecuteCommandAsync(args);
            var commitStrings = response.ResultText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var commits = commitStrings.Select(commitString => commitString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(commitDetails => new Commit
                {
                    Hash = commitDetails[0],
                    Author = commitDetails[1],
                    AuthorDate = DateTime.Parse(commitDetails[2]),
                    Message = commitDetails[3]
                });
            return commits;
        }
    }
}