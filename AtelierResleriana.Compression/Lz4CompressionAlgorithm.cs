using K4os.Compression.LZ4;

namespace AtelierResleriana.Compression
{
    public class Lz4CompressionAlgorithm : ICompressionAlgorithm
    {
        public LZ4Level CompressionLevel { get; set; }

        public Lz4CompressionAlgorithm() : this(null) { }
        public Lz4CompressionAlgorithm(Options? options)
        {
            options ??= new Options();
            CompressionLevel = options.CompressionLevel;
        }

        public byte[] Compress(byte[] bytes)
        {
            var maxLength = LZ4Codec.MaximumOutputSize(bytes.Length);
            var output = new byte[maxLength];

            int compressed = LZ4Codec.Encode(
                bytes, 0, bytes.Length,
                output, 0, output.Length, CompressionLevel);

            if (compressed <= 0)
            {
                throw new InvalidOperationException("LZ4 compression failed");
            }

            // Trim to actual compressed size
            var result = new byte[compressed];
            Array.Copy(output, 0, result, 0, compressed);
            return result;
        }

        public byte[] Decompress(byte[] bytes)
        {
            throw new InvalidOperationException("Cannot decompress LZ4 data without knowing the uncompressed size. Use Decompress(bytes, uncompressedSize) instead.");
        }

        public byte[] Decompress(byte[] bytes, int uncompressedSize)
        {
            var output = new byte[uncompressedSize];
            int decompressed = LZ4Codec.Decode(
                bytes, 0, bytes.Length,
                output, 0, uncompressedSize);

            if (decompressed != uncompressedSize)
            {
                throw new InvalidDataException($"LZ4 decode size mismatch. Expected {uncompressedSize} bytes but got {decompressed} bytes. Initial compressed bytes bytes: {string.Join(" ", bytes.Take(32).Select(x => x.ToString("X2")))}.");
            }

            return output;
        }

        public void Compress(Stream input, Stream output)
        {
            const int bufferSize = 81920; // 80KB buffer
            byte[] buffer = new byte[bufferSize];
            int bytesRead;

            using var ms = new MemoryStream();
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] compressed = Compress(buffer[..bytesRead]);
                output.Write(compressed, 0, compressed.Length);
            }
        }

        public void Decompress(Stream input, Stream output)
        {
            // Read the input stream
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            var compressedBytes = buffer.ToArray();

            // Decompress
            byte[] decompressed = Decompress(compressedBytes);

            // Write to output
            output.Write(decompressed, 0, decompressed.Length);
        }

        public void Decompress(Stream input, Stream output, int uncompressedSize)
        {
            // Read the input stream
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            var compressedBytes = buffer.ToArray();

            // Create buffer of exact size
            var decompressed = new byte[uncompressedSize];

            int decoded = LZ4Codec.Decode(
                compressedBytes, 0, compressedBytes.Length,
                decompressed, 0, uncompressedSize);

            if (decoded != uncompressedSize)
                throw new InvalidDataException($"LZ4 decode size mismatch. Expected {uncompressedSize} bytes but got {decoded} bytes");

            output.Write(decompressed, 0, decoded);
        }

        public class Options
        {
            public LZ4Level CompressionLevel { get; set; } = LZ4Level.L10_OPT;
        }
    }
}