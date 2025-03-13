namespace WishServer.Model
{
    public class MessageMapping
    {
        public MessageKind Kind { get; set; }

        public Type RequestType { get; set; }

        public Type ResponseType { get; set; }
    }
}
