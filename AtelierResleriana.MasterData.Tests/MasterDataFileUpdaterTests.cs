using System.Linq;
using System.Text.Json.Nodes;

namespace AtelierResleriana.MasterData
{
    [TestClass]
    [TestCategory(nameof(MasterDataFileUpdater))]
    public sealed class MasterDataFileUpdaterTests
    {
        private readonly MasterDataFileUpdater _updater = new();

        [TestMethod]
        public void SimpleObjects_ShouldUpdateMatchingProperties()
        {
            // Arrange
            string baseJson = @"[
    {
        ""gender_id"": 2,
        ""id"": 101,
        ""name"": ""original""
    },
    {
        ""gender_id"": 1,
        ""id"": 102,
        ""name"": ""unchanged""
    }
]";
            string updateJson = @"[
    {
        ""id"": 101,
        ""name"": ""changed""
    }
]";
            string expectedJson = @"[
    {
        ""gender_id"": 2,
        ""id"": 101,
        ""name"": ""changed""
    },
    {
        ""gender_id"": 1,
        ""id"": 102,
        ""name"": ""unchanged""
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void UpdatesWithMissingIds_ShouldBeIgnored()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 101,
        ""name"": ""original"",
        ""value"": 42
    }
]";
            string updateJson = @"[
    {
        ""name"": ""changed""
    },
    {
        ""value"": ""wrong type""
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(baseJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void NestedObjects_ShouldUpdateMatchingProperties()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""name"": ""original"",
            ""stats"": {
                ""hp"": 100,
                ""mp"": 50
            }
        }
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""name"": ""changed"",
            ""stats"": {
                ""hp"": 200
            }
        }
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""name"": ""changed"",
            ""stats"": {
                ""hp"": 200,
                ""mp"": 50
            }
        }
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void ArraysWithSameLength_ShouldBeUpdated()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""names"": [""one"", ""two"", ""three""],
        ""objects"": [
            {""value"": 1},
            {""value"": 2}
        ]
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""names"": [""uno"", ""dos"", ""tres""],
        ""objects"": [
            {""value"": 10},
            {""value"": 20}
        ]
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""names"": [""uno"", ""dos"", ""tres""],
        ""objects"": [
            {""value"": 10},
            {""value"": 20}
        ]
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void ArraysWithDifferentLength_ShouldNotBeUpdated()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""values"": [1, 2, 3]
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""values"": [10, 20]
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(baseJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void NonexistentProperties_ShouldBeIgnored()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""existing"": ""original""
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""nonexistent"": ""new"",
        ""existing"": ""changed""
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""existing"": ""changed""
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void ComplexNestedStructure_ShouldUpdateCorrectly()
        {
            // Arrange
            string baseJson = @"[{
    ""id"": 1,
    ""character"": {
        ""name"": ""original"",
        ""stats"": {
            ""base"": {
                ""hp"": 100,
                ""mp"": 50
            }
        },
        ""skills"": [
            {""id"": 1, ""name"": ""skill1"", ""power"": 10},
            {""id"": 2, ""name"": ""skill2"", ""power"": 20}
        ]
    }
}]";
            string updateJson = @"[{
    ""id"": 1,
    ""character"": {
        ""name"": ""changed"",
        ""stats"": {
            ""base"": {
                ""hp"": 200
            }
        },
        ""skills"": [
            {""id"": 1, ""name"": ""updated1"", ""power"": 15},
            {""id"": 2, ""name"": ""updated2"", ""power"": 25}
        ]
    }
}]";
            string expectedJson = @"[{
    ""id"": 1,
    ""character"": {
        ""name"": ""changed"",
        ""stats"": {
            ""base"": {
                ""hp"": 200,
                ""mp"": 50
            }
        },
        ""skills"": [
            {""id"": 1, ""name"": ""updated1"", ""power"": 15},
            {""id"": 2, ""name"": ""updated2"", ""power"": 25}
        ]
    }
}]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void DifferentPrimitiveTypes_ShouldNotUpdate()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""stringValue"": ""text"",
        ""numberValue"": 42,
        ""boolValue"": true
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""stringValue"": 123,
        ""numberValue"": ""42"",
        ""boolValue"": 1
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""stringValue"": ""text"",
        ""numberValue"": 42,
        ""boolValue"": true
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void NestedObjectsWithDifferentTypes_ShouldNotUpdate()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""text"": ""original"",
            ""number"": 42,
            ""nested"": {
                ""value"": ""string""
            }
        }
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""text"": 100,
            ""number"": ""changed"",
            ""nested"": {
                ""value"": 123
            }
        }
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""data"": {
            ""text"": ""original"",
            ""number"": 42,
            ""nested"": {
                ""value"": ""string""
            }
        }
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void ArrayElementsWithDifferentTypes_ShouldNotUpdate()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""items"": [
            {
                ""name"": ""item1"",
                ""value"": ""string value"",
                ""count"": 5
            },
            {
                ""name"": ""item2"",
                ""value"": ""another string"",
                ""count"": 10
            }
        ]
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""items"": [
            {
                ""name"": 123,
                ""value"": 456,
                ""count"": ""invalid""
            },
            {
                ""name"": true,
                ""value"": 789,
                ""count"": ""twenty""
            }
        ]
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""items"": [
            {
                ""name"": ""item1"",
                ""value"": ""string value"",
                ""count"": 5
            },
            {
                ""name"": ""item2"",
                ""value"": ""another string"",
                ""count"": 10
            }
        ]
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void MixedTypeUpdates_ShouldOnlyUpdateMatchingTypes()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""string1"": ""keep"",
        ""string2"": ""update"",
        ""number1"": 100,
        ""number2"": 200,
        ""bool1"": true,
        ""bool2"": false
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""string1"": 123,
        ""string2"": ""changed"",
        ""number1"": ""invalid"",
        ""number2"": 300,
        ""bool1"": ""invalid"",
        ""bool2"": true
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""string1"": ""keep"",
        ""string2"": ""changed"",
        ""number1"": 100,
        ""number2"": 300,
        ""bool1"": true,
        ""bool2"": true
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            // Act
            _updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void UpdateCondition_ShouldOnlyUpdateStringsWithMatchingFormatParameters()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""text1"": ""Hello {0}!"",
        ""text2"": ""Greet {0} and {1}"",
        ""text3"": ""Simple text"",
        ""number"": 42
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""text1"": ""Hi {0}!"",
        ""text2"": ""Welcome {0}!"",
        ""text3"": ""New {0} text"",
        ""number"": 100
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""text1"": ""Hi {0}!"",
        ""text2"": ""Greet {0} and {1}"",
        ""text3"": ""Simple text"",
        ""number"": 100
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            var updater = new MasterDataFileUpdater(new MasterDataFileUpdater.Options
            {
                ShouldUpdate = (baseValue, updateValue) =>
                {
                    // Only apply condition to strings
                    if (baseValue is not string baseStr || updateValue is not string updateStr)
                    {
                        return true;
                    }

                    // Count format parameters in both strings
                    int GetFormatParamCount(string s) =>
                        System.Text.RegularExpressions.Regex.Matches(s, @"\{(\d+)\}").Count;

                    int baseParams = GetFormatParamCount(baseStr);
                    int updateParams = GetFormatParamCount(updateStr);

                    return baseParams == updateParams;
                }
            });

            // Act
            updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode));
        }

        [TestMethod]
        public void UpdateCondition_ShouldOnlyUpdateIfSourceHasJapaneseCharacters()
        {
            // Arrange
            string baseJson = @"[
    {
        ""id"": 1,
        ""text1"": ""こんにちは"",
        ""text2"": ""Hello world"",
        ""text3"": ""さようなら！"",
        ""number"": 42
    }
]";
            string updateJson = @"[
    {
        ""id"": 1,
        ""text1"": ""Updated JP 1"",
        ""text2"": ""Updated EN"",
        ""text3"": ""Updated JP 2"",
        ""number"": 100
    }
]";
            string expectedJson = @"[
    {
        ""id"": 1,
        ""text1"": ""Updated JP 1"",
        ""text2"": ""Hello world"",
        ""text3"": ""Updated JP 2"",
        ""number"": 100
    }
]";

            JsonNode baseNode = JsonNode.Parse(baseJson)!;
            JsonNode updateNode = JsonNode.Parse(updateJson)!;
            JsonNode expectedNode = JsonNode.Parse(expectedJson)!;

            var updater = new MasterDataFileUpdater(new MasterDataFileUpdater.Options
            {
                ShouldUpdate = (baseValue, updateValue) =>
                {
                    // Only apply condition to strings
                    if (baseValue is not string baseStr)
                    {
                        return true;
                    }

                    // Check if the base string contains Japanese characters
                    bool HasJapaneseCharacters(string s) =>
                        s.Any(c => (c >= '\u3040' && c <= '\u309F') ||    // Hiragana
                                  (c >= '\u30A0' && c <= '\u30FF') ||    // Katakana
                                  (c >= '\u4E00' && c <= '\u9FFF'));     // Kanji

                    return HasJapaneseCharacters(baseStr);
                }
            });

            // Act
            updater.UpdateEntities(baseNode, updateNode);

            // Assert
            Assert.IsTrue(JsonNode.DeepEquals(expectedNode, baseNode), $"Received: {baseNode.ToJsonString()}");
        }
    }
}