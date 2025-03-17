namespace AtelierResleriana.Unity
{
    public class SerializedFileHeader
    {
        public uint MetadataSize { get; set; }
        public long FileSize { get; set; }
        public uint Version { get; set; }
        public long DataOffset { get; set; }
        public bool IsBigEndian { get; set; }
    }
}
