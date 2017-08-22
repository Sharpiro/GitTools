namespace GitSharp.Models
{
    public class KitDelta
    {
        public DeltaType DeltaType { get; set; }
        public PathNode Node { get; set; }
    }

    public enum DeltaType { Add, Delete, Edit }
}