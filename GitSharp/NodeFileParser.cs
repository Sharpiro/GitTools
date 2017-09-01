using GitSharp.Models;
using System;
using System.IO;
using System.Linq;

namespace GitSharp
{
    public class NodeFileParser
    {
        private readonly string _kitObjectsPath;

        public NodeFileParser(string kitRoot)
        {
            _kitObjectsPath = $"{kitRoot}\\.gitsharp\\objects";
        }

        public CommitNode ParseCommitNode(string hash)
        {
            var objectData = GetKitObject(hash);
            var split = objectData.Split(new[] { "\t" }, StringSplitOptions.None);
            if (split.Length != 7) throw new InvalidDataException("The commit object data format was invalid");

            var type = (NodeType)Enum.Parse(typeof(NodeType), split[0]);
            if (type != NodeType.Commit) throw new InvalidDataException($"The object type was expected to be 'commit', but was really, '{split[0]}'");
            return new CommitNode
            {
                Type = type,
                Hash = split[1],
                Parent = split[2],
                TreeNodeRoot = ParseTreeNode(split[3]),
                Author = split[4],
                Message = split[5],
                AuthorDate = DateTime.Parse(split[6])
            };
        }

        public TreeNode ParseTreeNode(string hash)
        {
            var objectData = GetKitObject(hash);
            var split = objectData.Split(new[] { "\t" }, StringSplitOptions.None);
            if (split.Length != 4) throw new InvalidDataException("The commit object data format was invalid");

            var type = (NodeType)Enum.Parse(typeof(NodeType), split[0]);
            if (type != NodeType.Tree) throw new InvalidDataException($"The object type was expected to be 'tree', but was really, '{split[0]}'");

            var childHashes = split[3].Split(new[] { " " }, StringSplitOptions.None);
            return new TreeNode
            {
                Type = type,
                Hash = split[1],
                RelativePath = split[2],
                Children = childHashes.Select(ParsePathNode).ToList()
            };
        }

        public BlobNode ParseBlobNode(string hash)
        {
            var objectData = GetKitObject(hash);
            var split = objectData.Split(new[] { "\t" }, StringSplitOptions.None);
            if (split.Length != 4) throw new InvalidDataException("The commit object data format was invalid");

            var type = (NodeType)Enum.Parse(typeof(NodeType), split[0]);
            if (type != NodeType.Blob) throw new InvalidDataException($"The object type was expected to be 'blob', but was really, '{split[0]}'");

            var childHashes = split[3].Split(new[] { " " }, StringSplitOptions.None);
            return new BlobNode
            {
                Type = type,
                Hash = split[1],
                RelativePath = split[2],
                Data = split[3]
            };
        }

        public PathNode ParsePathNode(string hash)
        {
            var objectData = GetKitObject(hash);
            var split = objectData.Split(new[] { "\t" }, StringSplitOptions.None);
            if (split.Length != 4) throw new InvalidDataException("The commit object data format was invalid");

            var type = (NodeType)Enum.Parse(typeof(NodeType), split[0]);
            if (type != NodeType.Tree && type != NodeType.Blob) throw new InvalidDataException($"The object type was expected to be a path node, but was really, '{split[0]}'");

            var childHashes = split[3].Split(new[] { " " }, StringSplitOptions.None);
            return type == NodeType.Tree ? (PathNode)new TreeNode
            {
                Type = type,
                Hash = split[1],
                RelativePath = split[2],
                Children = childHashes.Select(ParsePathNode).ToList()
            } :
            new BlobNode
            {
                Type = type,
                Hash = split[1],
                RelativePath = split[2],
                Data = split[3]
            };
        }

        private string GetKitObject(string hash)
        {
            var path = $"{_kitObjectsPath}\\{hash}";
            if (!File.Exists(path)) throw new FileNotFoundException($"Unable to find file @ '{path}");
            return File.ReadAllText(path);
        }
    }
}