namespace AtelierResleriana.Unity
{
    public class SerializedFileObject
    {
        public long PathId { get; set; }
        public long Offset { get; set; }
        public uint Size { get; set; }
        public int TypeId { get; set; }
        public SerializedFileType Type { get; set; }
        public int ClassId { get; set; }
        public ushort? IsDestroyed { get; set; }
        public short? ScriptTypeIndex { get; set; }
        public byte? IsStripped { get; set; }
        public byte[] Data { get; set; }
    }
}
