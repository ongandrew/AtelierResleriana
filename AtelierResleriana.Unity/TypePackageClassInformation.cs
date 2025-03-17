namespace AtelierResleriana.Unity
{
    public class TypePackageClassInformation
    {
        public int Id { get; set; }
        public List<(UnityVersion Version, TypePackageUnityClass Class)> Classes { get; set; }
    }
}
