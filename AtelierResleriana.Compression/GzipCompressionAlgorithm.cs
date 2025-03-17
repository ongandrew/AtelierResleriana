using System.IO.Compression;

namespace AtelierResleriana.Compression
{
    [Obsolete("This class has not been tested for correctness.")]
    public class GzipCompressionAlgorithm : ICompressionAlgorithm
    {
        private readonly CompressionLevel CompressionLevel;

        public GzipCompressionAlgorithm() : this(CompressionLevel.Optimal) { }
        public GzipCompressionAlgorithm(CompressionLevel compressionLevel)
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
            using var gzipStream = new GZipStream(output, CompressionLevel, true);
            input.CopyTo(gzipStream);
        }

        public void Decompress(Stream input, Stream output)
        {
            using var gzipStream = new GZipStream(input, CompressionMode.Decompress, true);
            gzipStream.CopyTo(output);
        }
    }
}
