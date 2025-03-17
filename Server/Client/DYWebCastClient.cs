using Refit;
using WishServer.Client.DY;

namespace WishServer.Client
{
    public interface DYWebCastClient
    {
        [Headers("content-type: application/json")]
        [Post("/webcastmate/info")]
        Task<DYWebCastInfoRes> GetLiveInfo([Body] DYWebCastInfoReq param, [Header("X-Token")] string accessToken);

        [Headers("content-type: application/json")]
        [Post("/live_data/task/start")]
        Task<DYLiveDataTaskRes> StartPush([Body] DYLiveDataTaskReq param, [Header("X-Token")] string accessToken);

        [Headers("content-type: application/json")]
        [Post("/live_data/task/stop")]
        Task<DYLiveDataTaskRes> StopPush([Body] DYLiveDataTaskReq param, [Header("X-Token")] string accessToken);
    }
}
