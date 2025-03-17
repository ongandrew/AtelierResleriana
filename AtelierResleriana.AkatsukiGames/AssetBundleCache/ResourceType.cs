using System.Text.Json.Serialization;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class ObjectType
    {
        [JsonPropertyName("m_AssemblyName")]
        public string AssemblyName { get; set; }

        [JsonPropertyName("m_ClassName")]
        public string ClassName { get; set; }
    }

}
