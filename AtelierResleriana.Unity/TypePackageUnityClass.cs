namespace AtelierResleriana.Unity
{
    public class TypePackageUnityClass
    {
        public ushort Name { get; set; }
        public ushort Base { get; set; }
        public TypePackageUnityClassFlags Flags { get; set; }
        public ushort? EditorRootNode { get; set; }
        public ushort? ReleaseRootNode { get; set; }
    }
}
