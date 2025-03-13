using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace WishServer.Model
{
    public class MessageDTO
    {
        [JsonPropertyName("kind")]
        public MessageKind Kind { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
