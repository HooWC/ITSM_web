using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ITSM_DomainModelEntity.Function
{
    public class NewtonsoftBase64ToByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var base64 = reader.Value?.ToString();
                return string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(base64);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                // Handle {"type":"Buffer","data":[...]} format
                var obj = JObject.Load(reader);
                if (obj.ContainsKey("type") && obj["type"].ToString() == "Buffer" && obj.ContainsKey("data"))
                {
                    var dataArray = obj["data"] as JArray;
                    if (dataArray != null)
                    {
                        var byteArray = new byte[dataArray.Count];
                        for (int i = 0; i < dataArray.Count; i++)
                        {
                            byteArray[i] = dataArray[i].Value<byte>();
                        }
                        return byteArray;
                    }
                }
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // Write Base64 strings directly, not Buffer objects
            writer.WriteValue(Convert.ToBase64String(value));
        }
    }
}

