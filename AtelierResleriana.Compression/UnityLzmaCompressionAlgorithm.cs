using SevenZip;
using SevenZip.Compression.LZMA;

namespace AtelierResleriana.Compression
{
    public class UnityLzmaCompressionAlgorithm : ICompressionAlgorithm
    {
        public byte PropertiesByte { get; set; }
        public uint DictionarySize { get; set; }
        public int Lc { get; set; }
        public int Lp { get; set; }
        public int Pb { get; set; }
        public int NumFastBytes { get; set; }
        public string MatchFinder { get; set; }

        public UnityLzmaCompressionAlgorithm() : this(null) { }

        public UnityLzmaCompressionAlgorithm(Options? options)
        {
            options ??= new Options();
            PropertiesByte = options.PropertiesByte;
            DictionarySize = options.DictionarySize;
            Lc = options.Lc;
            Lp = options.Lp;
            Pb = options.Pb;
            NumFastBytes = options.NumFastBytes;
            MatchFinder = options.MatchFinder;
        }

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

            // Write properties byte
            output.WriteByte(PropertiesByte);

            // Write dictionary size (4 bytes, little-endian)
            for (int i = 0; i < 4; i++)
                output.WriteByte((byte)(DictionarySize >> (8 * i)));

            // Write decompressed size (8 bytes, little-endian)
            var length = input.Length - input.Position;
            for (int i = 0; i < 8; i++)
                output.WriteByte((byte)(length >> (8 * i)));

            // Set encoder properties
            encoder.SetCoderProperties(new CoderPropID[]
            {
                CoderPropID.DictionarySize,
                CoderPropID.PosStateBits,
                CoderPropID.LitContextBits,
                CoderPropID.LitPosBits,
                CoderPropID.Algorithm,
                CoderPropID.NumFastBytes,
                CoderPropID.MatchFinder,
                CoderPropID.EndMarker
            },
            new object[]
            {
                (int)DictionarySize,
                Pb,                // PosStateBits
                Lc,                // LitContextBits
                Lp,                // LitPosBits
                2,                 // Algorithm
                NumFastBytes,
                MatchFinder,
                false             // EndMarker
            });

            // Compress the data
            encoder.Code(input, output, length, -1, null);
        }

        public void Decompress(Stream input, Stream output)
        {
            var decoder = new Decoder();

            // Read and verify properties byte (shouldn't be used by decoder)
            byte propsByte = (byte)input.ReadByte();

            // Read dictionary size (4 bytes)
            byte[] dictSizeBytes = new byte[4];
            if (input.Read(dictSizeBytes, 0, 4) != 4)
                throw new InvalidDataException("Cannot read dictionary size");

            uint actualDictSize = BitConverter.ToUInt32(dictSizeBytes, 0);
            if (actualDictSize != DictionarySize)
                throw new InvalidDataException($"Dictionary size mismatch. Expected 0x{DictionarySize:X8}, got 0x{actualDictSize:X8}");

            // Read size (8 bytes)
            long dataLength = 0;
            for (int i = 0; i < 8; i++)
            {
                int v = input.ReadByte();
                if (v < 0)
                    throw new InvalidDataException("Cannot read data length");
                dataLength |= ((long)(byte)v) << (8 * i);
            }

            // Construct and set properties
            byte[] properties = new byte[5];
            properties[0] = (byte)((Lc * 9 + Lp) * 5 + Pb);
            Array.Copy(dictSizeBytes, 0, properties, 1, 4);

            decoder.SetDecoderProperties(properties);

            // Decompress
            var remaining = input.Length - input.Position;
            decoder.Code(input, output, remaining, dataLength, null);
        }

        public class Options
        {
            public byte PropertiesByte { get; set; } = 0x5D;
            public uint DictionarySize { get; set; } = 0x800000; // 8MB
            public int Lc { get; set; } = 3;
            public int Lp { get; set; } = 0;
            public int Pb { get; set; } = 2;
            public int NumFastBytes { get; set; } = 128;
            public string MatchFinder { get; set; } = "bt4";
        }
    }
}