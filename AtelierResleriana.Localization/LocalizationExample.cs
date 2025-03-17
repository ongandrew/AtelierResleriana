using System.Text.Json.Serialization;

namespace AtelierResleriana.Localization
{
    public class LocalizationExample<T>
    {
        [JsonPropertyName("original")]
        public T Original { get; set; }
        [JsonPropertyName("localized")]
        public T Localized { get; set; }
    }
}
