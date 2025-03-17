using SevenZip.Compression.LZMA;

namespace AtelierResleriana.Compression
{
    public class LzmaCompressionAlgorithm : ICompressionAlgorithm
    {
        public byte[] Compress(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Compress(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public byte[] Decompress(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Decompress(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public void Compress(Stream input, Stream output)
        {
            var encoder = new Encoder();

            encoder.WriteCoderProperties(output);

            var length = input.Length - input.Position;
            for (int i = 0; i < 8; i++)
                output.WriteByte((byte)(length >> (8 * i)));

            encoder.Code(input, output, length, -1, null);
        }

        public void Decompress(Stream input, Stream output)
        {
            var decoder = new Decoder();

            byte[] properties = new byte[5];
            if (input.Read(properties, 0, 5) != 5)
                throw new InvalidDataException("Input too short");
            decoder.SetDecoderProperties(properties);

            long dataLength = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = input.ReadByte();
                if (v < 0)
                    throw new InvalidDataException("Cannot read data length");
                dataLength |= ((long)(byte)v) << (8 * i);
            }

            var remaining = input.Length - input.Position;
            decoder.Code(input, output, remaining, dataLength, null);
        }
    }
}