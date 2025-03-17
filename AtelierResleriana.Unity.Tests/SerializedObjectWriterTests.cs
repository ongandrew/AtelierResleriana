using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AtelierResleriana.Unity
{
    [TestClass]
    [TestCategory(nameof(SerializedObjectWriter))]
    public sealed class SerializedObjectWriterTests
    {
        private static void CompareStreams(Stream expected, Stream actual)
        {
            Assert.AreEqual(expected.Length, actual.Length, "Stream lengths should match");

            expected.Position = 0;
            actual.Position = 0;

            var expectedBuffer = new byte[4096];
            var actualBuffer = new byte[4096];

            while (true)
            {
                int expectedRead = expected.Read(expectedBuffer, 0, expectedBuffer.Length);
                int actualRead = actual.Read(actualBuffer, 0, actualBuffer.Length);

                Assert.AreEqual(expectedRead, actualRead, "Stream read lengths should match");

                if (expectedRead == 0)
                    break;

                for (int i = 0; i < expectedRead; i++)
                {
                    Assert.AreEqual(expectedBuffer[i], actualBuffer[i], $"Byte mismatch at position {expected.Position - expectedRead + i}");
                }
            }
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void WrittenObjectMatchesOriginal(string filePath)
        {
            // Read the original file
            using Stream stream = File.OpenRead(filePath);
            var reader = new SerializedFileReader();
            var serializedFile = reader.Read(stream);

            // Test each object in the file
            foreach (var obj in serializedFile.Objects)
            {
                var writer = new SerializedObjectWriter(new SerializedObjectWriter.Options
                {
                    IsBigEndian = serializedFile.Header.IsBigEndian,
                    BaseOffset = obj.Offset
                });

                // Get the original serialized object
                var serializedObject = serializedFile.GetSerializedObject(obj);
                Assert.IsNotNull(serializedObject);

                // Write the object back to a new stream
                using var writtenStream = writer.Write(serializedObject, obj.Type.Node);

                // Create a memory stream with the original object data for comparison
                using var originalStream = new MemoryStream(obj.Data);

                // Compare the streams
                CompareStreams(originalStream, writtenStream);
            }
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41", "Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41.Objects.0.bin")]
        public void CanRoundTripSpecificObject(string filePath, string objectFilePath)
        {
            // Read the first object from the test file
            using Stream stream = File.OpenRead(filePath);
            var reader = new SerializedFileReader();
            var serializedFile = reader.Read(stream);

            var firstObject = serializedFile.Objects[0];
            var serializedObject = serializedFile.GetSerializedObject(firstObject);
            Assert.IsNotNull(serializedObject);

            // Write it back out
            var writer = new SerializedObjectWriter(new SerializedObjectWriter.Options
            {
                IsBigEndian = serializedFile.Header.IsBigEndian,
                BaseOffset = firstObject.Offset
            });
            using var writtenStream = writer.Write(serializedObject, firstObject.Type.Node);

            // Compare with reference data
            using var referenceStream = new MemoryStream(
                File.ReadAllBytes(objectFilePath)
            );

            CompareStreams(referenceStream, writtenStream);
        }

        [TestMethod]
        [DataRow("Resources/CAB-0a0ae5c51d164d2acdc818d2f8a1aa41")]
        [DataRow("Resources/CAB-282243a14cbd7835f618f9c2f852f687")]
        [DataRow("Resources/CAB-cb51948bc4ba920cd3a24703432f3b99")]
        public void WriterPreservesAlignment(string filePath)
        {
            using Stream stream = File.OpenRead(filePath);
            var reader = new SerializedFileReader();
            var serializedFile = reader.Read(stream);

            foreach (var obj in serializedFile.Objects)
            {
                var writer = new SerializedObjectWriter(new SerializedObjectWriter.Options
                {
                    IsBigEndian = serializedFile.Header.IsBigEndian,
                    BaseOffset = obj.Offset
                });

                var serializedObject = serializedFile.GetSerializedObject(obj);
                Assert.IsNotNull(serializedObject);

                using var writtenStream = writer.Write(serializedObject, obj.Type.Node);

                byte[] bytes = ((MemoryStream)writtenStream).ToArray();
                byte[] referenceBytes = obj.Data;

                Assert.AreEqual(referenceBytes.Length, bytes.Length, $"Byte length mismatch. Wrote {bytes.Length}, expected {referenceBytes.Length}");

                Assert.IsTrue(bytes.AsSpan().SequenceEqual(referenceBytes), "Byte mismatch.");
            }
        }
    }
}