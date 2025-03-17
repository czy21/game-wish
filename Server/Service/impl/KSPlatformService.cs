using WishServer.Model;

namespace WishServer.Service.impl
{
    public class KSPlatformService : IPlatformService
    {
        public Task<string?> GetAccessToken()
        {
            throw new NotImplementedException();
        }

        public string GetAccessTokenKey()
        {
            throw new NotImplementedException();
        }

        public PlatformEnum GetPlatform()
        {
           return PlatformEnum.KS;
        }

        public Task Init(Session session, string? roomId)
        {
            throw new NotImplementedException();
        }
    }
}
