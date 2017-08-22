using GitSharp.Models;
using GitSharp.Tools;
using GitSharp.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitSharp
{
    public class GitSharpExecution : ISourceControlApi
    {
        private readonly string _kitRoot;
        private readonly ILogger _logger;
        private readonly IHasher _hasher;
        private readonly NodeFileParser _nodeParser;
        private readonly string _kitDataPath;
        private readonly string _kitObjectsPath;
        private readonly string _kitHeadsPath;

        private GitSharpExecution(string kitRoot, ILogger logger, IHasher hasher,
            NodeFileParser nodeParser)
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

        public List<KitDelta> Status()
        {
            ValidateIsKitRepo();

            var headCommitId = GetMostRecentCommitId();

            var newTreeRoot = new TreeNode
            {
                Type = NodeType.Tree,
                RelativePath = _kitRoot.GetRelativePath(_kitRoot)
            };

            var deltas = GetCommitDeltas(headCommitId, newTreeRoot);

            return deltas;
        }

        public List<KitDelta> Commit()
        {
            ValidateIsKitRepo();

            var headCommitId = GetMostRecentCommitId();

            var newTreeRoot = new TreeNode
            {
                Type = NodeType.Tree,
                RelativePath = _kitRoot.GetRelativePath(_kitRoot)
            };

            var deltas = GetCommitDeltas(headCommitId, newTreeRoot);

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

            ////write new objects
            newCommit.Traverse(currentNode =>
            {
                var filePath = $"{_kitObjectsPath}\\{currentNode.Hash}";
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, currentNode.GetWriteableData());
                }
            });

            //write branch head commit
            File.WriteAllText($"{_kitHeadsPath}\\master", newCommit.Hash);

            return deltas;
        }

        private List<KitDelta> GetCommitDeltas(string headCommitId, TreeNode newTreeRoot)
        {
            BuildTree(new DirectoryInfo(_kitRoot), newTreeRoot);

            var newHash = newTreeRoot.Hash.GetHash(_hasher);
            var deltas = new List<KitDelta>();

            if (newHash.Equals(headCommitId, StringComparison.InvariantCultureIgnoreCase))
                return deltas;

            var headCommit = _nodeParser.ParseCommitNode(headCommitId);

            GetNodeDiff(headCommit.TreeNodeRoot, newTreeRoot, deltas);

            return deltas;
        }

        private void GetNodeDiff(PathNode oldNode, PathNode newNode, List<KitDelta> deltas)
        {
            if (oldNode == null)
            {
                throw new InvalidOperationException($"Old node was null, this should never occur. Path: {oldNode.RelativePath}");
            }
            if (newNode == null)
            {
                deltas.Add(new KitDelta { Node = oldNode, DeltaType = DeltaType.Delete });
                return;
            }
            if (oldNode.Hash == newNode.Hash) return;
            if (oldNode.Type == NodeType.Blob && newNode.Type != NodeType.Blob)
                throw new Exception($"Type mismatch for old node type: '{oldNode.Type}' and new node type: '{newNode.Type}' for old node path: '{oldNode.RelativePath}' and new node path: '{newNode.RelativePath}'");

            if (oldNode.Type == NodeType.Blob && newNode.Type == NodeType.Blob)
            {
                deltas.Add(new KitDelta { Node = newNode, DeltaType = DeltaType.Edit });
                return;
            }

            var oldChildNodes = oldNode.GetChildNodes().Select(n => n as PathNode);
            var newChildNodes = newNode.GetChildNodes().Select(n => n as PathNode);

            var oldDictionary = oldChildNodes.ToDictionary(n => n.RelativePath, n => n);
            var newDictionary = newChildNodes.ToDictionary(n => n.RelativePath, n => n);
            foreach (var oldKvp in oldDictionary)
            {
                var thisNode = oldKvp.Value;
                var nodeExists = newDictionary.TryGetValue(oldKvp.Key, out PathNode otherNode);
                GetNodeDiff(thisNode, otherNode, deltas);
                newDictionary.Remove(oldKvp.Key);
            }

            deltas.AddRange(newDictionary.Select(kvp => new KitDelta { Node = kvp.Value, DeltaType = DeltaType.Add }));
        }

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

        //private IEnumerable<(FileInfo fileInfo, string Hash)> GetKitObjects()
        //{
        //    var objectsDirectory = new DirectoryInfo($"{_kitDataPath}\\objects");
        //    var objectFiles = objectsDirectory.GetFiles();
        //    return objectFiles.Select(f =>
        //    {
        //        return (f, f.Name.Substring(0, f.Name.Length - f.Extension.Length));
        //    });
        //}

        //private IEnumerable<(FileInfo fileInfo, string Hash)> GetAllFiles()
        //{

        //    var directoryInfo = new DirectoryInfo(_kitRoot);
        //    var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories)
        //        .Where(f => !f.FullName.Contains(".gitsharp"));
        //    return files.Select(f =>
        //    {
        //        var fileBytes = File.ReadAllBytes(f.FullName);
        //        var hashedBytes = _hasher.CreateHash(fileBytes);
        //        var hashedHex = string.Join(string.Empty, hashedBytes.Select(b => b.ToString("X2"))).ToLower();
        //        return (FileInfo: f, Hash: hashedHex);
        //    });
        //}

        public Task<string> CatFileAsync(string hash)
        {
            return Task.Run(() =>
            {
                var path = $"{_kitObjectsPath}\\{hash}";
                if (!File.Exists(path)) throw new FileNotFoundException($"Unable to find file @ '{path}");
                return File.ReadAllText(path);
            });
        }

        public static GitSharpExecution Create(string kitRoot, ILogger logger, Sha1Hasher hasher, NodeFileParser nodeParser)
        {
            if (!Directory.Exists(kitRoot)) throw new DirectoryNotFoundException($"Root directory not found or invalid @ '{kitRoot}'");
            return new GitSharpExecution(kitRoot, logger, hasher, nodeParser);
        }
    }
}