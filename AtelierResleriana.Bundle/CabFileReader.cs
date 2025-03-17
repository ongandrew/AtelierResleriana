using AtelierResleriana.Compression;
using System.Text;
using Universal.Common;
using BinaryReader = Universal.Common.BinaryReader;

namespace AtelierResleriana.Bundle
{
    [Obsolete("This class has not been tested for correctness.")]
    public class CabFileReader
    {
        public CabFile Read(Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, true);

            // Check for compression
            byte[] magic = reader.ReadBytes(2);
            stream.Position = 0;

            Stream workingStream = stream;
            if (magic[0] == 0x1F && magic[1] == 0x8B) // GZIP magic
            {
                var gzipAlgorithm = new GzipCompressionAlgorithm();
                var memStream = new MemoryStream();
                gzipAlgorithm.Decompress(stream, memStream);
                memStream.Position = 0;
                workingStream = memStream;
                reader.Dispose();
                reader = new BinaryReader(workingStream);
            }
            else
            {
                // Check for Brotli
                stream.Position = 0x20;
                byte[] brotliMagic = reader.ReadBytes(6);
                stream.Position = 0;

                if (IsBrotliMagic(brotliMagic))
                {
                    var brotliAlgorithm = new BrotliCompressionAlgorithm();
                    var memStream = new MemoryStream();
                    brotliAlgorithm.Decompress(stream, memStream);
                    memStream.Position = 0;
                    workingStream = memStream;
                    reader.Dispose();
                    reader = new BinaryReader(workingStream);
                }
            }

            var cabFile = new CabFile { BaseStream = workingStream };

            // Read signature
            var signatureBytes = new List<byte>();
            byte b;
            while ((b = reader.ReadByte()) != 0)
                signatureBytes.Add(b);
            cabFile.Signature = Encoding.UTF8.GetString(signatureBytes.ToArray());

            // Validate signature
            if (cabFile.Signature != CabFile.DefaultSignature)
                throw new InvalidDataException($"Invalid CAB file signature: {cabFile.Signature}");

            // Read header length
            uint headerLength = reader.ReadUInt32();
            long endOfHeader = reader.BaseStream.Position + headerLength;

            // Read file entries
            while (reader.BaseStream.Position < endOfHeader)
            {
                var entry = new CabFile.Entry
                {
                    Offset = reader.ReadUInt32(),
                    Length = reader.ReadUInt32(),
                    Path = Encoding.UTF8.GetString(reader.ReadBytes((int)reader.ReadUInt32()))
                };

                // Set up data provider for this entry
                entry.DataProvider = e => new StreamSegment(cabFile.BaseStream!, e.Offset, e.Length);
                cabFile.Entries.Add(entry);
            }

            reader.Dispose();

            return cabFile;
        }

        private bool IsBrotliMagic(byte[] magic)
        {
            byte[] expectedMagic = new byte[] { 0x28, 0xB5, 0x2F, 0xFD, 0x20, 0x30 };

            if (magic.Length != expectedMagic.Length)
                return false;

            for (int i = 0; i < magic.Length; i++)
            {
                if (magic[i] != expectedMagic[i])
                    return false;
            }

            return true;
        }
    }
}
