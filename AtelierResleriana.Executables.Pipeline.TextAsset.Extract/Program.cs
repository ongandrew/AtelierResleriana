using AtelierResleriana.Game;

namespace AtelierResleriana.Executables.Pipeline.TextAsset.Extract
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string dataDirectoryPath = "../../../../Data";
            Directory.CreateDirectory(dataDirectoryPath);

            foreach (Region region in new Region[] { Region.Japan, Region.Global })
            {
                Localization.Utilities.AssetBundleDownloader assetBundleDownloader = new Localization.Utilities.AssetBundleDownloader(new Localization.Utilities.AssetBundleDownloader.Options()
                {
                    Region = region
                });
                // RIP global.
                if (region != Region.Global)
                {
                    await assetBundleDownloader.DownloadAsync();
                }

                Localization.Utilities.AssetBundleDecryptor assetBundleDecryptor = new Localization.Utilities.AssetBundleDecryptor();
                assetBundleDecryptor.Decrypt(region, dataDirectoryPath);

                Localization.Utilities.TextAssetExtractor textAssetExtractor = new Localization.Utilities.TextAssetExtractor(new Localization.Utilities.TextAssetExtractor.Options()
                {
                    WriteJson = true
                });
                textAssetExtractor.Extract(region, dataDirectoryPath);
            }
        }
    }
}
