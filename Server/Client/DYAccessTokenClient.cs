using Refit;
using WishServer.Client.DY;

namespace WishServer.Client
{
    public interface DYAccessTokenClient
    {
        [Headers("content-type: application/json")]
        [Post("/apps/v2/token")]
        Task<DYAccessTokenRes> GetAccessToken([Body] DYAccessTokenReq param);
    }
}