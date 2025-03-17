using System.Text.Json.Serialization;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class Catalog
    {
        [JsonPropertyName("m_LocatorId")]
        public string LocatorId { get; set; }

        [JsonPropertyName("m_BuildResultHash")]
        public string BuildResultHash { get; set; }

        [JsonPropertyName("_mainAssetLabel")]
        public string MainAssetLabel { get; set; }

        [JsonPropertyName("_uniqueBuildId")]
        public string UniqueBuildId { get; set; }

        [JsonPropertyName("_version")]
        public int Version { get; set; }

        [JsonPropertyName("_userData")]
        public string UserData { get; set; }

        [JsonPropertyName("m_InstanceProviderData")]
        public ProviderData InstanceProviderData { get; set; }

        [JsonPropertyName("m_SceneProviderData")]
        public ProviderData SceneProviderData { get; set; }

        [JsonPropertyName("m_ResourceProviderData")]
        public ProviderData[] ResourceProviderData { get; set; }

        [JsonPropertyName("m_ProviderIds")]
        public string[] ProviderIds { get; set; }

        [JsonPropertyName("m_InternalIds")]
        public string[] InternalIds { get; set; }

        [JsonPropertyName("m_KeyDataString")]
        public string KeyDataString { get; set; }

        [JsonPropertyName("m_BucketDataString")]
        public string BucketDataString { get; set; }

        [JsonPropertyName("m_EntryDataString")]
        public string EntryDataString { get; set; }

        [JsonPropertyName("m_ExtraDataString")]
        public string ExtraDataString { get; set; }

        [JsonPropertyName("m_resourceTypes")]
        public ObjectType[] ResourceTypes { get; set; }

        [JsonPropertyName("m_InternalIdPrefixes")]
        public object[] InternalIdPrefixes { get; set; }

        [JsonPropertyName("_fileCatalog")]
        public FileCatalog FileCatalog { get; set; }

        [JsonPropertyName("_mainAssetBundles")]
        public string[] MainAssetBundles { get; set; }
    }
}