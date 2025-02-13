using System.Text;

namespace AtelierResleriana.Text
{
    [TestClass]
    [TestCategory(nameof(PackedText))]
    public sealed class PackedTextTests
    {
        [TestMethod]
        public void ToBytesProducesSameOutput()
        {
            byte[] bytes = File.ReadAllBytes("Resources/PackedText.bin");

            PackedText packedText = new PackedText(bytes);
            Assert.IsTrue(bytes.SequenceEqual(packedText.ToBytes()));

            StringBuilder hexOutput = new StringBuilder();

            for (int j = 0; j < 64; j++)
            {
                hexOutput.Append($"{bytes[j]:X2} ");
                if ((j + 1) % 8 == 0)
                    hexOutput.AppendLine();
            }
            Console.WriteLine(hexOutput);
        }

        [TestMethod]
        public void CanWriteToJson()
        {
            byte[] bytes = File.ReadAllBytes("Resources/PackedText.bin");
            PackedText packedText = new PackedText(bytes);
            string json = packedText.ToJson();
            Assert.IsNotNull(json);
            Console.WriteLine(json);
        }
    }
}
