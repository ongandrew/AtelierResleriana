namespace AtelierResleriana.Bundle
{
    [Obsolete("This class has not been tested for correctness.")]
    public class CabFile
    {
        public enum CompressionType
        {
            None,
            Gzip,
            Brotli
        }

        public class Entry
        {
            public uint Offset { get; set; }
            public uint Length { get; set; }
            public string Path { get; set; }
            public Stream GetStream() => DataProvider?.Invoke(this);

            // Internal use for providing data access
            internal Func<Entry, Stream>? DataProvider { get; set; }
            internal byte[]? CachedData { get; set; }
        }

        public const string DefaultSignature = "UnityWebData1.0";

        public string Signature { get; set; } = DefaultSignature;
        public CompressionType Compression { get; set; } = CompressionType.None;
        public List<Entry> Entries { get; } = new();

        // For reading: tracks the source stream
        internal Stream? BaseStream { get; set; }

        public Stream GetEntryStream(string path)
        {
            var entry = Entries.FirstOrDefault(e => e.Path == path)
                ?? throw new FileNotFoundException($"Entry not found: {path}");
            return entry.GetStream()
                ?? throw new InvalidOperationException("No data provider available for this entry");
        }

        public void AddEntry(string path, byte[] data)
        {
            var entry = new Entry
            {
                Path = path,
                Length = (uint)data.Length,
                CachedData = data,
                DataProvider = e => new MemoryStream(e.CachedData!)
            };
            Entries.Add(entry);
        }

        public void AddEntry(string path, Stream stream)
        {
            // Read stream into memory since we need to know its length
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            AddEntry(path, ms.ToArray());
        }

        public static CabFile FromStream(Stream stream)
        {
            var reader = new CabFileReader();
            return reader.Read(stream);
        }

        public Stream WriteToStream()
        {
            var writer = new CabFileWriter();
            return writer.Write(this);
        }
    }
}
