namespace WishServer.Model;

public interface IMessage
{
    MessageKind Kind { get; set; }
}