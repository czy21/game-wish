using System.Text.Json;

namespace WishServer.Util
{
    public class JsonUtil
    {

        public static JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static string Serialize<TValue>(TValue value)
        {
            return JsonSerializer.Serialize(value, JSON_SERIALIZER_OPTIONS);
        }

        public static TValue? Deserialize<TValue>(string json)
        {
            return JsonSerializer.Deserialize<TValue>(json, JSON_SERIALIZER_OPTIONS);
        }

        public static TValue? Deserialize<TValue>(JsonElement element)
        {
            return JsonSerializer.Deserialize<TValue>(element, JSON_SERIALIZER_OPTIONS);
        }
    }
}
