namespace AtelierResleriana.Unity
{
    public class SerializedObject
    {
        public int ClassId { get; set; }
        public Dictionary<string, object> Values { get; set; }

        public SerializedObject(int classId, Dictionary<string, object> values)
        {
            ClassId = classId;
            Values = values;
        }

        public object this[string key]
        {
            get
            {
                return Values[key];
            }
            set
            {
                Values[key] = value;
            }
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (Values.TryGetValue(key, out var value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                // Handle numeric type conversions
                if (typeof(T).IsPrimitive && value is IConvertible)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            }
            return defaultValue;
        }

        public bool HasValue(string key) => Values.ContainsKey(key);

        public T As<T>() where T : UnityObject, new()
        {
            return UnityObject.FromSerializedObject<T>(this);
        }

        public UnityObject ToUnityObject()
        {
            // Factory method to create the appropriate UnityObject type based on ClassID
            switch (ClassId)
            {
                case 49: // TextAsset
                    return As<TextAsset>();
                // Add other types as needed
                default:
                    throw new NotSupportedException($"ClassID {ClassId} is not supported");
            }
        }
    }
}
