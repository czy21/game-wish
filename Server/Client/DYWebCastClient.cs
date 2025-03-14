using Refit;
using WishServer.Client.DY;

namespace WishServer.Client
{
    public interface DYWebCastClient
    {
        [Headers("content-type: application/json")]
        [Post("/webcastmate/info")]
        DYWebCastInfoRes WebCastMateInfo([Body] DyWebCastInfoReq param, [Header("X-Token")] string accessToken);
    }
}
