using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AtelierResleriana.Text
{
    public class PackedTextJsonConverter : JsonConverter<PackedText>
    {
        public override PackedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var packedText = new PackedText();

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLower())
                {
                    case "properties":
                        ReadProperties(ref reader, packedText);
                        break;
                    case "entries":
                        ReadEntries(ref reader, packedText);
                        break;
                    default:
                        throw new JsonException($"Unexpected property: {propertyName}");
                }
            }

            return packedText;
        }

        public override void Write(Utf8JsonWriter writer, PackedText value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName("Properties") ?? "Properties");
            writer.WriteStartArray();
            foreach (var property in value.Properties)
            {
                writer.WriteStartObject();
                writer.WriteNumber(options.PropertyNamingPolicy?.ConvertName("Id") ?? "Id", property.Id);
                writer.WriteNumber(options.PropertyNamingPolicy?.ConvertName("Type") ?? "Type", property.Type);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName("Entries") ?? "Entries");
            writer.WriteStartArray();
            foreach (var entry in value.Entries)
            {
                writer.WriteStartObject();
                foreach (var (key, v) in entry)
                {
                    writer.WritePropertyName(key.ToString());
                    JsonSerializer.Serialize(writer, v, options);
                }
                writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        private static void ReadProperties(ref Utf8JsonReader reader, PackedText packedText)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of properties array");
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of property object");
                }

                uint id = 0, type = 0;
                bool hasId = false, hasType = false;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name");
                    }

                    var propName = reader.GetString()?.ToLower();
                    reader.Read();

                    switch (propName)
                    {
                        case "id":
                            id = reader.GetUInt32();
                            hasId = true;
                            break;
                        case "type":
                            type = reader.GetUInt32();
                            hasType = true;
                            break;
                        default:
                            throw new JsonException($"Unexpected property in Property object: {propName}");
                    }
                }

                if (!hasId || !hasType)
                {
                    throw new JsonException("Property object missing required fields");
                }

                packedText.AddProperty(id, type);
            }
        }

        private static void ReadEntries(ref Utf8JsonReader reader, PackedText packedText)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of entries array");
            }

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of entry object");
                }

                var entry = new Dictionary<uint, object>();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name");
                    }

                    var key = uint.Parse(reader.GetString() ?? throw new JsonException("Null property name"));
                    reader.Read();

                    object value = reader.TokenType switch
                    {
                        JsonTokenType.Number => reader.GetUInt64(),
                        JsonTokenType.String => reader.GetString() ?? "",
                        _ => throw new JsonException($"Unexpected value type for property {key}")
                    };

                    entry[key] = value;
                }

                packedText.AddEntry(entry);
            }
        }
    }

    public static class PackedTextJsonSerializerOptions
    {
        public static JsonSerializerOptions DefaultOptions => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Converters = { new PackedTextJsonConverter() }
        };
    }
}