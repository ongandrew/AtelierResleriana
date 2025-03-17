using System.Text.Json.Nodes;
using System.Text.Json;
using AtelierResleriana.MasterData;
using AtelierResleriana.Localization;

namespace AtelierResleriana.Executables.Pipeline.All.Finalize
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] locales =
            [
                "en",
                "zh-CN",
                "zh-TW"
            ];

            string localizationDirectoryPath = "../../../../Localization";
            string localizationMasterDataDirectoryPath = Path.Combine(localizationDirectoryPath, "MasterData");
            string localizationMasterDataBaselineExtractedDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "BaselineExtracted");
            string localizationMasterDataMachineDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Machine");
            string localizationMasterDataManualDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Manual");
            string localizationMasterDataFinalDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Final");
            string localizationPluginProjectDirectoryPath = "../../../../AtelierResleriana.Plugin.Localization";

            MasterDataFileUpdater masterDataFileUpdater = new MasterDataFileUpdater();
            Dictionary<string, Dictionary<string, JsonNode>> masterDataLocalizationData = new Dictionary<string, Dictionary<string, JsonNode>>();

            foreach (string locale in locales)
            {
                Dictionary<string, JsonNode> masterDataFileUpdateMap = new Dictionary<string, JsonNode>();
                masterDataLocalizationData.Add(locale, masterDataFileUpdateMap);

                string localizationMasterDataBaselineExtractedLocaleDirectoryPath = Path.Combine(localizationMasterDataBaselineExtractedDirectoryPath, locale);
                string localizationMasterDataMachineLocaleDirectoryPath = Path.Combine(localizationMasterDataMachineDirectoryPath, locale);
                string localizationMasterDataManualLocaleDirectoryPath = Path.Combine(localizationMasterDataManualDirectoryPath, locale);
                string localizationMasterDataFinalLocaleDirectoryPath = Path.Combine(localizationMasterDataFinalDirectoryPath, locale);
                if (Directory.Exists(localizationMasterDataFinalLocaleDirectoryPath))
                {
                    Directory.Delete(localizationMasterDataFinalLocaleDirectoryPath, true);
                }
                Directory.CreateDirectory(localizationMasterDataFinalLocaleDirectoryPath);

                foreach (string baselineExtractedFile in Directory.EnumerateFiles(localizationMasterDataBaselineExtractedLocaleDirectoryPath))
                {
                    string masterDataFileName = Path.GetFileNameWithoutExtension(baselineExtractedFile);

                    JsonNode? masterDataRootNode = JsonNode.Parse(File.ReadAllText(baselineExtractedFile));

                    if (masterDataRootNode != null)
                    {
                        string machineFile = Path.Combine(localizationMasterDataMachineLocaleDirectoryPath, Path.GetFileName(baselineExtractedFile));
                        string manualFile = Path.Combine(localizationMasterDataManualLocaleDirectoryPath, Path.GetFileName(baselineExtractedFile));
                        string finalFile = Path.Combine(localizationMasterDataFinalLocaleDirectoryPath, Path.GetFileName(baselineExtractedFile));

                        if (File.Exists(machineFile))
                        {
                            JsonNode? machineMasterDataRootNode = JsonNode.Parse(File.ReadAllText(machineFile));

                            if (machineMasterDataRootNode != null)
                            {
                                masterDataFileUpdater.UpdateEntities(masterDataRootNode, machineMasterDataRootNode);
                            }
                        }

                        if (File.Exists(manualFile))
                        {
                            JsonNode? manualMasterDataRootNode = JsonNode.Parse(File.ReadAllText(manualFile));

                            if (manualMasterDataRootNode != null)
                            {
                                masterDataFileUpdater.UpdateEntities(masterDataRootNode, manualMasterDataRootNode);
                            }
                        }

                        File.WriteAllText(finalFile, masterDataRootNode.ToJsonString(new JsonSerializerOptions()
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true
                        }));
                        masterDataFileUpdateMap.Add(masterDataFileName, masterDataRootNode);
                    }
                }
            }

            string localizationSerializedFileRegistryPath = Path.Combine(localizationDirectoryPath, "LocalizationSerializedFileRegistry.json");

            LocalizationSerializedFileRegistry localizationSerializedFileRegistry = JsonSerializer.Deserialize<LocalizationSerializedFileRegistry>(
                File.ReadAllText(localizationSerializedFileRegistryPath),
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            string localizationPackedTextEntryFinalDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Final");
            Directory.CreateDirectory(localizationPackedTextEntryFinalDirectory);

            var localizationData = new LocalizationData
            {
                Version = DateTimeOffset.UtcNow.Ticks,
                LocalizationSerializedFileRegistry = localizationSerializedFileRegistry,
                MasterDataLocalizationData = masterDataLocalizationData,
                TextAssetLocalizationData = Directory.GetFiles(localizationPackedTextEntryFinalDirectory)
                .ToDictionary(
                    filePath => Path.GetFileNameWithoutExtension(filePath),
                    filePath => JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                        File.ReadAllText(filePath),
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
                )
            };

            string localizationDataWorkingDirectoryPath = Path.Combine(localizationDirectoryPath, "LocalizationData.json");
            string localizationDataPluginProjectDirectoryPath = Path.Combine(localizationPluginProjectDirectoryPath, "Resources", "LocalizationData.json");

            File.WriteAllText(
                localizationDataWorkingDirectoryPath,
                JsonSerializer.Serialize(localizationData, new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

            File.Copy(localizationDataWorkingDirectoryPath, localizationDataPluginProjectDirectoryPath, overwrite: true);
        }
    }
}
