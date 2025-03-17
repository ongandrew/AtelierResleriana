using System.Text.Json;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class CatalogReader
    {
        public Catalog Read(string path)
        {
            return Read(File.OpenRead(path));
        }

        public Catalog Read(Stream stream)
        {
            using StreamReader streamReader = new StreamReader(stream);
            return JsonSerializer.Deserialize<Catalog>(stream);
        }
    }
}
