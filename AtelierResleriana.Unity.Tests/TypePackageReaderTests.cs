using System.IO;
using System.Text.Json;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(TypePackageReader))]
    public sealed class TypePackageReaderTests
    {
        [TestMethod]
        public void CanReadHeader()
        {
            using Stream stream = typeof(TypePackage).Assembly.GetManifestResourceStream(typeof(Resources.Root), "Uncompressed.tpk");
            TypePackageReader typePackageReader = new TypePackageReader();
            TypePackageHeader header = typePackageReader.ReadHeader(stream);
            Assert.AreEqual(TypePackageCompressionType.None, header.CompressionType);
            Assert.AreEqual(TypePackageDataType.TypeTreeInformation, header.DataType);
            Assert.AreEqual(1293823U, header.CompressedSize);
            Assert.AreEqual(1293823U, header.UncompressedSize);
            //Assert.IsNotNull(typePackage);
        }

        [TestMethod]
        public void CanReadData()
        {
            using Stream stream = typeof(TypePackage).Assembly.GetManifestResourceStream(typeof(Resources.Root), "Uncompressed.tpk");
            TypePackageReader typePackageReader = new TypePackageReader();
            TypePackageHeader header = typePackageReader.ReadHeader(stream);
            byte[] data = typePackageReader.ReadData(stream, header);

            Assert.IsNotNull(data);
            Assert.AreEqual(header.UncompressedSize, (uint)data.Length);
        }

        [TestMethod]
        public void CanReadTypeTreeBlob()
        {
            using Stream stream = typeof(TypePackage).Assembly.GetManifestResourceStream(typeof(Resources.Root), "Uncompressed.tpk");
            TypePackageReader typePackageReader = new TypePackageReader();
            TypePackage typePackage = typePackageReader.Read(stream);
            System.Console.WriteLine(JsonSerializer.Serialize(typePackage));
        }
    }
}
