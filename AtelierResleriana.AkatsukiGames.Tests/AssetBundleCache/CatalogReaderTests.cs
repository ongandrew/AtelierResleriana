using System;
using System.Linq;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    [TestClass]
    [TestCategory(nameof(CatalogReader))]
    public sealed class CatalogReaderTests
    {
        [TestMethod]
        [DataRow("Resources/catalog.json")]
        public void CanRead(string path)
        {
            CatalogReader catalogReader = new CatalogReader();
            Catalog catalog = catalogReader.Read(path);
            Assert.IsNotNull(catalog);
            Assert.IsNotNull(catalog.FileCatalog);
            Assert.IsNotNull(catalog.FileCatalog.Bundles);
            Assert.AreNotEqual(0, catalog.FileCatalog.Bundles.Count());
        }
    }
}
