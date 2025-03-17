namespace AtelierResleriana.Unity
{
    public class UnityFSFileMetadata
    {
        public byte[] UncompressedDataHash { get; set; } // 16 bytes
        public UnityFSFileBlockInfo[] BlockInfos { get; set; }
        public UnityFSFileDirectoryInfo[] DirectoryInfos { get; set; }
    }
}
