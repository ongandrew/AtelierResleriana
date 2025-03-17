using AtelierResleriana.Localization;
using AtelierResleriana.MasterData;
using AtelierResleriana.Text;
using BepInEx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Universal.Common;

namespace AtelierResleriana.Plugin.Localization
{
    public class LocalizationService
    {
        private static ISet<uint> PackedTextLocalizationTargetPropertyIds = new HashSet<uint>()
        {
            PropertyIds.Text,
            PropertyIds.LocalizedName
        };

        private LocalizationData LocalizationData { get; set; }
        private MasterDataSerializer MasterDataSerializer { get; set; } = new MasterDataSerializer();
        private MasterDataFileRuntimeUpdater MasterDataFileRuntimeUpdater { get; set; } = new MasterDataFileRuntimeUpdater();

        internal LocalizationService(LocalizationData localizationData)
        {
            LocalizationData = localizationData;

            StringFormatParameterMatcher stringFormatParameterMatcher = new StringFormatParameterMatcher();
            MasterDataFileRuntimeUpdater = new MasterDataFileRuntimeUpdater(new MasterDataFileRuntimeUpdater.Options()
            {
                ShouldUpdate = (object baseValue, object updateValue) =>
                {
                    if (!(baseValue is string baseString) || !(updateValue is string updateString))
                    {
                        return false;
                    }

                    return stringFormatParameterMatcher.IsMatch(baseString, updateString);
                }
            });
        }

        public bool IsLocalizableTextAsset(string name)
        {
            return LocalizationData.TextAssetLocalizationData.ContainsKey(name);
        }

        public bool IsLocalizableTextAsset(string name, string locale)
        {
            if (!IsLocalizableTextAsset(name))
                return false;

            // Check if any entries have localizations for this language
            return LocalizationData.TextAssetLocalizationData[name]
                .Any(entry => entry.Properties
                    .Any(prop => PackedTextLocalizationTargetPropertyIds.Contains(prop.Key) &&
                                prop.Value.Localizations.ContainsKey(locale)));
        }

        public bool IsWhitelistedSerializedFile(string directoryPath)
        {
            return LocalizationData.LocalizationSerializedFileRegistry.Whitelist.Contains(directoryPath);
        }

        public bool IsBlacklistedSerializedFile(string directoryPath)
        {
            return LocalizationData.LocalizationSerializedFileRegistry.Blacklist.Contains(directoryPath);
        }

        public bool TryLocalize(string name, string language, TextAsset textAsset, out TextAsset localizedTextAsset)
        {
            localizedTextAsset = null;

            if (!TryLocalize(name, language, textAsset.bytes, out byte[] localizedBytes))
            {
                return false;
            }

            // Create a new TextAsset with the localized content
            TextAssetFactory textAssetFactory = new TextAssetFactory();
            localizedTextAsset = textAssetFactory.CreateFromBytes(localizedBytes);
            localizedTextAsset.name = textAsset.name;

            return true;
        }

        public bool TryLocalize(string name, string locale, byte[] textAssetBytes, out byte[] localizedTextAssetBytes)
        {
            localizedTextAssetBytes = null;

            if (!IsLocalizableTextAsset(name, locale))
            {
                return false;
            }

            var reader = new PackedTextReader();

            // Validate input bytes are a valid packed text
            if (!reader.IsValid(textAssetBytes))
            {
                return false;
            }

            PackedText packedText = reader.Read(textAssetBytes);

            // Verify the source has the required properties
            if (!packedText.Properties.Any(x => x.Id == PropertyIds.Id && x.Type == PropertyTypes.UnsignedInteger) ||
                !packedText.Properties.Any(x => PackedTextLocalizationTargetPropertyIds.Contains(x.Id) && x.Type == PropertyTypes.String))
            {
                return false;
            }

            // Get localization data for this asset
            var localizationEntries = LocalizationData.TextAssetLocalizationData[name];

            // Build a map of ID to localized properties
            Dictionary<uint, Dictionary<uint, string>> localizedTextMap = new Dictionary<uint, Dictionary<uint, string>>();
            foreach (var entry in localizationEntries)
            {
                Dictionary<uint, string> propertyMap = new Dictionary<uint, string>();
                foreach (var propertyPair in entry.Properties)
                {
                    if (PackedTextLocalizationTargetPropertyIds.Contains(propertyPair.Key) &&
                        propertyPair.Value.Localizations.ContainsKey(locale))
                    {
                        propertyMap[propertyPair.Key] = propertyPair.Value.Localizations[locale];
                    }
                }
                if (propertyMap.Any())
                {
                    localizedTextMap[entry.Id] = propertyMap;
                }
            }

            if (!localizedTextMap.Any())
            {
                return false;
            }

            // Apply localized text
            bool textModified = false;
            StringFormatParameterMatcher stringFormatParameterMatcher = new StringFormatParameterMatcher();

            for (int entryIndex = 0; entryIndex < packedText.Entries.Count; entryIndex++)
            {
                uint id = packedText.GetValue<uint>(entryIndex, PropertyIds.Id);

                if (localizedTextMap.ContainsKey(id))
                {
                    foreach (var propertyPair in localizedTextMap[id])
                    {
                        string sourceText = packedText.GetValue<string>(entryIndex, propertyPair.Key);
                        string translatedText = propertyPair.Value;

                        if (stringFormatParameterMatcher.IsMatch(sourceText, translatedText))
                        {
                            packedText.SetValue(entryIndex, propertyPair.Key, translatedText);
                            textModified = true;
                        }
                    }
                }
            }

            if (!textModified)
            {
                return false;
            }

            // Return the localized bytes
            localizedTextAssetBytes = packedText.ToBytes();
            return true;
        }

        public bool TryLocalize(MasterDataFile masterDataFile, string locale)
        {
            if (LocalizationData.MasterDataLocalizationData.ContainsKey(locale) && LocalizationData.MasterDataLocalizationData[locale].ContainsKey(masterDataFile.Name))
            {
                object masterData = MasterDataSerializer.Deserialize(masterDataFile.Bytes);
                MasterDataFileRuntimeUpdater.UpdateEntities(masterData, LocalizationData.MasterDataLocalizationData[locale][masterDataFile.Name]);
                masterDataFile.Bytes = MasterDataSerializer.Serialize(masterData);

                return true;
            }

            return false;
        }

        public static LocalizationService Create(string localizationDataPath)
        {
            if (!Path.IsPathFullyQualified(localizationDataPath))
            {
                localizationDataPath = Path.Combine(Paths.PluginPath, localizationDataPath);
            }

            LocalizationData localizationData = JsonSerializer.Deserialize<LocalizationData>(
                File.ReadAllText(localizationDataPath),
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            return new LocalizationService(localizationData);
        }
    }
}