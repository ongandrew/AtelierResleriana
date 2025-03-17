namespace AtelierResleriana.Unity
{
    public class TypePackageUnityNode
    {
        public ushort TypeName { get; set; }        // Index into string buffer
        public ushort Name { get; set; }            // Index into string buffer
        public int ByteSize { get; set; }
        public int Version { get; set; }
        public byte TypeFlags { get; set; }
        public uint MetaFlag { get; set; }
        public ushort[] SubNodes { get; set; }      // References to other nodes
    }
}
