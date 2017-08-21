using GitSharp.Models;
using GitSharp.Tools;
using GitSharp.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitSharp
{
    public class GitSharpExecution
    {
        private readonly string _kitRoot;
        private readonly ILogger _logger;
        private readonly IHasher _hasher;
        private readonly NodeParser _nodeParser;
        private readonly string _kitDataPath;
        private readonly string _kitObjectsPath;
        private readonly string _kitHeadsPath;

        private GitSharpExecution(string kitRoot, ILogger logger, IHasher hasher,
            NodeParser nodeParser)
        {
            _kitRoot = kitRoot ?? throw new ArgumentNullException(nameof(kitRoot));
            _kitDataPath = $"{_kitRoot}\\.gitsharp";
            _kitObjectsPath = $"{_kitRoot}\\.gitsharp\\objects";
            _kitHeadsPath = $"{_kitRoot}\\.gitsharp\\heads";
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hasher = hasher ?? throw new ArgumentNullException(nameof(hasher));
            _nodeParser = nodeParser ?? throw new ArgumentNullException(nameof(nodeParser));
        }

        public bool IsKitRepo() => Directory.Exists(_kitDataPath);

        public void Init()
        {
            var gitSharpDataDirectoryInfo = new DirectoryInfo(_kitDataPath);
            if (gitSharpDataDirectoryInfo.Exists) throw new InvalidOperationException($"GitSharp data path already exists @ '{_kitDataPath}'");
            gitSharpDataDirectoryInfo.Create();
            gitSharpDataDirectoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            gitSharpDataDirectoryInfo.CreateSubdirectory("objects");
            gitSharpDataDirectoryInfo.CreateSubdirectory("heads");
            _logger.Info($"GitSharp data path created @ '{_kitDataPath}'");
        }

        public IEnumerable<(FileInfo FileInfo, string Hash)> Status()
        {
            ValidateIsKitRepo();

            var allFileData = GetAllFiles();
            var allKitObjects = GetKitObjects();
            var newHashes = allFileData.Where(h => !allKitObjects.Select(o => o.Hash).Contains(h.Hash));

            return newHashes;
        }

        public IEnumerable<(FileInfo FileInfo, string Hash)> Commit()
        {
            ValidateIsKitRepo();

            var headCommitId = GetMostRecentCommitId();

            var newTreeRoot = new TreeNode
            {
                Type = NodeType.Tree,
                RelativePath = _kitRoot.GetRelativePath(_kitRoot)
            };

            BuildTree(new DirectoryInfo(_kitRoot), newTreeRoot);

            var newCommit = new CommitNode
            {
                Type = NodeType.Commit,
                Author = "me",
                Message = "message",
                Parent = headCommitId,
                TreeNodeRoot = newTreeRoot,
                AuthorDate = DateTime.UtcNow,
                Hash = newTreeRoot.Hash.GetHash(_hasher)
            };
            //var data = $"{commit.Author}\t{commit.Message}\t{commit.TreeNodeRoot.Hash}";

            if (newCommit.Hash.Equals(headCommitId, StringComparison.InvariantCultureIgnoreCase))
                return Enumerable.Empty<(FileInfo FileInfo, string Hash)>();

            var headCommit = GetCommitNode(headCommitId);

            newCommit.Traverse(currentNode =>
            {
                var filePath = $"{_kitObjectsPath}\\{currentNode.Hash}";
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, currentNode.GetWriteableData());
                }
            });

            //WriteTreeObjects(commit);

            File.WriteAllText($"{_kitHeadsPath}\\master", newCommit.Hash);

            return Enumerable.Empty<(FileInfo FileInfo, string Hash)>();
        }

        //private void Traverse(Node currentNode, Action<Node> action)
        //{
        //    action(currentNode);
        //    var childNodes = currentNode.GetChildNodes();
        //    foreach (var childNode in childNodes)
        //    {
        //        Traverse(childNode, action);
        //    }
        //}

        //private void WriteTreeObjects(Node currentNode)
        //{
        //    var filePath = $"{_kitObjectsPath}\\{currentNode.Hash}";
        //    if (!File.Exists(filePath))
        //        File.WriteAllBytes(filePath, new byte[0]);
        //    var childNodes = currentNode.GetChildNodes();
        //    foreach (var childNode in childNodes)
        //    {
        //        WriteTreeObjects(childNode);
        //    }
        //}

        private string GetMostRecentCommitId()
        {
            var fileInfo = new DirectoryInfo(_kitHeadsPath).GetFiles().FirstOrDefault();
            if (fileInfo == null) return null;
            var text = File.ReadAllText(fileInfo.FullName).Replace(Environment.NewLine, string.Empty);
            return text;
        }

        private void BuildTree(DirectoryInfo currentDir, TreeNode currentTreeNode)
        {
            var childDirectories = currentDir.GetDirectories().Where(d => !d.Name.Equals(".gitsharp", StringComparison.InvariantCultureIgnoreCase));

            foreach (var childDirectory in childDirectories)
            {
                var childTreeNode = new TreeNode
                {
                    Type = NodeType.Tree,
                    RelativePath = childDirectory.FullName.GetRelativePath(_kitRoot)
                };
                currentTreeNode.Children.Add(childTreeNode);
                BuildTree(childDirectory, childTreeNode);
            }

            var files = currentDir.GetFiles().Select(GetKitFileInfo);
            foreach (var file in files)
            {
                var blobNode = new BlobNode
                {
                    Type = NodeType.Blob,
                    Hash = file.Hash,
                    RelativePath = file.RelativePath,
                    Data = File.ReadAllText(file.FileInfo.FullName) //currently loading all file data into memory for simplicity
                };
                currentTreeNode.Children.Add(blobNode);
            }
            var childHashes = currentTreeNode.Children.Select(c => c.Hash);
            var combinedChildHashes = string.Join(string.Empty, childHashes);
            var parentHash = combinedChildHashes.GetHash(_hasher);
            currentTreeNode.Hash = parentHash;
        }

        private CommitNode GetCommitNode(string hash)
        {
            //var objectData = GetKitObject(hash);
            var temp = _nodeParser.ParseCommitNode(hash);
            throw new NotImplementedException();
        }

        //private string GetKitObject(string hash)
        //{
        //    var path = $"{_kitObjectsPath}\\{hash}";
        //    if (!File.Exists(path)) throw new FileNotFoundException($"Unable to find file @ '{path}");
        //    return File.ReadAllText(path);
        //}

        private KitFileInfo GetKitFileInfo(FileInfo fileInfo)
        {
            var bytes = File.ReadAllBytes(fileInfo.FullName);
            var hash = string.Join(string.Empty, _hasher.CreateHash(bytes).Select(h => h.ToString("X2"))).ToLower();
            return new KitFileInfo
            {
                Hash = hash,
                FileInfo = fileInfo,
                RelativePath = fileInfo.FullName.GetRelativePath(_kitRoot)
            };
        }

        private void ValidateIsKitRepo()
        {
            if (!Directory.Exists(_kitDataPath)) throw new DirectoryNotFoundException($"GitSharp data path not found @ '{_kitDataPath}'");
            _logger.Debug($"GitSharp data path exists @ '{_kitDataPath}'");
        }

        private IEnumerable<(FileInfo fileInfo, string Hash)> GetKitObjects()
        {
            var objectsDirectory = new DirectoryInfo($"{_kitDataPath}\\objects");
            var objectFiles = objectsDirectory.GetFiles();
            return objectFiles.Select(f =>
            {
                return (f, f.Name.Substring(0, f.Name.Length - f.Extension.Length));
            });
        }

        private IEnumerable<(FileInfo fileInfo, string Hash)> GetAllFiles()
        {

            var directoryInfo = new DirectoryInfo(_kitRoot);
            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
                .Where(f => !f.FullName.Contains(".gitsharp"));
            return files.Select(f =>
            {
                var fileBytes = File.ReadAllBytes(f.FullName);
                var hashedBytes = _hasher.CreateHash(fileBytes);
                var hashedHex = string.Join(string.Empty, hashedBytes.Select(b => b.ToString("X2"))).ToLower();
                return (FileInfo: f, Hash: hashedHex);
            });
        }

        public static GitSharpExecution Create(string kitRoot, ILogger logger, Sha1Hasher hasher, NodeParser nodeParser)
        {
            if (!Directory.Exists(kitRoot)) throw new DirectoryNotFoundException($"Root directory not found or invalid @ '{kitRoot}'");
            return new GitSharpExecution(kitRoot, logger, hasher, nodeParser);
        }
    }
}