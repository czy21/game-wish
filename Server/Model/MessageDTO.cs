using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace WishServer.Model
{
    public class MessageDTO
    {
        
        public MessageKind Kind { get; set; }

        public JsonElement Data { get; set; }
    }
}
