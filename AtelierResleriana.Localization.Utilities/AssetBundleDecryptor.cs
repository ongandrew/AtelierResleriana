using AtelierResleriana.AkatsukiGames.AssetBundleCache;
using AtelierResleriana.Encryption;
using AtelierResleriana.Game;
using System.Security.Cryptography;

namespace AtelierResleriana.Localization.Utilities
{
    public class AssetBundleDecryptor
    {
        public bool ForceDecrypt { get; set; }

        public AssetBundleDecryptor() : this(new Options()) { }
        public AssetBundleDecryptor(Options options)
        {
            ForceDecrypt = options.ForceDecrypt;
        }

        public void Decrypt(Region region, string outputDirectoryPath)
        {
            string? contentCatalogFilePath = Paths.ContentCatalogFilePath(region);

            if (contentCatalogFilePath == null)
            {
                throw new NotImplementedException();
            }

            string fileAssetsVersion = Versions.FileAssets(region);

            CatalogReader catalogReader = new CatalogReader();
            Catalog catalog = catalogReader.Read(contentCatalogFilePath);

            string regionOutputDirectoryPath = Path.Combine(outputDirectoryPath, $"UnityFS/{region}");
            Directory.CreateDirectory(regionOutputDirectoryPath);

            Parallel.ForEach(catalog.FileCatalog.Bundles, bundleInfo =>
            {
                string assetFilePath = Path.Combine(Paths.AssetBundleCacheBundlesDirectory(region), $"{bundleInfo.BundleName}_{bundleInfo.Hash}");
                byte[] md5Hash = Convert.FromBase64String(bundleInfo.FileMd5);

                if (File.Exists(assetFilePath))
                {
                    string destinationFilePath = Path.Combine(regionOutputDirectoryPath, bundleInfo.BundleName);

                    if (!ForceDecrypt && File.Exists(destinationFilePath))
                    {
                        return;
                    }

                    using MD5 md5 = MD5.Create();
                    byte[] bundleBytes = File.ReadAllBytes(assetFilePath);
                    byte[] computedMd5Hash = md5.ComputeHash(File.ReadAllBytes(assetFilePath));

                    if (md5Hash.SequenceEqual(computedMd5Hash))
                    {
                        if (bundleInfo.Compression == 3)
                        {
                            AkatsukiAssetBundleEncryptionAlgorithm akatsukiAssetBundleEncryptionAlgorithm = new AkatsukiAssetBundleEncryptionAlgorithm(bundleInfo.BundleName, bundleInfo.FileSize, bundleInfo.Hash, bundleInfo.Crc);
                            bundleBytes = akatsukiAssetBundleEncryptionAlgorithm.Decrypt(bundleBytes);
                        }

                        File.WriteAllBytes(destinationFilePath, bundleBytes);
                        Console.WriteLine($"Decrypted {bundleInfo.BundleName}");
                    }
                }
            });
        }

        public class Options
        {
            public bool ForceDecrypt { get; set; } = false;
        }
    }
}
