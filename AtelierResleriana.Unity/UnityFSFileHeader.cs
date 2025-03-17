namespace AtelierResleriana.Unity
{
    public class UnityFSFileHeader
    {
        public const string Signature = "UnityFS"; 
        
        public uint Version { get; set; }
        public string PlayerVersion { get; set; }
        public EngineVersion EngineVersion { get; set; }

        public long FileSize { get; set; }
        public uint CompressedMetadataSize { get; set; }
        public uint UncompressedMetadataSize { get; set; }
        public uint Flags { get; set; }

        public UnityFSFileCompression Compression
        {
            get
            {
                return (UnityFSFileCompression)(Flags & 0x3FU);
            }
            set
            {
                Flags = (Flags & ~0x3FU) | (uint)value;
            }
        }
    }
}