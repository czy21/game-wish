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
        Task<DYLiveDataTaskRes> StartTaskPush([Body] DYLiveDataTaskReq param, [Header("X-Token")] string accessToken);

        [Headers("content-type: application/json")]
        [Post("/live_data/task/stop")]
        Task<DYLiveDataTaskRes> StopTaskPush([Body] DYLiveDataTaskReq param, [Header("X-Token")] string accessToken);

        [Headers("content-type: application/json")]
        [Post("/live_data/task/get")]
        Task<DYLiveDataTaskRes> GetTaskStatus([Body] DYLiveDataTaskReq param, [Header("X-Token")] string accessToken);

        [Headers("content-type: application/json")]
        [Post("/live_data/ack")]
        Task<DYLiveDataAckRes> Ack([Body] DYLiveDataAckReq param, [Header("access-token")] string accessToken);
    }
}
