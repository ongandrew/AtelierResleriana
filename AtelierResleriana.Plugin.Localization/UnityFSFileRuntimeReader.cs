using AtelierResleriana.Compression;
using AtelierResleriana.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.IO;
using System.Linq;
using Universal.Common;

namespace AtelierResleriana.Plugin.Localization
{
    public class UnityFSFileRuntimeReader
    {
        public UnityFSFile Read(Il2CppSystem.IO.Stream stream)
        {
            UnityFSFileHeader unityFSHeader = ReadHeader(stream);
            UnityFSFileMetadata unityFSFileMetadata = ReadMetadata(stream, unityFSHeader);

            return new UnityFSFile()
            {
                Header = unityFSHeader,
                Metadata = unityFSFileMetadata
            };
        }

        internal UnityFSFileHeader ReadHeader(Il2CppSystem.IO.Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Il2CppSystem.Text.Encoding.ASCII, true, Endian.Big);

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

            binaryReader.Dispose();

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

        internal UnityFSFileMetadata ReadMetadata(Il2CppSystem.IO.Stream stream, UnityFSFileHeader header)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Il2CppSystem.Text.Encoding.ASCII, true, Endian.Big);

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

                Il2CppStructArray<byte> alignmentData = new Il2CppStructArray<byte>(alignmentBytes);
                int bytesRead = stream.Read(alignmentData, 0, alignmentBytes);

                bool allZeros = alignmentData.All(b => b == 0);

