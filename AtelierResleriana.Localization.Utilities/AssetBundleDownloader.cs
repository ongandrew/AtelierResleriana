using AtelierResleriana.AkatsukiGames.AssetBundleCache;
using AtelierResleriana.Game;
using System.Security.Cryptography;

namespace AtelierResleriana.Localization.Utilities
{
    /// <summary>
    /// Class to download game assets ahead of time into their respective game directories.
    /// </summary>
    public class AssetBundleDownloader
    {
        public string Platform { get; private set; }
        public Region Region { get; private set; }

        public AssetBundleDownloader(Options options)
        {
            Platform = options.Platform;
            Region = options.Region;
        }

        public async Task DownloadAsync(CancellationToken cancellationToken = default)
        {
            Region region = Region;
            string platform = Platform;

            string? contentCatalogFilePath = Paths.ContentCatalogFilePath(region);

            if (contentCatalogFilePath == null)
            {
                throw new NotImplementedException();
            }

            string fileAssetsVersion = Versions.FileAssets(region);

            CatalogReader catalogReader = new CatalogReader();
            Catalog catalog = catalogReader.Read(contentCatalogFilePath);

            using AssetClient assetClient = AssetClient.ForRegion(region);

            await Parallel.ForEachAsync(catalog.FileCatalog.Bundles, async (bundleInfo, CancellationToken) =>
            {
                string assetFilePath = Path.Combine(Paths.AssetBundleCacheBundlesDirectory(region), $"{bundleInfo.BundleName}_{bundleInfo.Hash}");
                byte[] md5Hash = Convert.FromBase64String(bundleInfo.FileMd5);

                if (File.Exists(assetFilePath))
                {
                    using MD5 md5 = MD5.Create();
                    byte[] computedMd5Hash = md5.ComputeHash(File.ReadAllBytes(assetFilePath));

                    if (md5Hash.SequenceEqual(computedMd5Hash))
                    {
                        return;
                    }
                }

                byte[] assetBytes = await assetClient.GetAssetAsync(fileAssetsVersion, platform, bundleInfo.RelativePath);
                File.WriteAllBytes(assetFilePath, assetBytes);

                Console.WriteLine($"[{region}] Downloaded {bundleInfo.BundleName} ({assetBytes.Length}) from {bundleInfo.RelativePath}.");
            });
        }

        public class Options
        {
            public string Platform { get; set; } = "StandaloneWindows64";
            public required Region Region { get; set; }
        }
    }
}
