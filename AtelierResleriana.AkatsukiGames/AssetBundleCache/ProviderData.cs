using System.Text.Json.Serialization;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class ProviderData
    {
        [JsonPropertyName("m_Id")]
        public string Id { get; set; }

        [JsonPropertyName("m_ObjectType")]
        public ObjectType ObjectType { get; set; }

        [JsonPropertyName("m_Data")]
        public string Data { get; set; }
    }
}
