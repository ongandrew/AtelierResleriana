using System.Text.Json.Nodes;

namespace AtelierResleriana.Localization
{
    public class LocalizationData
    {
        public long Version { get; set; }
        public LocalizationSerializedFileRegistry LocalizationSerializedFileRegistry { get; set; }
        public Dictionary<string, Dictionary<string, JsonNode>> MasterDataLocalizationData { get; set; }
        public Dictionary<string, PackedTextEntryLocalization[]> TextAssetLocalizationData { get; set; }
    }
}
