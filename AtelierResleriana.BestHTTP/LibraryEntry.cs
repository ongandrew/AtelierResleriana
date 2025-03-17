namespace AtelierResleriana.BestHTTP
{
    public class LibraryEntry
    {
        public Uri Uri { get; set; }

        public DateTime LastAccess { get; set; }
        public int BodyLength { get; set; }
        public ulong MappedNameIndex { get; set; }
        public string ETag { get; set; }
        public string LastModified { get; set; }
        public DateTime Expires { get; set; }
        public long Age { get; set; }
        public long MaxAge { get; set; }
        public DateTime Date { get; set; }
        public bool MustRevalidate { get; set; }
        public DateTime Received { get; set; }
    }
}
