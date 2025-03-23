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

        [TestMethod]
        public void CanHandleNested()
        {
            var sourceJson =
                """
                [
                    {
                      "banner_path_hash": 0,
                      "bgm_path_hash": 0,
                      "bonuses_per_draw": [],
                      "description": "星祈石の花冠",
                      "end_at": null,
                      "gacha_battle_ids": [],
                      "gacha_button_groups": [
                        {
                          "gacha_buttons": [
                            {
                              "description": "",
                              "gacha_button_type": 1,
                              "id": 210041,
                              "ticket_description": null
                            }
                          ]
                        }
                      ],
                      "gacha_rate_set_id": 210042,
                      "gacha_rate_set_name": "星祈石の花冠",
                      "gacha_step_up_button_ids": [],
                      "gacha_type": 2,
                      "id": 21004,
                      "is_shop": true,
                      "is_step_up": false,
                      "medal_id": null,
                      "mixed_wish_list": null,
                      "movie_path_hashes": [
                        7989998204633788932
                      ],
                      "name": "星祈石の花冠",
                      "picked_up_memoria_ids": [],
                      "priority": 1002,
                      "shop_id": null,
                      "start_at": "2025-03-06T03:00:00Z",
                      "start_dash_hours": null,
                      "step_up_display_type": null,
                      "step_up_loop_count": null,
                      "step_up_loop_description": "",
                      "ticket_id": null,
                      "wish_list_character_count": 0,
                      "wish_list_gacha_type": 1,
                      "wish_list_memoria_count": 0
                    }
                ]
                """;

            var updateJson =
                """
                [
                    {
                      "id": 21004,
                      "description": "Starwish Flower",
                      "gacha_button_groups": [
                        {
                          "gacha_buttons": [
                            {
                              "id": 210041,
                              "description": ""
                            }
                          ]
                        }
                      ],
                      "gacha_rate_set_name": "Starwish Flower",
                      "name": "Starwish Flower",
                      "start_at": "2025-03-06T03:00:00Z",
                      "step_up_loop_description": ""
                    }
                ]
                """;

            // Arrange
            JsonNode sourceNode = JsonNode.Parse(sourceJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;

            // Deserialize source JSON to mimic MsgPack data structure
            var sourceData = _serializer.Deserialize(_serializer.Serialize(sourceNode.ToJsonString()));

            // Act
            _updater.UpdateEntities(sourceData, updateNode);

            // Assert
            bool found = false;
            var gachaArray = ((IEnumerable)sourceData).Cast<Dictionary<object, object>>();
            foreach (var gacha in gachaArray)
            {
                if (Convert.ToUInt16(gacha["id"]) == 21004)
                {
                    // Verify top-level properties were updated
                    Assert.AreEqual("Starwish Flower", gacha["description"]);
                    Assert.AreEqual("Starwish Flower", gacha["name"]);
                    Assert.AreEqual("Starwish Flower", gacha["gacha_rate_set_name"]);

                    // Verify nested properties - access the nested structure
                    var buttonGroups = (IList)gacha["gacha_button_groups"];
                    Assert.IsNotNull(buttonGroups, "Button groups should exist");
                    Assert.AreEqual(1, buttonGroups.Count, "Should have one button group");

                    var firstGroup = (Dictionary<object, object>)buttonGroups[0];
                    var buttons = (IList)firstGroup["gacha_buttons"];
                    Assert.IsNotNull(buttons, "Buttons should exist in group");
                    Assert.AreEqual(1, buttons.Count, "Should have one button");

                    var firstButton = (Dictionary<object, object>)buttons[0];
                    Assert.AreEqual(210041, Convert.ToInt64(firstButton["id"]), "Button ID should match");
                    Assert.AreEqual("", firstButton["description"], "Button description should be empty string");

                    // Verify unmodified fields remain intact
                    Assert.AreEqual(0, Convert.ToInt64(gacha["banner_path_hash"]));
                    Assert.AreEqual(210042, Convert.ToInt64(gacha["gacha_rate_set_id"]));
                    Assert.AreEqual(true, Convert.ToBoolean(gacha["is_shop"]));

                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Target gacha ID 21004 was not found in the data");
        }
    }
}