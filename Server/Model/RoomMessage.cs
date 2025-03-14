
namespace WishServer.Model
{
    public class RoomMessage : MessageBase
    {
        public required string ID { get; set; }

        public string? Name { get; set; }

        public string? Content { get; set; }

        public int? Count { get; set; }
    }
}