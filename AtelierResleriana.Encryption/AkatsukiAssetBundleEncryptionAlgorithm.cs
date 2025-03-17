using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AtelierResleriana.Encryption
{
    public class AkatsukiAssetBundleEncryptionAlgorithm : IEncryptionAlgorithm
    {
        private readonly string BundleName;
        private readonly long FileSize;
        private readonly string Hash;
        private readonly uint Crc;
        private const uint MAGIC = 0x6b746b41; // "Aktk"

        public bool SupportsStreaming => true;

        public AkatsukiAssetBundleEncryptionAlgorithm(string bundleName, long fileSize, string hash, uint crc)
        {
            BundleName = bundleName;
            FileSize = fileSize;
            Hash = hash;
            Crc = crc;
        }

        public byte[] Encrypt(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Encrypt(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public byte[] Decrypt(byte[] bytes)
        {
            using var inputStream = new MemoryStream(bytes);
            using var outputStream = new MemoryStream();
            Decrypt(inputStream, outputStream);
            return outputStream.ToArray();
        }

        public void Encrypt(Stream input, Stream output)
        {
            // Read input data
            var inputData = new byte[input.Length];
            input.Read(inputData, 0, (int)input.Length);

            // Create header
            var header = new Header(MAGIC, 1, 0, 1);
            var headerBytes = new byte[Header.Size];
            System.Runtime.InteropServices.MemoryMarshal.Write(headerBytes, ref header);

            // Calculate MD5 hash of input data
            var hash = MD5.HashData(inputData);

            // Write header
            output.Write(headerBytes, 0, headerBytes.Length);
            output.Write(hash, 0, hash.Length);

            // Encrypt data using ChaCha20
            var encryptedData = EncryptData(inputData);
            output.Write(encryptedData, 0, encryptedData.Length);
        }

        public void Decrypt(Stream input, Stream output)
        {
            // Read header
            var headerBytes = new byte[Header.Size];
            input.Read(headerBytes, 0, Header.Size);

            var header = System.Runtime.InteropServices.MemoryMarshal.Read<Header>(headerBytes);

            if (header.Magic != MAGIC || header.Version != 1 || header.Reserved != 0 || (header.Encrypted != 0 && header.Encrypted != 1))
                throw new InvalidOperationException("Invalid bundle header");

            // Read hash
            var hashBytes = new byte[Header.HashSize];
            input.Read(hashBytes, 0, Header.HashSize);

            // Read encrypted data
            var encryptedData = new byte[input.Length - Header.Size - Header.HashSize];
            input.Read(encryptedData, 0, encryptedData.Length);

            // Verify hash
            var calculatedHash = MD5.HashData(encryptedData);
            if (!calculatedHash.SequenceEqual(hashBytes))
                throw new InvalidOperationException("Hash mismatch");

            if (header.Encrypted == 1)
            {
                var decryptedData = DecryptData(encryptedData);
                output.Write(decryptedData, 0, decryptedData.Length);
            }
            else
            {
                output.Write(encryptedData, 0, encryptedData.Length);
            }
        }

        private byte[] EncryptData(byte[] data)
        {
            var keyMaterial = $"{BundleName}-{data.Length}-{Hash}-{Crc}";
            var hash = SHA512.HashData(Encoding.UTF8.GetBytes(keyMaterial));
            var key = hash[..0x20];
            var nonceMaterial = SHA512.HashData(hash);

            return ProcessData(data, key, nonceMaterial);
        }

        private byte[] DecryptData(byte[] data)
        {
            var keyMaterial = $"{BundleName}-{data.Length}-{Hash}-{Crc}";
            var hash = SHA512.HashData(Encoding.UTF8.GetBytes(keyMaterial));
            var key = hash[..0x20];
            var nonceMaterial = SHA512.HashData(hash);

            return ProcessData(data, key, nonceMaterial);
        }

        private static byte[] ProcessData(byte[] data, byte[] key, byte[] nonceMaterial)
        {
            var result = data.ToArray();
            var counter = 0;

            Span<byte> context = stackalloc byte[0x40 * 8]; // 8 ChaCha contexts
            context.Clear();

            Span<byte> nonce = stackalloc byte[12];
            nonce.Clear();

            var blockCount = data.Length / context.Length;
            var lastBlockCount = data.Length % context.Length;

            if (blockCount > 0)
            {
                for (int i = 0; i < blockCount; i++)
                {
                    GenerateKeyStream(ref context, ref nonce, ref counter, key, nonceMaterial);

                    var partData = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ulong>(result.AsSpan(i * context.Length, context.Length));
                    var keyStream = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, ulong>(context);
                    for (int j = 0; j < context.Length / 8; j++)
                    {
                        partData[j] ^= keyStream[j];
                    }
                }
            }

            if (lastBlockCount > 0)
            {
                GenerateKeyStream(ref context, ref nonce, ref counter, key, nonceMaterial);

                for (int i = 0; i < lastBlockCount; i++)
                {
                    result[i + context.Length * blockCount] ^= context[i];
                }
            }

            return result;
        }

        private static void GenerateKeyStream(ref Span<byte> context, ref Span<byte> nonce, ref int counter, Span<byte> keyMaterial, Span<byte> nonceMaterial)
        {
            var nonceMaterial1 = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(nonceMaterial.Slice((counter % 0xd) | 0x30, 4))[0];
            var nonceMaterial2 = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(nonceMaterial.Slice(counter / 0xd % 0xd, 4))[0];
            var nonceXor1 = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(nonceMaterial.Slice(counter / 0xA9 % 0xd | 0x10, 4))[0];
            var nonceXor2 = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(nonceMaterial.Slice(counter / 0x895 % 0xd | 0x20, 4))[0];

            var rotatedTemp = System.Numerics.BitOperations.RotateRight(nonceMaterial1, -(2 * (counter % 0x93e / 0xa9) % 0x1b));
            var rotatedTemp2 = System.Numerics.BitOperations.RotateRight(nonceMaterial2, -(3 * (counter / 0x93e) % 0x1b));
            var nonceSeed = rotatedTemp ^ rotatedTemp2;

            var nonceValues = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, uint>(nonce);
            nonceValues[0] = nonceSeed;
            nonceValues[1] = nonceSeed ^ nonceXor1;
            nonceValues[2] = nonceValues[1] ^ nonceXor2;

            var rounds = new[] { 12, 8, 8, 8, 4, 4, 4, 4 };

            Span<byte> state = stackalloc byte[0x40];
            ChaCha20.SetupContext(ref state, keyMaterial, nonce, ++counter);

            for (int i = 0; i < rounds.Length; i++)
            {
                ChaCha20.GenerateKeyMaterial(
                    state,
                    context.Slice(i * 0x40, 0x40),
                    i == 0 ? default : context.Slice(i * 0x40 - 0x40, 0x40),
                    rounds[i]);
            }
        }

        // https://github.com/theBowja/AtelierTool/blob/download-bundles/AtelierTool/BundleCrypto.cs
        internal static class ChaCha20
        {
            public static void SetupContext(ref Span<byte> context, Span<byte> key, Span<byte> nonce, int counter)
            {
                Debug.Assert(context.Length == 0x40, "context.Length == 0x40");

                var contextData = MemoryMarshal.Cast<byte, uint>(context);

                var constant = MemoryMarshal.Cast<byte, uint>(Encoding.UTF8.GetBytes("expand 32-byte k"));
                contextData[0] = constant[0];
                contextData[1] = constant[1];
                contextData[2] = constant[2];
                contextData[3] = constant[3];

                var keyData = MemoryMarshal.Cast<byte, uint>(key);
                contextData[4] = keyData[0];
                contextData[5] = keyData[1];
                contextData[6] = keyData[2];
                contextData[7] = keyData[3];
                contextData[8] = keyData[4];
                contextData[9] = keyData[5];
                contextData[10] = keyData[6];
                contextData[11] = keyData[7];

                var nonceData = MemoryMarshal.Cast<byte, uint>(nonce);
                contextData[12] = (uint)counter;
                contextData[13] = nonceData[0];
                contextData[14] = nonceData[1];
                contextData[15] = nonceData[2];
            }

            public static void GenerateKeyMaterial(Span<byte> context, Span<byte> keyStream, Span<byte> initialState, int rounds)
            {
                var x = (stackalloc uint[16]);
                var y = (stackalloc uint[16]);

                var contextData = MemoryMarshal.Cast<byte, uint>(context);
                var initialStateData = MemoryMarshal.Cast<byte, uint>(initialState);
                if (initialState != default)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        x[i] = y[i] = contextData[i] ^ initialStateData[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        x[i] = y[i] = contextData[i];
                    }
                }

                for (int i = rounds; i > 0; i -= 2)
                {
                    QuarterRound(ref x, 0, 4, 8, 12);
                    QuarterRound(ref x, 1, 5, 9, 13);
                    QuarterRound(ref x, 2, 6, 10, 14);
                    QuarterRound(ref x, 3, 7, 11, 15);

                    QuarterRound(ref x, 0, 5, 10, 15);
                    QuarterRound(ref x, 1, 6, 11, 12);
                    QuarterRound(ref x, 2, 7, 8, 13);
                    QuarterRound(ref x, 3, 4, 9, 14);
                }

                var keyStreamData = MemoryMarshal.Cast<byte, uint>(keyStream);
                for (int i = 0; i < 16; i++)
                {
                    keyStreamData[i] = x[i] + y[i];
                }

                contextData[12]++;
                if (contextData[12] <= 0)
                {
                    contextData[13]++;
                }
            }

            private static void QuarterRound(ref Span<uint> x, int a, int b, int c, int d)
            {
                x[a] += x[b];
                x[d] = BitOperations.RotateLeft(x[d] ^ x[a], 16);
                x[c] += x[d];
                x[b] = BitOperations.RotateLeft(x[b] ^ x[c], 12);
                x[a] += x[b];
                x[d] = BitOperations.RotateLeft(x[d] ^ x[a], 8);
                x[c] += x[d];
                x[b] = BitOperations.RotateLeft(x[b] ^ x[c], 7);
            }
        }

        internal readonly struct Header
        {
            public const int Size = 0xc;
            public const int HashSize = 0x10;

            public readonly uint Magic;
            public readonly ushort Version;
            public readonly ushort Reserved;
            public readonly uint Encrypted;

            public Header(uint magic, ushort version, ushort reserved, uint encrypted)
            {
                Magic = magic;
                Version = version;
                Reserved = reserved;
                Encrypted = encrypted;
            }
        }
    }
}