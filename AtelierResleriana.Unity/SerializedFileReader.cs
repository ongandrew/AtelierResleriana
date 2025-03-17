using System.Drawing;
using System;
using System.IO;
using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    public class SerializedFileReader
    {
        public SerializedFile Read(Stream stream)
        {
            SerializedFileHeader header = ReadHeader(stream);
            SerializedFileMetadata metadata = ReadMetadata(stream, header);
            SerializedFileType[] types = ReadTypes(stream, header, metadata);
            SerializedFileObject[] objects = ReadObjects(stream, header, types);
            SerializedFileObjectReference[] scriptReferences = ReadScriptReferences(stream, header);
            SerializedFileAssetReference[] assetReferences = ReadAssetReferences(stream, header);
            SerializedFileType[] referenceTypes = ReadReferenceTypes(stream, header, metadata);
            string? userInformation = ReadUserInformation(stream, header);

            return new SerializedFile()
            {
                Header = header,
                Metadata = metadata,
                Types = types,
                Objects = objects,
                ScriptReferences = scriptReferences,
                AssetReferences = assetReferences,
                ReferenceTypes = referenceTypes,
                UserInformation = userInformation
            };
        }

        internal SerializedFileHeader ReadHeader(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, Endian.Big);

            var header = new SerializedFileHeader
            {
                MetadataSize = binaryReader.ReadUInt32(),
                FileSize = binaryReader.ReadUInt32(),
                Version = binaryReader.ReadUInt32(),
                DataOffset = binaryReader.ReadUInt32()
            };

            if (header.Version >= 9)
            {
                header.IsBigEndian = binaryReader.ReadBoolean();
                byte[] reserved = binaryReader.ReadBytes(3);
                if (header.Version >= 22)
                {
                    header.MetadataSize = binaryReader.ReadUInt32();
                    header.FileSize = binaryReader.ReadInt64();
                    header.DataOffset = binaryReader.ReadInt64();
                    _ = binaryReader.ReadInt64();
                }
            }
            else
            {
                // For older versions, endian flag is at end of metadata
                long currentPosition = stream.Position;
                stream.Position = header.FileSize - header.MetadataSize;
                header.IsBigEndian = binaryReader.ReadBoolean();
                stream.Position = currentPosition;
            }

            return header;
        }

        internal SerializedFileMetadata ReadMetadata(Stream stream, SerializedFileHeader header)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            SerializedFileMetadata serializedFileMetadata = new SerializedFileMetadata();

            if (header.Version >= 7)
            {
                serializedFileMetadata.UnityVersion = binaryReader.ReadNullTerminatedString(Encoding.UTF8);
            }

            if (header.Version >= 8)
            {
                serializedFileMetadata.TargetPlatform = binaryReader.ReadInt32();
            }

            if (header.Version >= 13)
            {
                serializedFileMetadata.EnableTypeTree = binaryReader.ReadBoolean();
            }

            return serializedFileMetadata;
        }

        internal SerializedFileType[] ReadTypes(Stream stream, SerializedFileHeader header, SerializedFileMetadata metadata)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            int typeCount = binaryReader.ReadInt32();

            SerializedFileType[] serializedFileTypes = new SerializedFileType[typeCount];

            for (int i = 0; i < typeCount; i++)
            {
                serializedFileTypes[i] = ReadType(stream, header, metadata, false);
            }

            return serializedFileTypes;
        }

        internal SerializedFileType ReadType(Stream stream, SerializedFileHeader header, SerializedFileMetadata metadata, bool isRefType)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            var type = new SerializedFileType
            {
                ClassId = binaryReader.ReadInt32()
            };

            if (header.Version >= 16)
            {
                type.IsStrippedType = binaryReader.ReadBoolean();
            }

            if (header.Version >= 17)
            {
                type.ScriptTypeIndex = binaryReader.ReadInt16();
            }

            if (header.Version >= 13)
            {
                if ((isRefType && type.ScriptTypeIndex >= 0) ||
                    (header.Version < 16 && type.ClassId < 0) ||
                    (header.Version >= 16 && type.ClassId == 114))
                {
                    type.ScriptId = binaryReader.ReadBytes(16);
                }
                type.OldTypeHash = binaryReader.ReadBytes(16);
            }

            if (metadata.EnableTypeTree)
            {
                if (header.Version >= 12 || header.Version == 10)
                {
                    type.Node = ReadTypeTreeNodeBlob(binaryReader, header.Version);
                }
                else
                {
                    type.Node = ReadTypeTreeNode(binaryReader, header.Version);
                }

                if (header.Version >= 21)
                {
                    if (isRefType)
                    {
                        type.ClassName = binaryReader.ReadNullTerminatedString(Encoding.UTF8);
                        type.Namespace = binaryReader.ReadNullTerminatedString(Encoding.UTF8);
                        type.AssemblyName = binaryReader.ReadNullTerminatedString(Encoding.UTF8);
                    }
                    else
                    {
                        int dependencyCount = binaryReader.ReadInt32();
                        type.Dependencies = new int[dependencyCount];
                        for (int i = 0; i < dependencyCount; i++)
                        {
                            type.Dependencies[i] = binaryReader.ReadInt32();
                        }
                    }
                }
            }

            return type;
        }

        internal SerializedFileTypeTreeNode ReadTypeTreeNode(BinaryReader binaryReader, uint version)
        {
            SerializedFileTypeTreeNode node = new SerializedFileTypeTreeNode
            {
                Type = binaryReader.ReadNullTerminatedString(Encoding.UTF8),
                Name = binaryReader.ReadNullTerminatedString(Encoding.UTF8),
                Size = binaryReader.ReadInt32(),
                VariableCount = version == 2 ? binaryReader.ReadInt32() : null,
                Index = version != 3 ? binaryReader.ReadInt32() : null,
                TypeFlags = binaryReader.ReadInt32(),
                Version = binaryReader.ReadInt32(),
                MetaFlag = version != 3 ? binaryReader.ReadUInt32() : null,
            };

            int childCount = binaryReader.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                var child = ReadTypeTreeNode(binaryReader, version);
                child.Level = node.Level + 1;
                node.Children.Add(child);
            }

            return node;
        }

        internal struct BlobNode
        {
            public short Version { get; set; }
            public byte Level { get; set; }
            public byte TypeFlags { get; set; }
            public uint TypeStringOffset { get; set; }
            public uint NameStringOffset { get; set; }
            public int ByteSize { get; set; }
            public int Index { get; set; }
            public int MetaFlag { get; set; }
            public ulong RefTypeHash { get; set; }
        }

        internal SerializedFileTypeTreeNode ReadTypeTreeNodeBlob(BinaryReader binaryReader, uint version)
        {
            var nodeCount = binaryReader.ReadInt32();
            var stringBufferSize = binaryReader.ReadInt32();

            // Read all node data at once
            var nodeStructSize = 24 + (version >= 19 ? 8 : 0);
            var nodeData = binaryReader.ReadBytes(nodeCount * nodeStructSize);
            var stringBuffer = binaryReader.ReadBytes(stringBufferSize);

            var commonStrings = new Dictionary<int, string>();
            int offset = 0;
            foreach (var str in TypePackage.Instance.TypeTreeBlob.CommonString.GetStrings(TypePackage.Instance.TypeTreeBlob.StringBuffer))
            {
                commonStrings[offset] = str;
                offset += Encoding.UTF8.GetByteCount(str) + 1; // +1 for null terminator
            }

            var nodes = new List<SerializedFileTypeTreeNode>(nodeCount);
            using var nodeReader = new BinaryReader(new MemoryStream(nodeData), Encoding.UTF8, true, binaryReader.Endian);
            using var stringReader = new BinaryReader(new MemoryStream(stringBuffer), Encoding.UTF8, true, binaryReader.Endian);

            // Read all nodes
            for (int i = 0; i < nodeCount; i++)
            {
                var node = new BlobNode
                {
                    Version = nodeReader.ReadInt16(),
                    Level = nodeReader.ReadByte(),
                    TypeFlags = nodeReader.ReadByte(),
                    TypeStringOffset = nodeReader.ReadUInt32(),
                    NameStringOffset = nodeReader.ReadUInt32(),
                    ByteSize = nodeReader.ReadInt32(),
                    Index = nodeReader.ReadInt32(),
                    MetaFlag = nodeReader.ReadInt32()
                };

                if (version >= 19)
                {
                    node.RefTypeHash = nodeReader.ReadUInt64();
                }

                SerializedFileTypeTreeNode serializedFileTypeTreeNode = new SerializedFileTypeTreeNode()
                {
                    Version = node.Version,
                    Level = node.Level,
                    TypeFlags = node.TypeFlags,
                    Size = node.ByteSize,
                    Index = node.Index,
                    MetaFlag = (uint)node.MetaFlag,
                    Type = ReadString(node.TypeStringOffset, stringReader, commonStrings),
                    Name = ReadString(node.NameStringOffset, stringReader, commonStrings)
                };

                nodes.Add(serializedFileTypeTreeNode);
            }

            // Build tree structure
            var fakeRoot = new SerializedFileTypeTreeNode { Level = -1 };
            var stack = new List<SerializedFileTypeTreeNode> { fakeRoot };
            var parent = fakeRoot;
            var prev = fakeRoot;

            foreach (var node in nodes)
            {
                if (node.Level > prev.Level)
                {
                    stack.Add(parent);
                    parent = prev;
                }
                else if (node.Level < prev.Level)
                {
                    while (node.Level <= parent.Level)
                    {
                        parent = stack[stack.Count - 1];
                        stack.RemoveAt(stack.Count - 1);
                    }
                }

                parent.Children.Add(node);
                prev = node;
            }
            return fakeRoot.Children[0];
        }

        private string ReadString(uint value, BinaryReader stringReader, Dictionary<int, string> commonStrings)
        {
            bool isOffset = (value & 0x80000000) == 0;
            if (isOffset)
            {
                long originalPosition = stringReader.BaseStream.Position;
                stringReader.BaseStream.Position = value;
                string result = stringReader.ReadNullTerminatedString(Encoding.UTF8);
                stringReader.BaseStream.Position = originalPosition;
                return result;
            }

            int offset = (int)(value & 0x7FFFFFFF);
            return commonStrings.TryGetValue(offset, out var str) ? str : offset.ToString();
        }

        internal SerializedFileObject[] ReadObjects(Stream stream, SerializedFileHeader header, SerializedFileType[] types)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            int bigIdEnabled = 0;
            if (7 <= header.Version && header.Version < 14)
            {
                bigIdEnabled = binaryReader.ReadInt32();
            }

            int objectCount = binaryReader.ReadInt32();
            SerializedFileObject[] objects = new SerializedFileObject[objectCount];
            for (int i = 0; i < objectCount; i++)
            {
                objects[i] = ReadObject(binaryReader, header, types, bigIdEnabled);
            }
            return objects;
        }

        internal SerializedFileObject ReadObject(BinaryReader binaryReader, SerializedFileHeader header, SerializedFileType[] types, int isBigIdEnabled)
        {
            var @object = new SerializedFileObject();

            if (header.Version < 14)
            {
                @object.PathId = binaryReader.ReadInt32();
            }
            else
            {
                binaryReader.Align(4);
                @object.PathId = binaryReader.ReadInt64();
            }

            if (header.Version >= 22)
            {
                @object.Offset = binaryReader.ReadInt64();
            }
            else
            {
                @object.Offset = binaryReader.ReadUInt32();
            }

            // Read Size and Type info
            @object.Size = binaryReader.ReadUInt32();
            @object.TypeId = binaryReader.ReadInt32();

            // Read ClassID
            if (header.Version < 16)
            {
                @object.ClassId = binaryReader.ReadUInt16();
                foreach (var type in types)
                {
                    if (type.ClassId == @object.ClassId)
                    {
                        @object.Type = type;
                        break;
                    }
                 }
            }
            else
            {
                var type = types[@object.TypeId];
                @object.Type = type;
                @object.ClassId = type.ClassId;
            }

            // Read IsDestroyed for old versions
            if (header.Version < 11)
            {
                @object.IsDestroyed = binaryReader.ReadUInt16();
            }

            // Script type index for versions 11-16
            if (header.Version >= 11 && header.Version < 17)
            {
                short scriptTypeIndex = binaryReader.ReadInt16();
                if (@object.Type != null)
                {
                    @object.Type.ScriptTypeIndex = scriptTypeIndex;
                }
            }

            if (header.Version == 15 || header.Version == 16)
            {
                @object.IsStripped = binaryReader.ReadByte();
            }

            @object.Data = ReadObjectData(binaryReader.BaseStream, header, @object.Offset, @object.Size);

            return @object;
        }

        internal byte[] ReadObjectData(Stream stream, SerializedFileHeader header, long offset, uint size)
        {
            // Save current position
            long currentPosition = stream.Position;

            // Calculate absolute position of object data
            long dataStartPosition = header.DataOffset + offset;

            // Seek to data position
            stream.Position = dataStartPosition;

            // Read the raw bytes
            byte[] bytes = new byte[size];
            int readBytes = stream.Read(bytes);

            if (readBytes != size)
            {
                throw new InvalidOperationException("Could not read the required number of bytes.");
            }

            // Restore position
            stream.Position = currentPosition;

            return bytes;
        }

        internal SerializedFileObjectReference[] ReadScriptReferences(Stream stream, SerializedFileHeader header)
        {
            if (header.Version < 11)
            {
                return Array.Empty<SerializedFileObjectReference>();
            }

            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            int scriptReferenceCount = binaryReader.ReadInt32();
            var scriptReferences = new SerializedFileObjectReference[scriptReferenceCount];

            for (int i = 0; i < scriptReferenceCount; i++)
            {
                var scriptReference = new SerializedFileObjectReference
                {
                    Index = binaryReader.ReadInt32()
                };

                if (header.Version < 14)
                {
                    scriptReference.PathId = binaryReader.ReadInt32();
                }
                else
                {
                    binaryReader.Align(4);
                    scriptReference.PathId = binaryReader.ReadInt64();
                }

                scriptReferences[i] = scriptReference;
            }

            return scriptReferences;
        }

        internal SerializedFileAssetReference[] ReadAssetReferences(Stream stream, SerializedFileHeader header)
        {
            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            int assetReferenceCount = binaryReader.ReadInt32();
            var assetReferences = new SerializedFileAssetReference[assetReferenceCount];

            for (int i = 0; i < assetReferenceCount; i++)
            {
                var assetReference = new SerializedFileAssetReference();

                if (header.Version >= 6)
                {
                    // Should be empty.
                    binaryReader.ReadNullTerminatedString(Encoding.UTF8);
                }

                if (header.Version >= 5)
                {
                    assetReference.Guid = new Guid(binaryReader.ReadBytes(16));
                    assetReference.Type = binaryReader.ReadInt32();
                }

                assetReference.Path = binaryReader.ReadNullTerminatedString(Encoding.UTF8);

                assetReferences[i] = assetReference;
            }

            return assetReferences;
        }

        internal SerializedFileType[] ReadReferenceTypes(Stream stream, SerializedFileHeader header, SerializedFileMetadata metadata)
        {
            if (header.Version < 20)
            {
                return Array.Empty<SerializedFileType>();
            }

            BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8, true, header.IsBigEndian ? Endian.Big : Endian.Little);

            int referenceTypeCount = binaryReader.ReadInt32();
            SerializedFileType[] referenceTypes = new SerializedFileType[referenceTypeCount];

            for (int i = 0; i < referenceTypeCount; i++)
            {
                referenceTypes[i] = ReadType(stream, header, metadata, true);
            }

            return referenceTypes;
        }

        internal string? ReadUserInformation(Stream stream, SerializedFileHeader header)
        {
            if (header.Version < 5)
            {
                return null;
            }

            return stream.ReadNullTerminatedString(Encoding.UTF8);
        }
    }
}
