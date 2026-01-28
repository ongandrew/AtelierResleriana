using AtelierResleriana.Encryption;
using AtelierResleriana.MessagePack;
using System.Buffers;

namespace AtelierResleriana.MasterData
{
    public class MasterDataReader
    {
        private class Catalog : Dictionary<string, CatalogEntry>
        {
        }

        private record class CatalogEntry(long Offset, long Size);


        /// <summary>
        /// Reads from a decrypted/unencrypted master data stream.
        /// </summary>
        /// <param name="decryptedStream"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public IEnumerable<MasterDataFile> Read(Stream decryptedStream)
        {
            byte[] allBytes;
            using (var ms = new MemoryStream())
            {
                decryptedStream.CopyTo(ms);
                allBytes = ms.ToArray();
            }

            var options = MessagePackSerializerOptions.Standard;
            var reader = new MessagePackReader(allBytes);

            Dictionary<string, long[]> rawCatalog = MessagePackSerializer.Deserialize<Dictionary<string, long[]>>(
                ref reader,
                options
            );

            Catalog catalog = new Catalog();
            foreach (var kvp in rawCatalog)
            {
                catalog.Add(kvp.Key, new CatalogEntry(kvp.Value[0], kvp.Value[1]));
            }

            long dataBlockStartOffset = reader.Consumed;

            List<MasterDataFile> files = new List<MasterDataFile>(catalog.Count);
            foreach (var entry in catalog)
            {
                long absoluteOffset = dataBlockStartOffset + entry.Value.Offset;

                byte[] fileBytes = new byte[entry.Value.Size];
                Array.Copy(allBytes, absoluteOffset, fileBytes, 0, entry.Value.Size);

                files.Add(new MasterDataFile()
                {
                    Name = entry.Key,
                    Bytes = fileBytes
                });
            }

            return files;
        }

        /// <summary>
        /// Reads from a decrypted/unencrypted master data stream.
        /// </summary>
        /// <param name="decryptedStream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public async Task<IEnumerable<MasterDataFile>> ReadAsync(Stream decryptedStream, CancellationToken cancellationToken = default)
        {
            var options = MessagePackSerializerOptions.Standard;

            using var streamReader = new MessagePackStreamReader(decryptedStream);

            Catalog catalog = new Catalog();

            ReadOnlySequence<byte>? byteSequence = await streamReader.ReadAsync(cancellationToken).ConfigureAwait(false);

            if (byteSequence == null)
            {
                throw new FormatException("Expected catalog as first object.");
            }

            Dictionary<string, long[]> rawCatalog = MessagePackSerializer.Deserialize<Dictionary<string, long[]>>(
                byteSequence.Value,
                options
            );

            foreach ((string fileName, long[] data) in rawCatalog)
            {
                catalog.Add(fileName, new CatalogEntry(data[0], data[1]));
            }

            MemoryStream memoryStream = new MemoryStream();

            while (await streamReader.ReadAsync(cancellationToken).ConfigureAwait(false) is ReadOnlySequence<byte> bytes)
            {
                memoryStream.Write(bytes.ToArray());
            }

            List<MasterDataFile> files = new List<MasterDataFile>(catalog.Count);
            foreach ((string fileName, CatalogEntry catalogEntry) in catalog)
            {
                memoryStream.Position = catalogEntry.Offset;
                byte[] bytes = new byte[catalogEntry.Size];
                await memoryStream.ReadAsync(bytes, 0, (int)catalogEntry.Size, cancellationToken);

                files.Add(new MasterDataFile()
                {
                    Name = fileName,
                    Bytes = bytes
                });
            }

            return files;
        }

        /// <summary>
        /// Reads from an encrypted master data stream with the given version.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="version"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MasterDataFile>> ReadEncryptedAsync(Stream stream, string version, CancellationToken cancellationToken = default)
        {
            var algorithm = MasterDataEncryptionAlgorithm.FromVersion(version);
            using var decryptedStream = new MemoryStream();
            algorithm.Decrypt(stream, decryptedStream);
            decryptedStream.Position = 0;

            return await ReadAsync(decryptedStream, cancellationToken).ConfigureAwait(false);
        }
    }
}
