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
                case JsonTokenType.String: // 处理 Base64 字符串
                    string base64 = reader.GetString();
                    return string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(base64);

                case JsonTokenType.StartArray: // 处理直接字节数组 [255,216,...]
                    List<byte> bytes = new List<byte>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TryGetByte(out byte b)) bytes.Add(b);
                    }
                    return bytes.ToArray();

                case JsonTokenType.StartObject: // 处理 Buffer 格式 {"type":"Buffer","data":[255,216,...]}
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType == JsonTokenType.PropertyName &&
                            reader.GetString() == "data" &&
                            reader.Read())
                        {
                            return Read(ref reader, typeToConvert, options); // 递归解析 data 数组
                        }
                    }
                    return null;

                default:
                    return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value); // 统一输出为 Base64 字符串
        }
    }
} 