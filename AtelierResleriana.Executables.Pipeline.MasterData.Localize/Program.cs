using AtelierResleriana.Localization;
using AtelierResleriana.Localization.Utilities;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Universal.Anthropic.Client.V1;
using Universal.GenerativeAI.Anthropic;

namespace AtelierResleriana.Executables.Pipeline.MasterData.Localize
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            string[] locales =
            [
                "en"
            ];

            string localizationDirectoryPath = "../../../../Localization";
            string localizationMasterDataDirectoryPath = Path.Combine(localizationDirectoryPath, "MasterData");
            string localizationMasterDataGlobalDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Global");
            string localizationMasterDataBaselineDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Baseline");
            string localizationMasterDataExtractedDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Extracted");
            string localizationMasterDataBaselineExtractedDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "BaselineExtracted");
            string localizationMasterDataManualDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Manual");
            string localizationMasterDataMachineDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Machine");
            string localizationMasterDataIncompleteDirectoryPath = Path.Combine(localizationMasterDataDirectoryPath, "Incomplete");

            if (Directory.Exists(localizationMasterDataIncompleteDirectoryPath))
            {
                Directory.Delete(localizationMasterDataIncompleteDirectoryPath, true);
            }
            Directory.CreateDirectory(localizationMasterDataIncompleteDirectoryPath);

            foreach (string locale in locales)
            {
                string localizationMasterDataGlobalLocaleDirectoryPath = Path.Combine(localizationMasterDataGlobalDirectoryPath, locale);
                string localizationMasterDataIncompleteLocaleDirectoryPath = Path.Combine(localizationMasterDataIncompleteDirectoryPath, locale);
                Directory.CreateDirectory(localizationMasterDataIncompleteLocaleDirectoryPath);

                // Process each file in the baseline and compare with global
                foreach (string baselineFile in Directory.EnumerateFiles(localizationMasterDataBaselineDirectoryPath))
                {
                    string baselineContent = File.ReadAllText(baselineFile);
                    JsonNode? baselineJson = JsonNode.Parse(baselineContent);

                    if (baselineJson?.AsArray() == null) continue; // Skip if not an array

                    string globalFilePath = Path.Combine(localizationMasterDataGlobalLocaleDirectoryPath, Path.GetFileName(baselineFile));

                    // Get baseline IDs
                    var baselineIds = new HashSet<int>();
                    foreach (var entry in baselineJson.AsArray())
                    {
                        if (entry?["id"]?.GetValue<int>() is int id)
                        {
                            baselineIds.Add(id);
                        }
                    }

                    // Get global IDs
                    var globalIds = new HashSet<int>();
                    if (File.Exists(globalFilePath))
                    {
                        string globalContent = File.ReadAllText(globalFilePath);
                        if (JsonNode.Parse(globalContent)?.AsArray() is JsonArray globalJson)
                        {
                            foreach (var entry in globalJson)
                            {
                                if (entry?["id"]?.GetValue<int>() is int id)
                                {
                                    globalIds.Add(id);
                                }
                            }
                        }
                    }

                    // Check manual directory
                    string manualFilePath = Path.Combine(localizationMasterDataManualDirectoryPath, locale, Path.GetFileName(baselineFile));
                    if (File.Exists(manualFilePath))
                    {
                        string manualContent = File.ReadAllText(manualFilePath);
                        if (JsonNode.Parse(manualContent)?.AsArray() is JsonArray manualJson)
                        {
                            foreach (var entry in manualJson)
                            {
                                if (entry?["id"]?.GetValue<int>() is int id)
                                {
                                    globalIds.Add(id); // Add manual IDs to the "complete" set
                                }
                            }
                        }
                    }

                    // Check machine directory
                    string machineFilePath = Path.Combine(localizationMasterDataMachineDirectoryPath, locale, Path.GetFileName(baselineFile));
                    if (File.Exists(machineFilePath))
                    {
                        string machineContent = File.ReadAllText(machineFilePath);
                        if (JsonNode.Parse(machineContent)?.AsArray() is JsonArray machineJson)
                        {
                            foreach (var entry in machineJson)
                            {
                                if (entry?["id"]?.GetValue<int>() is int id)
                                {
                                    globalIds.Add(id); // Add machine IDs to the "complete" set
                                }
                            }
                        }
                    }

                    // Find incomplete entries (now considers global, manual, and machine translations)
                    var incompleteIds = baselineIds.Except(globalIds);
                    if (incompleteIds.Any())
                    {
                        var incompleteEntries = new JsonArray();
                        foreach (var entry in baselineJson.AsArray())
                        {
                            if (entry?["id"]?.GetValue<int>() is int id && incompleteIds.Contains(id))
                            {
                                incompleteEntries.Add(entry.DeepClone());
                            }
                        }

                        if (incompleteEntries.Count > 0)
                        {
                            string incompleteFilePath = Path.Combine(localizationMasterDataIncompleteLocaleDirectoryPath,
                                Path.GetFileName(baselineFile));

                            File.WriteAllText(incompleteFilePath,
                                incompleteEntries.ToJsonString(new JsonSerializerOptions()
                                {
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                    WriteIndented = true
                                }));
                        }
                    }
                }
            }

            ILocalizer[] localizers =
            [
                new EnglishLocalizer(
                    new AnthropicTextGenerator(new AnthropicClient(configuration["Anthropic:ApiKey"]), new AnthropicTextGenerator.Options()
                    {
                        Model = Models.ClaudeSonnet46
                    }),
                    new AnthropicTextTransformer(new AnthropicClient(configuration["Anthropic:ApiKey"]), new AnthropicTextTransformer.Options()
                    {
                        Model = Models.ClaudeSonnet4
                    })
                )
            ];


            foreach (string locale in locales)
            {
                ILocalizer localizer = localizers.First(x => x.Locale == locale);

                string localizationMasterDataExtractedLocaleDirectoryPath = Path.Combine(localizationMasterDataExtractedDirectoryPath, locale);
                string localizationMasterDataManualLocaleDirectoryPath = Path.Combine(localizationMasterDataManualDirectoryPath, locale);
                string localizationMasterDataMachineLocaleDirectoryPath = Path.Combine(localizationMasterDataMachineDirectoryPath, locale);
                string localizationMasterDataIncompleteLocaleDirectoryPath = Path.Combine(localizationMasterDataIncompleteDirectoryPath, locale);
                Directory.CreateDirectory(localizationMasterDataMachineLocaleDirectoryPath);

                foreach (string incompleteFilePath in Directory.EnumerateFiles(localizationMasterDataIncompleteLocaleDirectoryPath))
                {
                    IList<JsonObject> incompleteObjects = new List<JsonObject>();

                    JsonNode? incompleteJsonNode = JsonNode.Parse(File.ReadAllText(incompleteFilePath));

                    if (incompleteJsonNode is JsonArray incompleteJsonArray)
                    {
                        foreach (JsonNode? incompleteJsonArrayItem in incompleteJsonArray)
                        {
                            if (incompleteJsonArrayItem is JsonObject incompleteObject)
                            {
                                incompleteObjects.Add(incompleteObject);
                            }
                        }
                    }

                    string fileName = Path.GetFileName(incompleteFilePath);
                    string masterDataFileName = Path.GetFileNameWithoutExtension(fileName);

                    string baselineFilePath = Path.Combine(localizationMasterDataBaselineDirectoryPath, fileName);
                    string extractedFilePath = Path.Combine(localizationMasterDataExtractedLocaleDirectoryPath, fileName);
                    string machineFilePath = Path.Combine(localizationMasterDataMachineLocaleDirectoryPath, fileName);
                    string manualFilePath = Path.Combine(localizationMasterDataManualLocaleDirectoryPath, fileName);

                    Dictionary<int, JsonObject> localizedObjectMap = new Dictionary<int, JsonObject>();

                    // Load extracted localizations
                    if (File.Exists(extractedFilePath))
                    {
                        JsonNode? jsonNode = JsonNode.Parse(File.ReadAllText(extractedFilePath));

                        if (jsonNode is JsonArray jsonArray)
                        {
                            foreach (JsonNode? jsonArrayItem in jsonArray)
                            {
                                if (jsonArrayItem is JsonObject jsonObject &&
                                    jsonObject["id"] is JsonValue idValue &&
                                    idValue.TryGetValue<int>(out int id))
                                {
                                    localizedObjectMap[id] = jsonObject;
                                }
                            }
                        }
                    }

                    // Load and merge machine translations
                    if (File.Exists(machineFilePath))
                    {
                        JsonNode? jsonNode = JsonNode.Parse(File.ReadAllText(machineFilePath));

                        if (jsonNode is JsonArray jsonArray)
                        {
                            foreach (JsonNode? jsonArrayItem in jsonArray)
                            {
                                if (jsonArrayItem is JsonObject jsonObject &&
                                    jsonObject["id"] is JsonValue idValue &&
                                    idValue.TryGetValue<int>(out int id))
                                {
                                    localizedObjectMap[id] = jsonObject;
                                }
                            }
                        }
                    }

                    // Load and merge manual translations (highest priority)
                    if (File.Exists(manualFilePath))
                    {
                        JsonNode? jsonNode = JsonNode.Parse(File.ReadAllText(manualFilePath));

                        if (jsonNode is JsonArray jsonArray)
                        {
                            foreach (JsonNode? jsonArrayItem in jsonArray)
                            {
                                if (jsonArrayItem is JsonObject jsonObject &&
                                    jsonObject["id"] is JsonValue idValue &&
                                    idValue.TryGetValue<int>(out int id))
                                {
                                    localizedObjectMap[id] = jsonObject;
                                }
                            }
                        }
                    }

                    Dictionary<int, JsonObject> baselineObjectMap = new Dictionary<int, JsonObject>();

                    // Load baseline entries
                    if (File.Exists(baselineFilePath))
                    {
                        JsonNode? jsonNode = JsonNode.Parse(File.ReadAllText(baselineFilePath));

                        if (jsonNode is JsonArray jsonArray)
                        {
                            foreach (JsonNode? jsonArrayItem in jsonArray)
                            {
                                if (jsonArrayItem is JsonObject jsonObject &&
                                    jsonObject["id"] is JsonValue idValue &&
                                    idValue.TryGetValue<int>(out int id))
                                {
                                    baselineObjectMap[id] = jsonObject;
                                }
                            }
                        }
                    }

                    IList<MasterDataLocalizationExample> localizationExamples = new List<MasterDataLocalizationExample>();
                    foreach ((int id, JsonObject localizedObject) in localizedObjectMap)
                    {
                        if (baselineObjectMap.TryGetValue(id, out JsonObject baselineObject))
                        {
                            var (trimmedBaseline, trimmedLocalized) = GetMatchingSchemaObjects(baselineObject, localizedObject);

                            // Only add if we have more than just the ID field
                            if (trimmedBaseline.Count > 1 && trimmedLocalized.Count > 1)
                            {
                                localizationExamples.Add(new MasterDataLocalizationExample
                                {
                                    Original = trimmedBaseline,
                                    Localized = trimmedLocalized
                                });
                            }
                        }
                    }

                    JsonArray machineLocalizations = new JsonArray();

                    if (File.Exists(machineFilePath))
                    {
                        JsonArray? existingMachineLocalizations = JsonNode.Parse(File.ReadAllText(machineFilePath)) as JsonArray;

                        if (existingMachineLocalizations != null)
                        {
                            machineLocalizations = existingMachineLocalizations;
                        }
                    }

                    if (machineLocalizations == null)
                    {
                        // Shouldn't happen.
                        continue;
                    }

                    int CountStringFields(JsonNode node)
                    {
                        int count = 0;
                        if (node is JsonObject jsonObj)
                        {
                            foreach (var prop in jsonObj)
                            {
                                if (prop.Value is JsonValue val && val.TryGetValue<string>(out _))
                                    count++;
                                else if (prop.Value != null)
                                    count += CountStringFields(prop.Value);
                            }
                        }
                        else if (node is JsonArray arr)
                        {
                            foreach (var item in arr)
                            {
                                if (item != null)
                                    count += CountStringFields(item);
                            }
                        }
                        return count;
                    }

                    int batchSize = 40; // Default for simple objects
                    if (incompleteObjects.Count > 0)
                    {
                        int complexity = CountStringFields(incompleteObjects[0]);
                        if (complexity > 5)
                        {
                            batchSize = 3;
                        }
                        else if (complexity > 3)
                        {
                            batchSize = 10;
                        }
                        else if (complexity > 1)
                        {
                            batchSize = 20;
                        }
                    }

                    for (int i = 0; i < incompleteObjects.Count; i += batchSize)
                    {
                        var batch = incompleteObjects.Skip(i).Take(batchSize).ToList();
                        Console.WriteLine($"[{masterDataFileName}] Processing batch {i / batchSize + 1} of {(incompleteObjects.Count + batchSize - 1) / batchSize}");

                        IEnumerable<JsonObject> localizedObjects = await localizer.LocalizeAsync(
                            masterDataFileName,
                            batch,
                            localizationExamples);

                        var localizedList = localizedObjects.ToList();
                        if (localizedList.Count != batch.Count)
                        {
                            throw new InvalidOperationException($"Localization returned {localizedList.Count} objects but expected {batch.Count}");
                        }

                        foreach (var localizedObj in localizedList)
                        {
                            // Because STJ doesn't allow adding to another array.
                            var clonedObj = JsonNode.Parse(localizedObj.ToJsonString())?.AsObject()
                                ?? throw new InvalidOperationException("Failed to clone JsonObject");
                            machineLocalizations.Add(clonedObj);
                        }

                        File.WriteAllText(
                            machineFilePath,
                            machineLocalizations.ToJsonString(new JsonSerializerOptions
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                WriteIndented = true
                            }));

                        Console.WriteLine($"[{masterDataFileName}] Successfully processed and saved batch of {batch.Count} objects");
                    }
                }
            }
        }

        private static (JsonObject baseline, JsonObject localized) GetMatchingSchemaObjects(JsonObject baselineObject, JsonObject localizedObject)
        {
            var trimmedBaseline = new JsonObject();
            var trimmedLocalized = new JsonObject();

            // Always preserve ID if it exists
            if (baselineObject.ContainsKey("id") && localizedObject.ContainsKey("id"))
            {
                trimmedBaseline["id"] = JsonNode.Parse(baselineObject["id"].ToJsonString());
                trimmedLocalized["id"] = JsonNode.Parse(localizedObject["id"].ToJsonString());
            }

            foreach (var baselineKey in baselineObject.Select(kvp => kvp.Key))
            {
                if (baselineKey == "id") continue;
                if (!localizedObject.ContainsKey(baselineKey)) continue;

                var baselineValue = baselineObject[baselineKey];
                var localizedValue = localizedObject[baselineKey];

                // Skip if either value is null
                if (baselineValue == null || localizedValue == null) continue;

                // Handle nested objects recursively
                if (baselineValue is JsonObject baselineChildObj && localizedValue is JsonObject localizedChildObj)
                {
                    var (trimmedChildBaseline, trimmedChildLocalized) = GetMatchingSchemaObjects(baselineChildObj, localizedChildObj);

                    // Only add if the nested objects have matching content
                    if (trimmedChildBaseline.Count > 0 && trimmedChildLocalized.Count > 0)
                    {
                        trimmedBaseline[baselineKey] = trimmedChildBaseline;
                        trimmedLocalized[baselineKey] = trimmedChildLocalized;
                    }
                }
                // Handle arrays
                else if (baselineValue is JsonArray baselineArray && localizedValue is JsonArray localizedArray)
                {
                    var (trimmedBaselineArray, trimmedLocalizedArray) = GetMatchingSchemaArrays(baselineArray, localizedArray);

                    // Only add if the arrays have content
                    if (trimmedBaselineArray.Count > 0 && trimmedLocalizedArray.Count > 0)
                    {
                        trimmedBaseline[baselineKey] = trimmedBaselineArray;
                        trimmedLocalized[baselineKey] = trimmedLocalizedArray;
                    }
                }
                // Handle primitive values
                else if (baselineValue is JsonValue && localizedValue is JsonValue)
                {
                    trimmedBaseline[baselineKey] = JsonNode.Parse(baselineValue.ToJsonString());
                    trimmedLocalized[baselineKey] = JsonNode.Parse(localizedValue.ToJsonString());
                }
            }

            return (trimmedBaseline, trimmedLocalized);
        }

        private static (JsonArray baseline, JsonArray localized) GetMatchingSchemaArrays(JsonArray baselineArray, JsonArray localizedArray)
        {
            var trimmedBaselineArray = new JsonArray();
            var trimmedLocalizedArray = new JsonArray();

            // Only process arrays of the same length
            if (baselineArray.Count != localizedArray.Count) return (trimmedBaselineArray, trimmedLocalizedArray);

            for (int i = 0; i < baselineArray.Count; i++)
            {
                var baselineItem = baselineArray[i];
                var localizedItem = localizedArray[i];

                // Skip if either item is null
                if (baselineItem == null || localizedItem == null) continue;

                // Handle nested objects in arrays
                if (baselineItem is JsonObject baselineObj && localizedItem is JsonObject localizedObj)
                {
                    var (trimmedBaselineObj, trimmedLocalizedObj) = GetMatchingSchemaObjects(baselineObj, localizedObj);

                    // Only add if the objects have matching content
                    if (trimmedBaselineObj.Count > 0 && trimmedLocalizedObj.Count > 0)
                    {
                        trimmedBaselineArray.Add(trimmedBaselineObj);
                        trimmedLocalizedArray.Add(trimmedLocalizedObj);
                    }
                }
                // Handle nested arrays
                else if (baselineItem is JsonArray nestedBaselineArray && localizedItem is JsonArray nestedLocalizedArray)
                {
                    var (trimmedNestedBaseline, trimmedNestedLocalized) = GetMatchingSchemaArrays(nestedBaselineArray, nestedLocalizedArray);

                    if (trimmedNestedBaseline.Count > 0 && trimmedNestedLocalized.Count > 0)
                    {
                        trimmedBaselineArray.Add(trimmedNestedBaseline);
                        trimmedLocalizedArray.Add(trimmedNestedLocalized);
                    }
                }
                // Handle primitive values in arrays
                else if (baselineItem is JsonValue && localizedItem is JsonValue)
                {
                    trimmedBaselineArray.Add(JsonNode.Parse(baselineItem.ToJsonString()));
                    trimmedLocalizedArray.Add(JsonNode.Parse(localizedItem.ToJsonString()));
                }
            }

            return (trimmedBaselineArray, trimmedLocalizedArray);
        }
    }
}