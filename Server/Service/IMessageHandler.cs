using WishServer.Model;

namespace WishServer.Service
{
    public interface IMessageHandler
    {
        PlatformEnum GetPlatform();
        string GetAccessTokenKey();
        Task<string> GetAccessToken();
        Task Init(Session session);
        Task Exit(Session session);
        Task SendMessages(string? roomId, string? msgType, List<Dictionary<string, object>> param);
    }
}
