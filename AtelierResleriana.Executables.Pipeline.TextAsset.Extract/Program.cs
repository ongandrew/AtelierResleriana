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
                // JP is live, so the developers may reuse asset bundles by changing their contents, always unpack again to ensure we don't miss bundle changes (rare but happens).
                if (region == Region.Japan)
                {
                    assetBundleDecryptor = new Localization.Utilities.AssetBundleDecryptor(new Localization.Utilities.AssetBundleDecryptor.Options()
                    {
                        ForceDecrypt = true
                    });
                }
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
