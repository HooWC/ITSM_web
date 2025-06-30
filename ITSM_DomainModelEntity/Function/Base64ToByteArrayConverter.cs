using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace ITSM_DomainModelEntity.Function
{
    public class Base64ToByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String: // Processing Base64 Strings
                    string base64 = reader.GetString();
                    return string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(base64);

                case JsonTokenType.StartArray: // Handles direct byte arrays [255,216,...]
                    List<byte> bytes = new List<byte>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TryGetByte(out byte b)) bytes.Add(b);
                    }
                    return bytes.ToArray();

                case JsonTokenType.StartObject: // Process Buffer format {"type":"Buffer","data":[255,216,...]}
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName &&
                            reader.GetString() == "data" &&
                            reader.Read())
                        {
                            return Read(ref reader, typeToConvert, options); // Recursively parse the data array
                        }
                    }
                    return null;

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value); // Unified output as Base64 string
        }
    }
} 