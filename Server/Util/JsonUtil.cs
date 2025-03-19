using System.Text.Encodings.Web;
using System.Text.Json;

namespace WishServer.Util
{
    public class JsonUtil
    {

        public static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
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

        public static object? Deserialize(JsonElement element, Type returnType)
        {
            return JsonSerializer.Deserialize(element, returnType, JSON_SERIALIZER_OPTIONS);
        }
    }
}
