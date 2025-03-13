using WishServer.Model;

namespace WishServer.Annotation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OnMessage : Attribute
    {
        private MessageKind Kind;

        public OnMessage(MessageKind kind)
        {
            Kind = kind;
        }

        public MessageKind GetKind() => Kind;
    }
}
