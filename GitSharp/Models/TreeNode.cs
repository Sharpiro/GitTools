using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GitSharp.Models
{
    public class TreeNode : PathNode
    {
        public List<PathNode> Children { get; set; } = new List<PathNode>();

        public override ImmutableList<Node> GetChildNodes() => Children.Select(c => c as Node).ToImmutableList();

        public override string GetWriteableData()
        {
            var childHashes = string.Join(" ", Children.Select(c => c.Hash));
            return $"{Type}\t{Hash}\t{RelativePath}\t{childHashes}";
        }
    }

    public class BlobNode : PathNode
    {
        public string Data { get; set; }
        public override ImmutableList<Node> GetChildNodes() => ImmutableList.Create<Node>();

        public override string GetWriteableData()
        {
            return $"{Type}\t{Hash}\t{RelativePath}\t{Data}";
        }
    }

    public class CommitNode : Node
    {
        public string Parent { get; set; }
        public TreeNode TreeNodeRoot { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTime AuthorDate { get; set; }

        public override ImmutableList<Node> GetChildNodes() => ImmutableList.Create<Node>(TreeNodeRoot);

        public override string GetWriteableData()
        {
            return $"{Type}\t{Hash}\t{Parent}\t{TreeNodeRoot.Hash}\t{Author}\t{Message}\t{AuthorDate}";
        }
    }

    public abstract class PathNode : Node
    {
        public string RelativePath { get; set; }
    }

    public abstract class Node
    {
        public NodeType Type { get; set; }
        public string Hash { get; set; }

        public abstract ImmutableList<Node> GetChildNodes();

        public abstract string GetWriteableData();

        public void Traverse(Action<Node> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(this);
            var childNodes = GetChildNodes();
            foreach (var childNode in childNodes) childNode.Traverse(action);
        }

        public T Traverse<T>(Func<Node, T> func)
        {
            throw new NotImplementedException();
        }
    }

    public enum NodeType { Blob, Tree, Commit }
}