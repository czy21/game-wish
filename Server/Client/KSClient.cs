using Refit;
using WishServer.Client.DY;
using WishServer.Client.KS;

namespace WishServer.Client
{
    public interface KSClient
    {
        [Headers("content-type: application/json")]
        [Get("/oauth2/access_token")]
        Task<KSAccessTokenRes> GetAccessToken([Query] KSAccessTokenReq param);

        [Headers("content-type: application/json")]
        [Post("/openapi/developer/live/smallPlay/bind")]
        Task<KSBindRes> Bind([Query("app_id")] string appId, [Query("access_token")] string accessToken, [Body] Dictionary<string,object> req);
    }
}
