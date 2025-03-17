using AtelierResleriana.MasterData;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AtelierResleriana.Executables.Pipeline.MasterData.Prepare
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
            string localizationMasterDataJapanDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Japan");
            string localizationMasterDataGlobalDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Global");
            string localizationMasterDataBaselineDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Baseline");
            string localizationMasterDataMachineDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Machine");
            string localizationMasterDataManualDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Manual");

            if (Directory.Exists(localizationMasterDataBaselineDirectoryPath))
            {
                Directory.Delete(localizationMasterDataBaselineDirectoryPath, true);
            }
            Directory.CreateDirectory(localizationMasterDataBaselineDirectoryPath);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            MasterDataFileReducer masterDataFileReducer = new MasterDataFileReducer();
            MasterDataFileUpdater masterDataFileUpdater = new MasterDataFileUpdater();

            // Process Japanese files first
            foreach (string japanMasterDataFile in Directory.EnumerateFiles(localizationMasterDataJapanDirectoryPath))
            {
                string jsonContent = File.ReadAllText(japanMasterDataFile);
                var reducedJson = masterDataFileReducer.Reduce(jsonContent);

                if (reducedJson != null)
                {
                    string baselineFilePath = Path.Combine(localizationMasterDataBaselineDirectoryPath,
                        Path.GetFileName(japanMasterDataFile));

                    File.WriteAllText(baselineFilePath,
                        reducedJson.ToJsonString(jsonSerializerOptions));
                }
            }

            string localizationMasterDataExtractedDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Extracted");
            string localizationMasterDataBaselineExtractedDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "BaselineExtracted");

            foreach (string locale in locales)
            {
                string localizationMasterDataGlobalLocaleDirectoryPath = Path.Combine(localizationMasterDataGlobalDirectoryPath, locale);

                string localizationMasterDataExtractedLocaleDirectoryPath = Path.Combine(localizationMasterDataExtractedDirectoryPath, locale);
                if (Directory.Exists(localizationMasterDataExtractedLocaleDirectoryPath))
                {
                    Directory.Delete(localizationMasterDataExtractedLocaleDirectoryPath, true);
                }
                Directory.CreateDirectory(localizationMasterDataExtractedLocaleDirectoryPath);

                string localizationMasterDataBaselineExtractedLocaleDirectoryPath = Path.Combine(localizationMasterDataBaselineExtractedDirectoryPath, locale);
                if (Directory.Exists(localizationMasterDataBaselineExtractedLocaleDirectoryPath))
                {
                    Directory.Delete(localizationMasterDataBaselineExtractedLocaleDirectoryPath, true);
                }
                Directory.CreateDirectory(localizationMasterDataBaselineExtractedLocaleDirectoryPath);

                foreach (string globalMasterDataFile in Directory.EnumerateFiles(localizationMasterDataGlobalLocaleDirectoryPath))
                {
                    string jsonContent = File.ReadAllText(globalMasterDataFile);
                    var reducedJson = masterDataFileReducer.Reduce(jsonContent);

                    if (reducedJson != null)
                    {
                        string extractedFilePath = Path.Combine(localizationMasterDataExtractedLocaleDirectoryPath,
                            Path.GetFileName(globalMasterDataFile));

                        File.WriteAllText(extractedFilePath, reducedJson.ToJsonString(jsonSerializerOptions));

                        string baselineFilePath = Path.Combine(localizationMasterDataBaselineDirectoryPath,
                            Path.GetFileName(globalMasterDataFile));

                        if (File.Exists(baselineFilePath))
                        {
                            string baselineContent = File.ReadAllText(baselineFilePath);
                            JsonNode? baseline = JsonNode.Parse(baselineContent);

                            if (baseline != null)
                            {
                                masterDataFileUpdater.UpdateEntities(baseline, reducedJson);

                                string baselineExtractedFilePath = Path.Combine(localizationMasterDataBaselineExtractedLocaleDirectoryPath,
                                    Path.GetFileName(baselineFilePath));

                                File.WriteAllText(baselineExtractedFilePath, baseline.ToJsonString(jsonSerializerOptions));
                            }
                        }
                    }
                }
            }
        }
    }
}