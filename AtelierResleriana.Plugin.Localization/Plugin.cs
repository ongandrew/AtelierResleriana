global using TextAsset = UnityEngine.TextAsset;
using AtelierResleriana.Localization;
using AtelierResleriana.MasterData;
using AtelierResleriana.Unity;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using Vuplex.WebView.Internal;
using UriBuilder = Universal.Common.UriBuilder;

namespace AtelierResleriana.Plugin.Localization
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        private const string DefaultLocalizationDataPath = "Resources/LocalizationData.json";

        public static new ManualLogSource Log { get; set; }
        public static Harmony Harmony { get; set; }
        private static LocalizationService LocalizationService { get; set; }

        private static ConfigEntry<bool> ConfigEntryEnabled { get; set; }
        private static ConfigEntry<int> ConfigEntryDialogueFontSize { get; set; }
        private static ConfigEntry<string> ConfigEntryLocale { get; set; }
        private static ConfigEntry<bool> ConfigEntryLocalizeAssetBundles { get; set; }
        private static ConfigEntry<bool> ConfigEntryLocalizeMasterData { get; set; }
        private static ConfigEntry<bool> ConfigEntryLocalizeNews { get; set; }
        private static ConfigEntry<bool> ConfigEntryUseSerializedFileCache { get; set; }

        private static ConfigEntry<string> ConfigEntryDataPath { get; set; }
        private static ConfigEntry<bool> ConfigEntryDataAutoUpdate { get; set; }
        private static ConfigEntry<string> ConfigEntryDataUpdateServer { get; set; }
        private static ConfigEntry<string> ConfigEntryNewsServerUrl { get; set; }
        private static ConfigEntry<int> ConfigEntryDataUpdateCheckTimeout { get; set; }
        private static ConfigEntry<int> ConfigEntryDataUpdateDownloadTimeout { get; set; }

        private static IDictionary<string, Il2CppStructArray<byte>> LocalizedSerializedFileCache { get; set; } = new Dictionary<string, Il2CppStructArray<byte>>();

        public static string Locale => ConfigEntryLocale.Value;

        public override void Load()
        {
            Configure();
            if (!ConfigEntryEnabled.Value)
            {
                return;
            }

            Console.OutputEncoding = Encoding.UTF8;
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            Log.LogInfo(Paths.PluginPath);

            if (ConfigEntryDataAutoUpdate.Value)
            {
                UpdateDataAsync().GetAwaiter().GetResult();
            }

            LocalizationService = LocalizationService.Create(ConfigEntryDataPath.Value);
            Log.LogInfo("Created localization service.");

            Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

            // Text asset direct loading.
            Harmony.Patch(AccessTools.Method(typeof(ProvideHandle), nameof(ProvideHandle.Complete)).MakeGenericMethod(typeof(UnityEngine.Object)),
                prefix: new HarmonyMethod(typeof(Plugin), nameof(ProvideHandleCompletePrefix)));

            if (ConfigEntryLocalizeMasterData.Value)
            {
                // Master data loading.
                Harmony.Patch(AccessTools.Method(typeof(ILNFJFLDFMJ), nameof(ILNFJFLDFMJ.IPFEBHBEJCL)),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(ILNFJFLDFMJIPFEBHBEJCLPrefix)));

                // Character profile voice line.
                Harmony.Patch(AccessTools.Method(typeof(CharaProfileDisplay), nameof(CharaProfileDisplay.KDNMGKADBEF)),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(CharaProfileDisplayKDNMGKADBEFPrefix)));

                // Character awakening window synthesis item gift description.
                Harmony.Patch(AccessTools.Method(typeof(CharaRarityUpWindow), nameof(CharaRarityUpWindow.NICCOKABDGM)),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(CharaRarityUpWindowNICCOKABDGMPrefix)));

                Harmony.Patch(AccessTools.Method(typeof(GachaResultCharacter), nameof(GachaResultCharacter.SetDefault)),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(GachaResultCharacterSetDefaultPrefix)));

                // Synthesis item material window description.
                Harmony.Patch(AccessTools.Method(typeof(SynthesisMaterialInfoDisp), nameof(SynthesisMaterialInfoDisp.KDNMGKADBEF), new Type[] { typeof(AECFEKECHCK), typeof(bool) }),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(SynthesisMaterialInfoDispKDNMGKADBEFPrefix)));
            }

            if (ConfigEntryLocalizeAssetBundles.Value)
            {
                // Asset bundle (text asset) loading.
                Harmony.Patch(AccessTools.Method(typeof(AssetBundle), nameof(AssetBundle.LoadFromStream), new Type[] { typeof(Il2CppSystem.IO.Stream) }),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(AssetBundleLoadFromStreamPrefix)));

                // Story dialogue display wrapping.
                Harmony.Patch(AccessTools.Method(typeof(DialogueDisplay), nameof(DialogueDisplay.DispText)),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(DialogueDisplayDispTextPrefix)));
            }

            if (ConfigEntryLocalizeNews.Value)
            {
                // News - experimental - unsupported.
                Harmony.Patch(AccessTools.Method(typeof(BaseWebView), nameof(BaseWebView.LoadUrl), new Type[] { typeof(string), typeof(Il2CppSystem.Collections.Generic.Dictionary<string, string>) }),
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(BaseWebViewLoadUrlPrefix)));
            }
        }

        private void Configure()
        {
            Config.SaveOnConfigSet = false;

            ConfigEntryEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable or disable this localization plugin.");
            ConfigEntryLocale = Config.Bind<string>("General", "Language", "en", "Language/locale to target - should be one of en, zh-CN, or zh-TW.");
            ConfigEntryLocalizeMasterData = Config.Bind<bool>("General", "LocalizeMasterData", true, "Enable this to enable the localization of master data.");
            ConfigEntryLocalizeNews = Config.Bind<bool>("General", "LocalizeNews", false, "[Experimental] Enable this to use a separate news server for localized news in-game. This doesn't quite work right (external browser is opened) so no support is provided if this is enabled.");

            ConfigEntryLocalizeAssetBundles = Config.Bind<bool>("General", "LocalizeAssetBundles", true, "Enable this to enable interception of asset bundles for localized dialogue scenes. Enabling this will have some impact on loading times.");
            ConfigEntryUseSerializedFileCache = Config.Bind<bool>("General", "UseSerializedFileCache", true, "Enable this to trade increased RAM usage for reduced loading times by caching processed files that the game may request multiple times.");
            ConfigEntryDialogueFontSize = Config.Bind<int>("General", "DialogueFontSize", 32, "The font size to use for dialogue displays.");

            ConfigEntryDataPath = Config.Bind<string>("Data", "Path", DefaultLocalizationDataPath, "The path to the localization data file to use - specified as a relative path to the plugin directory or an absolute path.");
            ConfigEntryDataAutoUpdate = Config.Bind<bool>("Data", "AutoUpdate", true, "Enable this to automatically check for updates to the localization data. This will only run if the default localization data path is used.");
            ConfigEntryNewsServerUrl = Config.Bind<string>("Data", "NewsServerUrl", "https://atelierresleriana.azurewebsites.net/news/", "[Experimental] The news server to use.");
            ConfigEntryDataUpdateServer = Config.Bind<string>("Data", "UpdateServer", "atelierresleriana.azurewebsites.net", "The host of the server to use to check for automatic updates.");
            ConfigEntryDataUpdateCheckTimeout = Config.Bind<int>("Data", "UpdateCheckTimeout", 5, "The number of seconds to wait before timing out on the check for localization data updates.");
            ConfigEntryDataUpdateDownloadTimeout = Config.Bind<int>("Data", "UpdateDownloadTimeout", 30, "The number of seconds to wait for localization updates to download before timing out.");

            Config.Save();
        }

        private unsafe static void AssetBundleLoadFromStreamPrefix(ref Il2CppSystem.Object stream)
        {
            if (stream == null) return;

            try
            {
                Il2CppSystem.IO.Stream originalStream = stream.TryCast<Il2CppSystem.IO.Stream>();
                if (originalStream == null) return;

                int size = (int)originalStream.Length;
                Il2CppStructArray<byte> il2cppArray = new Il2CppStructArray<byte>(size);
                originalStream.Read(il2cppArray);
                originalStream.Seek(0, Il2CppSystem.IO.SeekOrigin.Begin);

                Il2CppSystem.IO.MemoryStream intermediateStream = new Il2CppSystem.IO.MemoryStream(il2cppArray);
                UnityFSFileRuntimeReader unityFSFileRuntimeReader = new UnityFSFileRuntimeReader();
                UnityFSFile runtimeUnityFSFile = unityFSFileRuntimeReader.Read(intermediateStream);
                intermediateStream.Dispose();

                string cacheKey = string.Join("|", runtimeUnityFSFile.Metadata.DirectoryInfos.Select(x => x.Path));
                if (ConfigEntryUseSerializedFileCache.Value && LocalizedSerializedFileCache.ContainsKey(cacheKey))
                {
                    Il2CppSystem.IO.MemoryStream newStream = new Il2CppSystem.IO.MemoryStream(LocalizedSerializedFileCache[cacheKey]);
                    stream = newStream;
                    Log.LogInfo("Returned asset bundle from cache.");
                    return;
                }

                bool anyWhitelisted = false;
                foreach (var directoryInfo in runtimeUnityFSFile.Metadata.DirectoryInfos)
                {
                    string directoryPath = directoryInfo.Path;
                    
                    if (directoryPath.StartsWith("CAB-") && !Path.HasExtension(directoryPath))
                    {
                        if (!LocalizationService.IsWhitelistedSerializedFile(directoryPath))
                        {
                            if (!LocalizationService.IsBlacklistedSerializedFile(directoryPath))
                            {
                                Log.LogWarning($"Unrecognized serialized file '{directoryPath}'. The plugin requires an update.");
                            }
                            continue;
                        }
                        anyWhitelisted = true;
                    }
                }

                if (!anyWhitelisted)
                {
                    return;
                }

                byte[] bytes = il2cppArray.ToBytes();

                using MemoryStream managedStream = new MemoryStream(bytes);
                UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
                UnityFSFile unityFSFile = unityFSFileReader.Read(managedStream);

                bool modified = false;

                IList<UnityFSFileDirectory> modifiedDirectories = new List<UnityFSFileDirectory>();

                foreach (var directoryInfo in unityFSFile.Metadata.DirectoryInfos)
                {
                    string directoryPath = directoryInfo.Path;
                    bool directoryModified = false;

                    if (directoryPath.StartsWith("CAB-") && !Path.HasExtension(directoryPath))
                    {
                        if (!LocalizationService.IsWhitelistedSerializedFile(directoryPath))
                        {
                            continue;
                        }

                        SerializedFileReader serializedFileReader = new SerializedFileReader();
                        using MemoryStream directoryStream = new MemoryStream(unityFSFile.GetDirectoryBytes(directoryInfo));
                        SerializedFile serializedFile = serializedFileReader.Read(directoryStream);

                        var modifiedObjects = new List<SerializedFileWriter.SerializedFileObject>();

                        bool anyObjectModified = false;

                        for (int i = 0; i < serializedFile.Objects.Length; i++)
                        {
                            var originalObject = serializedFile.Objects[i];
                            var serializedObject = serializedFile.GetSerializedObject(originalObject);

                            var writerObject = new SerializedFileWriter.SerializedFileObject
                            {
                                PathId = originalObject.PathId,
                                TypeId = originalObject.TypeId,
                                SerializedObject = serializedObject,
                                DataIndex = i  // Preserve order
                            };

                            // Check if it's a TextAsset that needs localization
                            if (originalObject.ClassId == ClassIds.TextAsset)
                            {
                                byte[] scriptBytes = (byte[])serializedObject["m_Script"];
                                string name = Encoding.ASCII.GetString((byte[])serializedObject["m_Name"]);

                                if (LocalizationService.IsLocalizableTextAsset(name, Locale))
                                {
                                    byte[] localizedBytes;
                                    if (LocalizationService.TryLocalize(name, Locale, scriptBytes, out localizedBytes))
                                    {
                                        // Update the serialized object with the localized bytes
                                        serializedObject["m_Script"] = localizedBytes;
                                        writerObject.SerializedObject = serializedObject;
                                        anyObjectModified = true;

                                        Log.LogInfo($"Localized TextAsset '{name}'.");
                                    }
                                }
                            }

                            modifiedObjects.Add(writerObject);
                        }

                        // If anything was modified, rewrite the serialized file
                        if (anyObjectModified)
                        {
                            SerializedFileWriter writer = new SerializedFileWriter(new SerializedFileWriter.Options
                            {
                                IsBigEndian = serializedFile.Header.IsBigEndian,
                                UnityVersion = serializedFile.Metadata.UnityVersion,
                                TargetPlatform = serializedFile.Metadata.TargetPlatform,
                                EnableTypeTree = serializedFile.Metadata.EnableTypeTree
                            });

                            using MemoryStream newDirectoryStream = (MemoryStream)writer.Write(
                                serializedFile.Types,
                                modifiedObjects.ToArray(),
                                serializedFile.ScriptReferences,
                                serializedFile.AssetReferences,
                                serializedFile.ReferenceTypes,
                                serializedFile.UserInformation
                            );

                            modifiedDirectories.Add(new UnityFSFileDirectory
                            {
                                Path = directoryPath,
                                Bytes = newDirectoryStream.ToArray(),
                                Flags = directoryInfo.Flags
                            });

                            directoryModified = true;
                            modified = true;
                        }
                    }

                    // If this directory wasn't modified, add the original
                    if (!directoryModified)
                    {
                        modifiedDirectories.Add(new UnityFSFileDirectory
                        {
                            Path = directoryPath,
                            Bytes = unityFSFile.GetDirectoryBytes(directoryInfo),
                            Flags = directoryInfo.Flags
                        });
                    }
                }

                // If any modifications were made, repack the bundle
                if (modified)
                {
                    UnityFSFileWriter unityFSFileWriter = new UnityFSFileWriter(new UnityFSFileWriter.Options
                    {
                        Compression = UnityFSFileCompression.Lz4
                    });

                    using MemoryStream newBundleStream = (MemoryStream)unityFSFileWriter.Write(modifiedDirectories);
                    byte[] newBundleBytes = newBundleStream.ToArray();

                    Il2CppStructArray<byte> newIl2cppArray = newBundleBytes.ToIl2CppBytes();

                    if (ConfigEntryUseSerializedFileCache.Value)
                    {
                        LocalizedSerializedFileCache.Add(cacheKey, newIl2cppArray);
                    }

                    Il2CppSystem.IO.MemoryStream newStream = new Il2CppSystem.IO.MemoryStream(newIl2cppArray);

                    // Replace the original stream
                    stream = newStream;
                    Log.LogInfo("Localized AssetBundle.");
                }
            }
            catch (Exception exception)
            {
                Log.LogError($"Error in AssetBundleLoadFromStreamPrefix: {exception.Message}");
            }
        }

        private static void BaseWebViewLoadUrlPrefix(ref string url)
        {
            if (url == "https://info.resleriana.jp/news/")
            {
                string newsServerUrl = ConfigEntryNewsServerUrl.Value;

                if (!string.IsNullOrWhiteSpace(newsServerUrl))
                {
                    url = newsServerUrl;
                }
            }
        }

        private static void CharaProfileDisplayKDNMGKADBEFPrefix(CharaProfileDisplay __instance)
        {
            if (__instance != null)
            {
                if (__instance.m_voiceDetailText != null)
                {
                    __instance.m_voiceDetailText.enableWordWrapping = true;
                    __instance.m_voiceDetailText.overflowMode = TMPro.TextOverflowModes.Overflow;
                }
            }
        }

        private static void CharaRarityUpWindowNICCOKABDGMPrefix(CharaRarityUpWindow __instance)
        {
            if (__instance != null)
            {
                if (__instance.m_giftBonusElement != null)
                {
                    if (__instance.m_giftBonusElement.m_detailText != null)
                    {
                        __instance.m_giftBonusElement.m_detailText.enableWordWrapping = true;
                        __instance.m_giftBonusElement.m_detailText.overflowMode = TMPro.TextOverflowModes.Overflow;
                    }
                }
            }
        }

        private static void GachaResultCharacterSetDefaultPrefix(GachaResultCharacter __instance)
        {
            if (__instance != null)
            {
                if (__instance.m_acquisitionText != null)
                {
                    __instance.m_acquisitionText.enableWordWrapping = true;
                    __instance.m_acquisitionText.overflowMode = TMPro.TextOverflowModes.Overflow;
                }
            }
        }

        private static void DialogueDisplayDispTextPrefix(DialogueDisplay __instance/*, 
            string LLMMBHHFAMB, // Speaker
            string CLLMMFAONMB // Text
            */
            )
        {
            try
            {
                if (__instance != null &&
                    __instance.m_speakerText != null &&
                    __instance.m_dialogueText != null)
                {
                    __instance.m_speakerText.enableAutoSizing = false;

                    __instance.m_dialogueText.enableWordWrapping = true;
                    __instance.m_dialogueText.overflowMode = TMPro.TextOverflowModes.Overflow;
                    __instance.m_dialogueText.enableAutoSizing = false;

                    __instance.m_speakerText.fontSize = ConfigEntryDialogueFontSize.Value;
                    __instance.m_speakerText.fontSizeMin = ConfigEntryDialogueFontSize.Value;
                    __instance.m_speakerText.fontSizeMax = ConfigEntryDialogueFontSize.Value;

                    __instance.m_dialogueText.fontSize = ConfigEntryDialogueFontSize.Value;
                    __instance.m_dialogueText.fontSizeMin = ConfigEntryDialogueFontSize.Value;
                    __instance.m_dialogueText.fontSizeMax = ConfigEntryDialogueFontSize.Value;
                }
            }
            catch (Exception exception)
            {
                Log.LogError($"Error in DialogueDisplayDispTextPrefix: {exception.Message}");
            }
        }

        // Loads the unencrypted master data.
        // How to find - look in MasterMemory for an abstract base class that looks like this.
        // Find the implementation in Blend.
        // Patch!
        private static void ILNFJFLDFMJIPFEBHBEJCLPrefix(ref Il2CppStructArray<byte> PIOKAGCFDFK)
        {
            int size = PIOKAGCFDFK.Length;

            byte[] managedBytes = new byte[size];

            unsafe
            {
                fixed (byte* managedPtr = managedBytes)
                {
                    byte* il2cppPtr = (byte*)IntPtr.Add(PIOKAGCFDFK.Pointer, 4 * IntPtr.Size).ToPointer();
                    Buffer.MemoryCopy(il2cppPtr, managedPtr, size, size);
                }
            }

            using MemoryStream memoryStream = new MemoryStream(managedBytes);

            MasterDataReader masterDataReader = new MasterDataReader();

            IEnumerable<MasterDataFile> masterDataFiles =
                masterDataReader.ReadAsync(memoryStream)
                    .GetAwaiter()
                    .GetResult();

            bool anyChange = false;

            foreach (MasterDataFile masterDataFile in masterDataFiles)
            {
                if (LocalizationService.TryLocalize(masterDataFile, Locale))
                {
                    anyChange = true;
                }
            }

            if (anyChange)
            {
                using MemoryStream outputStream = new MemoryStream();
                MasterDataWriter masterDataWriter = new MasterDataWriter();
                masterDataWriter.WriteAsync(outputStream, masterDataFiles)
                    .GetAwaiter()
                    .GetResult();

                outputStream.Seek(0, SeekOrigin.Begin);
                managedBytes = outputStream.ToArray();
            }

            Il2CppStructArray<byte> newArray = new Il2CppStructArray<byte>(managedBytes.Length);

            unsafe
            {
                byte* destPtr = (byte*)IntPtr.Add(newArray.Pointer, 4 * IntPtr.Size).ToPointer();
                fixed (byte* srcPtr = managedBytes)
                {
                    Buffer.MemoryCopy(srcPtr, destPtr, managedBytes.Length, managedBytes.Length);
                }
            }

            PIOKAGCFDFK = newArray;

            Log.LogInfo("Localized MasterData.");
        }

        private static void ProvideHandleCompletePrefix(ProvideHandle __instance, ref UnityEngine.Object result, bool status, Il2CppSystem.Exception exception)
        {
            if (result == null)
            {
                return;
            }

            if (result.GetIl2CppType().Equals(Il2CppType.Of<TextAsset>()))
            {
                TextAsset textAsset = result.TryCast<TextAsset>();

                if (textAsset != null)
                {
                    string name = textAsset.name;

                    TextAsset localizedTextAsset;
                    if (LocalizationService.TryLocalize(name, Locale, textAsset, out localizedTextAsset))
                    {
                        result = localizedTextAsset;
                        Log.LogInfo($"Localized TextAsset '{name}'.");
                    }
                }
            }
        }

        private static void SynthesisMaterialInfoDispKDNMGKADBEFPrefix(SynthesisMaterialInfoDisp __instance, AECFEKECHCK JFCOKAJJKAB)
        {
            __instance.m_itemDetailText.enableWordWrapping = true;
        }

        private async Task UpdateDataAsync()
        {
            Log.LogInfo("Checking for updates. Disable this behavior in the configuration file.");

            string filePath = ConfigEntryDataPath.Value;

            if (filePath != DefaultLocalizationDataPath)
            {
                Log.LogWarning($"Auto-updates are only possible with the default localization data path ({DefaultLocalizationDataPath}).");
                return;
            }

            filePath = Path.Combine(Paths.PluginPath, filePath);

            string host = ConfigEntryDataUpdateServer.Value;
            if (string.IsNullOrWhiteSpace(host))
            {
                Log.LogWarning("Updates enabled but an invalid server was specified.");
                return;
            }

            using HttpClient httpClient = new HttpClient()
            {
                Timeout = Timeout.InfiniteTimeSpan
            };

            UriBuilder localizationDataVersionsUriBuilder = new UriBuilder("https", host);
            localizationDataVersionsUriBuilder.AddSegments("api", "Localization", "Data", "Version");

            try
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                if (ConfigEntryDataUpdateCheckTimeout.Value > 0)
                {
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(ConfigEntryDataUpdateCheckTimeout.Value));
                }
                string localizationDataVersionsJson = await httpClient.GetStringAsync(localizationDataVersionsUriBuilder, cancellationTokenSource.Token);
                LocalizationDataVersion[] localizationDataVersions = JsonSerializer.Deserialize<LocalizationDataVersion[]>(localizationDataVersionsJson, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (!localizationDataVersions.Any())
                {
                    Log.LogWarning("No data versions were found.");
                    return;
                }

                bool shouldUpdate = false;

                if (!File.Exists(filePath))
                {
                    shouldUpdate = true;
                }
                else
                {
                    try
                    {
                        LocalizationData localizationData = JsonSerializer.Deserialize<LocalizationData>(File.ReadAllText(filePath), new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        shouldUpdate = localizationDataVersions.Any(x => x.Version > localizationData.Version);
                    }
                    catch
                    {
                        shouldUpdate = true;
                    }
                }

                if (!shouldUpdate)
                {
                    Log.LogInfo("Localization data is already up-to-date.");
                    return;
                }

                try
                {
                    Log.LogInfo("Update found. Downloading...");
                    LocalizationDataVersion localizationDataVersion = localizationDataVersions.OrderByDescending(x => x.Version).First();
                    Log.LogInfo(localizationDataVersion.Uri);

                    cancellationTokenSource = new CancellationTokenSource();
                    if (ConfigEntryDataUpdateDownloadTimeout.Value > 0)
                    {
                        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(ConfigEntryDataUpdateDownloadTimeout.Value));
                    }

                    byte[] localizationDataZipArchiveBytes = await httpClient.GetByteArrayAsync(localizationDataVersion.Uri, cancellationTokenSource.Token);

                    try
                    {
                        using Stream localizationDataZipArchiveStream = new MemoryStream(localizationDataZipArchiveBytes);
                        ZipArchive localizationDataZipArchive = new ZipArchive(localizationDataZipArchiveStream, ZipArchiveMode.Read);
                        ZipArchiveEntry localizationDataZipArchiveEntry = localizationDataZipArchive.GetEntry("LocalizationData.json");

                        if (localizationDataZipArchiveEntry == null)
                        {
                            Log.LogWarning("Could not find localization data in the archive.");
                            return;
                        }

                        using Stream localizationDataZipArchiveEntryStream = localizationDataZipArchiveEntry.Open();
                        using MemoryStream localizationDataStream = new MemoryStream();

                        localizationDataZipArchiveEntryStream.CopyTo(localizationDataStream);
                        localizationDataStream.Seek(0, SeekOrigin.Begin);

                        LocalizationData updatedlocalizationData = JsonSerializer.Deserialize<LocalizationData>(localizationDataStream, new JsonSerializerOptions()
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        if (updatedlocalizationData.LocalizationSerializedFileRegistry == null ||
                            updatedlocalizationData.MasterDataLocalizationData == null ||
                            updatedlocalizationData.TextAssetLocalizationData == null)
                        {
                            throw new FormatException("Localization data has incorrect format.");
                        }

                        localizationDataStream.Seek(0, SeekOrigin.Begin);
                        File.WriteAllBytes(filePath, localizationDataStream.ToArray());

                        Log.LogInfo("Successfully updated localization data.");
                        return;
                    }
                    catch (Exception e)
                    {
                        Log.LogInfo("Downloaded localization data is invalid.");
                        Log.LogError(e.ToString());
                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.LogWarning("Failed to download localization data update.");
                    Log.LogError(e.ToString());
                    return;
                }
            }
            catch
            {
                Log.LogWarning("Failed to retrieve data versions.");
                return;
            }
        }

        private record class LocalizationDataVersion
        {
            public long Version { get; set; }
            public Uri Uri { get; set; }
        }
    }
}