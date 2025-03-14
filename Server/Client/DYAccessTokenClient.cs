using Refit;
using WishServer.Client.DY;

namespace WishServer.Client
{
    public interface DYAccessTokenClient
    {
        [Headers("content-type: application/json")]
        [Post("/apps/v2/token")]
        DYAccessTokenRes GetAccessTokenRes([Body] DYAccessTokenReq param);
    }
}
