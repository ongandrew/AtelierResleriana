using System.Text.Json.Serialization;

namespace AtelierResleriana.AkatsukiGames.AssetBundleCache
{
    public class BundleInfo
    {
        [JsonPropertyName("_relativePath")]
        public string RelativePath { get; set; }

        [JsonPropertyName("_bundleName")]
        public string BundleName { get; set; }

        [JsonPropertyName("_hash")]
        public string Hash { get; set; }

        [JsonPropertyName("_crc")]
        public uint Crc { get; set; }

        [JsonPropertyName("_fileSize")]
        public long FileSize { get; set; }

        [JsonPropertyName("_fileMd5")]
        public string FileMd5 { get; set; }

        // Source:
        //namespace UnityEngine;

        //public enum CompressionType
        //{
        //    None,
        //    Lzma,
        //    Lz4,
        //    Lz4HC
        //}
        [JsonPropertyName("_compression")]
        public int Compression { get; set; }

        [JsonPropertyName("_userData")]
        public string UserData { get; set; }
    }
}
