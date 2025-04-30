using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace ITSM_Insfrastruture.Repository.Token
{
    public static class SessionExtensions
    {
        public static void SetString(this ISession session, string key, string value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(value));
        }

        public static string GetString(this ISession session, string key)
        {
            byte[] data;
            if (session.TryGetValue(key, out data))
            {
                return Encoding.UTF8.GetString(data);
            }
            return null;
        }

        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var data = session.GetString(key);
            if (string.IsNullOrEmpty(data))
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(data);
        }
    }
} 