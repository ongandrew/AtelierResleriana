namespace AtelierResleriana.Unity
{
    public class UnityVersion
    {
        public ulong Data { get; set; }

        private UnityVersion(ulong data)
        {
            Data = data;
        }

        public static UnityVersion FromBinaryReader(BinaryReader reader)
        {
            ulong data = reader.ReadUInt64();
            return new UnityVersion(data);
        }

        public static UnityVersion FromString(string version)
        {
            var parts = version.Split('.');
            return FromList(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3])
            );
        }

        public static UnityVersion FromList(int major = 0, int minor = 0, int patch = 0, int build = 0)
        {
            ulong data = ((ulong)major << 48) | ((ulong)minor << 32) | ((ulong)patch << 16) | (ulong)build;
            return new UnityVersion(data);
        }

        public int Major => (int)(Data >> 48) & 0xFFFF;
        public int Minor => (int)(Data >> 32) & 0xFFFF;
        public int Build => (int)(Data >> 16) & 0xFFFF;
        public UnityVersionType Type => (UnityVersionType)((Data >> 8) & 0xFF);
        public int TypeNumber => (int)Data & 0xFF;

        public override string ToString()
        {
            return $"UnityVersion {Major}.{Minor}.{Build}.{TypeNumber}";
        }

        // For comparison operations
        public static bool operator >=(UnityVersion a, UnityVersion b) => a.Data >= b.Data;
        public static bool operator <=(UnityVersion a, UnityVersion b) => a.Data <= b.Data;
        public static bool operator >(UnityVersion a, UnityVersion b) => a.Data > b.Data;
        public static bool operator <(UnityVersion a, UnityVersion b) => a.Data < b.Data;

        public override bool Equals(object obj)
        {
            if (obj is UnityVersion other)
                return Data == other.Data;
            return false;
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }
    }
}
