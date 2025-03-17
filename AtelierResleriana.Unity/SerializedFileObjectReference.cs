namespace AtelierResleriana.Unity
{
    public class SerializedFileObjectReference
    {
        /// <summary>
        /// An index that tells you which file the script/object is in:
        /// 0   - This file
        /// > 0 - External File
        /// < 0 - Built-in Files
        /// </summary>
        public int Index { get; set; }
        public long PathId { get; set; }
    }
}