                if (!allZeros)
                {
                    stream.Position = preAlignPosition;
                }
            }

            Il2CppStructArray<byte> compressedMetadataBytes;
            if ((header.Flags & (uint)UnityFSFileFlags.BlocksInfoAtTheEnd) != 0)
            {
                // Save current position
                long currentPos = stream.Position;

                // Move to end - compressedSize
                stream.Position = stream.Length - header.CompressedMetadataSize;
                compressedMetadataBytes = new Il2CppStructArray<byte>(header.CompressedMetadataSize);
                stream.Read(compressedMetadataBytes, 0, (int)header.CompressedMetadataSize);

                // Restore position
                stream.Position = currentPos;
            }
            else
            {
                // Read directly
                compressedMetadataBytes = new Il2CppStructArray<byte>(header.CompressedMetadataSize);
                stream.Read(compressedMetadataBytes, 0, (int)header.CompressedMetadataSize);
            }

            // Decompress the blocks info
            Il2CppStructArray<byte> decompressedMetadataBytes = DecompressMetadata(compressedMetadataBytes, header);

            // Read the metadata
            var metadataReader = new BinaryReader(new Il2CppSystem.IO.MemoryStream(decompressedMetadataBytes), Il2CppSystem.Text.Encoding.ASCII, true, Endian.Big);

            // Read uncompressed data hash (16 bytes)
            Il2CppStructArray<byte> uncompressedDataHash = metadataReader.ReadBytes(16);

            // Read blocks info
            int blocksInfoCount = metadataReader.ReadInt32();
            UnityFSFileBlockInfo[] blockInfos = new UnityFSFileBlockInfo[blocksInfoCount];

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
            UnityFSFileDirectoryInfo[] directoryInfos = new UnityFSFileDirectoryInfo[directoryInfoCount];

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

            metadataReader.Dispose();
            binaryReader.Dispose();

            return new UnityFSFileMetadata
            {
                UncompressedDataHash = uncompressedDataHash,
                BlockInfos = blockInfos,
                DirectoryInfos = directoryInfos
            };
        }

        private Il2CppStructArray<byte> DecompressMetadata(Il2CppStructArray<byte> compressedData, UnityFSFileHeader header)
        {
            var compressionType = header.Compression;

            switch (compressionType)
            {
                case UnityFSFileCompression.None:
                    return compressedData;
                case UnityFSFileCompression.Lzma:
                    throw new NotImplementedException("LZMA decompression not implemented.");
                case UnityFSFileCompression.Lz4:
                case UnityFSFileCompression.Lz4hc:
                    var lz4 = new Lz4CompressionAlgorithm();
                    return lz4.Decompress(compressedData, (int)header.UncompressedMetadataSize);
                    /*
                    byte[] nativeBytes = compressedData.ToBytes();
                    byte[] decompressedNativeBytes = lz4.Decompress(nativeBytes, (int)header.UncompressedMetadataSize);
                    return decompressedNativeBytes.ToIl2CppBytes();
                    */
                case UnityFSFileCompression.Lzham:
                    throw new NotImplementedException("LZHAM decompression not implemented.");

                default:
                    throw new InvalidOperationException($"Unknown compression type: {compressionType}");
            }
        }

        /// <summary>
        /// An endianness-aware <see cref="System.IO.BinaryReader"/>.
        /// </summary>
        private class BinaryReader : Il2CppSystem.IO.BinaryReader
        {
            public Endian Endian { get; private set; }
            public bool IsLittleEndian { get => Endian == Endian.Little; }

            public BinaryReader(Il2CppSystem.IO.Stream stream) : this(stream, Il2CppSystem.BitConverter.IsLittleEndian ? Endian.Little : Endian.Big) { }
            public BinaryReader(Il2CppSystem.IO.Stream stream, Il2CppSystem.Text.Encoding encoding) : this(stream, encoding, Il2CppSystem.BitConverter.IsLittleEndian ? Endian.Little : Endian.Big) { }
            public BinaryReader(Il2CppSystem.IO.Stream stream, Il2CppSystem.Text.Encoding encoding, bool leaveOpen) : this(stream, encoding, leaveOpen, Il2CppSystem.BitConverter.IsLittleEndian ? Endian.Little : Endian.Big) { }
            public BinaryReader(Il2CppSystem.IO.Stream stream, Endian endian) : this(stream, Il2CppSystem.Text.Encoding.Default, endian) { }
            public BinaryReader(Il2CppSystem.IO.Stream stream, Il2CppSystem.Text.Encoding encoding, Endian endian) : this(stream, encoding, false, endian) { }
            public BinaryReader(Il2CppSystem.IO.Stream stream, Il2CppSystem.Text.Encoding encoding, bool leaveOpen, Endian endian) : base(stream, encoding, leaveOpen)
            {
                Endian = endian;
            }

            public int Align(int alignment)
            {
                if (alignment <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(alignment), "Alignment must be positive.");
                }

                long position = BaseStream.Position;
                long remainder = position % alignment;

                if (remainder == 0)
                {
                    return 0;
                }

                int padding = (int)(alignment - remainder);
                BaseStream.Seek(padding, Il2CppSystem.IO.SeekOrigin.Current);
                return padding;
            }

            private Il2CppStructArray<byte> ReadAndReverseIfNecessary(int byteCount)
            {
                Il2CppStructArray<byte> bytes = base.ReadBytes(byteCount);
                if ((Il2CppSystem.BitConverter.IsLittleEndian && !IsLittleEndian) ||
                    (!Il2CppSystem.BitConverter.IsLittleEndian && IsLittleEndian))
                {
                    Il2CppSystem.Array.Reverse(bytes);
                }
                return bytes;
            }

            public override short ReadInt16()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(2);
                return Il2CppSystem.BitConverter.ToInt16(bytes, 0);
            }

            public override ushort ReadUInt16()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(2);
                return Il2CppSystem.BitConverter.ToUInt16(bytes, 0);
            }

            public override int ReadInt32()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(4);
                return Il2CppSystem.BitConverter.ToInt32(bytes, 0);
            }

            public override uint ReadUInt32()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(4);
                return Il2CppSystem.BitConverter.ToUInt32(bytes, 0);
            }

            public override long ReadInt64()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(8);
                return Il2CppSystem.BitConverter.ToInt64(bytes, 0);
            }

            public override ulong ReadUInt64()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(8);
                return Il2CppSystem.BitConverter.ToUInt64(bytes, 0);
            }

            public override float ReadSingle()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(4);
                return Il2CppSystem.BitConverter.ToSingle(bytes, 0);
            }

            public override Il2CppSystem.Decimal ReadDecimal()
            {
                Il2CppStructArray<byte> bytes = ReadAndReverseIfNecessary(16);
                int[] parts = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    parts[i] = Il2CppSystem.BitConverter.ToInt32(bytes, i * 4);
                }
                return new Il2CppSystem.Decimal(parts);
            }

            /// <summary>
            /// Reads a null-terminated string from the stream with default encoding.
            /// </summary>
            /// <param name="stream"></param>
            /// <returns></returns>
            public string ReadNullTerminatedString()
            {
                return ReadNullTerminatedString(Il2CppSystem.Text.Encoding.Default);
            }

            /// <summary>
            /// Reads a null-terminated string from the <see cref="Stream"/> with the specified encoding.
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="encoding"></param>
            /// <returns></returns>
            /// <exception cref="ArgumentNullException"></exception>
            public string ReadNullTerminatedString(Il2CppSystem.Text.Encoding encoding)
            {
                var memoryStream = new Il2CppSystem.IO.MemoryStream();
                int currentByte;

                while ((currentByte = BaseStream.ReadByte()) != -1)
                {
                    if (currentByte == 0)
                    {
                        break;
                    }

                    memoryStream.WriteByte((byte)currentByte);
                }

                Il2CppStructArray<byte> bytes = memoryStream.ToArray();

                memoryStream.Dispose();

                return encoding.GetString(bytes);
            }
        }
    }
}