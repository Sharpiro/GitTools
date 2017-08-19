using GitSharp.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GitSharp
{
    public class GitSharpExecution
    {
        private readonly string _gitSharpRoot;
        private readonly ILogger _logger;
        private readonly Sha1Hasher _hasher;
        private readonly string _gitSharpDataPath;

        public GitSharpExecution(string gitRoot, ILogger logger, Sha1Hasher hasher)
        {
            _gitSharpRoot = gitRoot ?? throw new ArgumentNullException(nameof(gitRoot));
            _gitSharpDataPath = $"{_gitSharpRoot}\\.gitsharp";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
        }

        public bool IsGitRepo() => Directory.Exists(_gitSharpDataPath);

        public void Init()
        {
            ValidateRoot();
            var gitSharpDataDirectoryInfo = new DirectoryInfo(_gitSharpDataPath);
            if (gitSharpDataDirectoryInfo.Exists) throw new InvalidOperationException($"GitSharp data path already exists @ '{_gitSharpDataPath}'");
            gitSharpDataDirectoryInfo.Create();
            gitSharpDataDirectoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            gitSharpDataDirectoryInfo.CreateSubdirectory("objects");
            _logger.Info($"GitSharp data path created @ '{_gitSharpDataPath}'");
        }

        public IEnumerable<(FileInfo FileInfo, string Hash)> Status()
        {
            ValidateRoot();
            ValidateDataDir();
            var directoryInfo = new DirectoryInfo(_gitSharpRoot);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => !f.FullName.Contains(".gitsharp"));

            var hashes = files.Select(f =>
            {
                var fileBytes = File.ReadAllBytes(f.FullName);
                var hashedBytes = _hasher.CreateHash(fileBytes);
                var hashedHex = hashedBytes.Select(b => b.ToString("X2"));
                return (FileInfo: f, Hash: string.Join("", hashedHex).ToLower());
            }).ToList();

            var objects = GetObjects();

            var newHashes = hashes.Where(h => !objects.Contains(h.Hash));
            return newHashes;
        }

        private void ValidateRoot()
        {
            if (!Directory.Exists(_gitSharpRoot)) throw new DirectoryNotFoundException($"Root directory not found or invalid @ '{_gitSharpRoot}'");
            _logger.Debug($"Root directory exists @ '{_gitSharpRoot}'");
        }

        private void ValidateDataDir()
        {
            if (!Directory.Exists(_gitSharpDataPath)) throw new DirectoryNotFoundException($"GitSharp data path not found @ '{_gitSharpDataPath}'");
            _logger.Debug($"GitSharp data path exists @ '{_gitSharpDataPath}'");
        }

        private IEnumerable<string> GetObjects()
        {
            var objectsDirectory = new DirectoryInfo($"{_gitSharpDataPath}\\objects");
            var objectFiles = objectsDirectory.GetFiles();
            return objectFiles.Select(f =>
            {
                return f.Name.Substring(0, f.Name.Length - f.Extension.Length);
            });
        }
    }
}