namespace AtelierResleriana.Unity
{
    public class TypePackageHeader
    {
        public const uint Magic = 0x2A4B5054; // "TPK*"
        public const byte Version = 1;

        public TypePackageCompressionType CompressionType { get; set; }
        public TypePackageDataType DataType { get; set; }
        public uint CompressedSize { get; set; }
        public uint UncompressedSize { get; set; }
    }
}
