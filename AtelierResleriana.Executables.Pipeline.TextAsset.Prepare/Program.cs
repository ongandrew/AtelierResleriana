using AtelierResleriana.Game;
using AtelierResleriana.Localization;
using AtelierResleriana.Text;
using AtelierResleriana.Unity;
using System.Text;
using System.Text.Json;
using Universal.Common;

namespace AtelierResleriana.Executables.Pipeline.TextAsset.Prepare
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ISet<string> localizableTextAssetPrefixes = new HashSet<string>()
            {
                TextAssetPrefixes.AvantTitleMovie,
                TextAssetPrefixes.BuiltinText,
                TextAssetPrefixes.CharacterEvent,
                TextAssetPrefixes.Date,
                TextAssetPrefixes.ErrorText,
                TextAssetPrefixes.LegendEvent,
                TextAssetPrefixes.SeasonalTalkEvent,
                TextAssetPrefixes.SystemText,
                TextAssetPrefixes.TalkEvent
            };

            ISet<string> locales = new HashSet<string>()
            {
                "en",
                "zh-CN",
                "zh-TW"
            };

            ISet<uint> localizablePropertyIds = new HashSet<uint>()
            {
                PropertyIds.Text,
                PropertyIds.LocalizedName
            };

            string dataDirectoryPath = "../../../../Data";
            string japanTextAssetDirectory = Path.Combine(dataDirectoryPath, $"TextAsset/{Region.Japan}");
            string globalTextAssetDirectory = Path.Combine(dataDirectoryPath, $"TextAsset/{Region.Global}");

            string localizationDirectoryPath = "../../../../Localization";
            Directory.CreateDirectory(localizationDirectoryPath);
            string originalTextAssetDirectory = Path.Combine(localizationDirectoryPath, "TextAsset/Original");
            Directory.CreateDirectory(originalTextAssetDirectory);
            string generatedPackedTextEntryLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Generated");
            Directory.CreateDirectory(generatedPackedTextEntryLocalizationsDirectory);
            string machinePackedTextEntryLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Machine");
            Directory.CreateDirectory(machinePackedTextEntryLocalizationsDirectory);
            string manualPackedTextEntryLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Manual");
            Directory.CreateDirectory(manualPackedTextEntryLocalizationsDirectory);
            string finalPackedTextEntryLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Final");
            Directory.CreateDirectory(finalPackedTextEntryLocalizationsDirectory);

            string[] japanTextAssets = Directory.EnumerateFiles(japanTextAssetDirectory)
                .Where(file => string.IsNullOrEmpty(Path.GetExtension(file)))
                .Select(x => Path.GetFileName(x))
                .ToArray();
            string[] globalTextAssets = Directory.EnumerateFiles(globalTextAssetDirectory)
                .Where(file => string.IsNullOrEmpty(Path.GetExtension(file)))
                .Select(x => Path.GetFileName(x))
                .ToArray();

            ISet<string> localizableTextAssets = new HashSet<string>();

            TextAssetReader textAssetReader = new TextAssetReader();
            PackedTextReader packedTextReader = new PackedTextReader();
            foreach (string japanTextAsset in japanTextAssets)
            {
                if (localizableTextAssetPrefixes.Any(x => japanTextAsset.StartsWith(x)))
                {
                    string japanTextAssetFilePath = Path.Combine(japanTextAssetDirectory, japanTextAsset);
                    File.Copy(japanTextAssetFilePath, Path.Combine(originalTextAssetDirectory, japanTextAsset), true);

                    IList<PackedTextEntryLocalization> packedTextEntryLocalizations = new List<PackedTextEntryLocalization>();
                    PackedText packedText = packedTextReader.Read(textAssetReader.Read(File.ReadAllBytes(japanTextAssetFilePath)).Script);

                    if (packedText.Properties.Any(x => x.Id == PropertyIds.Id && x.Type == PropertyTypes.UnsignedInteger) &&
                        packedText.Properties.Any(x => localizablePropertyIds.Contains(x.Id) && x.Type == PropertyTypes.String))
                    {
                        ISet<uint> propertyIds = new HashSet<uint>(localizablePropertyIds.Intersect(packedText.Properties.Select(x => x.Id)));
                        IDictionary<string, Dictionary<uint, Dictionary<uint, string>>> localeEntryPropertyMap = new Dictionary<string, Dictionary<uint, Dictionary<uint, string>>>();

                        foreach (string locale in locales)
                        {
                            string localizedTextAssetFilePath = Path.Combine(globalTextAssetDirectory, $"{japanTextAsset}_{locale}");
                            if (File.Exists(localizedTextAssetFilePath))
                            {
                                PackedText localizedPackedText = packedTextReader.Read(textAssetReader.Read(File.ReadAllBytes(localizedTextAssetFilePath)).Script);

                                Dictionary<uint, Dictionary<uint, string>> localizedPackedTextProperties = new Dictionary<uint, Dictionary<uint, string>>();
                                for (int entryIndex = 0; entryIndex < localizedPackedText.Entries.Count(); entryIndex++)
                                {
                                    uint id = localizedPackedText.GetValue<uint>(entryIndex, PropertyIds.Id);
                                    localizedPackedTextProperties.Add(id, new Dictionary<uint, string>());
                                    foreach (uint propertyId in propertyIds)
                                    {
                                        localizedPackedTextProperties[id].Add(propertyId, localizedPackedText.GetValue<string>(entryIndex, propertyId));
                                    }
                                }
                                localeEntryPropertyMap.Add(locale, localizedPackedTextProperties);
                            }
                        }

                        for (int entryIndex = 0; entryIndex < packedText.Entries.Count(); entryIndex++)
                        {
                            uint id = packedText.GetValue<uint>(entryIndex, PropertyIds.Id);
                            PackedTextEntryLocalization packedTextEntryLocalization = new PackedTextEntryLocalization()
                            {
                                Id = id
                            };

                            foreach (uint propertyId in propertyIds)
                            {
                                PackedTextEntryLocalization.Property property = new PackedTextEntryLocalization.Property()
                                {
                                    Text = packedText.GetValue<string>(entryIndex, propertyId)
                                };

                                foreach ((string locale, Dictionary<uint, Dictionary<uint, string>> localizedPackedTextProperties) in localeEntryPropertyMap)
                                {
                                    if (localizedPackedTextProperties.ContainsKey(id))
                                    {
                                        if (localizedPackedTextProperties[id].ContainsKey(propertyId))
                                        {
                                            property.Localizations.Add(locale, localizedPackedTextProperties[id][propertyId]);
                                        }
                                    }
                                }

                                packedTextEntryLocalization.Properties.Add(propertyId, property);
                            }

                            packedTextEntryLocalizations.Add(packedTextEntryLocalization);
                        }
                    }

                    string packedTextEntryLocalizationsFilePath = Path.Combine(generatedPackedTextEntryLocalizationsDirectory, $"{japanTextAsset}.json");
                    File.WriteAllText(packedTextEntryLocalizationsFilePath, JsonSerializer.Serialize(packedTextEntryLocalizations, new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }));
                }
            }

            Console.WriteLine("Generated baseline files.");

            Region region = Region.Japan;

            string assetBundleDirectoryPath = Path.Combine(dataDirectoryPath, $"UnityFS/{region}");

            string[] assetBundleFilePaths = Directory.EnumerateFiles(assetBundleDirectoryPath).ToArray();

            LocalizationSerializedFileRegistry localizationSerializedFileRegistry = new LocalizationSerializedFileRegistry();

            foreach (var assetBundleFilePath in assetBundleFilePaths)
            {
                UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
                UnityFSFile unityFSFile = unityFSFileReader.Read(File.OpenRead(assetBundleFilePath));

                foreach (var unityFSFileDirectoryInfo in unityFSFile.Metadata.DirectoryInfos)
                {
                    string unityFSFileDirectoryPath = unityFSFileDirectoryInfo.Path;
                    if (unityFSFileDirectoryPath.StartsWith("CAB-") && !Path.HasExtension(unityFSFileDirectoryPath))
                    {
                        bool onWhitelist = false;

                        SerializedFileReader serializedFileReader = new SerializedFileReader();
                        SerializedFile serializedFile = serializedFileReader.Read(unityFSFile.GetDirectoryStream(unityFSFileDirectoryInfo));

                        foreach (var @object in serializedFile.Objects)
                        {
                            if (@object.ClassId == ClassIds.TextAsset)
                            {
                                SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);
                                string name = Encoding.ASCII.GetString((byte[])serializedObject["m_Name"]);
                                if (localizableTextAssetPrefixes.Any(x => name.StartsWith(x)))
                                {
                                    onWhitelist = true;
                                    break;
                                }
                            }
                        }

                        if (onWhitelist)
                        {
                            localizationSerializedFileRegistry.Whitelist.Add(unityFSFileDirectoryPath);
                        }
                        else
                        {
                            localizationSerializedFileRegistry.Blacklist.Add(unityFSFileDirectoryPath);
                        }
                    }
                }
            }

            File.WriteAllText(Path.Combine(localizationDirectoryPath, "LocalizationSerializedFileRegistry.json"), JsonSerializer.Serialize(localizationSerializedFileRegistry, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));

            Console.WriteLine("Generated file registry.");
        }
    }
}
