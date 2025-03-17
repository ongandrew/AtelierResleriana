namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(UnityFSFileCompression))]
    public sealed class UnityFSFileCompressionTests
    {
        [TestMethod]
        [DataRow(579U, UnityFSFileCompression.Lz4hc)]
        public void CanRead(uint flags, UnityFSFileCompression expectedCompression)
        {
            UnityFSFileHeader header = new UnityFSFileHeader { Flags = flags };
            Assert.AreEqual(expectedCompression, header.Compression);
        }
    }
}
