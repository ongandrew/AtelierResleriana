using System.Text;
using AtelierResleriana.Compression;

namespace AtelierResleriana.Bundle
{
    [Obsolete("This class has not been tested for correctness.")]
    public class CabFileWriter
    {
        public Stream Write(CabFile cabFile)
        {
            var uncompressedStream = new MemoryStream();
            using (var writer = new BinaryWriter(uncompressedStream, Encoding.UTF8, true))
            {
                // Write signature with null terminator
                writer.Write(Encoding.UTF8.GetBytes(cabFile.Signature));
                writer.Write((byte)0);

                // Save position for header length
                long headerLengthPos = writer.BaseStream.Position;
                writer.Write(0u); // Placeholder

                // Calculate data offset
                long dataOffset = CalculateDataOffset(headerLengthPos + 4, cabFile.Entries);

                // Write entries
                long currentOffset = dataOffset;
                foreach (var entry in cabFile.Entries)
                {
                    writer.Write((uint)currentOffset);
                    writer.Write(entry.Length);

                    byte[] pathBytes = Encoding.UTF8.GetBytes(entry.Path);
                    writer.Write((uint)pathBytes.Length);
                    writer.Write(pathBytes);

                    currentOffset += entry.Length;
                }

                // Write header length
                long headerLength = writer.BaseStream.Position - (headerLengthPos + 4);
                writer.BaseStream.Position = headerLengthPos;
                writer.Write((uint)headerLength);
                writer.BaseStream.Position = dataOffset;

                // Write file data
                foreach (var entry in cabFile.Entries)
                {
                    using var entryStream = entry.GetStream();
                    entryStream.CopyTo(writer.BaseStream);
                }
            }

            // Apply compression if specified
            var finalStream = new MemoryStream();
            uncompressedStream.Position = 0;

            switch (cabFile.Compression)
            {
                case CabFile.CompressionType.None:
                    uncompressedStream.CopyTo(finalStream);
                    break;

                case CabFile.CompressionType.Gzip:
                    var gzipCompressor = new GzipCompressionAlgorithm();
                    gzipCompressor.Compress(uncompressedStream, finalStream);
                    break;

                case CabFile.CompressionType.Brotli:
                    var brotliCompressor = new BrotliCompressionAlgorithm();
                    brotliCompressor.Compress(uncompressedStream, finalStream);
                    break;
            }

            finalStream.Position = 0;
            return finalStream;
        }

        private long CalculateDataOffset(long startPos, List<CabFile.Entry> entries)
        {
            long offset = startPos;
            foreach (var entry in entries)
            {
                offset += 12; // offset(4) + length(4) + pathLength(4)
                offset += Encoding.UTF8.GetBytes(entry.Path).Length;
            }
            return offset;
        }
    }
}