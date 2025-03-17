using System.IO.Compression;

namespace AtelierResleriana.Compression
{
    [Obsolete("This class has not been tested for correctness.")]
    public class BrotliCompressionAlgorithm : ICompressionAlgorithm
    {
        private readonly CompressionLevel CompressionLevel;

        public BrotliCompressionAlgorithm() : this(CompressionLevel.Optimal) { }
        public BrotliCompressionAlgorithm(CompressionLevel compressionLevel)
        {
            CompressionLevel = compressionLevel;
        }

        public byte[] Compress(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Compress(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public byte[] Decompress(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Decompress(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public void Compress(Stream input, Stream output)
        {
            using var brotliStream = new BrotliStream(output, CompressionLevel, true);
            input.CopyTo(brotliStream);
        }

        public void Decompress(Stream input, Stream output)
        {
            using var brotliStream = new BrotliStream(input, CompressionMode.Decompress, true);
            brotliStream.CopyTo(output);
        }
    }
}
