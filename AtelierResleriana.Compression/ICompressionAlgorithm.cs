namespace AtelierResleriana.Compression
{
    public interface ICompressionAlgorithm
    {
        byte[] Compress(byte[] bytes);
        byte[] Decompress(byte[] bytes);
        void Compress(Stream stream, Stream output);
        void Decompress(Stream input, Stream output);
    }
}
