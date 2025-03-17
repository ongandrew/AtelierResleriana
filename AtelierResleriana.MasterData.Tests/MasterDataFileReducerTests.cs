using System.Text.Json;
using System.Text.Json.Nodes;

namespace AtelierResleriana.MasterData
{
    [TestClass]
    [TestCategory(nameof(MasterDataFileReducer))]
    public sealed class MasterDataFileReducerTests
    {
        private readonly MasterDataFileReducer _reducer = new();
        private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        [TestMethod]
        public void SimpleExample_ShouldRemoveNonStringProperties()
        {
            // Arrange
            string input = @"[
    {
        ""gender_id"": 2,
        ""id"": 101,
        ""name"": ""Marie""
    },
    {
        ""gender_id"": 2,
        ""id"": 103,
        ""name"": ""Mu""
    }
]";

            string expectedOutput = @"[
    {
        ""id"": 101,
        ""name"": ""Marie""
    },
    {
        ""id"": 103,
        ""name"": ""Mu""
    }
]";

            // Act
            JsonNode? result = _reducer.Reduce(input);

            // Assert
            Assert.IsNotNull(result);
            JsonNode? expected = JsonNode.Parse(expectedOutput);
            Assert.IsTrue(JsonNode.DeepEquals(expected, result));
        }

        [TestMethod]
        public void ComplexExample_ShouldKeepNestedStringsAndId()
        {
            // Arrange
            string input = @"[
  {
    ""ability_ids"": [
      1990056,
      1990057,
      1990324
    ],
    ""acquisition_movie_path_hash"": 1513029441945810930,
    ""acquisition_text"": ""錬金術には無限の可能性がある――\nあなたも一緒に探してみない？"",
    ""all_skill_evolved_ability_ids"": [
      1990324
    ],
    ""another_name"": ""【Lovely Bomber】"",
    ""attack_attributes"": [
      5
    ],
    ""base_character_id"": 101,
    ""battle_tool_trait_ids"": [
      5,
      13
    ],
    ""burst_skill_ids"": [
      14000281,
      14000281,
      14000281,
      14000282,
      14000283,
      14000284,
      14000285,
      14002304
    ],
    ""change"": null,
    ""character_growth_id"": 910101,
    ""character_size_id"": 2,
    ""description"": ""活発でおてんばな錬金術士。\nある街のアカデミーで錬金術を学んでいた。現在は錬金術の真理を追い求め、ランターナ大陸の各地を旅している。\n明るくお人好しだが、面倒くさがりでずぼら。\n困難を爆弾で解決しようとする節があり、周囲から爆弾魔と呼ばれていたことも。"",
    ""equipment_tool_trait_ids"": [
      13
    ],
    ""evolved_burst_skill_ids"": [
      14002441,
      14002441,
      14002441,
      14002441,
      14002441,
      14002441,
      14002441,
      14002441
    ],
    ""evolved_normal1_skill_ids"": [
      11002431,
      11002432,
      11002433,
      11002434,
      11002435
    ],
    ""evolved_normal2_skill_ids"": [
      12002436,
      12002437,
      12002438,
      12002439,
      12002440
    ],
    ""ex_growboard_id"": 101012,
    ""extra_skill_ids"": [],
    ""fullname"": ""マルローネ"",
    ""growboard_id"": 101011,
    ""id"": 10101,
    ""initial_rarity"": 3,
    ""initial_status"": {
      ""attack"": 53,
      ""defense"": 39,
      ""hp"": 158,
      ""magic"": 64,
      ""mental"": 41,
      ""speed"": 239
    },
    ""is_alchemist"": true,
    ""leader_skill"": {
      ""abilities"": [
        {
          ""ability_id"": 1980001,
          ""combat_power_coefficient"": 1500,
          ""condition_ids"": [
            28
          ]
        }
      ],
      ""description"": ""「学生」は「魔攻・魔防+25%」"",
      ""name"": ""アカデミーの卒業生""
    },
    ""max_rarity"": 8,
    ""model_path_hash"": 1543991191416950894,
    ""motion_type"": 2,
    ""name"": ""マリー"",
    ""normal1_skill_ids"": [
      11000271,
      11000272,
      11000273,
      11000274,
      11000275
    ],
    ""normal2_skill_ids"": [
      12000276,
      12000277,
      12000278,
      12000279,
      12000280
    ],
    ""original_title_id"": 1,
    ""overlay_name"": ""Marlone"",
    ""personality"": 3,
    ""profile_voice_text"": ""マルローネよ。アカデミー出身なんだ。\nあたしの錬金術が役に立つといいんだけど"",
    ""resistance"": {
      ""fire"": 5,
      ""ice"": 0,
      ""impact"": 0,
      ""lightning"": 0,
      ""piercing"": 0,
      ""slashing"": 0,
      ""wind"": 0
    },
    ""result_timeline_hash"": 6230336135097822874,
    ""role"": 2,
    ""series_id"": 2,
    ""start_at"": ""2024-04-01T12:00:00Z"",
    ""still_sets"": [
      {
        ""large_narrow_still_path_hash"": 5705818910680697359,
        ""large_still_path_hash"": 3784446357928211716,
        ""narrow_still_path_hash"": 9192457744998975910,
        ""rarity"": null,
        ""still_path_hash"": 1729358731240541415
      },
      {
        ""large_narrow_still_path_hash"": 7423108886445875695,
        ""large_still_path_hash"": 5332363376727634833,
        ""narrow_still_path_hash"": 7106463543486936391,
        ""rarity"": 8,
        ""still_path_hash"": 7712877980222666124
      }
    ],
    ""support_color_id"": 5,
    ""tag_ids"": [
      1,
      9,
      31,
      12,
      25
    ],
    ""trait_color_id"": 2,
    ""voice_actor_id"": 6
  }
]";

            string expectedOutput = @"[
  {
    ""acquisition_text"": ""錬金術には無限の可能性がある――\nあなたも一緒に探してみない？"",
    ""another_name"": ""【Lovely Bomber】"",
    ""description"": ""活発でおてんばな錬金術士。\nある街のアカデミーで錬金術を学んでいた。現在は錬金術の真理を追い求め、ランターナ大陸の各地を旅している。\n明るくお人好しだが、面倒くさがりでずぼら。\n困難を爆弾で解決しようとする節があり、周囲から爆弾魔と呼ばれていたことも。"",
    ""fullname"": ""マルローネ"",
    ""id"": 10101,
    ""leader_skill"": {
      ""description"": ""「学生」は「魔攻・魔防+25%」"",
      ""name"": ""アカデミーの卒業生""
    },
    ""name"": ""マリー"",
    ""overlay_name"": ""Marlone"",
    ""profile_voice_text"": ""マルローネよ。アカデミー出身なんだ。\nあたしの錬金術が役に立つといいんだけど"",
    ""start_at"": ""2024-04-01T12:00:00Z""
  }
]";

            // Act
            JsonNode? result = _reducer.Reduce(input);

            // Assert
            Assert.IsNotNull(result);
            JsonNode? expected = JsonNode.Parse(expectedOutput);
            Assert.IsTrue(JsonNode.DeepEquals(expected, result));
        }

        [TestMethod]
        public void NonArrayInput_ShouldReturnNull()
        {
            // Arrange
            string input = @"{""id"": 1, ""name"": ""test""}";

            // Act
            JsonNode? result = _reducer.Reduce(input);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EmptyArray_ShouldReturnNull()
        {
            // Arrange
            string input = "[]";

            // Act
            JsonNode? result = _reducer.Reduce(input);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ArrayWithoutStrings_ShouldReturnNull()
        {
            // Arrange
            string input = @"[{""id"": 1, ""number"": 42}]";

            // Act
            JsonNode? result = _reducer.Reduce(input);

            // Assert
            Assert.IsNull(result);
        }
    }
}