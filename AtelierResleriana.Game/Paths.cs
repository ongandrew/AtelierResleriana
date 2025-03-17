using Microsoft.Win32;
#if WINDOWS
using AtelierResleriana.Windows;
#endif

namespace AtelierResleriana.Game
{
    public static class Paths
    {
        internal const string ContentCatalogPostfix = "_catalog.json";

        public static string AssetBundleCacheDirectory(Region region)
        {
            return Path.Combine(GameDataDirectory(region), "ABCache");
        }

        public static string AssetBundleCacheBundlesDirectory(Region region)
        {
            return Path.Combine(AssetBundleCacheDirectory(region), "Bundles");
        }

        public static string AssetBundlesCacheContentCatalogsDirectory(Region region)
        {
            return Path.Combine(AssetBundleCacheDirectory(region), "content_catalogs");
        }

        public static string StreamingAssetsDirectory(Region region)
        {
            return Path.Combine(GameDataDirectory(region), "StreamingAssets");
        }

        public static string AddressableAssetsDirectory(Region region)
        {
            return Path.Combine(StreamingAssetsDirectory(region), "aa");
        }

        public static string? AddressableAssetsCatalogFilePath(Region region)
        {
            return Directory.EnumerateFiles(AddressableAssetsDirectory(region), $"catalog.json").FirstOrDefault();
        }

        public static string? ContentCatalogFilePath(Region region)
        {
            return Directory.EnumerateFiles(AssetBundlesCacheContentCatalogsDirectory(region), $"*{ContentCatalogPostfix}").FirstOrDefault();
        }

        public static string? GameDirectoryName(Region region)
        {
            if (region == Region.Japan)
            {
                return "AtelierResleriana";
            }
            else if (region == Region.Global)
            {
                return "AtelierReslerianaGL";
            }

            throw new NotSupportedException();
        }

        public static string GameDirectory(Region region)
        {
            if (SteamPath == null)
            {
                throw new NotSupportedException();
            }

            return Path.Combine(SteamPath, $"steamapps/common/{GameDirectoryName(region)}");
        }

        public static string GameDataDirectory(Region region)
        {
            return Path.Combine(GameDirectory(region), $"AtelierResleriana_Data");
        }

        private static string? mSteamPath;
        public static string? SteamPath
        {
            get
            {
                string? GetSteamPathFromRegistry()
                {
                    try
                    {
                        // Try 64-bit registry first
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam"))
                        {
                            if (key != null)
                            {
                                string path = key.GetValue("InstallPath") as string;
                                if (!string.IsNullOrEmpty(path))
                                {
                                    return path;
                                }
                            }
                        }

                        // Try 32-bit registry
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                        {
                            if (key != null)
                            {
                                string path = key.GetValue("InstallPath") as string;
                                if (!string.IsNullOrEmpty(path))
                                {
                                    return path;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading registry: {ex.Message}");
                    }

                    return null;
                }

                if (mSteamPath != null)
                {
                    return mSteamPath;
                }

                mSteamPath = GetSteamPathFromRegistry();
                return mSteamPath;
            }
        }

#if WINDOWS
        public static string PersistentDataDirectory(Region region) 
        {
            string localLowDirectory = KnownFolders.LocalAppDataLow;

            if (region == Region.Global)
            {
                return Path.Combine(localLowDirectory, "KOEI TECMO GAMES CO_, LTD_", "Atelier Resleriana_ Forgotten Alchemy and the Polar Night Liberator");
            }
            else if (region == Region.Japan)
            {
                // Used to be "レスレリアーナのアトリエ ～忘れられた錬金術と極夜の解放者～" before V2.0.0.
                return Path.Combine(localLowDirectory, "KOEI TECMO GAMES CO_, LTD_", "レスレリアーナのアトリエ");
            }

            throw new NotSupportedException();
        }
#endif
    }
}
