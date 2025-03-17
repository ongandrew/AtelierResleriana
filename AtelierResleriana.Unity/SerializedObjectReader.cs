using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    public class SerializedObjectReader
    {
        private enum NodeDataType
        {
            U8 = 0,
            U16 = 1,
            U32 = 2,
            U64 = 3,
            S8 = 4,
            S16 = 5,
            S32 = 6,
            S64 = 7,
            F32 = 8,
            F64 = 9,
            Boolean = 10,
            String = 11,
            Bytes = 12,
            Pair = 13,
            Array = 14,
            PPtr = 15,
            ReferencedObject = 16,
            ReferencedObjectData = 17,
            ManagedReferencesRegistry = 18,
            Unknown = 255
        }

        private const int kAlignBytes = 0x4000;
        private long OriginalOffset { get; set; }
        private static readonly HashSet<NodeDataType> SupportedArrayTypes = new()
        {
            NodeDataType.U8, NodeDataType.U16, NodeDataType.U32, NodeDataType.U64,
            NodeDataType.S8, NodeDataType.S16, NodeDataType.S32, NodeDataType.S64,
            NodeDataType.F32, NodeDataType.F64, NodeDataType.Boolean, NodeDataType.Pair
        };

        public SerializedObject Read(Stream stream, SerializedFileHeader header, SerializedFileObject @object)
        {
            var objectData = new MemoryStream(@object.Data);
            OriginalOffset = @object.Offset;
            var reader = new BinaryReader(objectData, Encoding.ASCII, true, header.IsBigEndian ? Endian.Big : Endian.Little);
            var values = ReadTypeTreeNode(reader, header, @object.Type.Node, @object.Size);
            return new SerializedObject(@object.ClassId, values);
        }

        private void AlignRelative(BinaryReader binaryReader, int alignment)
        {
            var currentPosition = OriginalOffset + binaryReader.BaseStream.Position;
            var mod = currentPosition % alignment;
            if (mod != 0)
            {
                binaryReader.BaseStream.Position += alignment - mod;
            }
        }

        private NodeDataType GetNodeDataType(string type)
        {
            if (string.IsNullOrEmpty(type)) return NodeDataType.Unknown;

            if (type.StartsWith("PPtr<", StringComparison.OrdinalIgnoreCase))
                return NodeDataType.PPtr;

            return type.ToLowerInvariant() switch
            {
                "sint8" or "char" => NodeDataType.S8,
                "uint8" or "byte" => NodeDataType.U8,
                "sint16" or "short" => NodeDataType.S16,
                "uint16" or "unsigned short" => NodeDataType.U16,
                "sint32" or "int" => NodeDataType.S32,
                "uint32" or "unsigned int" or "type*" => NodeDataType.U32,
                "sint64" or "long long" => NodeDataType.S64,
                "uint64" or "unsigned long long" or "filesize" => NodeDataType.U64,
                "float" => NodeDataType.F32,
                "double" => NodeDataType.F64,
                "bool" => NodeDataType.Boolean,
                "string" => NodeDataType.String,
                "typelessdata" => NodeDataType.Bytes,
                "pair" => NodeDataType.Pair,
                "array" => NodeDataType.Array,
                "referencedobject" => NodeDataType.ReferencedObject,
                "referencedobjectdata" => NodeDataType.ReferencedObjectData,
                "managedreferencesregistry" => NodeDataType.ManagedReferencesRegistry,
                _ => NodeDataType.Unknown
            };
        }

        private object ReadPrimitiveArray(BinaryReader reader, SerializedFileTypeTreeNode node, NodeDataType dataType, int count)
        {
            switch (dataType)
            {
                case NodeDataType.Boolean:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadBoolean()).ToList();
                case NodeDataType.U8:
                    return reader.ReadBytes(count).ToList();
                case NodeDataType.S8:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadSByte()).ToList();
                case NodeDataType.U16:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadUInt16()).ToList();
                case NodeDataType.S16:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadInt16()).ToList();
                case NodeDataType.U32:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadUInt32()).ToList();
                case NodeDataType.S32:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadInt32()).ToList();
                case NodeDataType.U64:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadUInt64()).ToList();
                case NodeDataType.S64:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadInt64()).ToList();
                case NodeDataType.F32:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadSingle()).ToList();
                case NodeDataType.F64:
                    return Enumerable.Range(0, count).Select(_ => reader.ReadDouble()).ToList();
                case NodeDataType.Pair:
                    if (node.Children?.Count != 2)
                        throw new InvalidDataException("Pair node must have 2 children");
                    return Enumerable.Range(0, count).Select(_ => ReadPair(reader, node)).ToList();
                default:
                    throw new InvalidDataException($"Unsupported array type: {dataType}");
            }
        }

        private (object, object) ReadPair(BinaryReader reader, SerializedFileTypeTreeNode node)
        {
            var first = ReadTypeTreeValue(reader, node.Children[0]);
            var second = ReadTypeTreeValue(reader, node.Children[1]);
            return (first, second);
        }

        private Dictionary<string, object> ReadTypeTreeNode(BinaryReader reader, SerializedFileHeader header, SerializedFileTypeTreeNode node, long remainingBytes)
        {
            var result = new Dictionary<string, object>();
            long startPos = reader.BaseStream.Position;

            bool needsAlign = (node.MetaFlag & kAlignBytes) != 0;
            try
            {
                if (node.Children?.Count > 0)
                {
                    var firstChild = node.Children[0];
                    var nodeDataType = GetNodeDataType(firstChild.Type);

                    if (nodeDataType == NodeDataType.Array)
                    {
                        if (firstChild.Children?.Count != 2)
                            throw new InvalidDataException("Array node must have 2 children");

                        needsAlign = (firstChild.MetaFlag & kAlignBytes) != 0;

                        int length = reader.ReadInt32();
                        if (length < 0)
                            throw new InvalidDataException($"Invalid array length: {length}");

                        var elementNode = firstChild.Children[1];
                        var elementType = GetNodeDataType(elementNode.Type);

                        if (SupportedArrayTypes.Contains(elementType))
                        {
                            result[node.Name] = ReadPrimitiveArray(reader, elementNode, elementType, length);
                        }
                        else
                        {
                            var array = new List<object>();
                            for (int i = 0; i < length; i++)
                            {
                                array.Add(ReadTypeTreeValue(reader, elementNode));
                            }
                            result[node.Name] = array;
                        }
                    }
                    else
                    {
                        foreach (var child in node.Children)
                        {
                            result[child.Name] = ReadTypeTreeValue(reader, child);
                        }
                    }
                }

                if (needsAlign)
                {
                    AlignRelative(reader, 4);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error reading node {node.Name} of type {node.Type} at position {startPos}",
                    ex);
            }
        }

        private object ReadTypeTreeValue(BinaryReader reader, SerializedFileTypeTreeNode node)
        {
            var dataType = GetNodeDataType(node.Type);
            bool needsAlign = (node.MetaFlag & kAlignBytes) != 0;

            object value = dataType switch
            {
                NodeDataType.Boolean => reader.ReadBoolean(),
                NodeDataType.S8 => reader.ReadSByte(),
                NodeDataType.U8 => reader.ReadByte(),
                NodeDataType.S16 => reader.ReadInt16(),
                NodeDataType.U16 => reader.ReadUInt16(),
                NodeDataType.S32 => reader.ReadInt32(),
                NodeDataType.U32 => reader.ReadUInt32(),
                NodeDataType.S64 => reader.ReadInt64(),
                NodeDataType.U64 => reader.ReadUInt64(),
                NodeDataType.F32 => reader.ReadSingle(),
                NodeDataType.F64 => reader.ReadDouble(),
                NodeDataType.String => ReadString(reader),
                NodeDataType.Bytes => ReadBytes(reader),
                NodeDataType.Pair => ReadPair(reader, node),
                _ => ReadTypeTreeNode(reader, null, node, reader.BaseStream.Length - reader.BaseStream.Position)
            };

            if (needsAlign)
            {
                AlignRelative(reader, 4);
            }

            return value;
        }

        private byte[] ReadString(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length < 0)
                throw new InvalidDataException($"Invalid string length: {length}");

            byte[] bytes = reader.ReadBytes(length);

            AlignRelative(reader, 4);

            return bytes;
        }

        private byte[] ReadBytes(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length < 0)
                throw new InvalidDataException($"Invalid bytes length: {length}");

            return reader.ReadBytes(length);
        }
    }
}