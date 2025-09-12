using AtelierResleriana.Localization;
using AtelierResleriana.Localization.Utilities;
using AtelierResleriana.Text;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using Universal.Anthropic.Client.V1;
using Universal.GenerativeAI.Anthropic;

namespace AtelierResleriana.Executables.Pipeline.TextAsset.Localize
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            ISet<string> locales = new HashSet<string>()
            {
                "en"
            };

            ISet<uint> localizablePropertyIds = new HashSet<uint>()
            {
                PropertyIds.Text,
                PropertyIds.LocalizedName
            };

            string localizationDirectoryPath = "../../../../Localization";
            string generatedLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Generated");
            string manualLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Manual");
            string machineLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Machine");
            string combinedLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Combined");
            Directory.CreateDirectory(combinedLocalizationsDirectory);
            foreach (string file in Directory.GetFiles(combinedLocalizationsDirectory))
            {
                File.Delete(file);
            }
            string incompleteLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Incomplete");
            Directory.CreateDirectory(incompleteLocalizationsDirectory);
            foreach (string file in Directory.GetFiles(incompleteLocalizationsDirectory))
            {
                File.Delete(file);
            }
            string finalLocalizationsDirectory = Path.Combine(localizationDirectoryPath, "PackedTextEntryLocalization/Final");
            Directory.CreateDirectory(finalLocalizationsDirectory);
            foreach (string file in Directory.GetFiles(finalLocalizationsDirectory))
            {
                File.Delete(file);
            }
            string staticLocalizationDataFilePath = Path.Combine(localizationDirectoryPath, "StaticLocalizationData.json");
            Dictionary<string, Dictionary<string, string>> staticLocalizationData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(staticLocalizationDataFilePath));

            foreach (string generatedLocalizationFilePath in Directory.EnumerateFiles(generatedLocalizationsDirectory))
            {
                string fileName = Path.GetFileName(generatedLocalizationFilePath);
                PackedTextEntryLocalization[] generatedLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(File.ReadAllText(generatedLocalizationFilePath), new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                IDictionary<uint, PackedTextEntryLocalization> manualLocalizations = new Dictionary<uint, PackedTextEntryLocalization>();
                IDictionary<uint, PackedTextEntryLocalization> machineLocalizations = new Dictionary<uint, PackedTextEntryLocalization>();

                string manualLocalizationFilePath = Path.Combine(manualLocalizationsDirectory, fileName);
                if (File.Exists(manualLocalizationFilePath))
                {
                    manualLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(File.ReadAllText(manualLocalizationFilePath), new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }).ToDictionary(x => x.Id, x => x);
                }

                string machineLocalizationFilePath = Path.Combine(machineLocalizationsDirectory, fileName);
                if (File.Exists(machineLocalizationFilePath))
                {
                    machineLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(File.ReadAllText(machineLocalizationFilePath), new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }).ToDictionary(x => x.Id, x => x);
                }

                foreach (PackedTextEntryLocalization generatedLocalization in generatedLocalizations)
                {
                    // Combine in machine localizations first.
                    if (machineLocalizations.ContainsKey(generatedLocalization.Id))
                    {
                        PackedTextEntryLocalization machineLocalization = machineLocalizations[generatedLocalization.Id];
                        foreach (var propertyPair in machineLocalization.Properties)
                        {
                            if (localizablePropertyIds.Contains(propertyPair.Key) &&
                                generatedLocalization.Properties.ContainsKey(propertyPair.Key))
                            {
                                var generatedProperty = generatedLocalization.Properties[propertyPair.Key];
                                var machineProperty = propertyPair.Value;

                                // Merge the localizations from the machine translation
                                foreach (var localization in machineProperty.Localizations)
                                {
                                    if (locales.Contains(localization.Key))
                                    {
                                        generatedProperty.Localizations[localization.Key] = localization.Value;
                                    }
                                }
                            }
                        }
                    }

                    // Then combine in manual localizations.
                    if (manualLocalizations.ContainsKey(generatedLocalization.Id))
                    {
                        PackedTextEntryLocalization manualLocalization = manualLocalizations[generatedLocalization.Id];
                        foreach (var propertyPair in manualLocalization.Properties)
                        {
                            if (localizablePropertyIds.Contains(propertyPair.Key) &&
                                generatedLocalization.Properties.ContainsKey(propertyPair.Key))
                            {
                                var generatedProperty = generatedLocalization.Properties[propertyPair.Key];
                                var manualProperty = propertyPair.Value;

                                // Manual localizations take precedence over machine ones
                                foreach (var localization in manualProperty.Localizations)
                                {
                                    if (locales.Contains(localization.Key))
                                    {
                                        generatedProperty.Localizations[localization.Key] = localization.Value;
                                    }
                                }
                            }
                        }
                    }
                }

                string combinedLocalizationFilePath = Path.Combine(combinedLocalizationsDirectory, fileName);
                File.WriteAllText(combinedLocalizationFilePath, JsonSerializer.Serialize(generatedLocalizations, new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

                // Identify entries that need localization
                List<PackedTextEntryLocalization> incompleteLocalization = new List<PackedTextEntryLocalization>();
                foreach (var entry in generatedLocalizations)
                {
                    bool entryNeedsLocalization = false;
                    foreach (var propertyPair in entry.Properties)
                    {
                        if (localizablePropertyIds.Contains(propertyPair.Key))
                        {
                            foreach (string locale in locales)
                            {
                                if (!propertyPair.Value.Localizations.ContainsKey(locale))
                                {
                                    entryNeedsLocalization = true;
                                    break;
                                }
                            }
                            if (entryNeedsLocalization) break;
                        }
                    }
                    if (entryNeedsLocalization)
                    {
                        incompleteLocalization.Add(entry);
                    }
                }

                if (incompleteLocalization.Count > 0)
                {
                    string incompleteLocalizationFilePath = Path.Combine(incompleteLocalizationsDirectory, fileName);
                    File.WriteAllText(incompleteLocalizationFilePath, JsonSerializer.Serialize(incompleteLocalization.ToArray(), new JsonSerializerOptions()
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    }));
                }
            }

            ILocalizer[] localizers = 
            [
                new EnglishLocalizer(
                    new AnthropicTextGenerator(new AnthropicClient(configuration["Anthropic:ApiKey"]), new AnthropicTextGenerator.Options() 
                    {
                        Model = Models.Claude37Sonnet
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

                ISet<string> generalTextTypes = new HashSet<string>()
                {
                    TextAssetPrefixes.BuiltinText,
                    TextAssetPrefixes.ErrorText,
                    TextAssetPrefixes.SystemText
                };

                foreach (string generalTextType in generalTextTypes)
                {
                    string fileName = $"{generalTextType}.json";
                    string filePath = Path.Combine(incompleteLocalizationsDirectory, fileName);
                    if (File.Exists(filePath))
                    {
                        PackedTextEntryLocalization[] packedTextEntryLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(File.ReadAllText(filePath),
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                        GeneralText[] generalTexts = packedTextEntryLocalizations
                            .Select(x => new GeneralText()
                            {
                                Text = x.Properties[PropertyIds.Text].Text
                            }).ToArray();


                        GeneralText[] localizedGeneralTexts = (await localizer.LocalizeAsync(generalTexts)).ToArray();

                        if (generalTexts.Count() != localizedGeneralTexts.Count())
                        {
                            throw new InvalidOperationException("Mismatch in counts.");
                        }

                        string destinationFilePath = Path.Combine(machineLocalizationsDirectory, fileName);

                        PackedTextEntryLocalization[] machinePackedTextEntryLocalizations = new PackedTextEntryLocalization[localizedGeneralTexts.Count()];
                        for (int i = 0; i < localizedGeneralTexts.Count(); i++)
                        {
                            machinePackedTextEntryLocalizations[i] =
                                new PackedTextEntryLocalization()
                                {
                                    Id = packedTextEntryLocalizations[i].Id,
                                    Properties = new Dictionary<uint, PackedTextEntryLocalization.Property>()
                                    {
                                        [PropertyIds.Text] = new PackedTextEntryLocalization.Property()
                                        {
                                            Text = packedTextEntryLocalizations[i].Properties[PropertyIds.Text].Text,
                                            Localizations = new Dictionary<string, string>()
                                            {
                                                [locale] = localizedGeneralTexts[i].Text
                                            }
                                        }
                                    }
                                };
                        }

                        if (File.Exists(destinationFilePath))
                        {
                            // Read and deserialize existing translations
                            PackedTextEntryLocalization[] existingTranslations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                                File.ReadAllText(destinationFilePath),
                                new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                });

                            // Create a dictionary for faster lookups
                            var existingTranslationsDict = existingTranslations.ToDictionary(x => x.Id, x => x);

                            // Create a list to store the merged results
                            var mergedTranslations = new List<PackedTextEntryLocalization>();

                            // Add all existing translations first
                            mergedTranslations.AddRange(existingTranslations);

                            // Process each new translation
                            foreach (var newTranslation in machinePackedTextEntryLocalizations)
                            {
                                if (existingTranslationsDict.TryGetValue(newTranslation.Id, out var existingTranslation))
                                {
                                    // Entry exists, update properties
                                    foreach (var propertyPair in newTranslation.Properties)
                                    {
                                        if (!existingTranslation.Properties.ContainsKey(propertyPair.Key))
                                        {
                                            // Property doesn't exist, add it
                                            existingTranslation.Properties[propertyPair.Key] = propertyPair.Value;
                                        }
                                        else
                                        {
                                            // Property exists, update/add localizations for the current locale
                                            var existingProperty = existingTranslation.Properties[propertyPair.Key];
                                            foreach (var localization in propertyPair.Value.Localizations)
                                            {
                                                existingProperty.Localizations[localization.Key] = localization.Value;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Entry doesn't exist, add it
                                    mergedTranslations.Add(newTranslation);
                                }
                            }

                            // Replace machinePackedTextEntryLocalizations with merged results
                            machinePackedTextEntryLocalizations = mergedTranslations.ToArray();
                        }

                        File.WriteAllText(destinationFilePath, JsonSerializer.Serialize(
                            machinePackedTextEntryLocalizations,
                            new JsonSerializerOptions
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true
                            }));
                    }
                }

                ISet<string> basicDialogueTextTypes = new HashSet<string>()
                {
                    TextAssetPrefixes.CharacterEvent,
                    TextAssetPrefixes.Date,
                    TextAssetPrefixes.LegendEvent,
                    TextAssetPrefixes.SeasonalTalkEvent,
                    TextAssetPrefixes.SideStoryEvent
                };

                foreach (string dialogueTextType in basicDialogueTextTypes)
                {
                    var matchingFiles = Directory.GetFiles(incompleteLocalizationsDirectory)
                        .Where(file => Path.GetFileName(file).StartsWith($"{dialogueTextType}", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => Path.GetFileName(x).Replace($"{dialogueTextType}_", ""))
                        .ToList();

                    var combinedFiles = Directory.GetFiles(combinedLocalizationsDirectory)
                        .Where(file => Path.GetFileName(file).StartsWith($"{dialogueTextType}", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => Path.GetFileName(x).Replace($"{dialogueTextType}_", ""))
                        .ToList();

                    // Dictionary to store all localizations in chronological order
                    Dictionary<string, List<LocalizedDialogueText>> localizationHistory = new Dictionary<string, List<LocalizedDialogueText>>();

                    // Load existing localizations
                    foreach (var file in combinedFiles)
                    {
                        string fileKey = Path.GetFileName(file);
                        var entries = new List<LocalizedDialogueText>();

                        PackedTextEntryLocalization[] packedEntries = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                            File.ReadAllText(file),
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        foreach (var entry in packedEntries)
                        {
                            if (entry.Properties.ContainsKey(PropertyIds.LocalizedName) &&
                                entry.Properties[PropertyIds.LocalizedName].Localizations.ContainsKey(locale) &&
                                entry.Properties.ContainsKey(PropertyIds.Text) &&
                                entry.Properties[PropertyIds.Text].Localizations.ContainsKey(locale))
                            {
                                entries.Add(new LocalizedDialogueText
                                {
                                    Speaker = entry.Properties[PropertyIds.LocalizedName].Text,
                                    Text = entry.Properties[PropertyIds.Text].Text,
                                    LocalizedSpeaker = entry.Properties[PropertyIds.LocalizedName].Localizations[locale],
                                    LocalizedText = entry.Properties[PropertyIds.Text].Localizations[locale]
                                });
                            }
                        }

                        localizationHistory[fileKey] = entries;
                    }

                    // Process each file that needs translation
                    foreach (string filePath in matchingFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        PackedTextEntryLocalization[] packedTextEntryLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                            File.ReadAllText(filePath),
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        DialogueText[] dialogueTexts = packedTextEntryLocalizations
                            .Select(x => new DialogueText
                            {
                                Speaker = x.Properties[PropertyIds.LocalizedName].Text,
                                Text = x.Properties[PropertyIds.Text].Text
                            }).ToArray();

                        // Build context from preceding files
                        List<LocalizedDialogueText> contextDialogues = new List<LocalizedDialogueText>();
                        string currentFileNumber = fileName.Replace($"{dialogueTextType}_", "");

                        // Get chronologically preceding entries
                        var precedingKeys = localizationHistory.Keys
                            .Where(key =>
                            {
                                string keyNumber = key.Replace($"{dialogueTextType}_", "");
                                return string.Compare(keyNumber, currentFileNumber) < 0;
                            })
                            .OrderBy(key => key.Replace($"{dialogueTextType}_", ""))
                            .ToList();

                        foreach (var key in precedingKeys)
                        {
                            if (localizationHistory.ContainsKey(key))
                            {
                                contextDialogues.AddRange(localizationHistory[key]);
                            }
                        }

                        // Process the dialogues with the context
                        DialogueText[] localizedDialogueTexts = (await localizer.LocalizeAsync(
                            dialogueTexts,
                            contextDialogues)
                            ).ToArray();

                        if (dialogueTexts.Length != localizedDialogueTexts.Length)
                        {
                            throw new InvalidOperationException("Mismatch in counts.");
                        }

                        // Add to our dictionary for subsequent files
                        localizationHistory[fileName] = Enumerable.Range(0, dialogueTexts.Length)
                            .Select(i => new LocalizedDialogueText
                            {
                                Speaker = dialogueTexts[i].Speaker,
                                Text = dialogueTexts[i].Text,
                                LocalizedSpeaker = localizedDialogueTexts[i].Speaker,
                                LocalizedText = localizedDialogueTexts[i].Text
                            })
                            .ToList();

                        string destinationFilePath = Path.Combine(machineLocalizationsDirectory, fileName);

                        // Convert back to PackedTextEntryLocalizations
                        PackedTextEntryLocalization[] machinePackedTextEntryLocalizations = new PackedTextEntryLocalization[localizedDialogueTexts.Length];
                        for (int i = 0; i < localizedDialogueTexts.Length; i++)
                        {
                            machinePackedTextEntryLocalizations[i] = new PackedTextEntryLocalization
                            {
                                Id = packedTextEntryLocalizations[i].Id,
                                Properties = new Dictionary<uint, PackedTextEntryLocalization.Property>
                                {
                                    [PropertyIds.Text] = new PackedTextEntryLocalization.Property
                                    {
                                        Text = packedTextEntryLocalizations[i].Properties[PropertyIds.Text].Text,
                                        Localizations = new Dictionary<string, string>
                                        {
                                            [locale] = localizedDialogueTexts[i].Text
                                        }
                                    },
                                    [PropertyIds.LocalizedName] = new PackedTextEntryLocalization.Property
                                    {
                                        Text = packedTextEntryLocalizations[i].Properties[PropertyIds.LocalizedName].Text,
                                        Localizations = new Dictionary<string, string>
                                        {
                                            [locale] = localizedDialogueTexts[i].Speaker
                                        }
                                    }
                                }
                            };
                        }

                        // Handle existing translations (merge with new ones)
                        if (File.Exists(destinationFilePath))
                        {
                            // Read and deserialize existing translations
                            PackedTextEntryLocalization[] existingTranslations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                                File.ReadAllText(destinationFilePath),
                                new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                });

                            // Create a dictionary for faster lookups
                            var existingTranslationsDict = existingTranslations.ToDictionary(x => x.Id, x => x);

                            // Create a list to store the merged results
                            var mergedTranslations = new List<PackedTextEntryLocalization>();

                            // Add all existing translations first
                            mergedTranslations.AddRange(existingTranslations);

                            // Process each new translation
                            foreach (var newTranslation in machinePackedTextEntryLocalizations)
                            {
                                if (existingTranslationsDict.TryGetValue(newTranslation.Id, out var existingTranslation))
                                {
                                    // Entry exists, update properties
                                    foreach (var propertyPair in newTranslation.Properties)
                                    {
                                        if (!existingTranslation.Properties.ContainsKey(propertyPair.Key))
                                        {
                                            // Property doesn't exist, add it
                                            existingTranslation.Properties[propertyPair.Key] = propertyPair.Value;
                                        }
                                        else
                                        {
                                            // Property exists, update/add localizations for the current locale
                                            var existingProperty = existingTranslation.Properties[propertyPair.Key];
                                            foreach (var localization in propertyPair.Value.Localizations)
                                            {
                                                existingProperty.Localizations[localization.Key] = localization.Value;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Entry doesn't exist, add it
                                    mergedTranslations.Add(newTranslation);
                                }
                            }

                            // Replace machinePackedTextEntryLocalizations with merged results
                            machinePackedTextEntryLocalizations = mergedTranslations.ToArray();
                        }

                        // Write the final results
                        File.WriteAllText(destinationFilePath, JsonSerializer.Serialize(
                            machinePackedTextEntryLocalizations,
                            new JsonSerializerOptions
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true
                            }));
                    }
                }

                ISet<string> storyDialogueTextTypes = new HashSet<string>()
                {
                    TextAssetPrefixes.TalkEvent
                };

                foreach (string dialogueTextType in storyDialogueTextTypes)
                {
                    var matchingFiles = Directory.GetFiles(incompleteLocalizationsDirectory)
                        .Where(file => Path.GetFileName(file).StartsWith($"{dialogueTextType}", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => Path.GetFileName(x).Replace($"{dialogueTextType}_", ""))
                        .ToList();

                    var combinedFiles = Directory.GetFiles(combinedLocalizationsDirectory)
                        .Where(file => Path.GetFileName(file).StartsWith($"{dialogueTextType}", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(x => Path.GetFileName(x).Replace($"{dialogueTextType}_", ""))
                        .ToList();

                    // Dictionary to store all localizations in chronological order
                    Dictionary<string, List<LocalizedDialogueText>> localizationHistory = new Dictionary<string, List<LocalizedDialogueText>>();

                    // Load official localizations first
                    foreach (var file in combinedFiles)
                    {
                        string fileKey = Path.GetFileName(file);
                        var entries = new List<LocalizedDialogueText>();

                        PackedTextEntryLocalization[] packedEntries = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                            File.ReadAllText(file),
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        foreach (var entry in packedEntries)
                        {
                            if (entry.Properties.ContainsKey(PropertyIds.LocalizedName) &&
                                entry.Properties[PropertyIds.LocalizedName].Localizations.ContainsKey(locale) &&
                                entry.Properties.ContainsKey(PropertyIds.Text) &&
                                entry.Properties[PropertyIds.Text].Localizations.ContainsKey(locale))
                            {
                                entries.Add(new LocalizedDialogueText
                                {
                                    Speaker = entry.Properties[PropertyIds.LocalizedName].Text,
                                    Text = entry.Properties[PropertyIds.Text].Text,
                                    LocalizedSpeaker = entry.Properties[PropertyIds.LocalizedName].Localizations[locale],
                                    LocalizedText = entry.Properties[PropertyIds.Text].Localizations[locale]
                                });
                            }
                        }

                        localizationHistory[fileKey] = entries;
                    }

                    // Process each file that needs translation
                    foreach (string filePath in matchingFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        PackedTextEntryLocalization[] packedTextEntryLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                            File.ReadAllText(filePath),
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        DialogueText[] dialogueTexts = packedTextEntryLocalizations
                            .Select(x => new DialogueText
                            {
                                Speaker = x.Properties[PropertyIds.LocalizedName].Text,
                                Text = x.Properties[PropertyIds.Text].Text
                            }).ToArray();

                        List<LocalizedDialogueText> contextDialogues = new List<LocalizedDialogueText>();

                        string currentFileNumber = fileName.Replace($"{dialogueTextType}_", "");

                        // Get all keys that chronologically precede this file
                        var precedingKeys = localizationHistory.Keys
                            .Where(key =>
                            {
                                string keyNumber = key.Replace($"{dialogueTextType}_", "");
                                return string.Compare(keyNumber, currentFileNumber) < 0;
                            })
                            .OrderBy(key => key.Replace($"{dialogueTextType}_", ""))
                            .ToList();

                        foreach (var key in precedingKeys)
                        {
                            if (localizationHistory.ContainsKey(key))
                            {
                                contextDialogues.AddRange(localizationHistory[key]);
                            }
                        }

                        int entryQuota = 800;
                        var recentDialogues = contextDialogues.Count <= entryQuota
                            ? contextDialogues
                            : contextDialogues.Skip(contextDialogues.Count - entryQuota);

                        // Generate a story summary from the context
                        string storySummary = await localizer.SummarizeAsync(
                            recentDialogues.Select(d => new DialogueText
                            {
                                Speaker = d.LocalizedSpeaker,
                                Text = d.LocalizedText
                            }).ToList(),
                            CancellationToken.None);
                        Console.WriteLine($"Generated story summary for {fileName}: {storySummary.Length} characters");

                        // Process the dialogues with samples and the story summary
                        DialogueText[] localizedDialogueTexts = (await localizer.LocalizeAsync(
                            dialogueTexts,
                            contextDialogues,
                            storySummary)
                            ).ToArray();

                        if (dialogueTexts.Length != localizedDialogueTexts.Length)
                        {
                            // This sometimes happens, it's generative AI after all.
                            // If so the most logical thing to do is to just retry the program - this could be done automatically at some stage if we're confident it's just once in a blue moon errors (otherwise GG API key spend).
                            throw new InvalidOperationException("Mismatch in counts.");
                        }

                        // Add to our dictionary for subsequent files
                        localizationHistory[fileName] = Enumerable.Range(0, dialogueTexts.Length)
                            .Select(i => new LocalizedDialogueText
                            {
                                Speaker = dialogueTexts[i].Speaker,
                                Text = dialogueTexts[i].Text,
                                LocalizedSpeaker = localizedDialogueTexts[i].Speaker,
                                LocalizedText = localizedDialogueTexts[i].Text
                            })
                            .ToList();

                        string destinationFilePath = Path.Combine(machineLocalizationsDirectory, fileName);

                        // Convert back to PackedTextEntryLocalizations
                        PackedTextEntryLocalization[] machinePackedTextEntryLocalizations = new PackedTextEntryLocalization[localizedDialogueTexts.Length];
                        for (int i = 0; i < localizedDialogueTexts.Length; i++)
                        {
                            machinePackedTextEntryLocalizations[i] = new PackedTextEntryLocalization
                            {
                                Id = packedTextEntryLocalizations[i].Id,
                                Properties = new Dictionary<uint, PackedTextEntryLocalization.Property>
                                {
                                    [PropertyIds.Text] = new PackedTextEntryLocalization.Property
                                    {
                                        Text = packedTextEntryLocalizations[i].Properties[PropertyIds.Text].Text,
                                        Localizations = new Dictionary<string, string>
                                        {
                                            [locale] = localizedDialogueTexts[i].Text
                                        }
                                    },
                                    [PropertyIds.LocalizedName] = new PackedTextEntryLocalization.Property
                                    {
                                        Text = packedTextEntryLocalizations[i].Properties[PropertyIds.LocalizedName].Text,
                                        Localizations = new Dictionary<string, string>
                                        {
                                            [locale] = localizedDialogueTexts[i].Speaker
                                        }
                                    }
                                }
                            };
                        }

                        // Handle existing translations (merge with new ones)
                        if (File.Exists(destinationFilePath))
                        {
                            // Read and deserialize existing translations
                            PackedTextEntryLocalization[] existingTranslations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                                File.ReadAllText(destinationFilePath),
                                new JsonSerializerOptions
                                {
                                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                                });

                            // Create a dictionary for faster lookups
                            var existingTranslationsDict = existingTranslations.ToDictionary(x => x.Id, x => x);

                            var mergedTranslations = new List<PackedTextEntryLocalization>();

                            // Add all existing translations first
                            mergedTranslations.AddRange(existingTranslations);

                            // Process each new translation
                            foreach (var newTranslation in machinePackedTextEntryLocalizations)
                            {
                                if (existingTranslationsDict.TryGetValue(newTranslation.Id, out var existingTranslation))
                                {
                                    // Entry exists, update properties
                                    foreach (var propertyPair in newTranslation.Properties)
                                    {
                                        if (!existingTranslation.Properties.ContainsKey(propertyPair.Key))
                                        {
                                            // Property doesn't exist, add it
                                            existingTranslation.Properties[propertyPair.Key] = propertyPair.Value;
                                        }
                                        else
                                        {
                                            // Property exists, update/add localizations for the current locale
                                            var existingProperty = existingTranslation.Properties[propertyPair.Key];
                                            foreach (var localization in propertyPair.Value.Localizations)
                                            {
                                                existingProperty.Localizations[localization.Key] = localization.Value;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Entry doesn't exist, add it
                                    mergedTranslations.Add(newTranslation);
                                }
                            }

                            // Replace machinePackedTextEntryLocalizations with merged results
                            machinePackedTextEntryLocalizations = mergedTranslations.ToArray();
                        }

                        // Write the final results
                        File.WriteAllText(destinationFilePath, JsonSerializer.Serialize(
                            machinePackedTextEntryLocalizations,
                            new JsonSerializerOptions
                            {
                                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                WriteIndented = true
                            }));
                    }
                }

                foreach (string combinedFilePath in Directory.EnumerateFiles(combinedLocalizationsDirectory))
                {   
                    string fileName = Path.GetFileName(combinedFilePath);
                    string machineFilePath = Path.Combine(machineLocalizationsDirectory, fileName);
                    string finalFilePath = Path.Combine(finalLocalizationsDirectory, fileName);

                    // Read combined localizations
                    PackedTextEntryLocalization[] combinedLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                        File.ReadAllText(combinedFilePath),
                        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                    // If we have new machine translations, merge them
                    if (File.Exists(machineFilePath))
                    {
                        PackedTextEntryLocalization[] machineLocalizations = JsonSerializer.Deserialize<PackedTextEntryLocalization[]>(
                            File.ReadAllText(machineFilePath),
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                        var combinedDict = combinedLocalizations.ToDictionary(x => x.Id, x => x);

                        // Merge machine translations
                        foreach (var machineLocalization in machineLocalizations)
                        {
                            if (combinedDict.TryGetValue(machineLocalization.Id, out var existingLocalization))
                            {
                                foreach (var propertyPair in machineLocalization.Properties)
                                {
                                    if (!existingLocalization.Properties.ContainsKey(propertyPair.Key))
                                    {
                                        existingLocalization.Properties[propertyPair.Key] = propertyPair.Value;
                                    }
                                    else
                                    {
                                        var existingProperty = existingLocalization.Properties[propertyPair.Key];
                                        foreach (var localization in propertyPair.Value.Localizations)
                                        {
                                            if (!existingProperty.Localizations.ContainsKey(localization.Key))
                                            {
                                                existingProperty.Localizations[localization.Key] = localization.Value;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        combinedLocalizations = combinedDict.Values.ToArray();
                    }

                    File.WriteAllText(finalFilePath, JsonSerializer.Serialize(
                        combinedLocalizations,
                        new JsonSerializerOptions
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        }));
                }
            }
        }
    }
}
