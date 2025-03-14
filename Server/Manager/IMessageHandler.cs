
using WishServer.Model;

namespace WishServer.Manager
{
    public interface IMessageHandler
    {
        Task Exit(Session session);
    }
}
