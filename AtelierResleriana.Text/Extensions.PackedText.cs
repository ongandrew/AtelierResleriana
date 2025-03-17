using System.IO;
using System.Text.Json;

namespace AtelierResleriana.Text
{
    public static partial class Extensions
    {
        public static byte[] ToBytes(this PackedText packedText)
        {
            using var stream = new MemoryStream();
            var writer = new PackedTextWriter();
            writer.Write(stream, packedText);
            return stream.ToArray();
        }

        public static string ToJson(this PackedText packedText)
        {
            return JsonSerializer.Serialize(packedText, PackedTextJsonSerializerOptions.DefaultOptions);
        }

        public static string ToJson(this PackedText packedText, JsonSerializerOptions options)
        {
            return JsonSerializer.Serialize(packedText, options);
        }
    }
}
