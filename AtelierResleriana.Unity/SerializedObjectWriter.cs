using System.Text;
using Universal.Common;

namespace AtelierResleriana.Unity
{
    public class SerializedObjectWriter
    {
        private const int kAlignBytes = 0x4000;
        public long BaseOffset { get; set; }

        public bool IsBigEndian { get; set; }

        public SerializedObjectWriter() : this(new Options()) { }

        public SerializedObjectWriter(Options options)
        {
            IsBigEndian = options.IsBigEndian;
            BaseOffset = options.BaseOffset;
        }

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

        public Stream Write(SerializedObject serializedObject, SerializedFileTypeTreeNode rootNode)
        {
            Stream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true, IsBigEndian ? Endian.Big : Endian.Little);

            WriteTypeTreeNode(writer, rootNode, serializedObject.Values);

            stream.Seek(0, SeekOrigin.Begin);
            return stream;
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

        private void WriteTypeTreeNode(BinaryWriter writer, SerializedFileTypeTreeNode node, Dictionary<string, object> values)
        {
            bool needsAlign = (node.MetaFlag & kAlignBytes) != 0;
            long startPosition = writer.BaseStream.Position;

            try
            {
                if (node.Children?.Count > 0)
                {
                    var firstChild = node.Children[0];

                    if (firstChild.Type == "Array")
                    {
                        if (firstChild.Children?.Count != 2)
                            throw new InvalidDataException("Array node must have 2 children");

                        if (!values.TryGetValue(node.Name, out var arrayValue))
                            throw new InvalidDataException($"Array value not found for node {node.Name}");

                        System.Collections.IList? enumerable = arrayValue as System.Collections.IList;
                        if (enumerable == null)
                            throw new InvalidDataException($"Value is not an array/list type: {arrayValue?.GetType()}");

                        // Write array length
                        writer.Write(enumerable.Count);

                        var elementNode = firstChild.Children[1];
                        bool elementNeedsAlign = (elementNode.MetaFlag & kAlignBytes) != 0;

                        foreach (var item in enumerable)
                        {
                            WriteTypeTreeValue(writer, elementNode, item);

                            // Add array element alignment if needed
                            if (elementNeedsAlign)
                            {
                                AlignRelative(writer, 4);
                            }
                        }

                        // Add array alignment after all elements
                        if ((firstChild.MetaFlag & kAlignBytes) != 0)
                        {
                            AlignRelative(writer, 4);
                        }
                    }
                    else
                    {
                        foreach (var child in node.Children)
                        {
                            if (values.TryGetValue(child.Name, out var value))
                            {
                                WriteTypeTreeValue(writer, child, value);
                                if ((child.MetaFlag & kAlignBytes) != 0)
                                {
                                    AlignRelative(writer, 4);
                                }
                            }
                        }
                    }
                }

                if (needsAlign)
                {
                    AlignRelative(writer, 4);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error writing node {node.Name} of type {node.Type}",
                    ex);
            }
        }

        private void WriteTypeTreeValue(BinaryWriter writer, SerializedFileTypeTreeNode node, object value)
        {
            if (value == null)
                return;

            var dataType = GetNodeDataType(node.Type);
            bool needsAlign = (node.MetaFlag & kAlignBytes) != 0;
            long startPosition = writer.BaseStream.Position;

            switch (dataType)
            {
                case NodeDataType.Boolean:
                    writer.Write((bool)value);
                    break;
                case NodeDataType.S8:
                    writer.Write((sbyte)value);
                    break;
                case NodeDataType.U8:
                    writer.Write((byte)value);
                    break;
                case NodeDataType.S16:
                    writer.Write((short)value);
                    break;
                case NodeDataType.U16:
                    writer.Write((ushort)value);
                    break;
                case NodeDataType.S32:
                    writer.Write((int)value);
                    break;
                case NodeDataType.U32:
                    writer.Write((uint)value);
                    break;
                case NodeDataType.S64:
                    writer.Write((long)value);
                    break;
                case NodeDataType.U64:
                    writer.Write((ulong)value);
                    break;
                case NodeDataType.F32:
                    writer.Write((float)value);
                    break;
                case NodeDataType.F64:
                    writer.Write((double)value);
                    break;
                case NodeDataType.String:
                    WriteString(writer, (byte[])value);
                    break;
                case NodeDataType.Bytes:
                    WriteBytes(writer, (byte[])value);
                    break;
                case NodeDataType.Pair:
                    WritePair(writer, node, ((object First, object Second))value);
                    break;
                default:
                    if (value is Dictionary<string, object> dict)
                    {
                        WriteTypeTreeNode(writer, node, dict);
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported type: {value.GetType()} for node type {node.Type}");
                    }
                    break;
            }

            // Ensure proper alignment of the value if needed
            if (needsAlign)
            {
                long endPosition = writer.BaseStream.Position;
                long valueSize = endPosition - startPosition;
                if (valueSize % 4 != 0)
                {
                    AlignRelative(writer, 4);
                }
            }
        }

        private void WriteString(BinaryWriter writer, byte[] bytes)
        {
            writer.Write(bytes.Length);
            writer.Write(bytes);
            AlignRelative(writer, 4);
        }

        private void WriteBytes(BinaryWriter writer, byte[] bytes)
        {
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        private void WritePair(BinaryWriter writer, SerializedFileTypeTreeNode node, (object First, object Second) pair)
        {
            if (node.Children?.Count != 2)
                throw new InvalidDataException("Pair node must have 2 children");

            WriteTypeTreeValue(writer, node.Children[0], pair.First);
            WriteTypeTreeValue(writer, node.Children[1], pair.Second);
        }

        private void AlignRelative(BinaryWriter writer, int alignment)
        {
            var currentPosition = BaseOffset + writer.BaseStream.Position;
            var mod = currentPosition % alignment;
            if (mod != 0)
            {
                var padding = alignment - mod;
                for (int i = 0; i < padding; i++)
                {
                    writer.Write((byte)0);
                }
            }
        }

        public class Options
        {
            public bool IsBigEndian { get; set; } = false;
            public long BaseOffset { get; set; } = 0;
        }
    }
}