using Microsoft.AspNetCore.Mvc;
using WishServer.Service.impl;

namespace WishServer.Controllers
{
    [Route("ab")]
    public class ABPlatformController : Controller
    {
        private readonly ILogger<DYPlatformController> _logger;
        private readonly ABPlatformService _abPlatformService;

        public ABPlatformController(
            ILogger<DYPlatformController> logger,
            ABPlatformService abPlatformService
        )
        {
            _logger = logger;
            _abPlatformService = abPlatformService;
        }

        [HttpPost("receive")]
        public async Task Receive([FromBody] List<Dictionary<string, object>> param)
        {
            string? roomId = Request.Headers["x-roomid"];
            string? msgType = Request.Headers["x-msg-type"];
            await _abPlatformService.SendMessages(roomId, msgType, param);
        }
    }
}
