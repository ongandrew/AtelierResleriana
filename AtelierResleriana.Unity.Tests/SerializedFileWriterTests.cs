using System;
using System.IO;
using System.Linq;
using System.Text;
using Universal.Common;
using BinaryReader = Universal.Common.BinaryReader;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(SerializedFileWriter))]
    public sealed class SerializedFileWriterTests
    {
        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void CanRoundTripSerializedFile(string filePath)
        {
            // Read the original file
            SerializedFileReader reader = new SerializedFileReader();
            SerializedFile originalFile;
            using (Stream stream = File.OpenRead(filePath))
            {
                originalFile = reader.Read(stream);
            }

            // Configure writer with the same settings from the original file
            SerializedFileWriter writer = new SerializedFileWriter(new SerializedFileWriter.Options
            {
                IsBigEndian = originalFile.Header.IsBigEndian,
                UnityVersion = originalFile.Metadata.UnityVersion,
                TargetPlatform = originalFile.Metadata.TargetPlatform,
                EnableTypeTree = originalFile.Metadata.EnableTypeTree
            });

            // Convert file objects to SerializedFileWriter.SerializedFileObject format
            var writerObjects = new SerializedFileWriter.SerializedFileObject[originalFile.Objects.Length];
            // Create a mapping from offset to index to determine the data order
            var offsetToIndex = originalFile.Objects
                .Select((obj, index) => (obj.Offset, index))
                .OrderBy(pair => pair.Offset)
                .Select((pair, dataIndex) => (pair.index, dataIndex))
                .ToDictionary(pair => pair.index, pair => pair.dataIndex);

            for (int i = 0; i < originalFile.Objects.Length; i++)
            {
                var obj = originalFile.Objects[i];
                var serializedObject = originalFile.GetSerializedObject(obj);

                writerObjects[i] = new SerializedFileWriter.SerializedFileObject
                {
                    PathId = obj.PathId,
                    TypeId = obj.TypeId,
                    SerializedObject = serializedObject,
                    DataIndex = offsetToIndex[i]  // Assign the data index based on the original offset order
                };
            }

            // Generate the serialized data
            Stream writtenStream = writer.Write(
                originalFile.Types,
                writerObjects,
                originalFile.ScriptReferences,
                originalFile.AssetReferences,
                originalFile.ReferenceTypes,
                originalFile.UserInformation
            );

            // Read the generated file
            SerializedFile regeneratedFile;
            writtenStream.Position = 0;
            regeneratedFile = reader.Read(writtenStream);

            // Compare the files for semantic equivalence
            CompareSerializedFiles(originalFile, regeneratedFile);
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void VerifyByteEquivalence(string filePath)
        {
            // Read the original file
            SerializedFileReader reader = new SerializedFileReader();
            SerializedFile originalFile;
            byte[] originalBytes;

            using (Stream stream = File.OpenRead(filePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    originalBytes = memoryStream.ToArray();
                }

                // Reset stream position and read
                stream.Position = 0;
                originalFile = reader.Read(stream);
            }

            // Configure writer with the same settings from the original file
            SerializedFileWriter writer = new SerializedFileWriter(new SerializedFileWriter.Options
            {
                IsBigEndian = originalFile.Header.IsBigEndian,
                UnityVersion = originalFile.Metadata.UnityVersion,
                TargetPlatform = originalFile.Metadata.TargetPlatform,
                EnableTypeTree = originalFile.Metadata.EnableTypeTree
            });

            // Convert file objects to SerializedFileWriter.SerializedFileObject format
            var writerObjects = new SerializedFileWriter.SerializedFileObject[originalFile.Objects.Length];
            // Create a mapping from offset to index to determine the data order
            var offsetToIndex = originalFile.Objects
                .Select((obj, index) => (obj.Offset, index))
                .OrderBy(pair => pair.Offset)
                .Select((pair, dataIndex) => (pair.index, dataIndex))
                .ToDictionary(pair => pair.index, pair => pair.dataIndex);

            for (int i = 0; i < originalFile.Objects.Length; i++)
            {
                var obj = originalFile.Objects[i];
                var serializedObject = originalFile.GetSerializedObject(obj);

                writerObjects[i] = new SerializedFileWriter.SerializedFileObject
                {
                    PathId = obj.PathId,
                    TypeId = obj.TypeId,
                    SerializedObject = serializedObject,
                    DataIndex = offsetToIndex[i]  // Assign the data index based on the original offset order
                };
            }

            // Generate the serialized data
            Stream writtenStream = writer.Write(
                originalFile.Types,
                writerObjects,
                originalFile.ScriptReferences,
                originalFile.AssetReferences,
                originalFile.ReferenceTypes,
                originalFile.UserInformation
            );

            // Get written bytes
            byte[] writtenBytes;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                writtenStream.Position = 0;
                writtenStream.CopyTo(memoryStream);
                writtenBytes = memoryStream.ToArray();
            }

            // Compare file sizes
            Assert.AreEqual(originalBytes.Length, writtenBytes.Length, "File sizes should match");

            // Compare bytes chunk by chunk to make debugging easier
            const int chunkSize = 1024;
            for (int i = 0; i < originalBytes.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, originalBytes.Length - i);
                byte[] originalChunk = new byte[length];
                byte[] writtenChunk = new byte[length];

                Array.Copy(originalBytes, i, originalChunk, 0, length);
                Array.Copy(writtenBytes, i, writtenChunk, 0, length);

                if (!writtenBytes.AsSpan().SequenceEqual(originalBytes))
                {
                    FindFirstByteDifference(originalBytes, writtenBytes);
                    // Keep the original assertion that will fail, but now with more context:
                    Assert.IsTrue(writtenBytes.AsSpan().SequenceEqual(originalBytes), "Bytes differ at positions shown above");
                }
            }
        }

        private void FindFirstByteDifference(byte[] originalBytes, byte[] writtenBytes)
        {
            int minLength = Math.Min(originalBytes.Length, writtenBytes.Length);

            // Find the first differing byte
            for (int i = 0; i < minLength; i++)
            {
                if (originalBytes[i] != writtenBytes[i])
                {
                    Console.WriteLine($"First difference at position {i}:");
                    Console.WriteLine($"Original byte: {originalBytes[i]} (0x{originalBytes[i]:X2})");
                    Console.WriteLine($"Written byte: {writtenBytes[i]} (0x{writtenBytes[i]:X2})");

                    // Show context (10 bytes before and after)
                    int contextStart = Math.Max(0, i - 10);
                    int contextEnd = Math.Min(minLength - 1, i + 10);

                    Console.WriteLine("Context (original):");
                    for (int j = contextStart; j <= contextEnd; j++)
                    {
                        string marker = j == i ? " <-- " : "     ";
                        Console.WriteLine($"{j,6}: 0x{originalBytes[j]:X2}{marker}");
                    }

                    Console.WriteLine("Context (written):");
                    for (int j = contextStart; j <= contextEnd; j++)
                    {
                        string marker = j == i ? " <-- " : "     ";
                        Console.WriteLine($"{j,6}: 0x{writtenBytes[j]:X2}{marker}");
                    }

                    break;
                }
            }

            // If we got here without finding a difference, check for length mismatch
            if (originalBytes.Length != writtenBytes.Length)
            {
                Console.WriteLine($"No byte differences found in the first {minLength} bytes");
                Console.WriteLine($"But lengths differ: original={originalBytes.Length}, written={writtenBytes.Length}");
            }
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void VerifyObjectBytesPreservation(string filePath)
        {
            // Read the original file
            SerializedFileReader reader = new SerializedFileReader();
            SerializedFile originalFile;
            using (Stream stream = File.OpenRead(filePath))
            {
                originalFile = reader.Read(stream);
            }

            // Configure writer with the same settings from the original file
            SerializedFileWriter writer = new SerializedFileWriter(new SerializedFileWriter.Options
            {
                IsBigEndian = originalFile.Header.IsBigEndian,
                UnityVersion = originalFile.Metadata.UnityVersion,
                TargetPlatform = originalFile.Metadata.TargetPlatform,
                EnableTypeTree = originalFile.Metadata.EnableTypeTree
            });

            // Convert file objects to SerializedFileWriter.SerializedFileObject format
            var writerObjects = new SerializedFileWriter.SerializedFileObject[originalFile.Objects.Length];
            // Create a mapping from offset to index to determine the data order
            var offsetToIndex = originalFile.Objects
                .Select((obj, index) => (obj.Offset, index))
                .OrderBy(pair => pair.Offset)
                .Select((pair, dataIndex) => (pair.index, dataIndex))
                .ToDictionary(pair => pair.index, pair => pair.dataIndex);

            for (int i = 0; i < originalFile.Objects.Length; i++)
            {
                var obj = originalFile.Objects[i];
                var serializedObject = originalFile.GetSerializedObject(obj);

                writerObjects[i] = new SerializedFileWriter.SerializedFileObject
                {
                    PathId = obj.PathId,
                    TypeId = obj.TypeId,
                    SerializedObject = serializedObject,
                    DataIndex = offsetToIndex[i]  // Assign the data index based on the original offset order
                };
            }

            // Generate the serialized data
            Stream writtenStream = writer.Write(
                originalFile.Types,
                writerObjects,
                originalFile.ScriptReferences,
                originalFile.AssetReferences,
                originalFile.ReferenceTypes,
                originalFile.UserInformation
            );

            // Read the generated file
            SerializedFile regeneratedFile;
            writtenStream.Position = 0;
            regeneratedFile = reader.Read(writtenStream);

            // Compare object bytes specifically
            Assert.AreEqual(originalFile.Objects.Length, regeneratedFile.Objects.Length, "Number of objects should match");

            for (int i = 0; i < originalFile.Objects.Length; i++)
            {
                var originalObj = originalFile.Objects[i];
                var regeneratedObj = regeneratedFile.Objects[i];

                // Compare object data bytes
                Assert.AreEqual(originalObj.Data.Length, regeneratedObj.Data.Length, $"Object {i} data size mismatch");

                CollectionAssert.AreEqual(
                    originalObj.Data,
                    regeneratedObj.Data,
                    $"Object {i} data bytes don't match");

                // Verify deserialized object equivalence
                var originalSerialized = originalFile.GetSerializedObject(originalObj);
                var regeneratedSerialized = regeneratedFile.GetSerializedObject(regeneratedObj);

                CompareSerializedObjects(originalSerialized, regeneratedSerialized, $"Object {i}");
            }
        }

        private void CompareSerializedFiles(SerializedFile expected, SerializedFile actual)
        {
            // Compare header properties
            Assert.AreEqual(expected.Header.Version, actual.Header.Version, "Header.Version mismatch");
            Assert.AreEqual(expected.Header.IsBigEndian, actual.Header.IsBigEndian, "Header.IsBigEndian mismatch");

            // Compare metadata
            Assert.AreEqual(expected.Metadata.UnityVersion, actual.Metadata.UnityVersion, "Metadata.UnityVersion mismatch");
            Assert.AreEqual(expected.Metadata.TargetPlatform, actual.Metadata.TargetPlatform, "Metadata.TargetPlatform mismatch");
            Assert.AreEqual(expected.Metadata.EnableTypeTree, actual.Metadata.EnableTypeTree, "Metadata.EnableTypeTree mismatch");

            // Compare types length
            Assert.AreEqual(expected.Types.Length, actual.Types.Length, "Types length mismatch");

            // Compare objects length
            Assert.AreEqual(expected.Objects.Length, actual.Objects.Length, "Objects length mismatch");

            // Compare script references
            Assert.AreEqual(expected.ScriptReferences.Length, actual.ScriptReferences.Length, "ScriptReferences length mismatch");

            // Compare asset references
            Assert.AreEqual(expected.AssetReferences.Length, actual.AssetReferences.Length, "AssetReferences length mismatch");

            // Compare reference types
            Assert.AreEqual(expected.ReferenceTypes.Length, actual.ReferenceTypes.Length, "ReferenceTypes length mismatch");

            // Compare user information
            Assert.AreEqual(expected.UserInformation, actual.UserInformation, "UserInformation mismatch");

            // Compare objects in detail
            for (int i = 0; i < expected.Objects.Length; i++)
            {
                CompareSerializedFileObjects(expected.Objects[i], actual.Objects[i], $"Object {i}");
            }
        }

        private void CompareSerializedFileObjects(SerializedFileObject expected, SerializedFileObject actual, string context)
        {
            Assert.AreEqual(expected.PathId, actual.PathId, $"{context}: PathId mismatch");
            Assert.AreEqual(expected.TypeId, actual.TypeId, $"{context}: TypeId mismatch");
            Assert.AreEqual(expected.ClassId, actual.ClassId, $"{context}: ClassId mismatch");
            Assert.AreEqual(expected.Size, actual.Size, $"{context}: Size mismatch");
            Assert.AreEqual(expected.Offset, actual.Offset, $"{context}: Offset mismatch");
        }

        private void CompareSerializedObjects(SerializedObject expected, SerializedObject actual, string context)
        {
            if (expected == null || actual == null)
            {
                Assert.AreEqual(expected, actual, $"{context}: One object is null while the other is not");
                return;
            }

            Assert.AreEqual(expected.ClassId, actual.ClassId, $"{context}: ClassId mismatch");
            Assert.AreEqual(expected.Values.Count, actual.Values.Count, $"{context}: Values count mismatch");

            foreach (var key in expected.Values.Keys)
            {
                Assert.IsTrue(actual.Values.ContainsKey(key), $"{context}: Key '{key}' missing in actual object");

                // For complex comparisons of values, we would need more sophisticated logic,
                // but for this basic test we'll just verify key existence
            }
        }
    }
}