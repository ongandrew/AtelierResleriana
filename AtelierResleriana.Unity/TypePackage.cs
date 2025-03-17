using AtelierResleriana.Unity.Resources;

namespace AtelierResleriana.Unity
{
    public class TypePackage
    {
        private static TypePackage mInstance;
        public static TypePackage Instance
        {
            get
            {
                if (mInstance == null)
                {
                    TypePackageReader typePackageReader = new TypePackageReader();
                    mInstance = typePackageReader.Read(typeof(TypePackage).Assembly.GetManifestResourceStream(typeof(Root), "Uncompressed.tpk"));
                }

                return mInstance;
            }
        }

        public TypePackageHeader Header { get; set; }
        public TypePackageTypeTreeBlob TypeTreeBlob { get; set; }
    }
}
