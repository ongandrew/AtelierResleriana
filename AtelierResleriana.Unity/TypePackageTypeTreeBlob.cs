namespace AtelierResleriana.Unity
{
    public class TypePackageTypeTreeBlob
    {
        public long CreationTime { get; set; }
        public UnityVersion[] Versions { get; set; }
        public Dictionary<int, TypePackageClassInformation> ClassInformation { get; set; }
        public TypePackageCommonString CommonString { get; set; }
        public TypePackageNodeBuffer NodeBuffer { get; set; }
        public TypePackageStringBuffer StringBuffer { get; set; }
    }
}
