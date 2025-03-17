using WishServer.Model;

namespace WishServer.Service
{
    public interface IPlatformService
    {
        PlatformEnum GetPlatform();
        string GetAccessTokenKey();
        Task<string?> GetAccessToken();
        Task Init(Session session, string? roomId);
    }
}
