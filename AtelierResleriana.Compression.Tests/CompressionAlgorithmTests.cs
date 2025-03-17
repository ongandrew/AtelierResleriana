namespace AtelierResleriana.Compression.Tests
{
    [TestClass]
    [TestCategory(nameof(ICompressionAlgorithm))]
    public sealed class CompressionAlgorithmTests
    {
        [DataRow(typeof(BrotliCompressionAlgorithm))]
        [DataRow(typeof(GzipCompressionAlgorithm))]
        [DataRow(typeof(LzmaCompressionAlgorithm))]
        //[DataRow(typeof(UnityLzmaCompressionAlgorithm))]
        [TestMethod]
        public void CompressDecompress_AreEqual(Type type)
        {
            ICompressionAlgorithm compressionAlgorithm = (ICompressionAlgorithm)Activator.CreateInstance(type);

            byte[] bytes = [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7
            ];

            byte[] compressedBytes = compressionAlgorithm.Compress(bytes);
            byte[] decompressedBytes = compressionAlgorithm.Decompress(compressedBytes);

            Assert.IsTrue(bytes.SequenceEqual(decompressedBytes));
        }

        [TestMethod]
        public void CompressDecompressLz4_AreEqual()
        {
            Lz4CompressionAlgorithm compressionAlgorithm = new Lz4CompressionAlgorithm();

            byte[] bytes = [
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7
            ];

            byte[] compressedBytes = compressionAlgorithm.Compress(bytes);
            byte[] decompressedBytes = compressionAlgorithm.Decompress(compressedBytes, bytes.Length);

            Assert.IsTrue(bytes.SequenceEqual(decompressedBytes));
        }

        [TestMethod]
        public void CompressDecompressLz4Of2To17Length_AreEqual()
        {
            Lz4CompressionAlgorithm compressionAlgorithm = new Lz4CompressionAlgorithm();
            byte[] bytes = new byte[131072];  // 2^17
            Random.Shared.NextBytes(bytes);
            byte[] compressedBytes = compressionAlgorithm.Compress(bytes);
            byte[] decompressedBytes = compressionAlgorithm.Decompress(compressedBytes, bytes.Length);
            Assert.IsTrue(bytes.SequenceEqual(decompressedBytes));
        }
    }
}
