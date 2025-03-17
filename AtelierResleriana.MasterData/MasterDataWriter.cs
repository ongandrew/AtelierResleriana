using AtelierResleriana.Encryption;
using AtelierResleriana.MessagePack;

namespace AtelierResleriana.MasterData
{
    public class MasterDataWriter
    {
        private class Catalog : Dictionary<string, CatalogEntry>
        {
        }

        private record class CatalogEntry(long Offset, long Size);

        public async Task WriteAsync(Stream stream, IEnumerable<MasterDataFile> files, CancellationToken cancellationToken = default)
        {
            await WriteAsync(stream, files.ToDictionary(x => x.Name, x => x.Bytes), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes files to a stream in the master data format.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="files">Dictionary of files to write.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task WriteAsync(Stream stream, IDictionary<string, byte[]> files, CancellationToken cancellationToken = default)
        {
            MemoryStream dataStream = new MemoryStream();
            Catalog catalog = new Catalog();

            foreach ((string fileName, byte[] bytes) in files)
            {
                long offset = dataStream.Position;
                long size = bytes.LongLength;
                dataStream.Write(bytes);
                catalog.Add(fileName, new CatalogEntry(offset, size));
            }

            var rawCatalog = new Dictionary<string, long[]>();
            foreach ((string fileName, CatalogEntry entry) in catalog)
            {
                rawCatalog.Add(fileName, new[] { entry.Offset, entry.Size });
            }

            var options = MessagePackSerializerOptions.Standard;

            byte[] serializedCatalog = MessagePackSerializer.Serialize(rawCatalog, options);

            stream.Write(serializedCatalog);
            dataStream.Position = 0;
            await dataStream.CopyToAsync(stream, cancellationToken);
        }

        public async Task WriteEncryptedAsync(Stream stream, string version, IEnumerable<MasterDataFile> files, CancellationToken cancellationToken = default)
        {
            await WriteEncryptedAsync(stream, version, files.ToDictionary(x => x.Name, x => x.Bytes), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes files to a stream in the encrypted master data format.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="version">Version string used for encryption.</param>
        /// <param name="files">Dictionary of files to write.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task WriteEncryptedAsync(Stream stream, string version, IDictionary<string, byte[]> files, CancellationToken cancellationToken = default)
        {
            // Write unencrypted data to temporary stream
            using var unencryptedStream = new MemoryStream();
            await WriteAsync(unencryptedStream, files, cancellationToken);
            unencryptedStream.Position = 0;

            // Encrypt and write to output stream
            var algorithm = MasterDataEncryptionAlgorithm.FromVersion(version);
            algorithm.Encrypt(unencryptedStream, stream);
        }
    }
}
