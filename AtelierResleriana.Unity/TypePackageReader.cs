using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    public class TypePackageReader
    {
        public TypePackage Read(Stream stream)
        {
            TypePackageHeader header = ReadHeader(stream);
            byte[] data = ReadData(stream, header);
            using Stream dataStream = new MemoryStream(data);
            TypePackageTypeTreeBlob typeTreeBlob = ReadTypeTreeBlob(dataStream);

            return new TypePackage()
            {
                Header = header,
                TypeTreeBlob = typeTreeBlob
            };
        }

        internal TypePackageHeader ReadHeader(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true, Endian.Little);

            uint magic = binaryReader.ReadUInt32();
            if (magic != TypePackageHeader.Magic)
                throw new InvalidDataException("Invalid TPK magic bytes");

            byte version = binaryReader.ReadByte();
            if (version != TypePackageHeader.Version)
                throw new InvalidDataException("Invalid TPK version");

            TypePackageCompressionType compressionType = (TypePackageCompressionType)binaryReader.ReadByte();
            TypePackageDataType dataType = (TypePackageDataType)binaryReader.ReadByte();
            binaryReader.ReadByte(); // padding
            binaryReader.ReadUInt32(); // unused
            uint compressedSize = binaryReader.ReadUInt32();
            uint uncompressedSize = binaryReader.ReadUInt32();

            return new TypePackageHeader()
            {
                CompressionType = compressionType,
                DataType = dataType,
                CompressedSize = compressedSize,
                UncompressedSize = uncompressedSize
            };
        }

        internal byte[] ReadData(Stream stream, TypePackageHeader typePackageHeader)
        {
            byte[] compressedBytes = stream.ReadBytes((int)typePackageHeader.CompressedSize);

            if (typePackageHeader.CompressionType != TypePackageCompressionType.None)
            {
                throw new NotImplementedException();
            }

            return compressedBytes;
        }

        internal TypePackageTypeTreeBlob ReadTypeTreeBlob(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true, Endian.Little);

            long creationTime = binaryReader.ReadInt64();
            var versions = ReadVersions(binaryReader);
            var classInformation = ReadClassInformation(binaryReader);
            var commonString = ReadCommonString(binaryReader);
            var nodeBuffer = ReadNodeBuffer(binaryReader);
            var stringBuffer = ReadStringBuffer(binaryReader);

            return new TypePackageTypeTreeBlob()
            {
                CreationTime = creationTime,
                Versions = versions.ToArray(),
                ClassInformation = classInformation,
                CommonString = commonString,
                NodeBuffer = nodeBuffer,
                StringBuffer = stringBuffer
            };
        }

        private List<UnityVersion> ReadVersions(BinaryReader binaryReader)
        {
            int versionCount = binaryReader.ReadInt32();
            var versions = new List<UnityVersion>(versionCount);

            for (int i = 0; i < versionCount; i++)
            {
                versions.Add(UnityVersion.FromBinaryReader(binaryReader));
            }

            return versions;
        }

        private Dictionary<int, TypePackageClassInformation> ReadClassInformation(BinaryReader binaryReader)
        {
            int classCount = binaryReader.ReadInt32();
            var classInformation = new Dictionary<int, TypePackageClassInformation>();

            for (int i = 0; i < classCount; i++)
            {
                int id = binaryReader.ReadInt32();
                int count = binaryReader.ReadInt32();

                var classes = new List<(UnityVersion Version, TypePackageUnityClass Class)>();
                for (int j = 0; j < count; j++)
                {
                    var version = UnityVersion.FromBinaryReader(binaryReader);
                    bool hasClass = binaryReader.ReadByte() != 0;

                    TypePackageUnityClass unityClass = null;
                    if (hasClass)
                    {
                        var name = binaryReader.ReadUInt16();
                        var baseClass = binaryReader.ReadUInt16();
                        var flags = (TypePackageUnityClassFlags)binaryReader.ReadByte();

                        unityClass = new TypePackageUnityClass
                        {
                            Name = name,
                            Base = baseClass,
                            Flags = flags
                        };

                        if ((flags & TypePackageUnityClassFlags.HasEditorRootNode) != 0)
                        {
                            unityClass.EditorRootNode = binaryReader.ReadUInt16();
                        }
                        if ((flags & TypePackageUnityClassFlags.HasReleaseRootNode) != 0)
                        {
                            unityClass.ReleaseRootNode = binaryReader.ReadUInt16();
                        }
                    }

                    classes.Add((version, unityClass));
                }

                classInformation.Add(id, new TypePackageClassInformation
                {
                    Id = id,
                    Classes = classes
                });
            }

            return classInformation;
        }

        private TypePackageCommonString ReadCommonString(BinaryReader binaryReader)
        {
            int versionCount = binaryReader.ReadInt32();
            var versionInformation = new List<(UnityVersion Version, byte Count)>(versionCount);

            for (int i = 0; i < versionCount; i++)
            {
                var version = UnityVersion.FromBinaryReader(binaryReader);
                byte count = binaryReader.ReadByte();
                versionInformation.Add((version, count));
            }

            int indicesCount = binaryReader.ReadInt32();
            var indices = new ushort[indicesCount];
            for (int i = 0; i < indicesCount; i++)
            {
                indices[i] = binaryReader.ReadUInt16();
            }

            return new TypePackageCommonString()
            {
                VersionInformation = versionInformation,
                StringBufferIndices = indices
            };
        }

        private TypePackageNodeBuffer ReadNodeBuffer(BinaryReader binaryReader)
        {
            int count = binaryReader.ReadInt32();
            var nodes = new TypePackageUnityNode[count];

            for (int i = 0; i < count; i++)
            {
                var typeName = binaryReader.ReadUInt16();
                var name = binaryReader.ReadUInt16();
                var byteSize = binaryReader.ReadInt32();
                var version = binaryReader.ReadInt16();
                var typeFlags = binaryReader.ReadByte();
                var metaFlag = binaryReader.ReadUInt32();
                var subNodeCount = binaryReader.ReadUInt16();

                var subNodes = new ushort[subNodeCount];
                for (int j = 0; j < subNodeCount; j++)
                {
                    subNodes[j] = binaryReader.ReadUInt16();
                }

                nodes[i] = new TypePackageUnityNode
                {
                    TypeName = typeName,
                    Name = name,
                    ByteSize = byteSize,
                    Version = version,
                    TypeFlags = typeFlags,
                    MetaFlag = metaFlag,
                    SubNodes = subNodes
                };
            }

            return new TypePackageNodeBuffer { Nodes = nodes };
        }

        private TypePackageStringBuffer ReadStringBuffer(BinaryReader binaryReader)
        {
            int count = binaryReader.ReadInt32();
            var strings = new string[count];

            for (int i = 0; i < count; i++)
            {
                strings[i] = ReadVarString(binaryReader.BaseStream);
            }

            return new TypePackageStringBuffer() { Strings = strings };
        }

        private string ReadVarString(Stream stream)
        {
            // Read varint length
            int shift = 0;
            int length = 0;
            while (true)
            {
                byte b = (byte)stream.ReadByte();
                length |= (b & 0x7F) << shift;
                shift += 7;
                if ((b & 0x80) == 0)
                    break;
            }

            // Read string data
            byte[] data = new byte[length];
            stream.Read(data, 0, length);
            return Encoding.UTF8.GetString(data);
        }
    }
}
