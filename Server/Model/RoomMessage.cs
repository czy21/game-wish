
namespace WishServer.Model
{
    public class RoomMessage : MessageBase
    {

        public string ID { get; set; }

        public string? Name { get; set; }

        public string? Content { get; set; }

        public int? PersonCount { get; set; }
    }
}