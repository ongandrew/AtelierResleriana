using System;
using System.IO;
using System.Text.Json;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(UnityFSFileReader))]
    public sealed class UnityFSFileReaderTests
    {
        [TestMethod]
        [DataRow("Resources/UnityFS1", 8U, "5.x.x", "0.0.0", 29000211L, 954U, 2387U, 579U)]
        [DataRow("Resources/UnityFS2", 8U, "5.x.x", "0.0.0", 25639479U, 807U, 2137U, 579U)]
        [DataRow("Resources/UnityFS3", 8U, "5.x.x", "0.0.0", 902252U, 304U, 623U, 579U)]
        public void CanReadHeaders(string filePath, uint version, string playerVersion, string engineVersion, long fileSize, uint compressedMetadataSize, uint uncompressedMetadataSize, uint flags)
        {
            using Stream stream = File.OpenRead(filePath);
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFileHeader unityFSFileHeader = unityFSFileReader.ReadHeader(stream);

            Assert.AreEqual(version, unityFSFileHeader.Version);
            Assert.AreEqual(playerVersion, (string)unityFSFileHeader.PlayerVersion);
            Assert.AreEqual(engineVersion, (string)unityFSFileHeader.EngineVersion);
            Assert.AreEqual(fileSize, unityFSFileHeader.FileSize);
            Assert.AreEqual(compressedMetadataSize, unityFSFileHeader.CompressedMetadataSize);
            Assert.AreEqual(uncompressedMetadataSize, unityFSFileHeader.UncompressedMetadataSize);
            Assert.AreEqual(flags, unityFSFileHeader.Flags);
        }

        [TestMethod]
        [DataRow("Resources/UnityFS1", "Resources/UnityFS1.Metadata.json")]
        [DataRow("Resources/UnityFS2", "Resources/UnityFS2.Metadata.json")]
        [DataRow("Resources/UnityFS3", "Resources/UnityFS3.Metadata.json")]
        public void CanReadMetadata(string filePath, string metadataJsonFilePath)
        {
            using Stream stream = File.OpenRead(filePath);
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFileHeader unityFSFileHeader = unityFSFileReader.ReadHeader(stream);
            UnityFSFileMetadata unityFSFileMetadata = unityFSFileReader.ReadMetadata(stream, unityFSFileHeader);
            UnityFSFileMetadata referenceUnityFSFileMetadata = JsonSerializer.Deserialize<UnityFSFileMetadata>(File.ReadAllText(metadataJsonFilePath), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // Verify UncompressedDataHash
            Assert.IsNotNull(unityFSFileMetadata.UncompressedDataHash, "UncompressedDataHash should not be null");
            Assert.AreEqual(16, unityFSFileMetadata.UncompressedDataHash.Length, "UncompressedDataHash should be 16 bytes");
            CollectionAssert.AreEqual(
                referenceUnityFSFileMetadata.UncompressedDataHash,
                unityFSFileMetadata.UncompressedDataHash,
                "UncompressedDataHash mismatch");

            // Verify BlockInfos
            Assert.IsNotNull(unityFSFileMetadata.BlockInfos, "BlockInfos should not be null");
            Assert.AreEqual(
                referenceUnityFSFileMetadata.BlockInfos.Length,
                unityFSFileMetadata.BlockInfos.Length,
                "BlockInfos count mismatch");

            for (int i = 0; i < unityFSFileMetadata.BlockInfos.Length; i++)
            {
                Assert.IsNotNull(unityFSFileMetadata.BlockInfos[i], $"BlockInfo at index {i} should not be null");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.BlockInfos[i].UncompressedSize,
                    unityFSFileMetadata.BlockInfos[i].UncompressedSize,
                    $"BlockInfo UncompressedSize mismatch at index {i}");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.BlockInfos[i].CompressedSize,
                    unityFSFileMetadata.BlockInfos[i].CompressedSize,
                    $"BlockInfo CompressedSize mismatch at index {i}");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.BlockInfos[i].Flags,
                    unityFSFileMetadata.BlockInfos[i].Flags,
                    $"BlockInfo Flags mismatch at index {i}");
            }

            // Verify DirectoryInfos
            Assert.IsNotNull(unityFSFileMetadata.DirectoryInfos, "DirectoryInfos should not be null");
            Assert.AreEqual(
                referenceUnityFSFileMetadata.DirectoryInfos.Length,
                unityFSFileMetadata.DirectoryInfos.Length,
                "DirectoryInfos count mismatch");

            for (int i = 0; i < unityFSFileMetadata.DirectoryInfos.Length; i++)
            {
                Assert.IsNotNull(unityFSFileMetadata.DirectoryInfos[i], $"DirectoryInfo at index {i} should not be null");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.DirectoryInfos[i].Path,
                    unityFSFileMetadata.DirectoryInfos[i].Path,
                    $"DirectoryInfo Path mismatch at index {i}");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.DirectoryInfos[i].Offset,
                    unityFSFileMetadata.DirectoryInfos[i].Offset,
                    $"DirectoryInfo Offset mismatch at index {i}");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.DirectoryInfos[i].Size,
                    unityFSFileMetadata.DirectoryInfos[i].Size,
                    $"DirectoryInfo Size mismatch at index {i}");
                Assert.AreEqual(
                    referenceUnityFSFileMetadata.DirectoryInfos[i].Flags,
                    unityFSFileMetadata.DirectoryInfos[i].Flags,
                    $"DirectoryInfo Flags mismatch at index {i}");
            }
        }

        [TestMethod]
        [DataRow("Resources/UnityFS1", "Resources/UnityFS1.Data.bin")]
        [DataRow("Resources/UnityFS2", "Resources/UnityFS2.Data.bin")]
        [DataRow("Resources/UnityFS3", "Resources/UnityFS3.Data.bin")]
        public void CanReadData(string filePath, string dataBytesFilePath)
        {
            using Stream stream = File.OpenRead(filePath);
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFileHeader unityFSFileHeader = unityFSFileReader.ReadHeader(stream);
            UnityFSFileMetadata unityFSFileMetadata = unityFSFileReader.ReadMetadata(stream, unityFSFileHeader);
            byte[] data = unityFSFileReader.ReadData(stream, unityFSFileHeader, unityFSFileMetadata);
            byte[] referenceData = File.ReadAllBytes(dataBytesFilePath);
            Assert.IsTrue(data.AsSpan().SequenceEqual(referenceData));
        }

        [TestMethod]
        [DataRow("Resources/UnityFS1")]
        [DataRow("Resources/UnityFS2")]
        [DataRow("Resources/UnityFS3")]
        public void CanReadDirectories(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);
            UnityFSFileReader unityFSFileReader = new UnityFSFileReader();
            UnityFSFile unityFSFile = unityFSFileReader.Read(stream);

            foreach (var directoryInfo in unityFSFile.Metadata.DirectoryInfos)
            {
                byte[] bytes = unityFSFile.GetDirectoryBytes(directoryInfo);
                byte[] referenceBytes = File.ReadAllBytes($"Resources/{directoryInfo.Path}");
                Assert.IsTrue(bytes.AsSpan().SequenceEqual(referenceBytes));
            }
        }
    }
}
