namespace AtelierResleriana.Unity
{
    public class SerializedFileTypeTreeNode
    {
        public int Level { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int Version { get; set; }
        public int TypeFlags { get; set; }
        public uint? MetaFlag { get; set; }
        public int? VariableCount { get; set; }
        public int? Index { get; set; }

        public IList<SerializedFileTypeTreeNode> Children { get; set; } = new List<SerializedFileTypeTreeNode>();
    }
}
