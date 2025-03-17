namespace AtelierResleriana.Unity
{
    public class UnityFSFile
    {
        public UnityFSFileHeader Header { get; set; }
        public UnityFSFileMetadata Metadata { get; set; }
        public byte[] Data { get; set; }

        public byte[] GetDirectoryBytes(string path)
        {
            return GetDirectoryBytes(Metadata.DirectoryInfos.First(x => x.Path == path));
        }

        public byte[] GetDirectoryBytes(UnityFSFileDirectoryInfo directoryInfo)
        {
            // Create a new array of the exact size needed
            byte[] directoryData = new byte[directoryInfo.Size];

            // Copy the data from the main data array starting at the directory's offset
            Buffer.BlockCopy(Data, (int)directoryInfo.Offset, directoryData, 0, (int)directoryInfo.Size);

            return directoryData;
        }

        public Stream GetDirectoryStream(string path)
        {
            return GetDirectoryStream(Metadata.DirectoryInfos.First(x => x.Path == path));
        }

        public Stream GetDirectoryStream(UnityFSFileDirectoryInfo directoryInfo)
        {
            // Create a memory stream that wraps the specific section of data
            // This is more efficient than copying the data since it uses the same underlying buffer
            return new MemoryStream(Data, (int)directoryInfo.Offset, (int)directoryInfo.Size, false);
        }
    }
}
