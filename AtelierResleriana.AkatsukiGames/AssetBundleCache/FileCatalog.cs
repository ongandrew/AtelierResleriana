using System.Text.Json.Serialization;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class FileCatalog
    {
        [JsonPropertyName("_bundles")]
        public BundleInfo[] Bundles { get; set; }
    }
}
