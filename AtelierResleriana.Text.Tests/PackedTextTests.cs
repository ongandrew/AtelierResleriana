using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AtelierResleriana.Text
{
    [TestClass]
    [TestCategory(nameof(PackedText))   ]
    public sealed class PackedTextTests
    {
        private static byte[] GetTestData() => File.ReadAllBytes("Resources/PackedText.bin");

        [TestMethod]
        public void ReadAndWriteProducesSameOutput()
        {
            // Arrange
            byte[] originalBytes = GetTestData();
            var reader = new PackedTextReader();
            var writer = new PackedTextWriter();

            // Act
            var packedText = reader.Read(new MemoryStream(originalBytes));
            using var outputStream = writer.Write(packedText);
            var resultBytes = ((MemoryStream)outputStream).ToArray();

            // Assert
            Assert.IsTrue(originalBytes.SequenceEqual(resultBytes));
            LogHexDump(originalBytes);
        }

        [TestMethod]
        public void ReaderCreatesValidPackedText()
        {
            // Arrange
            byte[] bytes = GetTestData();
            var reader = new PackedTextReader();

            // Act
            var packedText = reader.Read(new MemoryStream(bytes));

            // Assert
            Assert.IsNotNull(packedText);
            Assert.IsTrue(packedText.Properties.Any());
            Assert.IsTrue(packedText.Entries.Any());
        }

        [TestMethod]
        public void WriterProducesValidStream()
        {
            // Arrange
            var packedText = new PackedText();
            packedText.AddProperty(1, PropertyTypes.UnsignedInteger);
            var entry = new Dictionary<uint, object> { { 1, 42u } };
            packedText.AddEntry(entry);
            var writer = new PackedTextWriter();

            // Act
            using var stream = writer.Write(packedText);

            // Assert
            Assert.IsTrue(stream.Length > 0);
            Assert.IsTrue(stream.Position == 0); // Stream should be ready to read
        }

        [TestMethod]
        public void WriterCanWriteToExistingStream()
        {
            // Arrange
            var packedText = new PackedText();
            packedText.AddProperty(1, PropertyTypes.String);
            var entry = new Dictionary<uint, object> { { 1, "test" } };
            packedText.AddEntry(entry);
            var writer = new PackedTextWriter();
            using var memoryStream = new MemoryStream();

            // Act
            writer.Write(memoryStream, packedText);

            // Assert
            Assert.IsTrue(memoryStream.Length > 0);
        }

        [TestMethod]
        public void RoundTripPreservesAllData()
        {
            // Arrange
            var originalText = new PackedText();
            originalText.AddProperty(1, PropertyTypes.UnsignedInteger);
            originalText.AddProperty(2, PropertyTypes.String);
            var entry = new Dictionary<uint, object>
            {
                { 1, 42u },
                { 2, "test string" }
            };
            originalText.AddEntry(entry);

            var writer = new PackedTextWriter();
            var reader = new PackedTextReader();

            // Act
            using var stream = writer.Write(originalText);
            var roundTrippedText = reader.Read(stream);

            // Assert
            Assert.AreEqual(originalText.Properties.Count, roundTrippedText.Properties.Count);
            Assert.AreEqual(originalText.Entries.Count, roundTrippedText.Entries.Count);
            Assert.AreEqual(42u, roundTrippedText.GetValue<uint>(0, 1));
            Assert.AreEqual("test string", roundTrippedText.GetValue<string>(0, 2));
        }

        private static void LogHexDump(byte[] bytes, int maxBytes = 64)
        {
            StringBuilder hexOutput = new StringBuilder();
            for (int j = 0; j < Math.Min(bytes.Length, maxBytes); j++)
            {
                hexOutput.Append($"{bytes[j]:X2} ");
                if ((j + 1) % 8 == 0)
                    hexOutput.AppendLine();
            }
            Console.WriteLine(hexOutput);
        }

        [TestMethod]
        public void CanSerializeToJson()
        {
            // Arrange
            byte[] bytes = File.ReadAllBytes("Resources/PackedText.bin");
            var reader = new PackedTextReader();
            var packedText = reader.Read(new MemoryStream(bytes));

            // Act
            string json = packedText.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("properties"));
            Assert.IsTrue(json.Contains("entries"));
            Console.WriteLine(json);
        }

        [TestMethod]
        public void JsonContainsAllProperties()
        {
            // Arrange
            var packedText = new PackedText();
            packedText.AddProperty(1, PropertyTypes.UnsignedInteger);
            var entry = new Dictionary<uint, object> { { 1, 42u } };
            packedText.AddEntry(entry);

            // Act
            string json = packedText.ToJson();

            // Assert
            Assert.IsTrue(json.Contains("\"id\":1"));
            Assert.IsTrue(json.Contains("\"type\":0"));
            Assert.IsTrue(json.Contains("42"));
        }
    }
}