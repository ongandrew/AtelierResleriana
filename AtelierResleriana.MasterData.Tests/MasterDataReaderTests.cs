using AtelierResleriana.MessagePack;
using AtelierResleriana.MessagePack.Resolvers;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtelierResleriana.MasterData
{
    [TestClass]
    [TestCategory(nameof(MasterDataReader))]
    public sealed class MasterDataReaderTests
    {
        [TestMethod]
        [DataRow("Resources/CachedMasterData.1739164044_6p7CNzPRcccY1DVD.bin")]
        [DataRow("Resources/CachedMasterData.1739423910_cUAPYN4t6CGt_5gI.bin")]
        public async Task CanBulkRead(string filePath)
        {
            string version = Path.GetFileName(filePath).Split(".", StringSplitOptions.RemoveEmptyEntries)[1];
            MasterDataReader masterDataReader = new MasterDataReader();
            var files = await masterDataReader.ReadEncryptedAsync(File.OpenRead(filePath), version);

            Directory.CreateDirectory($"MasterData/{version}");

            foreach (var file in files)
            {
                File.WriteAllBytes($"MasterData/{version}/{file.Name}", file.Bytes);

                object dynamicModel = MessagePackSerializer.Deserialize<object>(
                    file.Bytes,
                    ContractlessStandardResolver.Options
                        .WithCompression(MessagePackCompression.Lz4Block)
                );

                string json = JsonSerializer.Serialize(dynamicModel);
                File.WriteAllText($"MasterData/{version}/{file.Name}.json", json);
            }
        }

        [TestMethod]
        [DataRow("Resources/MasterData.bytes")]
        public async Task Experiment(string filePath)
        {
            MasterDataReader masterDataReader = new MasterDataReader();
            var masterDataFiles = await masterDataReader.ReadAsync(File.OpenRead(filePath));


            MasterDataFile characterMasterDataFile = masterDataFiles.First(x => x.Name == "character");

            MasterDataSerializer masterDataSerializer = new MasterDataSerializer();

            dynamic characterMasterData = masterDataSerializer.Deserialize(characterMasterDataFile.Bytes);

            foreach (var character in characterMasterData)
            {
                if (character["id"] == 10103)
                {
                    character["another_name"] = "Woohoo bombaaaah";
                }
            }

            characterMasterDataFile.Bytes = masterDataSerializer.Serialize(characterMasterData);

            MasterDataWriter masterDataWriter = new MasterDataWriter();

            using (Stream destinationStream = File.Create("Modified.bytes"))
            {
                await masterDataWriter.WriteAsync(destinationStream, masterDataFiles.ToDictionary(x => x.Name, x => x.Bytes));
            }
        }
    }
}