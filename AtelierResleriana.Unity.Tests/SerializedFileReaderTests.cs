using System;
using System.IO;
using System.Text.Json;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(SerializedFileReader))]
    public sealed class SerializedFileReaderTests
    {
        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41", 206610U, 4930772L, 22U, 206672L, false)]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687", 49641U, 372096L, 22U, 49696L, false)]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99", 4217U, 4894L, 22U, 4272L, false)]
        public void CanReadHeader(string filePath, uint metadataSize, long fileSize, uint version, long dataOffset, bool isBigEndian)
        {
            using Stream stream = File.OpenRead(filePath);
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFileHeader header = serializedFileReader.ReadHeader(stream);
            Assert.AreEqual(metadataSize, header.MetadataSize);
            Assert.AreEqual(fileSize, header.FileSize);
            Assert.AreEqual(version, header.Version);
            Assert.AreEqual(dataOffset, header.DataOffset);
            Assert.AreEqual(isBigEndian, header.IsBigEndian);
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41", "0.0.0", 19, true)]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687", "0.0.0", 19, true)]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99", "0.0.0", 19, true)]
        public void CanReadMetadata(string filePath, string unityVersion, int targetPlatform, bool enableTypeTree)
        {
            using Stream stream = File.OpenRead(filePath);
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFileHeader header = serializedFileReader.ReadHeader(stream);
            SerializedFileMetadata metadata = serializedFileReader.ReadMetadata(stream, header);
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41", "Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41.Types.json")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687", "Resources/CAB-282243a14cbd7835f618f9c2f852f687.Types.json")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99", "Resources/CAB-cb51948bc4ba920cd3a24703432f3b99.Types.json")]
        public void CanReadTypes(string filePath, string referenceFilePath)
        {
            void CompareByteArrays(byte[]? expected, byte[]? actual, string context)
            {
                if (expected == null || actual == null)
                {
                    Assert.AreEqual(expected, actual, $"{context}: One array is null while the other is not");
                    return;
                }

                Assert.IsTrue(actual.AsSpan().SequenceEqual(expected), $"{context}: Byte array content mismatch");
            }

            void CompareTypeTreeNodes(SerializedFileTypeTreeNode? expected, SerializedFileTypeTreeNode? actual, string context)
            {
                if (expected == null || actual == null)
                {
                    Assert.AreEqual(expected, actual, $"{context}: One node is null while the other is not");
                    return;
                }

                Assert.AreEqual(expected.Level, actual.Level, $"{context}: Level mismatch");
                Assert.AreEqual(expected.Type, actual.Type, $"{context}: Type mismatch");
                Assert.AreEqual(expected.Name, actual.Name, $"{context}: Name mismatch");
                Assert.AreEqual(expected.Size, actual.Size, $"{context}: ByteSize mismatch");
                Assert.AreEqual(expected.Version, actual.Version, $"{context}: Version mismatch");
                Assert.AreEqual(expected.TypeFlags, actual.TypeFlags, $"{context}: TypeFlags mismatch");
                Assert.AreEqual(expected.MetaFlag, actual.MetaFlag, $"{context}: MetaFlag mismatch");
                Assert.AreEqual(expected.VariableCount, actual.VariableCount, $"{context}: VariableCount mismatch");
                Assert.AreEqual(expected.Index, actual.Index, $"{context}: Index mismatch");

                Assert.AreEqual(expected.Children?.Count ?? 0, actual.Children?.Count ?? 0, $"{context}: Children count mismatch");

                if (expected.Children != null && actual.Children != null)
                {
                    for (int i = 0; i < expected.Children.Count; i++)
                    {
                        CompareTypeTreeNodes(expected.Children[i], actual.Children[i], $"{context} Child[{i}]");
                    }
                }
            }

            using Stream stream = File.OpenRead(filePath);
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFileHeader header = serializedFileReader.ReadHeader(stream);
            SerializedFileMetadata metadata = serializedFileReader.ReadMetadata(stream, header);
            SerializedFileType[] types = serializedFileReader.ReadTypes(stream, header, metadata);

            SerializedFileType[] referenceTypes = JsonSerializer.Deserialize<SerializedFileType[]>(File.ReadAllText(referenceFilePath), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.AreEqual(referenceTypes.Length, types.Length, "Number of types should match");

            for (int i = 0; i < types.Length; i++)
            {
                var actual = types[i];
                var expected = referenceTypes[i];

                Assert.AreEqual(expected.ClassId, actual.ClassId, $"Type {i} ClassId mismatch");
                Assert.AreEqual(expected.IsStrippedType, actual.IsStrippedType, $"Type {i} IsStrippedType mismatch");
                Assert.AreEqual(expected.ScriptTypeIndex, actual.ScriptTypeIndex, $"Type {i} ScriptTypeIndex mismatch");

                CompareByteArrays(expected.ScriptId, actual.ScriptId, $"Type {i} ScriptId");
                CompareByteArrays(expected.OldTypeHash, actual.OldTypeHash, $"Type {i} OldTypeHash");

                Assert.AreEqual(expected.ClassName, actual.ClassName, $"Type {i} ClassName mismatch");
                Assert.AreEqual(expected.Namespace, actual.Namespace, $"Type {i} Namespace mismatch");
                Assert.AreEqual(expected.AssemblyName, actual.AssemblyName, $"Type {i} AssemblyName mismatch");

                if (expected.Dependencies != null || actual.Dependencies != null)
                {
                    Assert.IsNotNull(actual.Dependencies, $"Type {i} Dependencies should not be null");
                    Assert.IsNotNull(expected.Dependencies, $"Type {i} Reference Dependencies should not be null");
                    CollectionAssert.AreEqual(expected.Dependencies, actual.Dependencies, $"Type {i} Dependencies mismatch");
                }

                CompareTypeTreeNodes(expected.Node, actual.Node, $"Type {i} Node");
            }
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void CanReadObjects(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFileHeader header = serializedFileReader.ReadHeader(stream);
            SerializedFileMetadata metadata = serializedFileReader.ReadMetadata(stream, header);
            SerializedFileType[] types = serializedFileReader.ReadTypes(stream, header, metadata);
            SerializedFileObject[] objects = serializedFileReader.ReadObjects(stream, header, types);
        }

        [TestMethod]
        public void CanReadObjectBytes()
        {
            using Stream stream = File.OpenRead("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41");
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFile serializedFile = serializedFileReader.Read(stream);

            var @object = serializedFile.Objects[0];
            byte[] objectBytes = @object.Data;
            byte[] referenceObjectBytes = File.ReadAllBytes("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41.Objects.0.bin");

            Assert.AreEqual(referenceObjectBytes.Length, objectBytes.Length);
            CollectionAssert.AreEqual(referenceObjectBytes, objectBytes);
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void CanReadSerializedObjects(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);
            SerializedFileReader serializedFileReader = new SerializedFileReader();
            SerializedFile serializedFile = serializedFileReader.Read(stream);

            foreach (var @object in serializedFile.Objects)
            {
                SerializedObject serializedObject = serializedFile.GetSerializedObject(@object);
                Assert.IsNotNull(serializedObject);
                Assert.AreNotEqual(0, serializedObject.ClassId);
            }
        }
    }
}
