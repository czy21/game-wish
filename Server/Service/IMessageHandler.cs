using WishServer.Model;

namespace WishServer.Service
{
    public interface IMessageHandler
    {
        PlatformEnum GetPlatform();
        public string GetAccessTokenKey(string gameCode)
        {
            return $"game:{gameCode}:${GetPlatform()}:access_token";
        }

        Task<string> GetAccessToken(string gameCode);
        Task Init(Session session);
        Task Exit(Session session);
    }
}
