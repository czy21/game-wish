using System.Text.Json;

namespace WishServer.Util
{
    public class JsonUtil
    {


        public static TValue? Deserialize<TValue>(string json)
        {
            return JsonSerializer.Deserialize<TValue>(json);
        }
    }
}
