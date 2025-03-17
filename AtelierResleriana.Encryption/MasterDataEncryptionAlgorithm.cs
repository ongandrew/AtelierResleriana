using System.Security.Cryptography;
using System.Text;

namespace AtelierResleriana.Encryption
{
    public class MasterDataEncryptionAlgorithm : IEncryptionAlgorithm
    {
        public bool SupportsStreaming => true;
        private const string KeyVersionPrefix = "wTmkW6hwnA6HXnItdXjZp/BSOdPuh2L9QzdM3bx1e54=";
        private readonly byte[] Key;
        private readonly byte[] IV;

        public MasterDataEncryptionAlgorithm(byte[] key)
        {
            Key = key[..16];
            IV = key[16..32];
        }

        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Key = Key;
            aes.IV = IV;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        public byte[] Encrypt(byte[] bytes)
        {
            using var aes = CreateAes();
            using var encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public byte[] Decrypt(byte[] bytes)
        {
            using var aes = CreateAes();
            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        }

        public void Encrypt(Stream input, Stream output)
        {
            using var aes = CreateAes();
            using var cryptoStream = new CryptoStream(
                output,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write,
                leaveOpen: true
            );
            input.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
        }

        public void Decrypt(Stream input, Stream output)
        {
            using var aes = CreateAes();
            using var cryptoStream = new CryptoStream(
                output,
                aes.CreateDecryptor(),
                CryptoStreamMode.Write,
                leaveOpen: true
            );
            input.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
        }

        public static MasterDataEncryptionAlgorithm FromVersion(string version)
        {
            var input = $"{KeyVersionPrefix}{version}";
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new MasterDataEncryptionAlgorithm(hash);
        }
    }
}