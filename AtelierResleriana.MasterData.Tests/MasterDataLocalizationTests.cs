using AtelierResleriana.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AtelierResleriana.MasterData
{
    [TestClass]
    [TestCategory("MasterDataLocalization")]
    public sealed class MasterDataLocalizationTests
    {
        private readonly MasterDataReader _reader = new();
        private readonly MasterDataWriter _writer = new();
        private readonly MasterDataSerializer _serializer = new();
        private Dictionary<string, Dictionary<string, JsonNode>> _localizationData;
        private StringFormatParameterMatcher _stringFormatParameterMatcher = new();

        [TestInitialize]
        public void Setup()
        {
            // Load the sample localization data
            string jsonContent = File.ReadAllText("Resources/SampleMasterDataLocalizationData.json");
            _localizationData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonNode>>>(jsonContent);
        }

        [TestMethod]
        [DataRow("Resources/MasterData.bytes")]
        public async Task VerifyPluginLocalizationFlow(string filePath)
        {
            // Arrange
            var masterDataFiles = await _reader.ReadAsync(File.OpenRead(filePath));
            var characterFile = masterDataFiles.First(x => x.Name == "character");

            // Act - Simulate the plugin's localization flow
            var characterData = _serializer.Deserialize(characterFile.Bytes);

            var updater = new MasterDataFileRuntimeUpdater(new MasterDataFileRuntimeUpdater.Options
            {
                ShouldUpdate = (object baseValue, object updateValue) =>
                {
                    if (!(baseValue is string baseString) || !(updateValue is string updateString))
                    {
                        return false;
                    }
                    return _stringFormatParameterMatcher.IsMatch(baseString, updateString);
                }
            });

            updater.UpdateEntities(characterData, _localizationData["en"]["character"]);

            // Serialize back
            characterFile.Bytes = _serializer.Serialize(characterData);

            // Write to a test file to verify
            using (var outputStream = new MemoryStream())
            {
                await _writer.WriteAsync(outputStream, masterDataFiles.ToDictionary(x => x.Name, x => x.Bytes));

                // Read back and verify
                outputStream.Seek(0, SeekOrigin.Begin);
                var verificationFiles = await _reader.ReadAsync(outputStream);
                var verificationCharacterFile = verificationFiles.First(x => x.Name == "character");
                var verifiedData = _serializer.Deserialize(verificationCharacterFile.Bytes);

                // Assert - Check specific translations were applied
                var characterArray = ((IEnumerable)verifiedData).Cast<Dictionary<object, object>>();
                bool found = false;
                foreach (var character in characterArray)
                {
                    if (Convert.ToUInt16(character["id"]) == 10101)
                    {
                        Assert.AreEqual(
                            "Anything is possible through alchemy.\nShall we find out what we can accomplish together?",
                            character["acquisition_text"]
                        );
                        Assert.AreEqual("Lovely Bomber", character["another_name"]);
                        found = true;
                        break;
                    }
                }
                Assert.IsTrue(found, "Target character 10101 not found in the localized data");

                // Optional: Write to file for manual inspection
                await File.WriteAllBytesAsync("TestOutput.bytes", outputStream.ToArray());
            }
        }
    }
}