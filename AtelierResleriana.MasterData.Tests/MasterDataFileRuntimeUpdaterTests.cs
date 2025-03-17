using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AtelierResleriana.MasterData
{
    [TestClass]
    [TestCategory(nameof(MasterDataFileRuntimeUpdater))]
    public sealed class MasterDataFileRuntimeUpdaterTests
    {
        private readonly MasterDataFileRuntimeUpdater _updater = new();
        private readonly MasterDataSerializer _serializer = new();
        private readonly MasterDataReader _reader = new();
        private readonly MasterDataWriter _writer = new();

        [TestMethod]
        [DataRow("Resources/MasterData.bytes")]
        public async Task ShouldUpdateCharacterNameInMsgPackData(string filePath)
        {
            // Arrange
            var masterDataFiles = await _reader.ReadAsync(File.OpenRead(filePath));
            var characterFile = masterDataFiles.First(x => x.Name == "character");
            var characterData = _serializer.Deserialize(characterFile.Bytes);

            string updateJson = @"[
                {
                    ""id"": 10103,
                    ""another_name"": ""Woohoo bombaaaah""
                }
            ]";
            JsonNode updateNode = JsonNode.Parse(updateJson)!;

            // Act
            _updater.UpdateEntities(characterData, updateNode);

            // Verify the update in memory
            bool found = false;
            var characterArray = ((IEnumerable)characterData).Cast<Dictionary<object, object>>();
            foreach (var character in characterArray)
            {
                if (Convert.ToUInt16(character["id"]) == 10103)
                {
                    Assert.AreEqual("Woohoo bombaaaah", character["another_name"]);
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Target character ID 10103 was not found in the data");

            // Test full serialization round-trip
            characterFile.Bytes = _serializer.Serialize(characterData);

            // Write to file system and read back to verify persistence
            using (Stream destinationStream = File.Create("RuntimeUpdated.bytes"))
            {
                await _writer.WriteAsync(destinationStream, masterDataFiles.ToDictionary(x => x.Name, x => x.Bytes));
            }

            // Read back and verify
            var verificationFiles = await _reader.ReadAsync(File.OpenRead("RuntimeUpdated.bytes"));
            var verificationCharacterFile = verificationFiles.First(x => x.Name == "character");
            var verificationData = _serializer.Deserialize(verificationCharacterFile.Bytes);

            found = false;
            var verificationArray = ((IEnumerable)verificationData).Cast<Dictionary<object, object>>();
            foreach (var character in verificationArray)
            {
                if (Convert.ToUInt16(character["id"]) == 10103)
                {
                    Assert.AreEqual("Woohoo bombaaaah", character["another_name"]);
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Target character ID 10103 was not found in the serialized data");
        }

        [TestMethod]
        [DataRow("Resources/MasterData.bytes")]
        public async Task ShouldHandleMultipleUpdatesInMsgPackData(string filePath)
        {
            // Arrange
            var masterDataFiles = await _reader.ReadAsync(File.OpenRead(filePath));
            var characterFile = masterDataFiles.First(x => x.Name == "character");
            var characterData = _serializer.Deserialize(characterFile.Bytes);

            string updateJson = @"[
                {
                    ""id"": 10103,
                    ""another_name"": ""Updated Name"",
                    ""description"": ""Updated Description"",
                    ""profile_voice_text"": ""Updated Profile""
                }
            ]";
            JsonNode updateNode = JsonNode.Parse(updateJson)!;

            // Act
            _updater.UpdateEntities(characterData, updateNode);

            // Assert
            bool found = false;
            var characterArray = ((IEnumerable)characterData).Cast<Dictionary<object, object>>();
            foreach (var character in characterArray)
            {
                if (Convert.ToUInt16(character["id"]) == 10103)
                {
                    Assert.AreEqual("Updated Name", character["another_name"]);
                    Assert.AreEqual("Updated Description", character["description"]);
                    Assert.AreEqual("Updated Profile", character["profile_voice_text"]);
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, "Target character ID 10103 was not found in the data");
        }

        [TestMethod]
        [DataRow("Resources/MasterData.bytes")]
        public async Task ShouldPreserveUnmodifiedFieldsInMsgPackData(string filePath)
        {
            // Arrange
            var masterDataFiles = await _reader.ReadAsync(File.OpenRead(filePath));
            var characterFile = masterDataFiles.First(x => x.Name == "character");
            var characterData = _serializer.Deserialize(characterFile.Bytes);

            // Store original values
            Dictionary<object, object>? originalCharacter = null;
            var characterArray = ((IEnumerable)characterData).Cast<Dictionary<object, object>>();
            foreach (var character in characterArray)
            {
                if (Convert.ToUInt16(character["id"]) == 10103)
                {
                    originalCharacter = new Dictionary<object, object>(character);
                    break;
                }
            }
            Assert.IsNotNull(originalCharacter, "Target character not found for initial check");

            string updateJson = @"[
                {
                    ""id"": 10103,
                    ""another_name"": ""New Test Name""
                }
            ]";
            JsonNode updateNode = JsonNode.Parse(updateJson)!;

            // Act
            _updater.UpdateEntities(characterData, updateNode);

            // Assert - verify only target field changed
            var updatedArray = ((IEnumerable)characterData).Cast<Dictionary<object, object>>();
            foreach (var character in updatedArray)
            {
                if (Convert.ToUInt16(character["id"]) == 10103)
                {
                    Assert.AreEqual("New Test Name", character["another_name"]);
                    Assert.AreEqual(originalCharacter["description"], character["description"]);
                    Assert.AreEqual(originalCharacter["profile_voice_text"], character["profile_voice_text"]);
                    break;
                }
            }
        }
    }
}