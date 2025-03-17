using AtelierResleriana.MessagePack;
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
    [TestCategory(nameof(MasterDataSerializer))]
    public sealed class MasterDataSerializerTests
    {
        [TestMethod]
        [DataRow("gender")]
        public async Task SurvivesSimpleRoundTrip(string fileName)
        {
            MasterDataReader masterDataReader = new MasterDataReader();
            var masterDataFiles = await masterDataReader.ReadAsync(File.OpenRead("Resources/MasterData.bytes"));

            byte[] originalBytes = masterDataFiles.First(x => x.Name == fileName).Bytes;

            MasterDataSerializer masterDataSerializer = new MasterDataSerializer();
            byte[] roundTripBytes = masterDataSerializer.Serialize(masterDataSerializer.Deserialize(originalBytes));

            Console.WriteLine(string.Join(" ", originalBytes.Select(x => x.ToString("X2"))));
            Console.WriteLine(string.Join(" ", roundTripBytes.Select(x => x.ToString("X2"))));
        }

        [TestMethod]
        [DataRow("character")]
        [DataRow("gender")]
        public async Task SurvivesDynamicJsonRoundTrip(string fileName)
        {
            MasterDataReader masterDataReader = new MasterDataReader();

            using Stream stream = File.OpenRead("Resources/MasterData.bytes");
            IEnumerable<MasterDataFile> masterDataFiles = await masterDataReader.ReadAsync(stream);

            MasterDataFile masterDataFile = masterDataFiles.First(x => x.Name == fileName);
            byte[] originalBytes = masterDataFile.Bytes;

            MasterDataSerializer masterDataSerializer = new MasterDataSerializer();
            string tempFilePath = "temp.json";
            File.WriteAllText(tempFilePath, JsonSerializer.Serialize(masterDataSerializer.Deserialize(masterDataFile.Bytes), new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            }));

            JsonNode jsonNode = JsonNode.Parse(File.ReadAllText(tempFilePath));

            byte[] roundTripBytes = MessagePackSerializer.ConvertFromJson(jsonNode.ToJsonString(), MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));

            Console.WriteLine(string.Join(" ", originalBytes.Take(32).Select(x => x.ToString("X2"))));
            Console.WriteLine(string.Join(" ", roundTripBytes.Take(32).Select(x => x.ToString("X2"))));
        }

        [TestMethod]
        [DataRow("character")]
        public async Task DiagnoseMasterDataSerialization(string fileName)
        {
            MasterDataReader masterDataReader = new MasterDataReader();
            using Stream stream = File.OpenRead("Resources/MasterData.bytes");
            IEnumerable<MasterDataFile> masterDataFiles = await masterDataReader.ReadAsync(stream);
            MasterDataFile masterDataFile = masterDataFiles.First(x => x.Name == fileName);
            byte[] originalBytes = masterDataFile.Bytes;

            MasterDataSerializer masterDataSerializer = new MasterDataSerializer();
            var options = MessagePackSerializerOptions.Standard
                .WithCompression(MessagePackCompression.Lz4Block);

            // Deserialize with compression options
            var deserialized = MessagePackSerializer.Deserialize<dynamic>(originalBytes, options);

            // Serialize back to JSON
            string jsonString = JsonSerializer.Serialize(deserialized, new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

            Console.WriteLine(jsonString);

            // Convert back to MessagePack with the same compression options
            byte[] roundTripBytes = MessagePackSerializer.ConvertFromJson(jsonString, options);

            Console.WriteLine(string.Join(" ", originalBytes.Take(32).Select(x => x.ToString("X2"))));
            Console.WriteLine(string.Join(" ", roundTripBytes.Take(32).Select(x => x.ToString("X2"))));
        }

        [TestMethod]
        [DataRow("character")]
        public async Task InspectTypeTree(string fileName)
        {
            MasterDataReader masterDataReader = new MasterDataReader();
            using Stream stream = File.OpenRead("Resources/MasterData.bytes");
            IEnumerable<MasterDataFile> masterDataFiles = await masterDataReader.ReadAsync(stream);
            MasterDataFile masterDataFile = masterDataFiles.First(x => x.Name == fileName);

            MasterDataSerializer masterDataSerializer = new MasterDataSerializer();
            var deserialized = masterDataSerializer.Deserialize(masterDataFile.Bytes);

            void PrintTypeInfo(object obj, string indent = "", HashSet<object> visited = null)
            {
                visited = visited ?? new HashSet<object>();

                if (obj == null)
                {
                    Console.WriteLine($"{indent}null");
                    return;
                }

                if (!visited.Add(obj))
                {
                    Console.WriteLine($"{indent}[Circular reference]");
                    return;
                }

                var type = obj.GetType();
                Console.WriteLine($"{indent}Type: {type.FullName}");

                if (obj is IDictionary dict)
                {
                    Console.WriteLine($"{indent}Dictionary entries:");
                    foreach (DictionaryEntry entry in dict)
                    {
                        Console.WriteLine($"{indent}  Key ({entry.Key.GetType().Name}): {entry.Key}");
                        Console.WriteLine($"{indent}  Value ({entry.Value?.GetType().Name ?? "null"}): {entry.Value}");

                        if (entry.Value != null && !(entry.Value is string))
                        {
                            Console.WriteLine($"{indent}  Value contents:");
                            PrintTypeInfo(entry.Value, indent + "    ", visited);
                        }
                    }
                }
                else if (obj is System.Collections.IEnumerable enumerable && !(obj is string))
                {
                    int i = 0;
                    foreach (var item in enumerable)
                    {
                        Console.WriteLine($"{indent}Item[{i++}] ({item?.GetType().Name ?? "null"}):");
                        if (item != null)
                        {
                            PrintTypeInfo(item, indent + "  ", visited);
                        }
                        if (i > 5)
                        {
                            Console.WriteLine($"{indent}... (more items)");
                            break;
                        }
                    }
                }

                visited.Remove(obj);
            }

            PrintTypeInfo(deserialized);
        }
    }
}
