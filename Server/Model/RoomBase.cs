using System.Text.Json.Serialization;

namespace WishServer.Model
{
    public class RoomBase
    {
        [JsonPropertyName("id")]
        public string ID { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        [JsonPropertyName("personCount")]
        public int PersonCount { get; set; }
    }
}
