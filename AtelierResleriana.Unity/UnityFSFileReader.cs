using AtelierResleriana.Compression;
using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    /// <summary>
    /// Class that reads asset bundle files with signature UnityFS.
    /// </summary>
    public class UnityFSFileReader
    {
        public UnityFSFile Read(string path)
        {
            using Stream stream = File.OpenRead(path);
            return Read(stream);
        }

        public UnityFSFile Read(Stream stream)
        {
            UnityFSFileHeader unityFSHeader = ReadHeader(stream);
            UnityFSFileMetadata unityFSFileMetadata = ReadMetadata(stream, unityFSHeader);
            byte[] data = ReadData(stream, unityFSHeader, unityFSFileMetadata);

            return new UnityFSFile()
            {
                Header = unityFSHeader,
                Metadata = unityFSFileMetadata,
                Data = data
            };
        }

        internal UnityFSFileHeader ReadHeader(Stream stream)
        {
            using BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true, Endian.Big);

            string signature = binaryReader.ReadNullTerminatedString();
            if (signature != UnityFSFileHeader.Signature)
            {
                throw new InvalidDataException($"Invalid signature: {signature}");
            }

            uint version = binaryReader.ReadUInt32();
            string playerVersion = binaryReader.ReadNullTerminatedString();
            string engineVersion = binaryReader.ReadNullTerminatedString();
            long fileSize = binaryReader.ReadInt64();
            uint compressedMetadataSize = binaryReader.ReadUInt32();
            uint uncompressedMetadataSize = binaryReader.ReadUInt32();
            uint flags = binaryReader.ReadUInt32();

            return new UnityFSFileHeader()
            {
                Version = version,
                PlayerVersion = playerVersion,
                EngineVersion = engineVersion,
                FileSize = fileSize,
                CompressedMetadataSize = compressedMetadataSize,
                UncompressedMetadataSize = uncompressedMetadataSize,
                Flags = flags
            };
        }

        internal UnityFSFileMetadata ReadMetadata(Stream stream, UnityFSFileHeader header)
        {
            using BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true, Endian.Big);

            EngineVersion unityEngineVersion = header.EngineVersion;

            if (unityEngineVersion == new EngineVersion(0, 0, 0))
            {
                unityEngineVersion = new EngineVersion(2022, 3, 6, "f1");
            }

            if (header.Version >= 7)
            {
                binaryReader.Align(16);
            }
            else if (unityEngineVersion >= new EngineVersion(2019, 4, 0))
            {
                long preAlignPosition = stream.Position;

                int alignmentBytes = (16 - (int)(preAlignPosition % 16)) % 16;

                byte[] alignmentData = new byte[alignmentBytes];
                int bytesRead = stream.Read(alignmentData, 0, alignmentBytes);

                bool allZeros = alignmentData.All(b => b == 0);

                if (!allZeros)
                {
                    stream.Position = preAlignPosition;
                }
            }

            byte[] compressedMetadataBytes;
            if ((header.Flags & (uint)UnityFSFileFlags.BlocksInfoAtTheEnd) != 0)
            {
                // Save current position
                long currentPos = stream.Position;

                // Move to end - compressedSize
                stream.Position = stream.Length - header.CompressedMetadataSize;
                compressedMetadataBytes = new byte[header.CompressedMetadataSize];
                stream.Read(compressedMetadataBytes, 0, (int)header.CompressedMetadataSize);

                // Restore position
                stream.Position = currentPos;
            }
            else
            {
                // Read directly
                compressedMetadataBytes = new byte[header.CompressedMetadataSize];
                stream.Read(compressedMetadataBytes, 0, (int)header.CompressedMetadataSize);
            }

            // Decompress the blocks info
            byte[] decompressedMetadataBytes = DecompressMetadata(compressedMetadataBytes, header);

            // Read the metadata
            using var metadataReader = new BinaryReader(new MemoryStream(decompressedMetadataBytes), Encoding.ASCII, true, Endian.Big);

            // Read uncompressed data hash (16 bytes)
            byte[] uncompressedDataHash = metadataReader.ReadBytes(16);

            // Read blocks info
            int blocksInfoCount = metadataReader.ReadInt32();
            var blockInfos = new UnityFSFileBlockInfo[blocksInfoCount];

            for (int i = 0; i < blocksInfoCount; i++)
            {
                blockInfos[i] = new UnityFSFileBlockInfo
                {
                    UncompressedSize = metadataReader.ReadUInt32(),
                    CompressedSize = metadataReader.ReadUInt32(),
                    Flags = metadataReader.ReadUInt16()
                };
            }

            // Read directory info
            int directoryInfoCount = metadataReader.ReadInt32();
            var directoryInfos = new UnityFSFileDirectoryInfo[directoryInfoCount];

            for (int i = 0; i < directoryInfoCount; i++)
            {
                directoryInfos[i] = new UnityFSFileDirectoryInfo
                {
                    Offset = metadataReader.ReadInt64(),
                    Size = metadataReader.ReadInt64(),
                    Flags = metadataReader.ReadUInt32(),
                    Path = metadataReader.ReadNullTerminatedString()
                };
            }

            // If we need padding at start for block info
            if ((header.Flags & (uint)UnityFSFileFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                binaryReader.Align(16);
            }

            return new UnityFSFileMetadata
            {
                UncompressedDataHash = uncompressedDataHash,
                BlockInfos = blockInfos,
                DirectoryInfos = directoryInfos
            };
        }

        private byte[] DecompressMetadata(byte[] compressedData, UnityFSFileHeader header)
        {
            var compressionType = header.Compression;

            switch (compressionType)
            {
                case UnityFSFileCompression.None:
                    return compressedData;

                case UnityFSFileCompression.Lzma:
                    var lzma = new LzmaCompressionAlgorithm();
                    return lzma.Decompress(compressedData);

                case UnityFSFileCompression.Lz4:
                case UnityFSFileCompression.Lz4hc:
                    var lz4 = new Lz4CompressionAlgorithm();
                    return lz4.Decompress(compressedData, (int)header.UncompressedMetadataSize);

                case UnityFSFileCompression.Lzham:
                    throw new NotImplementedException("LZHAM decompression not implemented");

                default:
                    throw new InvalidOperationException($"Unknown compression type: {compressionType}");
            }
        }

        internal byte[] ReadData(Stream stream, UnityFSFileHeader header, UnityFSFileMetadata metadata)
        {
            // Calculate total uncompressed size
            int totalUncompressedSize = 0;
            foreach (var blockInfo in metadata.BlockInfos)
            {
                totalUncompressedSize += (int)blockInfo.UncompressedSize;
            }

            // Pre-allocate buffer for all blocks
            byte[] finalData = new byte[totalUncompressedSize];
            int currentOffset = 0;

            // Read and process each block
            foreach (var blockInfo in metadata.BlockInfos)
            {
                // Read compressed block data
                byte[] compressedBlock = new byte[blockInfo.CompressedSize];
                stream.Read(compressedBlock, 0, (int)blockInfo.CompressedSize);

                // Determine compression type from flags
                var compressionType = (UnityFSFileCompression)(blockInfo.Flags & 0x3F);

                // Decompress the block
                byte[] decompressedBlock;
                switch (compressionType)
                {
                    case UnityFSFileCompression.None:
                        decompressedBlock = compressedBlock;
                        break;
                    case UnityFSFileCompression.Lzma:
                        var lzma = new LzmaCompressionAlgorithm();
                        decompressedBlock = lzma.Decompress(compressedBlock);
                        break;
                    case UnityFSFileCompression.Lz4:
                    case UnityFSFileCompression.Lz4hc:
                        var lz4 = new Lz4CompressionAlgorithm();
                        decompressedBlock = lz4.Decompress(compressedBlock, (int)blockInfo.UncompressedSize);
                        break;

                    case UnityFSFileCompression.Lzham:
                        throw new NotImplementedException("LZHAM decompression not implemented.");

                    default:
                        throw new InvalidOperationException($"Unknown compression type: {compressionType}");
                }

                // Copy to final buffer
                Buffer.BlockCopy(decompressedBlock, 0, finalData, currentOffset, (int)blockInfo.UncompressedSize);
                currentOffset += (int)blockInfo.UncompressedSize;
            }

            return finalData;
        }
    }
}
