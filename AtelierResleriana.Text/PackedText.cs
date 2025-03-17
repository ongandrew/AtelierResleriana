using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}
