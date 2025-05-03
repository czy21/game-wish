using Microsoft.AspNetCore.Mvc;
using Sunny.Framework.External.Client.DY;
using WishServer.Service.impl;

namespace WishServer.Controllers
{
    [Route("dy")]
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
        public async Task<DYWebCastInfoRes> GetLiveInfo([FromQuery(Name = "token")] string token)
        {
            return await _dyPlatformService.GetLiveInfo(token);
        }

        [HttpPost("receive")]
        public async Task Receive([FromBody] List<Dictionary<string,object>> param)
        {
            string? roomId = Request.Headers["x-roomid"];
            string? msgType = Request.Headers["x-msg-type"];
            await _dyPlatformService.SendMessages(roomId, msgType, param);
        }
    }
}
