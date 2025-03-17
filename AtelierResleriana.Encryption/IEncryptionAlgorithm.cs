namespace AtelierResleriana.Encryption
{
    public interface IEncryptionAlgorithm
    {
        bool SupportsStreaming { get; }

        byte[] Encrypt(byte[] bytes);
        byte[] Decrypt(byte[] bytes);
        void Encrypt(Stream input, Stream output);
        void Decrypt(Stream input, Stream output);
    }
}
