using Universal.Common;

namespace AtelierResleriana.BestHTTP
{
    public class LibraryReader
    {
        public const int MinSupportedVersion = 1;

        public IEnumerable<LibraryEntry> Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true, Endian.Little))
            {
                int version = reader.ReadInt32();

                // Read next name index (version 2+)
                if (version >= 2)
                {
                    ulong nextNameIdx = reader.ReadUInt64();
                }

                // Read entry count
                int entryCount = reader.ReadInt32();

                // Read all entries
                for (int i = 0; i < entryCount; i++)
                {
                    var entry = new LibraryEntry();

                    // Read URI
                    string uriString = reader.ReadString();
                    entry.Uri = new Uri(uriString);

                    // Read basic info
                    entry.LastAccess = DateTime.FromBinary(reader.ReadInt64());
                    entry.BodyLength = reader.ReadInt32();

                    // Version specific fields
                    if (version >= 2)
                    {
                        entry.MappedNameIndex = reader.ReadUInt64();
                    }

                    if (version >= 3)
                    {
                        // No idea.
                        reader.BaseStream.Position += 17;
                    }

                    // Common fields for all versions
                    entry.ETag = reader.ReadString();
                    entry.LastModified = reader.ReadString();
                    entry.Expires = DateTime.FromBinary(reader.ReadInt64());
                    entry.Age = reader.ReadInt64();
                    entry.MaxAge = reader.ReadInt64();
                    entry.Date = DateTime.FromBinary(reader.ReadInt64());
                    entry.MustRevalidate = reader.ReadBoolean();
                    entry.Received = DateTime.FromBinary(reader.ReadInt64());

                    yield return entry;
                }
            }
        }
    }
}
