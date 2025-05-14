using System.Text.Json;

namespace WishServer.Model;

public class MessageDTO : MessageBase, IMessage
{
    public JsonElement Data { get; set; }
}