using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AtelierResleriana.Text
{
    public class PackedTextReader
    {
        public PackedText Read(byte[] bytes)
        {
            using Stream stream = new MemoryStream(bytes);
            return Read(stream);
        }

        public PackedText Read(Stream stream)
        {
            var packedText = new PackedText();
            var (entries, properties) = UnpackTextAsset(stream);
            foreach (var property in properties)
            {
                packedText.AddProperty(property.Id, property.Type);
            }
            foreach (var entry in entries)
            {
                packedText.AddEntry(entry);
            }
            return packedText;
        }

        public bool IsValid(byte[] bytes)
        {
            using Stream stream = new MemoryStream(bytes);
            return IsValid(stream);
        }

        public bool IsValid(Stream stream)
        {
            long originalPosition = stream.Position;

            if (stream.Length < 8)
            {
                return false;
            }

            using var reader = new BinaryReader(stream, Encoding.ASCII, true);
            uint propertyCount = reader.ReadUInt32();
            uint entryCount = reader.ReadUInt32();

            if (propertyCount == 0)
            {
                return false;
            }

            try
            {
                long requiredBytes = 8 + (propertyCount * 8);
                if (stream.Length < requiredBytes)
                {
                    return false;
                }

                for (int i = 0; i < propertyCount; i++)
                {
                    uint type = reader.ReadUInt32();
                    uint id = reader.ReadUInt32();

                    if (type > 2)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        private static (List<Dictionary<uint, object>> entries, List<PackedText.Property> properties) UnpackTextAsset(Stream stream)
        {
            var entries = new List<Dictionary<uint, object>>();
            var properties = new List<PackedText.Property>();
            var buffer = new byte[8]; // Reusable buffer for reading integers

            // Read property count
            ValidateAndRead(stream, buffer, 4);
            uint propertyCount = BitConverter.ToUInt32(buffer, 0);

            // Read entry count
            ValidateAndRead(stream, buffer, 4);
            uint entryCount = BitConverter.ToUInt32(buffer, 0);

            if (propertyCount == 0 && entryCount > 0)
            {
                throw new FormatException("Invalid number of properties on packed text.");
            }

            // Read properties
            for (int i = 0; i < propertyCount; i++)
            {
                ValidateAndRead(stream, buffer, 8);
                var prop = new PackedText.Property
                {
                    Type = BitConverter.ToUInt32(buffer, 0),
                    Id = BitConverter.ToUInt32(buffer, 4)
                };
                properties.Add(prop);
            }

            // Read entries
            for (int i = 0; i < entryCount; i++)
            {
                var entry = new Dictionary<uint, object>();
                foreach (var prop in properties)
                {
                    switch (prop.Type)
                    {
                        case 0: // int32
                            ValidateAndRead(stream, buffer, 4);
                            entry[prop.Id] = BitConverter.ToUInt32(buffer, 0);
                            break;

                        case 1: // int64
                            ValidateAndRead(stream, buffer, 8);
                            entry[prop.Id] = BitConverter.ToUInt64(buffer, 0);
                            break;

                        case 2: // string
                            ValidateAndRead(stream, buffer, 4);
                            uint strLen = BitConverter.ToUInt32(buffer, 0);

                            if (strLen > stream.Length - stream.Position)
                            {
                                throw new EndOfStreamException($"String length {strLen} exceeds remaining stream length {stream.Length - stream.Position}.");
                            }

                            var stringBuffer = new byte[strLen];
                            ValidateAndRead(stream, stringBuffer, (int)strLen);
                            entry[prop.Id] = Encoding.UTF8.GetString(stringBuffer);
                            break;

                        default:
                            throw new InvalidDataException($"Unknown property type: {prop.Type}");
                    }
                }
                entries.Add(entry);
            }

            return (entries, properties);
        }

        private static void ValidateAndRead(Stream stream, byte[] buffer, int count)
        {
            if (stream.Position + count > stream.Length)
            {
                throw new EndOfStreamException($"Attempted to read {count} bytes but only {stream.Length - stream.Position} bytes remain");
            }

            int bytesRead = stream.Read(buffer, 0, count);
            if (bytesRead != count)
            {
                throw new EndOfStreamException($"Expected to read {count} bytes but only read {bytesRead} bytes");
            }
        }
    }
}