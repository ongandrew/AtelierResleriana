namespace AtelierResleriana.Unity
{
    public class TypePackageCommonString
    {
        public List<(UnityVersion Version, byte Count)> VersionInformation { get; set; }
        public ushort[] StringBufferIndices { get; set; }

        public List<string> GetStrings(TypePackageStringBuffer buffer)
        {
            return StringBufferIndices.Select(i => buffer.Strings[i]).ToList();
        }
    }
}
