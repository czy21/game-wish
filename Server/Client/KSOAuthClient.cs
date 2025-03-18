using Refit;
using WishServer.Client.DY;
using WishServer.Client.KS;

namespace WishServer.Client
{
    public interface KSOAuthClient
    {
        [Headers("content-type: application/json")]
        [Post("/oauth2/access_token")]
        Task<KSAccessTokenRes> GetAccessToken([Body] KSAccessTokenReq param);
    }
}
