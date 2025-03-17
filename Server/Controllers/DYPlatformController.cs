using Microsoft.AspNetCore.Mvc;
using WishServer.Client.DY;
using WishServer.Service.impl;

namespace WishServer.Controllers
{
    [Route("platform/dy")]
    public class DYPlatformController : Controller
    {
        private readonly ILogger<DYPlatformController> _logger;
        private readonly DYPlatformService _dyPlatformService;

        public DYPlatformController(
            ILogger<DYPlatformController> logger,
            DYPlatformService dyPlatformService
        )
        {
            _logger = logger;
            _dyPlatformService = dyPlatformService;
        }

        /*
         * token 直播伴侣token
         */
        [HttpGet("getLiveInfo")]
        public Task<DYWebCastInfoRes> GetLiveInfo([FromQuery(Name = "token")] string token)
        {
            return _dyPlatformService.GetLiveInfo(token);
        }
    }
}
