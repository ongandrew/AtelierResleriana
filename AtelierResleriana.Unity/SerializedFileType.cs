namespace AtelierResleriana.Unity
{
    public class SerializedFileType
    {
        public int ClassId { get; set; }
        public bool? IsStrippedType { get; set; }
        public short? ScriptTypeIndex { get; set; }

        public byte[]? ScriptId { get; set; }
        public byte[]? OldTypeHash { get; set; }

        public SerializedFileTypeTreeNode Node { get; set; }

        public string? ClassName { get; set; }
        public string? Namespace { get; set; }
        public string? AssemblyName { get; set; }
        public int[]? Dependencies { get; set; }
    }
}
