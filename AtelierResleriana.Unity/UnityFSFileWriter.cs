using AtelierResleriana.Compression;
using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    public class UnityFSFileWriter
    {
        public string PlayerVersion { get; set; }
        public string EngineVersion { get; set; }
        public uint MaxBlockSize { get; set; }
        public UnityFSFileCompression Compression { get; set; }

        public UnityFSFileWriter() : this(new Options()) { }
        public UnityFSFileWriter(Options options)
        {
            PlayerVersion = options.PlayerVersion;
            EngineVersion = options.EngineVersion;
            MaxBlockSize = options.MaxUncompressedBlockSize;
            Compression = options.Compression;
        }

        public Stream Write(IEnumerable<UnityFSFileDirectory> directories)
        {
            Stream stream = new MemoryStream();

            BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.ASCII, true, Endian.Big);

            binaryWriter.WriteNullTerminatedString(UnityFSFileHeader.Signature);
            binaryWriter.Write((uint)8U);
            binaryWriter.WriteNullTerminatedString(PlayerVersion);
            binaryWriter.WriteNullTerminatedString(EngineVersion);
            long fileSizeOffset = binaryWriter.BaseStream.Position;
            binaryWriter.Write((long)0L);

            UnityFSFileDirectoryInfo[] directoryInfos = new UnityFSFileDirectoryInfo[directories.Count()];

            using MemoryStream uncompressedDataStream = new MemoryStream();
            using MemoryStream compressedDataStream = new MemoryStream();

            int directoryIndex = 0;
            long offset = 0;
            foreach (var directory in directories)
            {
                long size = directory.Bytes.Length;
                directoryInfos[directoryIndex] = new UnityFSFileDirectoryInfo()
                {
                    Path = directory.Path,
                    Offset = offset,
                    Size = size,
                    Flags = directory.Flags
                };
                offset += size;
                directoryIndex++;
                uncompressedDataStream.Write(directory.Bytes);
            }

            uncompressedDataStream.Seek(0, SeekOrigin.Begin);

            long totalSize = directoryInfos.Sum(x => x.Size);

            IList<UnityFSFileBlockInfo> blockInfos = new List<UnityFSFileBlockInfo>();
            long dataOffset = 0;
            long remainingBytes = uncompressedDataStream.Length - dataOffset;

            while (remainingBytes > 0)
            {
                UnityFSFileCompression compression = Compression;

                uint blockSize = (uint)Math.Min(remainingBytes, (long)MaxBlockSize);

                byte[] uncompressedBlockBytes = new byte[blockSize];

                int bytesRead = uncompressedDataStream.Read(uncompressedBlockBytes);

                if (bytesRead != blockSize)
                {
                    throw new InvalidOperationException();
                }

                uint uncompressedBlockSize = blockSize;

                byte[] compressedBlockBytes;

                if (compression == UnityFSFileCompression.None)
                {
                    compressedBlockBytes = uncompressedBlockBytes;
                }
                else if (compression == UnityFSFileCompression.Lz4)
                {
                    Lz4CompressionAlgorithm lz4CompressionAlgorithm = new Lz4CompressionAlgorithm(new Lz4CompressionAlgorithm.Options()
                    {
                        CompressionLevel = K4os.Compression.LZ4.LZ4Level.L00_FAST
                    });
                    compressedBlockBytes = lz4CompressionAlgorithm.Compress(uncompressedBlockBytes);
                }
                else if (compression == UnityFSFileCompression.Lz4hc)
                {
                    Lz4CompressionAlgorithm lz4CompressionAlgorithm = new Lz4CompressionAlgorithm(new Lz4CompressionAlgorithm.Options()
                    {
                        CompressionLevel = K4os.Compression.LZ4.LZ4Level.L09_HC
                    });
                    compressedBlockBytes = lz4CompressionAlgorithm.Compress(uncompressedBlockBytes);
                }
                else
                {
                    throw new NotImplementedException();
                }

                uint compressedBlockSize = (uint)compressedBlockBytes.Length;

                if (compressedBlockSize > uncompressedBlockSize)
                {
                    compressedBlockSize = uncompressedBlockSize;
                    compressedBlockBytes = uncompressedBlockBytes;
                    compression = UnityFSFileCompression.None;
                }

                compressedDataStream.Write(compressedBlockBytes);

                UnityFSFileBlockInfo blockInfo = new UnityFSFileBlockInfo()
                {
                    CompressedSize = compressedBlockSize,
                    UncompressedSize = uncompressedBlockSize,
                    Flags = (ushort)compression
                };

                blockInfos.Add(blockInfo);
                dataOffset += bytesRead;
                remainingBytes = uncompressedDataStream.Length - dataOffset;
            }

            using Stream uncompressedMetadataStream = new MemoryStream();
            using BinaryWriter metadataBinaryWriter = new BinaryWriter(uncompressedMetadataStream, Encoding.ASCII, true, Endian.Big);

            byte[] uncompressedDataHash = new byte[16];
            metadataBinaryWriter.Write(uncompressedDataHash);

            metadataBinaryWriter.Write((int)blockInfos.Count());
            foreach (var blockInfo in blockInfos)
            {
                metadataBinaryWriter.Write((uint)blockInfo.UncompressedSize);
                metadataBinaryWriter.Write((uint)blockInfo.CompressedSize);
                metadataBinaryWriter.Write((ushort)blockInfo.Flags);
            }

            metadataBinaryWriter.Write((int)directoryInfos.Count());
            foreach (var directoryInfo in directoryInfos)
            {
                metadataBinaryWriter.Write((long)directoryInfo.Offset);
                metadataBinaryWriter.Write((long)directoryInfo.Size);
                metadataBinaryWriter.Write((uint)directoryInfo.Flags);
                metadataBinaryWriter.WriteNullTerminatedString(directoryInfo.Path);
            }

            Stream compressedMetadataStream = new MemoryStream();

            if (Compression == UnityFSFileCompression.None)
            {
                compressedMetadataStream = uncompressedMetadataStream;
            }
            else if (Compression == UnityFSFileCompression.Lz4)
            {
                uncompressedMetadataStream.Seek(0, SeekOrigin.Begin);
                Lz4CompressionAlgorithm lz4CompressionAlgorithm = new Lz4CompressionAlgorithm(new Lz4CompressionAlgorithm.Options()
                {
                    CompressionLevel = K4os.Compression.LZ4.LZ4Level.L00_FAST
                });
                lz4CompressionAlgorithm.Compress(uncompressedMetadataStream, compressedMetadataStream);
            }
            else if (Compression == UnityFSFileCompression.Lz4hc)
            {
                uncompressedMetadataStream.Seek(0, SeekOrigin.Begin);
                Lz4CompressionAlgorithm lz4CompressionAlgorithm = new Lz4CompressionAlgorithm(new Lz4CompressionAlgorithm.Options()
                {
                    CompressionLevel = K4os.Compression.LZ4.LZ4Level.L09_HC
                });
                lz4CompressionAlgorithm.Compress(uncompressedMetadataStream, compressedMetadataStream);
            }
            else
            {
                throw new NotImplementedException();
            }

            uint compressedMetadataSize = (uint)compressedMetadataStream.Length;
            uint uncompressedMetadatSize = (uint)uncompressedMetadataStream.Length;
            binaryWriter.Write((uint)compressedMetadataSize);
            binaryWriter.Write((uint)uncompressedMetadatSize);

            UnityFSFileFlags flags = 0;
            flags |= UnityFSFileFlags.BlocksAndDirectoryInfoCombined;
            flags |= UnityFSFileFlags.BlockInfoNeedPaddingAtStart;
            flags |= (UnityFSFileFlags)(uint)Compression;

            binaryWriter.Write((uint)flags);

            binaryWriter.Align(16);

            compressedMetadataStream.Seek(0, SeekOrigin.Begin);
            compressedMetadataStream.CopyTo(stream);

            binaryWriter.Align(16);

            compressedDataStream.Seek(0, SeekOrigin.Begin);
            compressedDataStream.CopyTo(stream);

            stream.Seek(fileSizeOffset, SeekOrigin.Begin);
            binaryWriter.Write((long)stream.Length);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public class Options
        {
            public string PlayerVersion { get; set; } = "5.x.x";
            public string EngineVersion { get; set; } = "0.0.0";
            public uint MaxUncompressedBlockSize { get; set; } = 131072;
            public UnityFSFileCompression Compression { get; set; } = UnityFSFileCompression.Lz4hc;
        }
    }
}
