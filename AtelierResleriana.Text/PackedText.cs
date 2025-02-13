using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace AtelierResleriana.Text
{
    public class PackedText
    {
        public IReadOnlyList<Property> Properties { get => mProperties.AsReadOnly(); }
        public IReadOnlyList<Dictionary<uint, object>> Entries { get => mEntries.AsReadOnly(); }
        private List<Dictionary<uint, object>> mEntries { get; set; } = new List<Dictionary<uint, object>>();
        private List<Property> mProperties { get; set; } = new List<Property>();

        public record class Property
        {
            public uint Id { get; set; }
            public uint Type { get; set; }
        }

        public PackedText() { }
        public PackedText(byte[] bytes) : this(new MemoryStream(bytes)) { }
        public PackedText(Stream stream) : this()
        {
            using MemoryStream memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            (mEntries, mProperties) = UnpackTextAsset(memoryStream.ToArray());
        }

        public void AddProperty(uint id, uint type)
        {
            mProperties.Add(new Property()
            {
                Id = id,
                Type = type
            });
        }

        public void AddEntry(Dictionary<uint, object> entry)
        {
            mEntries.Add(entry);
        }

        public Dictionary<uint, object> GetEntry(int index)
        {
            if (index < 0 || index >= mEntries.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range.");
            }
            return mEntries[index];
        }

        public void SetEntry(int index, Dictionary<uint, object> newEntry)
        {
            if (index < 0 || index >= mEntries.Count)
            {
                throw new ArgumentOutOfRangeException("Index is out of range.");
            }

            foreach (var prop in mProperties)
            {
                if (!newEntry.ContainsKey(prop.Id))
                {
                    throw new ArgumentException("New entry is missing required property: " + prop.Id);
                }
            }
            mEntries[index] = newEntry;
        }

        public object GetValue(int entryIndex, uint propertyId)
        {
            var entry = GetEntry(entryIndex);
            if (!entry.ContainsKey(propertyId))
            {
                throw new ArgumentException($"Property with ID {propertyId} not found in entry.");
            }
            return entry[propertyId];
        }

        public T GetValue<T>(int entryIndex, uint propertyId)
        {
            object value = GetValue(entryIndex, propertyId);
            return (T)value;
        }

        public void SetValue(int entryIndex, uint propertyId, object newValue)
        {
            var entry = GetEntry(entryIndex);
            if (!entry.ContainsKey(propertyId))
            {
                throw new ArgumentException($"Property with ID {propertyId} not found in entry.");
            }

            var propertyType = mProperties.FirstOrDefault(p => p.Id == propertyId)?.Type;
            if (propertyType == null)
            {
                throw new ArgumentException($"Property with ID {propertyId} not defined.");
            }

            entry[propertyId] = newValue;
        }

        public byte[] ToBytes()
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);

            binaryWriter.Write((uint)mProperties.Count);
            binaryWriter.Write((uint)mEntries.Count);

            foreach (var prop in mProperties)
            {
                binaryWriter.Write(prop.Type);
                binaryWriter.Write(prop.Id);
            }

            foreach (var entry in mEntries)
            {
                foreach (var prop in mProperties)
                {
                    var value = entry[prop.Id];
                    switch (prop.Type)
                    {
                        case PropertyTypes.UnsignedInteger:
                            binaryWriter.Write(Convert.ToUInt32(value));
                            break;

                        case PropertyTypes.UnsignedLong:
                            binaryWriter.Write(Convert.ToUInt64(value));
                            break;

                        case PropertyTypes.String:
                            var strValue = value.ToString();
                            var strBytes = Encoding.UTF8.GetBytes(strValue);
                            binaryWriter.Write((uint)strBytes.Length);
                            binaryWriter.Write(strBytes);
                            break;
                    }
                }
            }

            return memoryStream.ToArray();
        }

        public string ToJson()
        {
            var packedTextJson = new
            {
                Properties = mProperties.Select(p => new { Id = p.Id, Type = p.Type }).ToList(),
                Entries = mEntries
            };

            return JsonSerializer.Serialize(packedTextJson, new JsonSerializerOptions { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            });
        }

        private static (List<Dictionary<uint, object>> entries, List<Property> properties) UnpackTextAsset(byte[] data)
        {
            var entries = new List<Dictionary<uint, object>>();
            var properties = new List<Property>();
            int offset = 0;

            uint propertyCount = BitConverter.ToUInt32(data, offset);
            offset += 4;
            uint entryCount = BitConverter.ToUInt32(data, offset);
            offset += 4;

            for (int i = 0; i < propertyCount; i++)
            {
                var prop = new Property
                {
                    Type = BitConverter.ToUInt32(data, offset),
                    Id = BitConverter.ToUInt32(data, offset + 4)
                };
                offset += 8;
                properties.Add(prop);
            }

            for (int i = 0; i < entryCount; i++)
            {
                var entry = new Dictionary<uint, object>();

                foreach (var prop in properties)
                {
                    switch (prop.Type)
                    {
                        case 0: // int32
                            entry[prop.Id] = BitConverter.ToUInt32(data, offset);
                            offset += 4;
                            break;

                        case 1: // int64
                            entry[prop.Id] = BitConverter.ToUInt64(data, offset);
                            offset += 8;
                            break;

                        case 2: // string
                            uint strLen = BitConverter.ToUInt32(data, offset);
                            offset += 4;
                            entry[prop.Id] = Encoding.UTF8.GetString(data, offset, (int)strLen);
                            offset += (int)strLen;
                            break;
                    }
                }
                entries.Add(entry);
            }

            return (entries, properties);
        }
    }
}
